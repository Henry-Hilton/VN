using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class ChapterEightTests
    {
        private static readonly string[] FamilyMetrics = { "familyCommunication", "familySupport", "emotionalRegulation" };
        private static PlayerProfile NewProfile()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();
            return profile;
        }

        [Test]
        public void ChapterEight_AllThirtyChoicesReachTheHomeConversationAndFinale()
        {
            var graph = StoryRepository.LoadById("CHAPTER-8");
            Assert.That(graph.Chapter.title, Is.EqualTo("Home Is Complicated"));
            Assert.That(graph.Chapter.number, Is.EqualTo(8));
            Assert.That(graph.Chapter.rewardXp, Is.EqualTo(300));
            Assert.That(graph.Chapter.nodes, Has.Length.EqualTo(12));
            Assert.That(graph.Chapter.reflectionLines, Has.Length.EqualTo(6));
            Assert.That(graph.Chapter.unlockLabels, Is.EqualTo(new[] { "Family & Support" }));
            Assert.That(graph.Get("opening").nextNodeId, Is.EqualTo("node-1"));
            Assert.That(graph.Get("node-11").nextNodeId, Is.EqualTo("END"));
            for (int i = 1; i <= 10; i++)
            {
                var node = graph.Get($"node-{i}");
                Assert.That(node.choices, Has.Length.EqualTo(3));
                Assert.That(node.choices.Select(c => c.id), Is.Unique);
                foreach (var choice in node.choices)
                {
                    Assert.That(choice.nextNodeId, Is.EqualTo($"node-{i + 1}"));
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
                Assert.That(Resources.Load<Texture2D>("YouthRise/Art/Backgrounds/bg_" + node.background), Is.Not.Null, node.id);
        }

        [Test]
        public void ChapterEight_PreservesEveryRequestedBaseDelta()
        {
            // Independent transcription of all user-specified deltas. Node 5 Trust belongs to Maya.
            string[] expected =
            {
                "selfControl:3", "risk:3,anxiety:2", "anxiety:3",
                "trustParent:5,socialSupport:4", "trustParent:1", "trustParent:-4,anxiety:3",
                "confidence:4,trustParent:4", "anxiety:4,trustParent:-3", "anxiety:2",
                "confidence:5,trustParent:3", "anxiety:4", "risk:4,trustParent:-4",
                "trustMaya:5,socialSupport:5", "trustMaya:1", "socialSupport:-3",
                "socialSupport:6,anxiety:-3", "anxiety:3", "anxiety:4,socialSupport:-2",
                "empathy:5,trustParent:4", "empathy:-3", "empathy:-1",
                "socialSupport:6,anxiety:-5", "anxiety:5", "confidence:2,anxiety:4",
                "knowledge:5,confidence:3", "knowledge:-2", "empathy:-4",
                "socialSupport:7,anxiety:-4", "anxiety:3", "confidence:2,anxiety:2"
            };
            var derived = FamilyMetrics.Concat(new[] { "helpSeekingTendency" }).ToArray();
            var graph = StoryRepository.LoadChapterEight();
            for (int i = 0; i < expected.Length; i++)
            {
                var choice = graph.Get($"node-{i / 3 + 1}").choices[i % 3];
                Assert.That(choice.effects.Where(e => !derived.Contains(e.stat)).Select(e => $"{e.stat}:{e.amount}"),
                    Is.EquivalentTo(expected[i].Split(',')), choice.id);
            }
        }

        [TestCase("AAAAAAAAAA", 86, 64, 79, 76, 30, 71, 62, 55, 78, 8, 53, 55)]
        [TestCase("BBBBBBBBBB", 31, 47, 40, 41, 33, 49, 50, 47, 50, 41, 50, 48)]
        [TestCase("CCCCCCCCCC", 29, 45, 36, 39, 34, 42, 54, 45, 45, 38, 50, 50)]
        public void ChapterEight_FullRoutesMatchIndependentExpectedSnapshots(string route, int communication,
            int family, int regulation, int help, int risk, int trust, int confidence, int empathy, int support,
            int anxiety, int selfControl, int knowledge)
        {
            var profile = NewProfile();
            var graph = StoryRepository.LoadChapterEight();
            for (int i = 0; i < route.Length; i++)
                profile.Apply(graph.Get($"node-{i + 1}").choices[route[i] - 'A'].effects);
            var snapshot = JsonUtility.FromJson<MetricSnapshot>(JsonUtility.ToJson(profile.Snapshot()));
            Assert.That(snapshot.familyCommunication, Is.EqualTo(communication));
            Assert.That(snapshot.familySupport, Is.EqualTo(family));
            Assert.That(snapshot.emotionalRegulation, Is.EqualTo(regulation));
            Assert.That(snapshot.helpSeekingTendency, Is.EqualTo(help));
            Assert.That(snapshot.risk, Is.EqualTo(risk));
            Assert.That(snapshot.trust, Is.EqualTo(trust));
            Assert.That(snapshot.confidence, Is.EqualTo(confidence));
            Assert.That(snapshot.empathy, Is.EqualTo(empathy));
            Assert.That(snapshot.socialSupport, Is.EqualTo(support));
            Assert.That(snapshot.anxiety, Is.EqualTo(anxiety));
            Assert.That(profile.selfControl, Is.EqualTo(selfControl));
            Assert.That(snapshot.knowledge, Is.EqualTo(knowledge));
        }

        [Test]
        public void ChapterEight_AloneFinishesSeasonOneAndAwardsOnce()
        {
            var profile = NewProfile();
            Assert.That(CampaignProgression.CanStartChapterEight(null), Is.False);
            for (int i = 1; i <= 7; i++)
            {
                Assert.That(CampaignProgression.CanStartChapterEight(profile), Is.False);
                CampaignProgression.Complete(StoryRepository.LoadById($"chapter-{i}").Chapter, profile);
                Assert.That(profile.seasonOneCompleted, Is.False, $"Chapter {i} must not finish the season.");
            }
            Assert.That(CampaignProgression.CanStartChapterEight(profile), Is.True);
            Assert.That(profile.xp, Is.EqualTo(1550));
            Assert.That(profile.familySupportArticleUnlocked, Is.False);
            var chapter = StoryRepository.LoadChapterEight().Chapter;
            Assert.That(CampaignProgression.Complete(chapter, profile), Is.EqualTo(300));
            Assert.That(profile.xp, Is.EqualTo(1850));
            Assert.That(profile.completedChapterEight && profile.seasonOneCompleted && profile.familySupportArticleUnlocked, Is.True);
            profile.PrepareForChapterEight();
            Assert.That(CampaignProgression.Complete(chapter, profile), Is.Zero);
            Assert.That(profile.xp, Is.EqualTo(1850));
            Assert.That(profile.safeZoneUnlocked && profile.relationshipPathUnlocked && profile.bullyingSupportArticleUnlocked &&
                profile.healthyRelationshipArticleUnlocked && profile.digitalSafetyGuideUnlocked &&
                profile.financialSafetyArticleUnlocked && profile.moneySmartGuideUnlocked &&
                profile.healthyLifestyleArticleUnlocked && profile.healthyRoutineGuideUnlocked, Is.True);
        }

        [TestCase(4, 750)]
        [TestCase(5, 1000)]
        [TestCase(6, 1250)]
        [TestCase(7, 1550)]
        public void ChapterEight_LegacySeasonFlagDoesNotUnlockTheFinaleEarlyOrLoseXp(int chapter, int xp)
        {
            var save = JsonUtility.FromJson<PrototypeSave>(
                "{\"chapterId\":\"chapter-" + chapter + "\",\"chapterCompleted\":true,\"profile\":{\"seasonOneCompleted\":true,\"xp\":" + xp +
                ",\"risk\":27,\"trustParent\":9,\"selfControl\":64,\"helpSeekingTendency\":83,\"health\":72,\"sleepAwareness\":81}}");
            CampaignProgression.Normalize(save);
            Assert.That(save.profile.seasonOneCompleted, Is.False);
            Assert.That(save.profile.completedChapterFour, Is.True);
            Assert.That(save.profile.completedChapterEight || save.profile.familySupportArticleUnlocked, Is.False);
            Assert.That(CampaignProgression.CanStartChapterEight(save.profile), Is.EqualTo(chapter == 7));
            save.profile.PrepareForChapterEight();
            foreach (string stat in FamilyMetrics)
                Assert.That(save.profile.GetStat(stat), Is.EqualTo(50));
            Assert.That(save.profile.xp, Is.EqualTo(xp));
            Assert.That(save.profile.risk, Is.EqualTo(27));
            Assert.That(save.profile.trustParent, Is.EqualTo(9));
            Assert.That(save.profile.selfControl, Is.EqualTo(64));
            Assert.That(save.profile.helpSeekingTendency, Is.EqualTo(83));
            Assert.That(save.profile.health, Is.EqualTo(72));
            Assert.That(save.profile.sleepAwareness, Is.EqualTo(81));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ChapterEight_SaveRoundTripPreservesMetricsAndFinaleState(bool completed)
        {
            var profile = NewProfile();
            profile.familyCommunication = 67;
            profile.familySupport = 62;
            profile.emotionalRegulation = 73;
            profile.helpSeekingTendency = 84;
            profile.xp = completed ? 1850 : 1550;
            var save = JsonUtility.FromJson<PrototypeSave>(JsonUtility.ToJson(new PrototypeSave
            {
                chapterId = "chapter-8", currentNodeId = completed ? "END" : "node-6",
                chapterCompleted = completed, branchPath = "node-1:1A>node-2:2B", profile = profile
            }));
            CampaignProgression.Normalize(save);
            CampaignProgression.Normalize(save);
            Assert.That(JsonUtility.ToJson(save.profile.Snapshot()), Is.EqualTo(JsonUtility.ToJson(profile.Snapshot())));
            Assert.That(save.profile.xp, Is.EqualTo(profile.xp));
            Assert.That(save.profile.completedChapterSeven && save.profile.completedChapterSix && save.profile.completedChapterFive &&
                save.profile.completedChapterFour && save.profile.completedChapterThree && save.profile.completedChapterTwo &&
                save.profile.completedChapterOne, Is.True);
            Assert.That(save.profile.completedChapterEight, Is.EqualTo(completed));
            Assert.That(save.profile.seasonOneCompleted, Is.EqualTo(completed));
            Assert.That(save.profile.familySupportArticleUnlocked, Is.EqualTo(completed));
            Assert.That(save.currentNodeId, Is.EqualTo(completed ? "END" : "node-6"));
            Assert.That(save.branchPath, Is.EqualTo("node-1:1A>node-2:2B"));
        }

        [Test]
        public void ChapterEight_ReplayingEarlierChaptersRetainsTheTrueSeasonCompletion()
        {
            var profile = NewProfile();
            for (int i = 1; i <= 8; i++)
                CampaignProgression.Complete(StoryRepository.LoadById($"chapter-{i}").Chapter, profile);
            for (int i = 1; i <= 7; i++)
            {
                CampaignProgression.Normalize(new PrototypeSave { chapterId = $"chapter-{i}", profile = profile });
                Assert.That(CampaignProgression.Complete(StoryRepository.LoadById($"chapter-{i}").Chapter, profile), Is.Zero);
                Assert.That(profile.seasonOneCompleted && profile.familySupportArticleUnlocked, Is.True);
            }
            Assert.That(profile.xp, Is.EqualTo(1850));
        }

        [TestCase("familyCommunication")]
        [TestCase("familySupport")]
        [TestCase("emotionalRegulation")]
        public void ChapterEight_NewMetricsClampAndResetWithNewGame(string stat)
        {
            var profile = NewProfile();
            profile.Apply(stat.ToUpperInvariant(), 500);
            Assert.That(profile.GetStat(stat), Is.EqualTo(100));
            profile.Apply(stat, -500);
            Assert.That(profile.GetStat(stat), Is.Zero);
            CampaignProgression.Complete(StoryRepository.LoadChapterEight().Chapter, profile);
            profile.ResetForChapterOne();
            Assert.That(profile.GetStat(stat), Is.EqualTo(50));
            Assert.That(profile.completedChapterEight || profile.seasonOneCompleted || profile.familySupportArticleUnlocked, Is.False);
            Assert.That(profile.xp, Is.Zero);
        }

        [Test]
        public void ChapterEight_AdaptiveParentConversationsAreReachableAndKeepAllChoices()
        {
            var graph = StoryRepository.LoadChapterEight();
            var generator = new LocalConversationGenerator();
            foreach (int target in new[] { 4, 11 })
            {
                var node = graph.Get($"node-{target}");
                string original = JsonUtility.ToJson(node);
                string[] generated = new string[2];
                for (int route = 0; route < 2; route++)
                {
                    var profile = NewProfile();
                    for (int i = 1; i < target; i++)
                        profile.Apply(graph.Get($"node-{i}").choices[route].effects);
                    generated[route] = generator.Generate(node, profile, 42);
                }
                Assert.That(generated, Is.EquivalentTo(node.variants.Select(v => v.text)));
                foreach (int value in new[] { 0, 49, 50, 100 })
                {
                    var profile = NewProfile();
                    profile.familyCommunication = value;
                    Assert.That(generator.Generate(node, profile, 42), Is.EqualTo(node.variants.Single(v =>
                        value >= v.minInclusive && value <= v.maxInclusive).text));
                }
                Assert.That(JsonUtility.ToJson(node), Is.EqualTo(original));
            }
        }

        [TestCase("Ayah", "char_dad_chroma")]
        [TestCase(" dad ", "char_dad_chroma")]
        [TestCase("Ibu", "char_ibu_chroma")]
        [TestCase("Mom", "char_ibu_chroma")]
        public void ChapterEight_ParentAliasesLoadTheirOwnPortrait(string speaker, string asset)
        {
            var method = typeof(YouthRisePrototype).GetMethod("CharacterResource", BindingFlags.Static | BindingFlags.NonPublic);
            string path = (string)method.Invoke(null, new object[] { speaker });
            Assert.That(path, Is.EqualTo("YouthRise/Art/Characters/" + asset));
            Assert.That(Resources.Load<Texture2D>(path), Is.Not.Null);
        }

        [Test]
        public void ChapterEight_ClosingMessageMatchesTheSuppliedSeasonEnding()
        {
            var chapter = StoryRepository.LoadChapterEight().Chapter;
            Assert.That(chapter.endingLines, Is.EqualTo(new[]
            {
                "Some problems take time.", "You don't have to solve everything today.",
                "You just need to know that you don't have to face it alone."
            }));
            Assert.That(chapter.endingHeading.Replace("\n", " "), Is.EqualTo("YOUR JOURNEY IS JUST BEGINNING."));
            Assert.That(StoryRepository.LoadChapterFour().Chapter.completionHeading, Does.Not.Contain("SEASON 1 SELESAI"));
            Assert.That(chapter.reflectionLines, Does.Contain("Kamu tidak bertanggung jawab memperbaiki setiap masalah keluarga."));
            Assert.That(StoryRepository.LoadChapterEight().Get("node-1").body, Does.Contain("tidak harus menjadi penengah"));
            Assert.That(StoryRepository.LoadChapterEight().Get("node-7").body, Does.Contain("tanggung jawab orang dewasa"));
        }
    }
}
