using NUnit.Framework;

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
    }
}
