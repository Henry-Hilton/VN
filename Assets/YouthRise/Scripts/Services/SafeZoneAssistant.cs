using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace YouthRise
{
    [Serializable]
    public sealed class SafeZoneAssessment
    {
        public string category;
        public string urgency;
        public string supportiveResponse;
        public string suggestedAction;
        public bool immediateSafetyConcern;
    }

    [Serializable]
    internal sealed class LocalReportDraft
    {
        public string reportId;
        public string createdUtc;
        public string status;
        public string category;
        public string urgency;
        public string description;
    }

    public sealed class ReportSaveResult
    {
        public bool success;
        public string reportId;
        public string message;
    }

    /// <summary>
    /// Explainable, local keyword triage for the prototype. It never diagnoses a person,
    /// contacts an authority, or claims that a report has been submitted.
    /// </summary>
    public sealed class SafeZoneAssistant
    {
        private static readonly string[] UrgentKeywords =
        {
            "bunuh diri", "ingin mati", "mau mati", "melukai diri", "self harm",
            "bahaya sekarang", "dipukul sekarang", "diancam bunuh", "senjata"
        };

        private static readonly Dictionary<string, string[]> CategoryKeywords =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "Perundungan", new[] { "bully", "dirundung", "diejek", "dihina", "dikucilkan" } },
                { "Kekerasan atau Ancaman", new[] { "dipukul", "ditendang", "kekerasan", "diancam", "ancam" } },
                { "Pelecehan", new[] { "pelecehan", "seksual", "diraba", "dipaksa", "foto pribadi" } },
                { "Zat Berisiko", new[] { "vape", "rokok", "narkoba", "alkohol", "obat terlarang" } },
                { "Kesehatan Mental", new[] { "cemas", "panik", "depresi", "sedih", "takut", "sendiri" } },
                { "Masalah Keluarga", new[] { "keluarga", "orang tua", "rumah", "cerai", "bertengkar" } }
            };

        public SafeZoneAssessment Assess(string input)
        {
            string normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
            bool urgent = UrgentKeywords.Any(normalized.Contains);
            string category = DetectCategory(normalized);

            if (urgent)
            {
                return new SafeZoneAssessment
                {
                    category = category,
                    urgency = "SEGERA",
                    immediateSafetyConcern = true,
                    supportiveResponse = "Terima kasih sudah bercerita. Kamu layak mendapat bantuan dan keselamatanmu adalah prioritas.",
                    suggestedAction = "Jika ada bahaya sekarang, jangan menunggu aplikasi. Pergi ke tempat aman dan hubungi layanan darurat setempat, orang dewasa tepercaya, atau Guru BK. Jangan hadapi situasi ini sendirian."
                };
            }

            bool concern = category != "Dukungan Umum";
            return new SafeZoneAssessment
            {
                category = category,
                urgency = concern ? "PERLU DITINJAU" : "DUKUNGAN",
                immediateSafetyConcern = false,
                supportiveResponse = "Terima kasih sudah cerita. Mencari bantuan adalah langkah yang berani, dan perasaanmu pantas didengarkan.",
                suggestedAction = concern
                    ? "Simpan sebagai draft jika kamu siap, lalu tunjukkan kepada Guru BK atau orang dewasa tepercaya. Prototype ini belum mengirim laporan ke mana pun."
                    : "Coba ceritakan apa yang terjadi, kapan, dan bantuan seperti apa yang terasa aman bagimu."
            };
        }

        public string CreateChatResponse(string input)
        {
            SafeZoneAssessment assessment = Assess(input);
            if (string.IsNullOrWhiteSpace(input))
                return "Aku siap mendengarkan. Kamu bisa mulai dari hal yang paling nyaman untuk diceritakan.";

            return assessment.supportiveResponse + "\n\n" + assessment.suggestedAction;
        }

        public ReportSaveResult SaveLocalDraft(string description, SafeZoneAssessment assessment)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return new ReportSaveResult
                {
                    success = false,
                    message = "Tuliskan kejadian terlebih dahulu."
                };
            }

            assessment ??= Assess(description);
            string id = "YR-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var draft = new LocalReportDraft
            {
                reportId = id,
                createdUtc = DateTime.UtcNow.ToString("O"),
                status = "LOCAL_DRAFT_NOT_SUBMITTED",
                category = assessment.category,
                urgency = assessment.urgency,
                description = description.Trim()
            };

            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "YouthRise", "Reports");
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, id + ".json");
                File.WriteAllText(path, JsonUtility.ToJson(draft, true));

                return new ReportSaveResult
                {
                    success = true,
                    reportId = id,
                    message = $"Draft {id} tersimpan lokal. Belum dikirim dan belum terenkripsi."
                };
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"YouthRise report draft failed: {exception.Message}");
                return new ReportSaveResult
                {
                    success = false,
                    message = "Draft tidak dapat disimpan pada perangkat ini."
                };
            }
        }

        private static string DetectCategory(string normalized)
        {
            foreach (KeyValuePair<string, string[]> category in CategoryKeywords)
            {
                if (category.Value.Any(normalized.Contains))
                    return category.Key;
            }

            return "Dukungan Umum";
        }
    }
}
