using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class ChapterSixTests
    {
        private static readonly string[] HealthMetrics =
        {
            "health", "healthAwareness", "sleepAwareness", "physicalActivityAwareness",
            "nutritionAwareness", "selfCare", "routineConsistency"
        };

        private static PlayerProfile NewProfile()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();
            return profile;
        }

        [Test]
        public void ChapterSix_AllThirtyChoicesAndBackgroundsReachTheReflection()
        {
            StoryGraph graph = StoryRepository.LoadById("CHAPTER-6");
            Assert.That(graph.Chapter.title, Is.EqualTo("Take Care of You"));
            Assert.That(graph.Chapter.number, Is.EqualTo(6));
            Assert.That(graph.Chapter.rewardXp, Is.EqualTo(250));
            Assert.That(graph.Chapter.nodes, Has.Length.EqualTo(12));
            Assert.That(graph.Chapter.reflectionLines, Has.Length.EqualTo(5));
            Assert.That(graph.Chapter.unlockLabels, Is.EquivalentTo(new[] { "Healthy Lifestyle", "My Healthy Routine" }));
            Assert.That(graph.Get("opening").nextNodeId, Is.EqualTo("node-1"));
            Assert.That(graph.Get("node-11").nextNodeId, Is.EqualTo("END"));
            for (int i = 1; i <= 10; i++)
            {
                StoryNode node = graph.Get($"node-{i}");
                Assert.That(node.choices, Has.Length.EqualTo(3));
                Assert.That(node.choices.Select(c => c.id), Is.Unique);
                foreach (StoryChoice choice in node.choices)
                {
                    Assert.That(choice.nextNodeId, Is.EqualTo($"node-{i + 1}"));
                    Assert.That(choice.label, Is.Not.Empty);
                    Assert.That(choice.tendency, Is.Not.Empty);
                    var profile = NewProfile();
                    foreach (StatDelta effect in choice.effects)
                    {
                        int before = profile.GetStat(effect.stat);
                        Assert.That(before, Is.GreaterThan(0), effect.stat);
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
        public void ChapterSix_PreservesEveryRequestedBaseEffect()
        {
            // Independent transcription of the supplied scenario, in node/choice order.
            string[] expected =
            {
                "risk:4", "healthAwareness:5", "risk:4",
                "healthAwareness:4", "health:-1", "health:-2",
                "health:5,confidence:2", "", "health:-3",
                "healthAwareness:5", "", "health:-3",
                "health:3", "health:-1", "health:-3",
                "health:4,confidence:2", "health:-3", "health:-6,anxiety:2",
                "empathy:5,healthAwareness:4", "empathy:-2", "socialSupport:4",
                "health:5", "confidence:2,anxiety:2", "health:-2",
                "confidence:4,health:4", "health:-4", "health:-1",
                "health:7", "health:3", ""
            };
            string[] additional = { "sleepAwareness", "physicalActivityAwareness", "nutritionAwareness", "selfCare", "routineConsistency" };
            StoryGraph graph = StoryRepository.LoadChapterSix();
            for (int i = 0; i < expected.Length; i++)
            {
                StoryChoice choice = graph.Get($"node-{i / 3 + 1}").choices[i % 3];
                string[] baseEffects = choice.effects.Where(e => !additional.Contains(e.stat))
                    .Select(e => $"{e.stat}:{e.amount}").ToArray();
                Assert.That(baseEffects, Is.EquivalentTo(expected[i].Length == 0 ? new string[0] : expected[i].Split(',')), choice.id);
            }
        }

        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(10, 2)]
        public void ChapterSix_NoChangeChoicesDoNotAlterAnyProfileMetric(int nodeNumber, int choiceIndex)
        {
            var profile = NewProfile();
            profile.health = 37;
            profile.selfCare = 84;
            string before = JsonUtility.ToJson(profile);
            StoryChoice choice = StoryRepository.LoadChapterSix().Get($"node-{nodeNumber}").choices[choiceIndex];
            Assert.That(choice.effects, Is.Empty);
            profile.Apply(choice.effects);
            Assert.That(JsonUtility.ToJson(profile), Is.EqualTo(before));
        }

        [TestCase("AAAAAAAAAA", 34, 78, 63, 60, 58, 62, 81, 71)]
        [TestCase("BAAAAAAAAA", 30, 78, 68, 67, 58, 62, 84, 78)]
        [TestCase("BBBBBBBBBB", 30, 44, 55, 48, 53, 49, 50, 46)]
        [TestCase("CCCCCCCCCC", 34, 30, 50, 39, 46, 45, 39, 43)]
        public void ChapterSix_FullRoutesProduceExpectedHealthSnapshots(
            string route, int risk, int health, int awareness, int sleep, int movement, int nutrition, int care, int routine)
        {
            var profile = NewProfile();
            StoryGraph graph = StoryRepository.LoadChapterSix();
            for (int i = 0; i < route.Length; i++)
                profile.Apply(graph.Get($"node-{i + 1}").choices[route[i] - 'A'].effects);
            var snapshot = JsonUtility.FromJson<MetricSnapshot>(JsonUtility.ToJson(profile.Snapshot()));
            Assert.That(snapshot.risk, Is.EqualTo(risk));
            Assert.That(snapshot.health, Is.EqualTo(health));
            Assert.That(snapshot.healthAwareness, Is.EqualTo(awareness));
            Assert.That(snapshot.sleepAwareness, Is.EqualTo(sleep));
            Assert.That(snapshot.physicalActivityAwareness, Is.EqualTo(movement));
            Assert.That(snapshot.nutritionAwareness, Is.EqualTo(nutrition));
            Assert.That(snapshot.selfCare, Is.EqualTo(care));
            Assert.That(snapshot.routineConsistency, Is.EqualTo(routine));
            Assert.That(snapshot.trust, Is.EqualTo(50));
        }

        [Test]
        public void ChapterSix_RequiresChapterFiveAndAwardsOnlyOnce()
        {
            var profile = NewProfile();
            Assert.That(CampaignProgression.CanStartChapterSix(null), Is.False);
            for (int i = 1; i <= 5; i++)
            {
                Assert.That(CampaignProgression.CanStartChapterSix(profile), Is.False);
                CampaignProgression.Complete(StoryRepository.LoadById($"chapter-{i}").Chapter, profile);
            }
            Assert.That(CampaignProgression.CanStartChapterSix(profile), Is.True);
            Assert.That(profile.healthyLifestyleArticleUnlocked, Is.False);
            Assert.That(profile.healthyRoutineGuideUnlocked, Is.False);
            StoryChapter chapter = StoryRepository.LoadChapterSix().Chapter;
            Assert.That(CampaignProgression.Complete(chapter, profile), Is.EqualTo(250));
            Assert.That(CampaignProgression.Complete(chapter, profile), Is.Zero);
            Assert.That(profile.xp, Is.EqualTo(1250));
            Assert.That(profile.completedChapterSix, Is.True);
            Assert.That(profile.seasonOneCompleted, Is.False);
            Assert.That(profile.healthyLifestyleArticleUnlocked, Is.True);
            Assert.That(profile.healthyRoutineGuideUnlocked, Is.True);
            Assert.That(profile.financialSafetyArticleUnlocked && profile.moneySmartGuideUnlocked, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ChapterSix_SaveRoundTripKeepsMetricsAndNormalizesPrerequisites(bool complete)
        {
            var profile = NewProfile();
            for (int i = 0; i < HealthMetrics.Length; i++)
                profile.Apply(HealthMetrics[i], i + 1);
            profile.financialAwareness = 79;
            profile.helpSeekingTendency = 81;
            profile.xp = complete ? 1250 : 1000;
            var original = new PrototypeSave
            {
                chapterId = "chapter-6", currentNodeId = complete ? "END" : "node-6",
                chapterCompleted = complete, branchPath = "node-1:1B", profile = profile
            };
            var save = JsonUtility.FromJson<PrototypeSave>(JsonUtility.ToJson(original));
            CampaignProgression.Normalize(save);
            CampaignProgression.Normalize(save);
            Assert.That(save.profile.completedChapterOne && save.profile.completedChapterTwo &&
                save.profile.completedChapterThree && save.profile.completedChapterFour &&
                save.profile.completedChapterFive, Is.True);
            Assert.That(save.profile.seasonOneCompleted, Is.False);
            Assert.That(save.profile.completedChapterSix, Is.EqualTo(complete));
            Assert.That(save.profile.healthyLifestyleArticleUnlocked, Is.EqualTo(complete));
            Assert.That(save.profile.healthyRoutineGuideUnlocked, Is.EqualTo(complete));
            Assert.That(save.profile.xp, Is.EqualTo(profile.xp));
            for (int i = 0; i < HealthMetrics.Length; i++)
                Assert.That(save.profile.GetStat(HealthMetrics[i]), Is.EqualTo(51 + i), HealthMetrics[i]);
            Assert.That(save.profile.financialAwareness, Is.EqualTo(79));
            Assert.That(save.profile.helpSeekingTendency, Is.EqualTo(81));
            Assert.That(save.currentNodeId, Is.EqualTo(original.currentNodeId));
            Assert.That(save.branchPath, Is.EqualTo(original.branchPath));
        }

        [Test]
        public void ChapterSix_LegacyChapterFiveSaveInitializesOnlyNewIndicators()
        {
            var save = JsonUtility.FromJson<PrototypeSave>(
                "{\"chapterId\":\"chapter-5\",\"chapterCompleted\":true,\"profile\":{\"xp\":1000,\"risk\":27,\"confidence\":63,\"trustRina\":9,\"financialAwareness\":79,\"helpSeekingTendency\":83}}");
            CampaignProgression.Normalize(save);
            Assert.That(CampaignProgression.CanStartChapterSix(save.profile), Is.True);
            save.profile.PrepareForChapterSix();
            foreach (string stat in HealthMetrics)
                Assert.That(save.profile.GetStat(stat), Is.EqualTo(50), stat);
            Assert.That(save.profile.risk, Is.EqualTo(27));
            Assert.That(save.profile.confidence, Is.EqualTo(63));
            Assert.That(save.profile.trustRina, Is.EqualTo(9));
            Assert.That(save.profile.financialAwareness, Is.EqualTo(79));
            Assert.That(save.profile.helpSeekingTendency, Is.EqualTo(83));
            Assert.That(save.profile.xp, Is.EqualTo(1000));
            Assert.That(save.profile.completedChapterSix, Is.False);
        }

        [TestCase("health")]
        [TestCase("healthAwareness")]
        [TestCase("sleepAwareness")]
        [TestCase("physicalActivityAwareness")]
        [TestCase("nutritionAwareness")]
        [TestCase("selfCare")]
        [TestCase("routineConsistency")]
        public void ChapterSix_IndicatorsClampAndNewGameResetsProgress(string stat)
        {
            var profile = NewProfile();
            profile.Apply(stat, 500);
            Assert.That(profile.GetStat(stat), Is.EqualTo(100));
            profile.Apply(stat, -500);
            Assert.That(profile.GetStat(stat), Is.Zero);
            CampaignProgression.Complete(StoryRepository.LoadChapterSix().Chapter, profile);
            profile.ResetForChapterOne();
            Assert.That(profile.GetStat(stat), Is.EqualTo(50));
            Assert.That(profile.completedChapterSix, Is.False);
            Assert.That(profile.healthyLifestyleArticleUnlocked, Is.False);
            Assert.That(profile.healthyRoutineGuideUnlocked, Is.False);
            Assert.That(profile.xp, Is.Zero);
        }

        [Test]
        public void ChapterSix_AdaptiveDialogueKeepsMatchingSleepAndSelfCareContext()
        {
            var profile = NewProfile();
            var generator = new LocalConversationGenerator();
            var graph = StoryRepository.LoadChapterSix();
            foreach (string nodeId in new[] { "node-6", "node-7" })
            {
                StoryNode node = graph.Get(nodeId);
                foreach (int value in new[] { 0, 49, 50, 100 })
                {
                    profile.sleepAwareness = profile.selfCare = value;
                    string expected = node.variants.Single(v => value >= v.minInclusive && value <= v.maxInclusive).text;
                    Assert.That(generator.Generate(node, profile, 42), Is.EqualTo(expected));
                }
            }
        }

        [TestCase("Coach Sarah", "char_coach_sarah_chroma")]
        [TestCase(" coach sarah ", "char_coach_sarah_chroma")]
        [TestCase("Sarah", "char_sarah_chroma")]
        public void ChapterSix_CoachAndStudentSarahResolveToDistinctPortraits(string speaker, string asset)
        {
            MethodInfo resolver = typeof(YouthRisePrototype).GetMethod("CharacterResource", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(resolver, Is.Not.Null);
            string resource = (string)resolver.Invoke(null, new object[] { speaker });
            Assert.That(resource, Is.EqualTo("YouthRise/Art/Characters/" + asset));
            Assert.That(Resources.Load<Texture2D>(resource), Is.Not.Null);
        }
    }
}
