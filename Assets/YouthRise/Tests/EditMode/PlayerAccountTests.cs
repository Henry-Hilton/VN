using System;
using NUnit.Framework;
using UnityEngine;

namespace YouthRise.Tests
{
    public sealed class PlayerAccountTests
    {
        [TestCase("male", "Alex")]
        [TestCase("female", "Anita")]
        public void GenderSelectsStoryCharacter(string gender, string expected)
        {
            Assert.That(new PlayerAccount { gender = gender }.CharacterName, Is.EqualTo(expected));
        }

        [Test]
        public void SavesAreIsolatedBetweenAccountsAndReplacementKeepsLatestProgress()
        {
            string first = Guid.NewGuid().ToString(), second = Guid.NewGuid().ToString();
            try
            {
                PrototypeSaveService.SetAccount(first);
                PrototypeSaveService.Save(new PrototypeSave { chapterId = "chapter-1", currentNodeId = "opening", profile = new PlayerProfile() });
                PrototypeSaveService.Save(new PrototypeSave { chapterId = "chapter-2", currentNodeId = "opening", profile = new PlayerProfile() });
                Assert.That(PrototypeSaveService.TryLoad(out PrototypeSave saved), Is.True);
                Assert.That(saved.chapterId, Is.EqualTo("chapter-2"));
                PrototypeSaveService.SetAccount(second);
                Assert.That(PrototypeSaveService.Exists, Is.False);
                PrototypeSaveService.Save(new PrototypeSave { chapterId = "chapter-3", profile = new PlayerProfile() });
                PrototypeSaveService.SetAccount(first);
                Assert.That(PrototypeSaveService.TryLoad(out saved), Is.True);
                Assert.That(saved.chapterId, Is.EqualTo("chapter-2"));
            }
            finally
            {
                PrototypeSaveService.SetAccount(first); PrototypeSaveService.Clear();
                PrototypeSaveService.SetAccount(second); PrototypeSaveService.Clear();
                PrototypeSaveService.SetAccount(null);
            }
        }

        [Test]
        public void AccountIdsCannotEscapeSaveDirectory()
        {
            Assert.Throws<ArgumentException>(() => PrototypeSaveService.SetAccount("../../other-player"));
        }

        [Test]
        public void AnitaPortraitExistsAndHasUsefulResolution()
        {
            Texture2D art = Resources.Load<Texture2D>("YouthRise/Art/Characters/char_anita");
            Assert.That(art, Is.Not.Null);
            Assert.That(art.width, Is.GreaterThanOrEqualTo(512));
            Assert.That(art.height, Is.GreaterThanOrEqualTo(768));
        }
    }
}
