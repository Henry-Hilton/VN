using System;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class LocalPortalTests
    {
        private string previous;
        private bool existed;
        [SetUp] public void SetUp() {
            existed = PlayerPrefs.HasKey(LocalPlayerPortal.StorageKey); previous = PlayerPrefs.GetString(LocalPlayerPortal.StorageKey);
            PlayerPrefs.DeleteKey(LocalPlayerPortal.StorageKey);
        }
        [TearDown] public void TearDown() {
            if (existed) PlayerPrefs.SetString(LocalPlayerPortal.StorageKey, previous); else PlayerPrefs.DeleteKey(LocalPlayerPortal.StorageKey);
            PlayerPrefs.Save();
        }
        private static AccountRequest User(string nick = "tester") => new AccountRequest { nickname = nick, password = "test-password-only", name = "Test", age = 16, gender = "female", consent = true };
        [Test] public void RegisterLoginAndDashboardPersistWithoutServer() {
            var request = User(); var account = LocalPlayerPortal.Authenticate(request, true);
            Assert.That(account.success, Is.True); Assert.That(account.user.CharacterName, Is.EqualTo("Anita"));
            Assert.That(PlayerPrefs.GetString(LocalPlayerPortal.StorageKey), Does.Not.Contain(request.password));
            request.nickname = "TESTER";
            Assert.That(LocalPlayerPortal.Authenticate(request, false).user.id, Is.EqualTo(account.user.id));
            Assert.That(LocalPlayerPortal.Authenticate(request, true).success, Is.False);
            request.password = "wrong"; Assert.That(LocalPlayerPortal.Authenticate(request, false).success, Is.False);
            string staff = LocalPlayerPortal.OpenDashboard("test-counselor-password");
            Assert.That(LocalPlayerPortal.Dashboard(staff).players.Count, Is.EqualTo(1));
            Assert.Throws<UnauthorizedAccessException>(() => LocalPlayerPortal.Dashboard(account.token));
            Assert.That(LocalPlayerPortal.OpenDashboard("incorrect-password"), Is.Null);
            LocalPlayerPortal.Logout(account.token); LocalPlayerPortal.Logout(staff);
        }
        [Test] public void ReportsNeedConsentAndRetriesAreIdempotent() {
            var first = LocalPlayerPortal.Authenticate(User(), true); var second = LocalPlayerPortal.Authenticate(User("second"), true);
            var report = new SupportReportRequest { reportId = "YR-" + Guid.NewGuid().ToString("N"), text = "Fictional test report" };
            Assert.That(LocalPlayerPortal.StoreIncident(first.token, report).success, Is.False);
            report.consent = true;
            Assert.That(LocalPlayerPortal.StoreIncident(first.token, report).success, Is.True);
            Assert.That(LocalPlayerPortal.StoreIncident(first.token, report).success, Is.True);
            Assert.That(LocalPlayerPortal.Read().reports.Count, Is.EqualTo(1));
            Assert.That(LocalPlayerPortal.StoreIncident(second.token, report).success, Is.False);
            string staff = LocalPlayerPortal.OpenDashboard("test-counselor-password");
            LocalPlayerPortal.Review(staff, report.reportId, "reviewed");
            Assert.That(LocalPlayerPortal.Dashboard(staff).reports[0].status, Is.EqualTo("reviewed"));
            LocalPlayerPortal.Logout(first.token); LocalPlayerPortal.Logout(second.token); LocalPlayerPortal.Logout(staff);
        }
        [Test] public void ResultsAreIsolatedAndLogoutRevokesWrites() {
            var first = LocalPlayerPortal.Authenticate(User(), true); var second = LocalPlayerPortal.Authenticate(User("second"), true);
            Assert.That(LocalPlayerPortal.StoreResults(first.token, new ResultUpload { choices = new[] { new RecordedChoice { chapter = 1, nodeId = "test", choiceId = "test" } } }).success, Is.True);
            var records = LocalPlayerPortal.Read().players;
            Assert.That(records[0].results.recordedChoices.Count, Is.EqualTo(1)); Assert.That(records[1].results, Is.Null);
            LocalPlayerPortal.Logout(first.token);
            Assert.That(LocalPlayerPortal.StoreResults(first.token, new ResultUpload()).success, Is.False);
            LocalPlayerPortal.Logout(second.token);
        }
        [Test] public void RegistrationRejectsMissingConsentAndInvalidFields() {
            var request = User(); request.consent = false; Assert.That(LocalPlayerPortal.Authenticate(request, true).success, Is.False);
            request.consent = true; request.age = 10; Assert.That(LocalPlayerPortal.Authenticate(request, true).success, Is.False);
            request.age = 16; request.nickname = "../escape"; Assert.That(LocalPlayerPortal.Authenticate(request, true).success, Is.False);
        }
    }
    public sealed class GeminiSupportTests
    {
        [Test] public void ContextMapsAssistantRoleAndContainsNoAccountData() {
            var body = GeminiSupportClient.BuildRequest(new[] { new SupportMessage { role = "user", content = "Fictional question" }, new SupportMessage { role = "assistant", content = "Fictional response" }, new SupportMessage { role = "user", content = "Next question" } });
            Assert.That(body.contents[1].role, Is.EqualTo("model")); Assert.That(body.safetySettings.Length, Is.EqualTo(4));
            Assert.That(JsonUtility.ToJson(body), Does.Not.Contain("nickname"));
            Assert.That(GeminiLocalSettings.ValidModel("../model"), Is.False);
        }
        [Test] public void OnlyCompletedSafeTextIsDisplayed() {
            const string okay = "{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"Hidden thought\",\"thought\":true},{\"text\":\"Safe reply\"}]}}]}";
            Assert.That(GeminiSupportClient.ReadReply(okay), Is.EqualTo("Safe reply"));
            Assert.That(GeminiSupportClient.ReadReply(okay.Replace("STOP", "MAX_TOKENS")), Is.Null);
            Assert.That(GeminiSupportClient.ReadReply("{\"promptFeedback\":{\"blockReason\":\"SAFETY\"}}"), Is.Null);
            Assert.That(GeminiSupportClient.ReadReply("not json"), Is.Null);
        }
    }
}
