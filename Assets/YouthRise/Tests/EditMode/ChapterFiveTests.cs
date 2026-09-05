using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class ChapterFiveTests
    {
        private static PlayerProfile NewProfile()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();
            return profile;
        }

        [Test]
        public void ChapterFive_LoadsEverySceneAndAllThirtyChoicesReachReflection()
        {
            StoryGraph graph = StoryRepository.LoadById("CHAPTER-5");
            Assert.That(graph.Chapter.number, Is.EqualTo(5));
            Assert.That(graph.Chapter.title, Is.EqualTo("Easy Money?"));
            Assert.That(graph.Chapter.rewardXp, Is.EqualTo(250));
            Assert.That(graph.Chapter.nodes, Has.Length.EqualTo(12));
            Assert.That(graph.Get("opening").nextNodeId, Is.EqualTo("node-1"));
            Assert.That(graph.Get("node-11").nextNodeId, Is.EqualTo("END"));
            for (int index = 1; index <= 10; index++)
            {
                StoryNode node = graph.Get($"node-{index}");
                Assert.That(node.choices, Has.Length.EqualTo(3));
                Assert.That(node.choices.Select(c => c.id), Is.Unique);
                foreach (StoryChoice choice in node.choices)
                {
                    Assert.That(choice.nextNodeId, Is.EqualTo($"node-{index + 1}"));
                    Assert.That(choice.label, Is.Not.Empty);
                    Assert.That(new[] { "reflective", "prosocial", "impulsive", "avoidant" }, Does.Contain(choice.tendency));
                    var profile = NewProfile();
                    foreach (StatDelta effect in choice.effects)
                    {
                        Assert.That(profile.GetStat(effect.stat), Is.GreaterThan(0), effect.stat);
                        int before = profile.GetStat(effect.stat);
                        profile.Apply(effect.stat, effect.amount);
                        Assert.That(profile.GetStat(effect.stat), Is.EqualTo(before + effect.amount), choice.id);
                    }
                }
            }
            foreach (StoryNode node in graph.Chapter.nodes)
                Assert.That(Resources.Load<Texture2D>("YouthRise/Art/Backgrounds/bg_" + node.background),
                    Is.Not.Null, node.id);
        }

        [Test]
        public void ChapterFive_PreservesAllRequestedBaseEffects()
        {
            // Independent transcription of the scenario; additional indicators are separate.
            string[] expected =
            {
                "knowledge:4,confidence:2", "risk:3", "financialAwareness:5",
                "confidence:3,financialAwareness:4", "risk:4", "financialAwareness:5",
                "financialAwareness:5", "risk:3", "risk:2",
                "financialAwareness:6,risk:-3", "risk:10", "socialSupport:5,financialAwareness:4",
                "financialAwareness:5", "risk:8", "socialSupport:5",
                "financialAwareness:6", "risk:4", "risk:6",
                "socialSupport:5,empathy:4", "empathy:-3", "socialSupport:6",
                "financialAwareness:6,risk:-5", "risk:10", "risk:5",
                "socialSupport:4,empathy:3", "socialSupport:-2", "knowledge:1",
                "financialAwareness:7,risk:-5", "risk:10", "socialSupport:5,financialAwareness:5"
            };
            string[] hidden = { "spendingControl", "impulseControl", "scamAwareness", "helpSeeking" };
            StoryGraph graph = StoryRepository.LoadChapterFive();
            for (int i = 0; i < expected.Length; i++)
            {
                StoryChoice choice = graph.Get($"node-{i / 3 + 1}").choices[i % 3];
                string[] actual = choice.effects.Where(e => !hidden.Contains(e.stat))
                    .Select(e => $"{e.stat}:{e.amount}").ToArray();
                Assert.That(actual, Is.EquivalentTo(expected[i].Split(',')), choice.id);
            }
        }

        [TestCase(0, 17, 89, 73, 71, 66)]
        [TestCase(1, 82, 50, 26, 22, 36)]
        [TestCase(2, 43, 69, 53, 59, 46)]
        public void ChapterFive_FullRoutesProduceExpectedFinancialSnapshots(
            int option, int risk, int awareness, int spending, int impulse, int scam)
        {
            var profile = NewProfile();
            StoryGraph graph = StoryRepository.LoadChapterFive();
            for (int i = 1; i <= 10; i++)
                profile.Apply(graph.Get($"node-{i}").choices[option].effects);
            MetricSnapshot snapshot = JsonUtility.FromJson<MetricSnapshot>(JsonUtility.ToJson(profile.Snapshot()));
            Assert.That(snapshot.risk, Is.EqualTo(risk));
            Assert.That(snapshot.financialAwareness, Is.EqualTo(awareness));
            Assert.That(snapshot.spendingControl, Is.EqualTo(spending));
            Assert.That(snapshot.impulseControl, Is.EqualTo(impulse));
            Assert.That(snapshot.scamAwareness, Is.EqualTo(scam));
            Assert.That(snapshot.trust, Is.EqualTo(50), "Chapter 5 does not invent extra Trust effects.");
        }

        [Test]
        public void ChapterFive_RequiresChapterFourAndAwardsExactlyOnce()
        {
            var profile = NewProfile();
            Assert.That(CampaignProgression.CanStartChapterFive(null), Is.False);
            Assert.That(CampaignProgression.CanStartChapterFive(profile), Is.False);
            for (int i = 1; i <= 3; i++)
                CampaignProgression.Complete(StoryRepository.LoadById($"chapter-{i}").Chapter, profile);
            Assert.That(CampaignProgression.CanStartChapterFive(profile), Is.False);
            CampaignProgression.Complete(StoryRepository.LoadChapterFour().Chapter, profile);
            Assert.That(CampaignProgression.CanStartChapterFive(profile), Is.True);
            Assert.That(profile.financialSafetyArticleUnlocked, Is.False);
            Assert.That(profile.moneySmartGuideUnlocked, Is.False);
            Assert.That(CampaignProgression.Complete(StoryRepository.LoadChapterFive().Chapter, profile), Is.EqualTo(250));
            Assert.That(CampaignProgression.Complete(StoryRepository.LoadChapterFive().Chapter, profile), Is.Zero);
            Assert.That(profile.xp, Is.EqualTo(1000));
            Assert.That(profile.completedChapterFive, Is.True);
            Assert.That(profile.seasonOneCompleted, Is.True);
            Assert.That(profile.financialSafetyArticleUnlocked, Is.True);
            Assert.That(profile.moneySmartGuideUnlocked, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ChapterFive_SaveRoundTripPreservesProgressAndNormalizesPrerequisites(bool complete)
        {
            var profile = NewProfile();
            profile.financialAwareness = 79;
            profile.spendingControl = 61;
            profile.impulseControl = 68;
            profile.scamAwareness = 72;
            profile.helpSeekingTendency = 84;
            profile.xp = complete ? 1000 : 750;
            var original = new PrototypeSave
            {
                chapterId = "chapter-5", currentNodeId = complete ? "END" : "node-6",
                branchPath = "node-1:1C", chapterCompleted = complete, profile = profile
            };
            var save = JsonUtility.FromJson<PrototypeSave>(JsonUtility.ToJson(original));
            CampaignProgression.Normalize(save);
            CampaignProgression.Normalize(save);
            Assert.That(save.profile.completedChapterOne && save.profile.completedChapterTwo
                && save.profile.completedChapterThree && save.profile.completedChapterFour, Is.True);
            Assert.That(save.profile.seasonOneCompleted, Is.True);
            Assert.That(save.profile.completedChapterFive, Is.EqualTo(complete));
            Assert.That(save.profile.financialSafetyArticleUnlocked, Is.EqualTo(complete));
            Assert.That(save.profile.moneySmartGuideUnlocked, Is.EqualTo(complete));
            Assert.That(save.profile.xp, Is.EqualTo(profile.xp));
            Assert.That(save.profile.financialAwareness, Is.EqualTo(79));
            Assert.That(save.profile.spendingControl, Is.EqualTo(61));
            Assert.That(save.profile.impulseControl, Is.EqualTo(68));
            Assert.That(save.profile.scamAwareness, Is.EqualTo(72));
            Assert.That(save.profile.helpSeekingTendency, Is.EqualTo(84));
            Assert.That(save.currentNodeId, Is.EqualTo(original.currentNodeId));
            Assert.That(save.branchPath, Is.EqualTo(original.branchPath));
        }

        [Test]
        public void ChapterFive_LegacySaveInitializesOnlyNewMetrics()
        {
            var save = JsonUtility.FromJson<PrototypeSave>(
                "{\"chapterId\":\"chapter-4\",\"chapterCompleted\":true,\"profile\":{\"xp\":750,\"risk\":27,\"trustRina\":9,\"helpSeekingTendency\":83}}");
            CampaignProgression.Normalize(save);
            Assert.That(CampaignProgression.CanStartChapterFive(save.profile), Is.True);
            save.profile.PrepareForChapterFive();
            Assert.That(save.profile.financialAwareness, Is.EqualTo(50));
            Assert.That(save.profile.spendingControl, Is.EqualTo(50));
            Assert.That(save.profile.impulseControl, Is.EqualTo(50));
            Assert.That(save.profile.scamAwareness, Is.EqualTo(50));
            Assert.That(save.profile.risk, Is.EqualTo(27));
            Assert.That(save.profile.trustRina, Is.EqualTo(9));
            Assert.That(save.profile.helpSeekingTendency, Is.EqualTo(83));
            Assert.That(save.profile.xp, Is.EqualTo(750));
            Assert.That(save.profile.completedChapterFive, Is.False);
        }

        [TestCase("financialAwareness")]
        [TestCase("spendingControl")]
        [TestCase("impulseControl")]
        [TestCase("scamAwareness")]
        public void ChapterFive_NewMetricsAreClampedAndReset(string stat)
        {
            var profile = NewProfile();
            profile.Apply(stat, 500);
            Assert.That(profile.GetStat(stat), Is.EqualTo(100));
            profile.Apply(stat, -500);
            Assert.That(profile.GetStat(stat), Is.Zero);
            CampaignProgression.Complete(StoryRepository.LoadChapterFive().Chapter, profile);
            profile.ResetForChapterOne();
            Assert.That(profile.GetStat(stat), Is.EqualTo(50));
            Assert.That(profile.completedChapterFive, Is.False);
            Assert.That(profile.financialSafetyArticleUnlocked, Is.False);
            Assert.That(profile.moneySmartGuideUnlocked, Is.False);
            Assert.That(profile.xp, Is.Zero);
        }

        [Test]
        public void ChapterFive_AdaptiveDialogueMatchesFinancialAndSupportState()
        {
            var profile = NewProfile();
            var generator = new LocalConversationGenerator();
            StoryGraph graph = StoryRepository.LoadChapterFive();
            foreach (string nodeId in new[] { "node-5", "node-9" })
            {
                StoryNode node = graph.Get(nodeId);
                foreach (int value in new[] { 0, 49, 50, 100 })
                {
                    profile.financialAwareness = profile.helpSeekingTendency = value;
                    string expected = node.variants.Single(v => value >= v.minInclusive && value <= v.maxInclusive).text;
                    Assert.That(generator.Generate(node, profile, 42), Is.EqualTo(expected));
                }
            }
        }
    }
}
