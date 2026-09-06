using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class ChapterSevenTests
    {
        private static readonly string[] DigitalMetrics =
        {
            "digitalAwareness", "selfControl", "screenTimeManagement", "fomoIndex",
            "socialMediaDependencyTendency", "offlineSocialSupport"
        };

        private static PlayerProfile NewProfile()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();
            return profile;
        }

        [Test]
        public void ChapterSeven_AllThirtyThreeChoicesReachCompletionAndUseExistingArt()
        {
            var graph = StoryRepository.LoadById("CHAPTER-7");
            Assert.That(graph.Chapter.title, Is.EqualTo("Always Connected"));
            Assert.That(graph.Chapter.number, Is.EqualTo(7));
            Assert.That(graph.Chapter.rewardXp, Is.EqualTo(300));
            Assert.That(graph.Chapter.nodes, Has.Length.EqualTo(12));
            Assert.That(graph.Chapter.reflectionLines, Has.Length.EqualTo(5));
            Assert.That(graph.Chapter.unlockLabels, Is.Empty, "No extra unlock was requested.");
            Assert.That(graph.Get(graph.Chapter.startNodeId).nextNodeId, Is.EqualTo("node-1"));
            for (int i = 1; i <= 11; i++)
            {
                var node = graph.Get($"node-{i}");
                Assert.That(node.choices, Has.Length.EqualTo(3));
                Assert.That(node.choices.Select(c => c.id), Is.Unique);
                foreach (var choice in node.choices)
                {
                    Assert.That(choice.nextNodeId, Is.EqualTo(i == 11 ? "END" : $"node-{i + 1}"));
                    Assert.That(choice.label, Is.Not.Empty);
                    Assert.That(choice.tendency, Is.Not.Empty);
                    Assert.That(choice.effects.Select(e => e.stat), Is.Unique);
                    var profile = NewProfile();
                    foreach (var effect in choice.effects)
                    {
                        int before = profile.GetStat(effect.stat);
                        profile.Apply(effect.stat, effect.amount);
                        Assert.That(profile.GetStat(effect.stat), Is.EqualTo(before + effect.amount), choice.id + "/" + effect.stat);
                    }
                }
            }
            foreach (var node in graph.Chapter.nodes)
            {
                Assert.That(node.body, Is.Not.Empty);
                Assert.That(node.branch, Is.Not.Empty);
                Assert.That(Resources.Load<Texture2D>("YouthRise/Art/Backgrounds/bg_" + node.background), Is.Not.Null, node.id);
            }
            Assert.That(graph.Get("opening").background, Is.EqualTo("bedroom"));
            Assert.That(graph.Get("node-2").background, Is.EqualTo("bedroom_morning"));
        }

        [Test]
        public void ChapterSeven_PreservesAllRequestedBaseEffects()
        {
            // Independent transcription of the user's 33 choices. Generic Trust maps to Maya at node 7.
            string[] expected =
            {
                "selfControl:5,health:3", "selfControl:-2", "selfControl:-5,health:-2",
                "digitalAwareness:5", "digitalAwareness:-2", "selfControl:5",
                "selfControl:4", "selfControl:-2", "anxiety:4,selfControl:-3",
                "empathy:5,digitalAwareness:5", "risk:3", "empathy:-2",
                "selfControl:5,confidence:2", "selfControl:-5", "selfControl:-2",
                "selfControl:5", "selfControl:-4", "risk:7",
                "trustMaya:5,socialSupport:4", "trustMaya:1", "trustMaya:-3",
                "selfControl:6", "selfControl:1", "selfControl:-4",
                "empathy:5,socialSupport:5", "socialSupport:-2", "digitalAwareness:4",
                "selfControl:7,digitalAwareness:5", "confidence:2,anxiety:2", "selfControl:-4",
                "selfControl:6,digitalAwareness:5", "selfControl:-4", "selfControl:5,socialSupport:4"
            };
            string[] derived = { "screenTimeManagement", "fomoIndex", "socialMediaDependencyTendency", "offlineSocialSupport", "sleepAwareness" };
            var graph = StoryRepository.LoadChapterSeven();
            for (int i = 0; i < expected.Length; i++)
            {
                var choice = graph.Get($"node-{i / 3 + 1}").choices[i % 3];
                var actual = choice.effects.Where(e => !derived.Contains(e.stat)).Select(e => $"{e.stat}:{e.amount}");
                Assert.That(actual, Is.EquivalentTo(expected[i].Split(',')), choice.id);
            }
            Assert.That(graph.Get("node-7").choices[1].effects, Has.Length.EqualTo(1),
                "Online friendship must not receive an extra hidden penalty.");
        }

        [TestCase("AAAAAAAAAAA", 70, 88, 88, 37, 37, 63, 53, 55, 30, 52, 60, 55, 59, 20)]
        [TestCase("BBBBBBBBBBB", 48, 34, 32, 57, 66, 48, 50, 49, 33, 52, 50, 51, 48, 22)]
        [TestCase("CCCCCCCCCCC", 54, 42, 36, 57, 48, 51, 48, 48, 37, 50, 48, 47, 54, 24)]
        public void ChapterSeven_FullRoutesMatchIndependentlyCalculatedSnapshots(
            string route, int awareness, int control, int screen, int fomo, int dependency,
            int offline, int health, int sleep, int risk, int confidence, int empathy, int trust, int support, int anxiety)
        {
            var graph = StoryRepository.LoadChapterSeven();
            var profile = NewProfile();
            for (int i = 0; i < route.Length; i++)
                profile.Apply(graph.Get($"node-{i + 1}").choices[route[i] - 'A'].effects);
            var snapshot = JsonUtility.FromJson<MetricSnapshot>(JsonUtility.ToJson(profile.Snapshot()));
            Assert.That(snapshot.digitalAwareness, Is.EqualTo(awareness));
            Assert.That(snapshot.selfControl, Is.EqualTo(control));
            Assert.That(snapshot.screenTimeManagement, Is.EqualTo(screen));
            Assert.That(snapshot.fomoIndex, Is.EqualTo(fomo));
            Assert.That(snapshot.socialMediaDependencyTendency, Is.EqualTo(dependency));
            Assert.That(snapshot.offlineSocialSupport, Is.EqualTo(offline));
            Assert.That(snapshot.health, Is.EqualTo(health));
            Assert.That(snapshot.sleepAwareness, Is.EqualTo(sleep));
            Assert.That(snapshot.risk, Is.EqualTo(risk));
            Assert.That(snapshot.confidence, Is.EqualTo(confidence));
            Assert.That(snapshot.empathy, Is.EqualTo(empathy));
            Assert.That(snapshot.trust, Is.EqualTo(trust));
            Assert.That(snapshot.socialSupport, Is.EqualTo(support));
            Assert.That(snapshot.anxiety, Is.EqualTo(anxiety));
            Assert.That(snapshot.digitalSafetyAwareness, Is.EqualTo(50));
            Assert.That(snapshot.impulseControl, Is.EqualTo(50));
        }

        [Test]
        public void ChapterSeven_RequiresChapterSixAndGrantsThreeHundredXpOnlyOnce()
        {
            var profile = NewProfile();
            Assert.That(CampaignProgression.CanStartChapterSeven(null), Is.False);
            for (int i = 1; i <= 6; i++)
            {
                Assert.That(CampaignProgression.CanStartChapterSeven(profile), Is.False);
                CampaignProgression.Complete(StoryRepository.LoadById($"chapter-{i}").Chapter, profile);
            }
            Assert.That(CampaignProgression.CanStartChapterSeven(profile), Is.True);
            Assert.That(profile.xp, Is.EqualTo(1250));
            var chapter = StoryRepository.LoadChapterSeven().Chapter;
            Assert.That(CampaignProgression.Complete(chapter, profile), Is.EqualTo(300));
            Assert.That(profile.completedChapterSeven, Is.True);
            profile.PrepareForChapterSeven();
            Assert.That(CampaignProgression.Complete(chapter, profile), Is.Zero);
            Assert.That(profile.xp, Is.EqualTo(1550));
            Assert.That(profile.safeZoneUnlocked && profile.relationshipPathUnlocked, Is.True);
            Assert.That(profile.seasonOneCompleted, Is.False);
            Assert.That(profile.bullyingSupportArticleUnlocked && profile.healthyRelationshipArticleUnlocked &&
                profile.digitalSafetyGuideUnlocked && profile.financialSafetyArticleUnlocked &&
                profile.moneySmartGuideUnlocked && profile.healthyLifestyleArticleUnlocked && profile.healthyRoutineGuideUnlocked, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ChapterSeven_SaveRoundTripRetainsAllMetricsAndPrerequisites(bool completed)
        {
            var profile = NewProfile();
            for (int i = 0; i < DigitalMetrics.Length; i++)
                profile.Apply(DigitalMetrics[i], i + 1);
            profile.health = 79;
            profile.sleepAwareness = 83;
            profile.anxiety = 26;
            profile.xp = completed ? 1550 : 1250;
            var original = new PrototypeSave
            {
                chapterId = "chapter-7", currentNodeId = completed ? "END" : "node-6",
                chapterCompleted = completed, branchPath = "node-1:1A>node-2:2C", profile = profile
            };
            var restored = JsonUtility.FromJson<PrototypeSave>(JsonUtility.ToJson(original));
            CampaignProgression.Normalize(restored);
            CampaignProgression.Normalize(restored);
            Assert.That(restored.profile.completedChapterOne && restored.profile.completedChapterTwo &&
                restored.profile.completedChapterThree && restored.profile.completedChapterFour &&
                restored.profile.completedChapterFive && restored.profile.completedChapterSix, Is.True);
            Assert.That(restored.profile.completedChapterSeven, Is.EqualTo(completed));
            Assert.That(restored.profile.healthyLifestyleArticleUnlocked && restored.profile.healthyRoutineGuideUnlocked, Is.True);
            Assert.That(restored.profile.xp, Is.EqualTo(profile.xp));
            Assert.That(JsonUtility.ToJson(restored.profile.Snapshot()), Is.EqualTo(JsonUtility.ToJson(profile.Snapshot())));
            Assert.That(restored.currentNodeId, Is.EqualTo(original.currentNodeId));
            Assert.That(restored.branchPath, Is.EqualTo(original.branchPath));
        }

        [Test]
        public void ChapterSeven_LegacySaveInitializesOnlyNewIndicators()
        {
            var save = JsonUtility.FromJson<PrototypeSave>(
                "{\"chapterId\":\"chapter-6\",\"chapterCompleted\":true,\"profile\":{\"xp\":1250,\"risk\":27,\"confidence\":63,\"trustMaya\":9,\"health\":73,\"sleepAwareness\":82,\"anxiety\":24,\"digitalSafetyAwareness\":89,\"impulseControl\":76}}");
            CampaignProgression.Normalize(save);
            Assert.That(CampaignProgression.CanStartChapterSeven(save.profile), Is.True);
            save.profile.PrepareForChapterSeven();
            foreach (string stat in DigitalMetrics)
                Assert.That(save.profile.GetStat(stat), Is.EqualTo(50), stat);
            Assert.That(save.profile.risk, Is.EqualTo(27));
            Assert.That(save.profile.confidence, Is.EqualTo(63));
            Assert.That(save.profile.trustMaya, Is.EqualTo(9));
            Assert.That(save.profile.health, Is.EqualTo(73));
            Assert.That(save.profile.sleepAwareness, Is.EqualTo(82));
            Assert.That(save.profile.anxiety, Is.EqualTo(24));
            Assert.That(save.profile.digitalSafetyAwareness, Is.EqualTo(89));
            Assert.That(save.profile.impulseControl, Is.EqualTo(76));
            Assert.That(save.profile.xp, Is.EqualTo(1250));
            Assert.That(save.profile.completedChapterSeven, Is.False);
        }

        [TestCase("digitalAwareness")]
        [TestCase("selfControl")]
        [TestCase("screenTimeManagement")]
        [TestCase("fomoIndex")]
        [TestCase("socialMediaDependencyTendency")]
        [TestCase("offlineSocialSupport")]
        public void ChapterSeven_IndicatorsClampAndNewGameResetsProgress(string stat)
        {
            var profile = NewProfile();
            profile.Apply(stat.ToUpperInvariant(), 500);
            Assert.That(profile.GetStat(stat), Is.EqualTo(100));
            profile.Apply(stat, -500);
            Assert.That(profile.GetStat(stat), Is.Zero);
            CampaignProgression.Complete(StoryRepository.LoadChapterSeven().Chapter, profile);
            profile.ResetForChapterOne();
            Assert.That(profile.GetStat(stat), Is.EqualTo(50));
            Assert.That(profile.completedChapterSeven, Is.False);
            Assert.That(profile.xp, Is.Zero);
        }

        [Test]
        public void ChapterSeven_AdaptiveDialogueMatchesPressureAndAwarenessWithoutChangingChoices()
        {
            var graph = StoryRepository.LoadChapterSeven();
            var generator = new LocalConversationGenerator();
            var profile = NewProfile();
            foreach (string nodeId in new[] { "node-3", "node-9" })
            {
                var node = graph.Get(nodeId);
                string before = JsonUtility.ToJson(node);
                foreach (int value in new[] { 0, 49, 50, 100 })
                {
                    profile.selfControl = profile.digitalAwareness = value;
                    string expected = node.variants.Single(v => value >= v.minInclusive && value <= v.maxInclusive).text;
                    Assert.That(generator.Generate(node, profile, 42), Is.EqualTo(expected));
                    Assert.That(JsonUtility.ToJson(node), Is.EqualTo(before));
                }
            }
        }

        [Test]
        public void ChapterSeven_BothAdaptiveVariantsAreReachableThroughRealChoices()
        {
            var graph = StoryRepository.LoadChapterSeven();
            var generator = new LocalConversationGenerator();
            foreach (int target in new[] { 3, 9 })
            {
                var node = graph.Get($"node-{target}");
                string[] generated = new string[2];
                for (int route = 0; route < 2; route++)
                {
                    var profile = NewProfile();
                    for (int i = 1; i < target; i++)
                        profile.Apply(graph.Get($"node-{i}").choices[route].effects);
                    generated[route] = generator.Generate(node, profile, 42);
                }
                Assert.That(generated, Is.EquivalentTo(node.variants.Select(v => v.text)), node.id);
                Assert.That(generated, Is.Unique, node.id);
            }
        }

        [Test]
        public void ChapterSeven_ScreenTimeExampleKeepsRequestedNumbersAndFictionalScope()
        {
            string body = StoryRepository.LoadChapterSeven().Get("node-10").body;
            foreach (string text in new[] { "4 jam 20 menit", "3 jam 10 menit", "2 jam 15 menit", "1 jam 30 menit", "bukan data perangkatmu" })
                Assert.That(body, Does.Contain(text));
        }
    }
}
