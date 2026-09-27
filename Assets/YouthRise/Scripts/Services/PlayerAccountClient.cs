using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YouthRise
{
    [Serializable] public sealed class PlayerAccount
    {
        public string id, name, nickname, gender, role;
        public int age;
        public string CharacterName => gender == "female" ? "Anita" : "Alex";
    }
    [Serializable] public sealed class AccountRequest
    {
        public string name, nickname, password, gender;
        public int age;
        public bool consent;
    }
    [Serializable] public sealed class AccountResponse
    {
        public bool success;
        public string token, error, reportId, status;
        public PlayerAccount user;
    }
    [Serializable] public sealed class ResultUpload { public RecordedChoice[] choices; }
    [Serializable] public sealed class AccountServiceSettings
    {
        public string baseUrl = "http://127.0.0.1:8787";
        public static AccountServiceSettings Load()
        {
            var resource = Resources.Load<TextAsset>("YouthRise/account-services");
            return resource == null ? new AccountServiceSettings() : JsonUtility.FromJson<AccountServiceSettings>(resource.text);
        }
    }
    public static class PlayerAccountClient
    {
        public static IEnumerator Post(string baseUrl, string route, string token, string json, Action<AccountResponse> done)
        {
            if (!OnlineSupportSettings.ValidBaseUrl(baseUrl)) { done(new AccountResponse { error = "Alamat server tidak valid." }); yield break; }
            using (var request = new UnityWebRequest(baseUrl.TrimEnd('/') + route, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-YouthRise-Client", "1");
                if (!string.IsNullOrEmpty(token)) request.SetRequestHeader("Authorization", "Bearer " + token);
                request.timeout = 15; request.redirectLimit = 0;
                UnityWebRequestAsyncOperation operation = null;
                try { operation = request.SendWebRequest(); } catch (Exception) { }
                if (operation == null) { done(new AccountResponse { error = "Server tidak dapat dihubungi. Jalankan backend lokal." }); yield break; }
                yield return operation;
                AccountResponse result = null;
                try { if (request.downloadHandler.data.Length <= 32768) result = JsonUtility.FromJson<AccountResponse>(request.downloadHandler.text); } catch (Exception) { }
                if (request.result != UnityWebRequest.Result.Success && result != null) result.success = false;
                done(result ?? new AccountResponse { error = "Koneksi gagal. Jalankan backend lokal; status pengiriman belum terkonfirmasi." });
            }
        }
    }
}
