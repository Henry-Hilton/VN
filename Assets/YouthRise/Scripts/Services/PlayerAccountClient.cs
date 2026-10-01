using System;
using System.Collections;
using UnityEngine;

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
    public static class PlayerAccountClient
    {
        public static IEnumerator Post(string route, string token, string json, Action<AccountResponse> done)
        {
            // Compatibility facade for the existing UI; no network or backend is used.
            yield return null;
            AccountResponse result;
            try {
                switch (route) {
                    case "/api/register": case "/api/login": result = LocalPlayerPortal.Authenticate(JsonUtility.FromJson<AccountRequest>(json), route == "/api/register"); break;
                    case "/api/logout": LocalPlayerPortal.Logout(token); result = new AccountResponse { success = true }; break;
                    case "/api/results": result = LocalPlayerPortal.StoreResults(token, JsonUtility.FromJson<ResultUpload>(json)); break;
                    case "/api/incidents": result = LocalPlayerPortal.StoreIncident(token, JsonUtility.FromJson<SupportReportRequest>(json)); break;
                    default: result = new AccountResponse { error = "Tindakan lokal tidak tersedia." }; break;
                }
            } catch { result = new AccountResponse { error = "Data lokal tidak dapat diproses." }; }
            done(result);
        }
    }
}
