#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YouthRise.EditorTools
{
    // Explicit opt-in integration test against an isolated local QA server only.
    public sealed class YouthRisePortalSmokeTest
    {
        private YouthRisePrototype host;
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        private T Get<T>(string name) => (T)typeof(YouthRisePrototype).GetField(name, Fields).GetValue(host);
        private void Set(string name, object value) => typeof(YouthRisePrototype).GetField(name, Fields).SetValue(host, value);
        private void Call(string name, params object[] args) => typeof(YouthRisePrototype).GetMethod(name, Fields).Invoke(host, args);
        private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

        public static void RunBatch()
        {
            if (!File.Exists("Logs/YouthRiseQA/portal-url.txt")) { Debug.LogError("Missing isolated QA server URL."); EditorApplication.Exit(1); return; }
            Directory.CreateDirectory("Logs/YouthRiseQA");
            File.WriteAllText("Logs/YouthRiseQA/portal-smoke.txt", "RUNNING");
            SessionState.SetBool("YouthRise.PortalBatch", true);
            SessionState.SetBool("YouthRise.PortalBatchStarted", false);
            SessionState.SetFloat("YouthRise.PortalBatchDeadline", (float)EditorApplication.timeSinceStartup + 120);
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void ResumeBatch()
        {
            EditorApplication.update += () => {
                if (!SessionState.GetBool("YouthRise.PortalBatch", false)) return;
                if (EditorApplication.timeSinceStartup > SessionState.GetFloat("YouthRise.PortalBatchDeadline", 0)) {
                    Debug.LogError("Portal smoke test timed out."); EditorApplication.Exit(1); return;
                }
                if (File.Exists("Logs/YouthRiseQA/portal-smoke.txt")) {
                    string result = File.ReadAllText("Logs/YouthRiseQA/portal-smoke.txt");
                    if (result.StartsWith("PASSED") || result.StartsWith("FAILED")) {
                        SessionState.SetBool("YouthRise.PortalBatch", false);
                        EditorApplication.Exit(result.StartsWith("PASSED") ? 0 : 1); return;
                    }
                }
                if (EditorApplication.isPlaying && !SessionState.GetBool("YouthRise.PortalBatchStarted", false)
                    && UnityEngine.Object.FindAnyObjectByType<YouthRisePrototype>() != null) {
                    SessionState.SetBool("YouthRise.PortalBatchStarted", true); Run();
                }
            };
        }

        [MenuItem("YouthRise/QA/Run Account Integration Smoke Test", false, 101)]
        private static void Run()
        {
            if (!Application.isPlaying || !File.Exists("Logs/YouthRiseQA/portal-url.txt"))
            { Debug.LogWarning("Start isolated QA backend, write its loopback URL to Logs/YouthRiseQA/portal-url.txt, then enter Play Mode first."); return; }
            var test = new YouthRisePortalSmokeTest { host = UnityEngine.Object.FindAnyObjectByType<YouthRisePrototype>() };
            if (test.host == null) { Debug.LogError("No active game host."); return; }
            test.host.StartCoroutine(test.Start());
        }

        private IEnumerator Start()
        {
            host = UnityEngine.Object.FindAnyObjectByType<YouthRisePrototype>();
            IEnumerator test = Exercise();
            while (true)
            {
                object current;
                try { if (!test.MoveNext()) break; current = test.Current; }
                catch (Exception e)
                {
                    File.WriteAllText("Logs/YouthRiseQA/portal-smoke.txt", "FAILED: " + e);
                    Debug.LogException(e); yield break;
                }
                yield return current;
            }
            File.WriteAllText("Logs/YouthRiseQA/portal-smoke.txt", "PASSED: register, female story/portrait, choice sync, ordinary local chat, urgent preview/consent/receipt, logout isolation, login and save restore.");
            Debug.Log("YouthRise portal integration smoke test passed.");
        }

        private IEnumerator Exercise()
        {
            string url = File.ReadAllText("Logs/YouthRiseQA/portal-url.txt").Trim();
            Check(Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.IsLoopback, "Only an isolated loopback test server is allowed.");
            Check(host != null && Get<PlayerAccount>("account") == null, "Start with a logged-out game.");
            Set("accountSettings", new AccountServiceSettings { baseUrl = url });
            string nickname = "qa-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            Set("registering", true); Set("female", true); Call("RefreshAccountMode");
            Get<InputField>("accountName").text = "Unity QA Fictional Player";
            Get<InputField>("accountNickname").text = nickname;
            Get<InputField>("accountPassword").text = "qa-fictional-password";
            Get<InputField>("accountAge").text = "16";
            Get<Toggle>("accountConsent").isOn = true;
            Call("Authenticate");
            float deadline = Time.realtimeSinceStartup + 20;
            while (Get<bool>("accountBusy") && Time.realtimeSinceStartup < deadline) yield return null;
            Check(Get<PlayerAccount>("account")?.gender == "female", "Registration failed: " + Get<Text>("accountStatus").text);
            yield return new WaitForSecondsRealtime(.5f);
            Call("StartNewGame");
            yield return new WaitForSecondsRealtime(.7f);
            Call("SetCharacterArt", "Alex");
            Check(Get<Image>("characterPortrait").sprite?.name.Contains("anita") == true, "Anita art not selected.");
            Check((string)typeof(YouthRisePrototype).GetMethod("CharacterText", Fields).Invoke(host, new object[] { "Alex dan ALEX" }) == "Anita dan ANITA", "Female text not personalized.");
            StoryGraph story = Get<StoryGraph>("story");
            StoryNode decision = Array.Find(story.Chapter.nodes, n => n.choices != null && n.choices.Length > 0);
            Call("ShowNode", decision.id);
            yield return new WaitForSecondsRealtime(.7f);
            Call("SelectChoice", decision.choices[0]);
            deadline = Time.realtimeSinceStartup + 20;
            while ((Get<bool>("resultsBusy") || Get<bool>("resultsDirty")) && Time.realtimeSinceStartup < deadline) yield return null;
            Check(Get<Text>("syncStatus").text.StartsWith("Hasil tersinkron"), "Result upload failed.");
            Check(PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile.recordedChoices.Count > 0, "Choice not saved.");
            Call("ShowSafeZone"); yield return new WaitForSecondsRealtime(.5f);
            Check(Get<GameObject>("safeZoneScreen").activeSelf, "Safe Zone unavailable before chapter completion.");
            Get<InputField>("chatInput").text = "Hari ini tugas sekolah membuatku lelah";
            Call("SendSupportChat");
            Check(!Get<GameObject>("incidentScreen").activeSelf && Get<Text>("chatResponse").text.Contains("tidak dikirim"), "Ordinary chat unexpectedly escalated.");
            Get<InputField>("chatInput").text = "PESAN UJI FIKTIF: bahaya sekarang. Ini bukan kejadian nyata.";
            Call("SendSupportChat"); yield return new WaitForSecondsRealtime(.5f);
            Check(Get<GameObject>("incidentScreen").activeSelf && !Get<bool>("incidentBusy"), "Urgency must show a preview without submitting.");
            Call("SubmitIncident"); deadline = Time.realtimeSinceStartup + 20;
            while (Get<bool>("incidentBusy") && Time.realtimeSinceStartup < deadline) yield return null;
            Check(Get<Text>("incidentStatus").text.StartsWith("Diterima dashboard"), "Confirmed incident did not reach dashboard.");
            Call("LogoutAccount");
            Check(Get<PlayerAccount>("account") == null && Get<string>("accountToken") == null && Get<InputField>("reportInput").text == "", "Logout left account data behind.");
            Check(Get<GameObject>("accountScreen").GetComponent<CanvasGroup>().interactable, "Logout left login controls disabled.");
            Set("registering", false); Call("RefreshAccountMode");
            Get<InputField>("accountNickname").text = nickname; Get<InputField>("accountPassword").text = "qa-fictional-password";
            Call("Authenticate"); deadline = Time.realtimeSinceStartup + 20;
            while (Get<bool>("accountBusy") && Time.realtimeSinceStartup < deadline) yield return null;
            Check(Get<PlayerAccount>("account")?.nickname == nickname, "Login failed.");
            Check(Get<PlayerProfile>("profile").recordedChoices.Count > 0, "Login did not restore account save.");
            deadline = Time.realtimeSinceStartup + 20;
            while (Get<bool>("resultsBusy") && Time.realtimeSinceStartup < deadline) yield return null;
            Call("LogoutAccount");
        }
    }
}
#endif
