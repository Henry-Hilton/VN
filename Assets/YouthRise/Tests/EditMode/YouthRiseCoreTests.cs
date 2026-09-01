using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class YouthRiseCoreTests
    {
        [Test]
        public void ChapterOne_IsValidAndContainsElevenDecisionNodes()
        {
            StoryGraph graph = StoryRepository.LoadChapterOne();

            Assert.That(graph.Chapter.nodes, Has.Length.EqualTo(13));
            Assert.That(graph.Chapter.startNodeId, Is.EqualTo("opening"));
            StoryNode bedtime = graph.Get("bedtime");
            Assert.That(bedtime, Is.Not.Null);
            Assert.That(bedtime.nextNodeId, Is.EqualTo("END"));

            int decisionNodes = 0;
            foreach (StoryNode node in graph.Chapter.nodes)
            {
                if (node.choices == null || node.choices.Length == 0)
                    continue;

                decisionNodes++;
                Assert.That(node.choices, Has.Length.EqualTo(3), node.id);
            }

            Assert.That(decisionNodes, Is.EqualTo(11));
        }

        [Test]
        public void ChapterTwo_IsValidAndContainsTenDecisionNodes()
        {
            StoryGraph graph = StoryRepository.LoadChapterTwo();

            Assert.That(graph.Chapter.id, Is.EqualTo("chapter-2"));
            Assert.That(graph.Chapter.number, Is.EqualTo(2));
            Assert.That(graph.Chapter.rewardXp, Is.EqualTo(150));
            Assert.That(graph.Chapter.nodes, Has.Length.EqualTo(12));
            Assert.That(graph.Chapter.startNodeId, Is.EqualTo("opening"));
            Assert.That(graph.Get("node-11").nextNodeId, Is.EqualTo("END"));

            int decisionNodes = 0;
            foreach (StoryNode node in graph.Chapter.nodes)
            {
                if (node.choices == null || node.choices.Length == 0)
                    continue;

                decisionNodes++;
                Assert.That(node.choices, Has.Length.EqualTo(3), node.id);
            }

            Assert.That(decisionNodes, Is.EqualTo(10));
        }

        [Test]
        public void Profile_AppliesHiddenRiskAndTrustEffects()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();

            profile.Apply("risk", 20);
            profile.Apply("trustFriend", 5);
            profile.Apply("anxiety", 3);

            Assert.That(profile.risk, Is.EqualTo(50));
            Assert.That(profile.TrustScore, Is.EqualTo(55));
            Assert.That(profile.anxiety, Is.EqualTo(23));
        }

        [Test]
        public void ChapterTwoProgression_AwardsOnceAndUnlocksSupportContent()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();

            int chapterOneReward = CampaignProgression.Complete(StoryRepository.LoadChapterOne().Chapter, profile);
            int repeatedChapterOneReward = CampaignProgression.Complete(StoryRepository.LoadChapterOne().Chapter, profile);
            profile.PrepareForChapterTwo();
            profile.Apply("trustLeo", 4);
            profile.Apply("bystander", 6);
            int chapterTwoReward = CampaignProgression.Complete(StoryRepository.LoadChapterTwo().Chapter, profile);
            int repeatedChapterTwoReward = CampaignProgression.Complete(StoryRepository.LoadChapterTwo().Chapter, profile);

            Assert.That(chapterOneReward, Is.EqualTo(100));
            Assert.That(repeatedChapterOneReward, Is.Zero);
            Assert.That(chapterTwoReward, Is.EqualTo(150));
            Assert.That(repeatedChapterTwoReward, Is.Zero);
            Assert.That(profile.xp, Is.EqualTo(250));
            Assert.That(profile.TrustScore, Is.EqualTo(54));
            Assert.That(profile.Snapshot().bystanderResponse, Is.EqualTo(56));
            Assert.That(profile.safeZoneUnlocked, Is.True);
            Assert.That(profile.relationshipPathUnlocked, Is.True);
            Assert.That(profile.bullyingSupportArticleUnlocked, Is.True);
        }

        [Test]
        public void Progression_MigratesCompletedChapterOneSave()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();
            var save = new PrototypeSave
            {
                chapterId = "chapter-1",
                chapterCompleted = true,
                profile = profile
            };

            CampaignProgression.Normalize(save);

            Assert.That(profile.completedChapterOne, Is.True);
            Assert.That(profile.safeZoneUnlocked, Is.True);
            Assert.That(CampaignProgression.CanStartChapterTwo(profile), Is.True);
        }

        [Test]
        public void ConversationGenerator_UsesMatchingHiddenStateVariant()
        {
            var profile = new PlayerProfile();
            profile.ResetForChapterOne();
            profile.anxiety = 80;
            var node = new StoryNode
            {
                id = "test",
                body = "default",
                variants = new[]
                {
                    new StoryVariant
                    {
                        stat = "anxiety",
                        minInclusive = 70,
                        maxInclusive = 100,
                        text = "napasku terasa pendek"
                    }
                }
            };

            string result = new LocalConversationGenerator().Generate(node, profile, 12);

            Assert.That(result, Is.EqualTo("napasku terasa pendek"));
        }

        [Test]
        public void SafeZoneAssistant_EscalatesImmediateSafetyLanguage()
        {
            var assistant = new SafeZoneAssistant();

            SafeZoneAssessment urgent = assistant.Assess("Aku ingin mati dan sedang sendirian.");
            SafeZoneAssessment nonUrgent = assistant.Assess("Aku diejek teman sekelas.");

            Assert.That(urgent.immediateSafetyConcern, Is.True);
            Assert.That(urgent.urgency, Is.EqualTo("SEGERA"));
            Assert.That(nonUrgent.immediateSafetyConcern, Is.False);
            Assert.That(nonUrgent.category, Is.EqualTo("Perundungan"));
        }

        [Test]
        public void VisualArtCatalog_ContainsEveryChapterOneAsset()
        {
            string[] resourcePaths =
            {
                "YouthRise/Art/Backgrounds/bg_home",
                "YouthRise/Art/Backgrounds/bg_school_gate",
                "YouthRise/Art/Backgrounds/bg_classroom",
                "YouthRise/Art/Backgrounds/bg_hallway",
                "YouthRise/Art/Backgrounds/bg_back_school",
                "YouthRise/Art/Backgrounds/bg_street",
                "YouthRise/Art/Backgrounds/bg_bedroom",
                "YouthRise/Art/Backgrounds/bg_locker",
                "YouthRise/Art/Backgrounds/bg_counselor",
                "YouthRise/Art/Characters/char_maya_chroma",
                "YouthRise/Art/Characters/char_kevin_chroma",
                "YouthRise/Art/Characters/char_rina_chroma",
                "YouthRise/Art/Characters/char_ibu_chroma",
                "YouthRise/Art/Characters/char_senior_chroma",
                "YouthRise/Art/Characters/char_mr_daniel_chroma",
                "YouthRise/Art/Characters/char_leo_chroma"
            };

            foreach (string resourcePath in resourcePaths)
            {
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                Assert.That(texture, Is.Not.Null, resourcePath);
                Assert.That(texture.width, Is.GreaterThan(512), resourcePath);
                Assert.That(texture.height, Is.GreaterThan(512), resourcePath);
            }
        }
    }
}
