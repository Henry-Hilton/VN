using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YouthRise
{
    [Serializable]
    public sealed class OnlineSupportSettings
    {
        public string baseUrl = "";
        public string recipientLabel = "Guru BK (belum dikonfigurasi)";
        public bool enabled;

        public bool IsConfigured => enabled && ValidBaseUrl(baseUrl) && !string.IsNullOrWhiteSpace(recipientLabel);
        public static bool ValidBaseUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || !string.IsNullOrEmpty(uri.UserInfo)
                || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)) return false;
            return uri.Scheme == "https" || (uri.Scheme == "http" && uri.IsLoopback);
        }
        public static OnlineSupportSettings Load()
        {
            try
            {
                TextAsset asset = Resources.Load<TextAsset>("YouthRise/online-services");
                return asset == null ? new OnlineSupportSettings() : JsonUtility.FromJson<OnlineSupportSettings>(asset.text) ?? new OnlineSupportSettings();
            }
            catch { return new OnlineSupportSettings(); }
        }
    }
    [Serializable] public sealed class SupportMessage { public string role, content; }
    [Serializable] public sealed class SupportChatRequest { public bool consent; public SupportMessage[] messages; }
    [Serializable] public sealed class SupportReportRequest
    {
        public string reportId, kind, text, recipientLabel;
        public bool consent;
    }
    [Serializable] public sealed class SupportResponse
    {
        public bool success;
        public string reply, error, reportId, status, messageId, acceptedUtc, recipientLabel;
    }
    public static class OnlineSupportClient
    {
        public static IEnumerator Post(OnlineSupportSettings settings, string route, string accessCode, string json, Action<SupportResponse> done)
        {
            if (!settings.IsConfigured || string.IsNullOrWhiteSpace(accessCode))
            { done(new SupportResponse { error = "Koneksi belum dikonfigurasi atau kode akses belum diisi." }); yield break; }
            using (var request = new UnityWebRequest(settings.baseUrl.TrimEnd('/') + route, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + accessCode);
                request.timeout = 35;
                request.redirectLimit = 0;
                UnityWebRequestAsyncOperation operation = null;
                try { operation = request.SendWebRequest(); }
                catch (Exception) { /* For example, HTTP is disabled by platform settings. */ }
                if (operation == null)
                {
                    done(new SupportResponse { error = "Koneksi tidak dapat dimulai. Periksa alamat HTTPS dan pengaturan platform." });
                    yield break;
                }
                yield return operation;
                SupportResponse result = null;
                try
                {
                    if (request.result == UnityWebRequest.Result.Success && request.downloadHandler.data.Length <= 32768)
                        result = JsonUtility.FromJson<SupportResponse>(request.downloadHandler.text);
                }
                catch { /* A malformed response is not proof of submission. */ }
                done(result ?? new SupportResponse { error = "Layanan gagal atau waktunya habis. Status belum terkonfirmasi; jangan menganggap laporan terkirim." });
            }
        }
        public static bool IsAcceptedReceipt(SupportReportRequest request, SupportResponse result)
        {
            return request != null && result != null && result.success && result.status == "accepted_by_whatsapp"
                && result.reportId == request.reportId && result.recipientLabel == request.recipientLabel
                && !string.IsNullOrWhiteSpace(result.messageId) && DateTime.TryParse(result.acceptedUtc, out _);
        }
    }
}
