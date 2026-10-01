using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YouthRise
{
    [Serializable] public sealed class LocalPlayerRecord
    {
        public PlayerAccount user;
        public string salt, passwordHash;
        public PlayerProfile results;
        public string updatedUtc;
    }
    [Serializable] public sealed class LocalIncident
    {
        public string id, userId, text, createdUtc, reviewedUtc;
        public string status = "new";
    }
    [Serializable] public sealed class LocalPortalData
    {
        public List<LocalPlayerRecord> players = new List<LocalPlayerRecord>();
        public List<LocalIncident> reports = new List<LocalIncident>();
        public string counselorSalt, counselorHash;
    }

    /// <summary>Device-only demo storage. PlayerPrefs is not encrypted or tamper-proof.</summary>
    public static class LocalPlayerPortal
    {
        public const string StorageKey = "YouthRise.localPortal.v1";
        private static readonly Dictionary<string, string> Sessions = new Dictionary<string, string>();
        public static LocalPortalData Read()
        {
            string json = PlayerPrefs.GetString(StorageKey, "");
            if (string.IsNullOrEmpty(json)) return new LocalPortalData();
            var data = JsonUtility.FromJson<LocalPortalData>(json);
            if (data == null || data.players == null || data.reports == null)
                throw new InvalidOperationException("Data akun lokal tidak dapat dibaca. Jangan hapus PlayerPrefs jika ingin mempertahankan data.");
            // Unity serializes null inline objects as empty objects. Preserve "no results yet".
            foreach (var player in data.players)
                if (string.IsNullOrEmpty(player.updatedUtc)) player.results = null;
            return data;
        }
        private static void Write(LocalPortalData data)
        {
            PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(data)); PlayerPrefs.Save();
        }
        private static string Salt()
        {
            var bytes = new byte[16]; using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        private static string Hash(string password, string salt)
        {
            using (var hash = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 100000, HashAlgorithmName.SHA256))
                return Convert.ToBase64String(hash.GetBytes(32));
        }
        private static bool Matches(string password, string salt, string expected)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(expected)) return false;
            byte[] a = Convert.FromBase64String(Hash(password, salt)), b = Convert.FromBase64String(expected);
            if (a.Length != b.Length) return false;
            int difference = 0; for (int i = 0; i < a.Length; i++) difference |= a[i] ^ b[i];
            return difference == 0;
        }
        public static AccountResponse Authenticate(AccountRequest request, bool register)
        {
            try
            {
                string nickname = (request.nickname ?? "").Trim().ToLowerInvariant();
                if (!Regex.IsMatch(nickname, @"^[a-z0-9_.-]{3,30}$") || string.IsNullOrEmpty(request.password) || request.password.Length > 128)
                    return new AccountResponse { error = "Periksa nickname dan password." };
                var data = Read(); var record = data.players.FirstOrDefault(p => p.user.nickname == nickname);
                if (register)
                {
                    if (!request.consent || request.password.Length < 8 || string.IsNullOrWhiteSpace(request.name) || request.name.Trim().Length > 80 ||
                        request.age < 11 || request.age > 18 || (request.gender != "male" && request.gender != "female"))
                        return new AccountResponse { error = "Isi nama, usia 11–18, gender, password minimal 8 karakter, dan persetujuan." };
                    if (record != null) return new AccountResponse { error = "Nickname sudah digunakan di perangkat ini." };
                    string salt = Salt();
                    record = new LocalPlayerRecord { user = new PlayerAccount { id = Guid.NewGuid().ToString(), nickname = nickname, name = request.name.Trim(),
                        age = request.age, gender = request.gender, role = "player" }, salt = salt, passwordHash = Hash(request.password, salt) };
                    data.players.Add(record); Write(data);
                }
                else if (record == null || !Matches(request.password, record.salt, record.passwordHash))
                    return new AccountResponse { error = "Nickname atau password salah." };
                string token = Guid.NewGuid().ToString("N"); Sessions[token] = record.user.id;
                return new AccountResponse { success = true, user = record.user, token = token };
            }
            catch { return new AccountResponse { error = "Penyimpanan akun lokal tidak dapat diakses." }; }
        }
        public static void Logout(string token) { if (token != null) Sessions.Remove(token); }
        public static AccountResponse StoreResults(string token, ResultUpload request)
        {
            try
            {
                if (token == null || !Sessions.TryGetValue(token, out string id)) return new AccountResponse { error = "Silakan masuk lagi." };
                var data = Read(); var record = data.players.First(p => p.user.id == id);
                record.results = new PlayerProfile { recordedChoices = (request.choices ?? new RecordedChoice[0]).ToList() };
                record.updatedUtc = DateTime.UtcNow.ToString("O"); Write(data);
                return new AccountResponse { success = true };
            }
            catch { return new AccountResponse { error = "Hasil belum tersimpan di dashboard lokal." }; }
        }
        public static AccountResponse StoreIncident(string token, SupportReportRequest request)
        {
            try
            {
                if (token == null || !Sessions.TryGetValue(token, out string id)) return new AccountResponse { error = "Silakan masuk lagi." };
                if (!request.consent || !Regex.IsMatch(request.reportId ?? "", @"^YR-[a-f0-9]{32}$") || string.IsNullOrWhiteSpace(request.text) || request.text.Length > 3500)
                    return new AccountResponse { error = "Periksa isi laporan dan konfirmasi." };
                var data = Read(); var existing = data.reports.FirstOrDefault(r => r.id == request.reportId);
                if (existing != null && (existing.userId != id || existing.text != request.text)) return new AccountResponse { error = "ID laporan sudah digunakan." };
                if (existing == null) { data.reports.Add(new LocalIncident { id = request.reportId, userId = id, text = request.text, createdUtc = DateTime.UtcNow.ToString("O") }); Write(data); }
                return new AccountResponse { success = true, reportId = request.reportId, status = "received_by_dashboard" };
            }
            catch { return new AccountResponse { error = "Laporan belum tersimpan di perangkat." }; }
        }
        public static bool HasCounselorPassword => !string.IsNullOrEmpty(Read().counselorHash);
        public static string OpenDashboard(string password)
        {
            var data = Read();
            if (string.IsNullOrEmpty(data.counselorHash))
            {
                if (string.IsNullOrEmpty(password) || password.Length < 12 || password.Length > 128) return null;
                data.counselorSalt = Salt(); data.counselorHash = Hash(password, data.counselorSalt); Write(data);
            }
            else if (string.IsNullOrEmpty(password) || !Matches(password, data.counselorSalt, data.counselorHash)) return null;
            string token = Guid.NewGuid().ToString("N"); Sessions[token] = "counselor"; return token;
        }
        public static LocalPortalData Dashboard(string token)
        {
            if (token == null || !Sessions.TryGetValue(token, out string role) || role != "counselor") throw new UnauthorizedAccessException();
            return Read();
        }
        public static void Review(string token, string reportId, string status)
        {
            var data = Dashboard(token);
            if (status != "new" && status != "reviewed" && status != "closed") throw new ArgumentException("Invalid status");
            var report = data.reports.First(r => r.id == reportId); report.status = status; report.reviewedUtc = DateTime.UtcNow.ToString("O"); Write(data);
        }
    }
}
