using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class LecturerFeaturesTests
    {
        private static PlayerProfile Profile() { var p = new PlayerProfile(); p.ResetForChapterOne(); return p; }
        [Test] public void LegacySaveDoesNotInventEightIssueAssessments()
        {
            var p = Profile(); p.completedChapterEight = true; p.recordedChoices = null;
            Assert.That(PlayerJourneyReport.Summarize(p), Has.Length.EqualTo(8));
            Assert.That(PlayerJourneyReport.Summarize(p).All(r => r.observed == 0), Is.True);
            Assert.That(PlayerJourneyReport.CreateText(p), Does.Contain("Bukan diagnosis"));
        }
        [Test] public void ReportsDeduplicateResumeAndResetOnlyReplayedChapter()
        {
            var p = Profile(); var first = StoryRepository.LoadChapterEight().Get("node-1");
            PlayerJourneyReport.Record(p,8,first,first.choices[0]); PlayerJourneyReport.Record(p,8,first,first.choices[1]);
            Assert.That(PlayerJourneyReport.Summarize(p)[7].observed, Is.EqualTo(1));
            var other=StoryRepository.LoadChapterSeven().Get("node-1"); PlayerJourneyReport.Record(p,7,other,other.choices[0]);
            PlayerJourneyReport.BeginChapter(p,8);
            Assert.That(PlayerJourneyReport.Summarize(p)[7].observed, Is.Zero);
            Assert.That(PlayerJourneyReport.Summarize(p)[6].observed, Is.EqualTo(1));
        }
        [Test] public void ReportRubricDescribesChoicesAndPreservesPartialCoverage()
        {
            var p=Profile(); var g=StoryRepository.LoadChapterEight();
            PlayerJourneyReport.Record(p,8,g.Get("node-1"),g.Get("node-1").choices[0]);
            PlayerJourneyReport.Record(p,8,g.Get("node-2"),g.Get("node-2").choices[2]);
            PlayerJourneyReport.Record(p,8,g.Get("node-8"),g.Get("node-8").choices[2]);
            var row=PlayerJourneyReport.Summarize(p)[7];
            Assert.That(row.observed,Is.EqualTo(3)); Assert.That(row.total,Is.EqualTo(10));
            Assert.That(row.supportive,Is.EqualTo(1)); Assert.That(row.pressure,Is.EqualTo(1)); Assert.That(row.mixed,Is.EqualTo(1));
        }
        [Test] public void ReportHistoryRoundTripsAndNewGameClearsIt()
        {
            var p=Profile(); var g=StoryRepository.LoadChapterSeven(); var node=g.Get("node-1"); PlayerJourneyReport.Record(p,7,node,node.choices[0]);
            var restored=JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(p));
            Assert.That(PlayerJourneyReport.CreateText(restored),Is.EqualTo(PlayerJourneyReport.CreateText(p)));
            restored.ResetForChapterOne(); Assert.That(restored.recordedChoices,Is.Empty);
        }
        [TestCase("https://school.example/connect",true)]
        [TestCase("http://127.0.0.1:8787",true)]
        [TestCase("http://school.example",false)]
        [TestCase("https://token@school.example",false)]
        [TestCase("https://school.example?secret=x",false)]
        [TestCase("file:///tmp/data",false)]
        [TestCase("",false)]
        public void ConnectorOnlyAllowsSafeConfiguredBaseUrls(string url,bool expected) => Assert.That(OnlineSupportSettings.ValidBaseUrl(url),Is.EqualTo(expected));
        [Test] public void ExternalServicesShipDisabled()
        {
            var settings=OnlineSupportSettings.Load(); Assert.That(settings.enabled,Is.False); Assert.That(settings.IsConfigured,Is.False);
        }
        [Test] public void OnlyMatchedAcceptedReceiptIsProofOfApiAcceptance()
        {
            var request=new SupportReportRequest { reportId="YR-test",recipientLabel="Guru BK",consent=true };
            var response=new SupportResponse {success=true,reportId=request.reportId,recipientLabel=request.recipientLabel,messageId="wamid.test",status="accepted_by_whatsapp",acceptedUtc=DateTime.UtcNow.ToString("O")};
            Assert.That(OnlineSupportClient.IsAcceptedReceipt(request,response),Is.True);
            response.status="opened_whatsapp"; Assert.That(OnlineSupportClient.IsAcceptedReceipt(request,response),Is.False);
            response.status="accepted_by_whatsapp"; response.reportId="other"; Assert.That(OnlineSupportClient.IsAcceptedReceipt(request,response),Is.False);
            response.reportId=request.reportId; response.messageId=null; Assert.That(OnlineSupportClient.IsAcceptedReceipt(request,response),Is.False);
        }
        [Test] public void OriginalMusicHasSignalAndDoesNotClip()
        {
            AudioClip clip=StoryAudio.CreateAmbientLoop();
            try { var samples=new float[clip.samples]; clip.GetData(samples,0); Assert.That(clip.length,Is.EqualTo(24).Within(.01)); Assert.That(samples.Max(x=>Mathf.Abs(x)),Is.InRange(.01f,.99f)); Assert.That(Mathf.Abs(samples[0]-samples[samples.Length-1]),Is.LessThan(.03f)); }
            finally { UnityEngine.Object.DestroyImmediate(clip); }
        }
        [TestCase(-1f, 0f)]
        [TestCase(.58f, .58f)]
        [TestCase(2f, 1f)]
        public void SpriteLessMetersHaveProportionalWidth(float value, float expected)
        {
            var root = new GameObject("Meter test", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            try
            {
                var fill = root.GetComponent<UnityEngine.UI.Image>();
                fill.rectTransform.anchorMin = Vector2.zero;
                fill.rectTransform.anchorMax = Vector2.one;
                MeterFill.SetValue(fill, value);
                Assert.That(fill.rectTransform.anchorMax.x, Is.EqualTo(expected));
                Assert.That(fill.rectTransform.anchorMax.y, Is.EqualTo(1f));
                Assert.That(fill.type, Is.EqualTo(UnityEngine.UI.Image.Type.Simple));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
