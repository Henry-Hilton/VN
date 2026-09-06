using System;

namespace YouthRise
{
    public static class CampaignProgression
    {
        public static bool CanStartChapterTwo(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterOne;
        }

        public static bool CanStartChapterThree(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterTwo;
        }

        public static bool CanStartChapterFour(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterThree;
        }

        public static bool CanStartChapterFive(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterFour;
        }

        public static bool CanStartChapterSix(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterFive;
        }

        public static bool CanStartChapterSeven(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterSix;
        }

        public static bool CanStartChapterEight(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterSeven;
        }

        public static void Normalize(PrototypeSave save)
        {
            if (save?.profile == null)
                return;

            bool chapterTwo = IsChapterTwo(save.chapterId);
            bool chapterThree = IsChapterThree(save.chapterId);
            bool chapterFour = IsChapterFour(save.chapterId);
            bool chapterFive = IsChapterFive(save.chapterId);
            bool chapterSix = IsChapterSix(save.chapterId);
            bool chapterSeven = IsChapterSeven(save.chapterId);
            bool chapterEight = IsChapterEight(save.chapterId);
            if ((!chapterTwo && !chapterThree && !chapterFour && !chapterFive && !chapterSix && !chapterSeven && !chapterEight && save.chapterCompleted)
                || chapterTwo || chapterThree || chapterFour || chapterFive || chapterSix || chapterSeven || chapterEight)
            {
                save.profile.completedChapterOne = true;
                save.profile.safeZoneUnlocked = true;
            }

            if ((chapterTwo && save.chapterCompleted) || chapterThree || chapterFour || chapterFive || chapterSix || chapterSeven || chapterEight)
            {
                save.profile.completedChapterTwo = true;
                save.profile.relationshipPathUnlocked = true;
                save.profile.bullyingSupportArticleUnlocked = true;
            }

            if ((chapterThree && save.chapterCompleted) || chapterFour || chapterFive || chapterSix || chapterSeven || chapterEight)
            {
                save.profile.completedChapterThree = true;
                save.profile.healthyRelationshipArticleUnlocked = true;
                save.profile.digitalSafetyGuideUnlocked = true;
            }

            if ((chapterFour && save.chapterCompleted) || chapterFive || chapterSix || chapterSeven || chapterEight)
            {
                save.profile.completedChapterFour = true;
            }

            if ((chapterFive && save.chapterCompleted) || chapterSix || chapterSeven || chapterEight)
            {
                save.profile.completedChapterFive = true;
                save.profile.financialSafetyArticleUnlocked = true;
                save.profile.moneySmartGuideUnlocked = true;
            }

            if ((chapterSix && save.chapterCompleted) || chapterSeven || chapterEight)
            {
                save.profile.completedChapterSix = true;
                save.profile.healthyLifestyleArticleUnlocked = true;
                save.profile.healthyRoutineGuideUnlocked = true;
            }

            if ((chapterSeven && save.chapterCompleted) || chapterEight)
                save.profile.completedChapterSeven = true;

            if (chapterEight && save.chapterCompleted)
                save.profile.completedChapterEight = true;

            // Older saves used Chapter 4 as the finale. Keep its completion and XP,
            // but derive the expanded Season 1 milestone only from Chapter 8.
            save.profile.seasonOneCompleted = save.profile.completedChapterEight;
            if (save.profile.completedChapterEight)
                save.profile.familySupportArticleUnlocked = true;
        }

        public static int Complete(StoryChapter chapter, PlayerProfile profile)
        {
            if (chapter == null || profile == null)
                return 0;

            int reward = Math.Max(0, chapter.rewardXp);
            profile.seasonOneCompleted = profile.completedChapterEight;
            if (IsChapterEight(chapter.id))
            {
                bool alreadyCompleted = profile.completedChapterEight;
                Normalize(new PrototypeSave { chapterId = chapter.id, chapterCompleted = true, profile = profile });
                if (alreadyCompleted)
                    return 0;

                profile.Apply("xp", reward);
                return reward;
            }

            if (IsChapterSeven(chapter.id))
            {
                bool alreadyCompleted = profile.completedChapterSeven;
                Normalize(new PrototypeSave { chapterId = chapter.id, chapterCompleted = true, profile = profile });
                if (alreadyCompleted)
                    return 0;

                profile.Apply("xp", reward);
                return reward;
            }

            if (IsChapterSix(chapter.id))
            {
                bool alreadyCompleted = profile.completedChapterSix;
                Normalize(new PrototypeSave { chapterId = chapter.id, chapterCompleted = true, profile = profile });
                if (alreadyCompleted)
                    return 0;

                profile.Apply("xp", reward);
                return reward;
            }

            if (IsChapterFive(chapter.id))
            {
                bool alreadyCompleted = profile.completedChapterFive;
                Normalize(new PrototypeSave { chapterId = chapter.id, chapterCompleted = true, profile = profile });
                if (alreadyCompleted)
                    return 0;

                profile.Apply("xp", reward);
                return reward;
            }

            if (IsChapterFour(chapter.id))
            {
                profile.completedChapterOne = true;
                profile.completedChapterTwo = true;
                profile.completedChapterThree = true;
                profile.safeZoneUnlocked = true;
                profile.relationshipPathUnlocked = true;
                profile.bullyingSupportArticleUnlocked = true;
                profile.healthyRelationshipArticleUnlocked = true;
                profile.digitalSafetyGuideUnlocked = true;
                if (profile.completedChapterFour)
                    return 0;

                profile.Apply("xp", reward);
                profile.completedChapterFour = true;
                return reward;
            }

            if (IsChapterThree(chapter.id))
            {
                profile.completedChapterOne = true;
                profile.completedChapterTwo = true;
                profile.safeZoneUnlocked = true;
                profile.relationshipPathUnlocked = true;
                profile.bullyingSupportArticleUnlocked = true;
                if (profile.completedChapterThree)
                    return 0;

                profile.Apply("xp", reward);
                profile.completedChapterThree = true;
                profile.healthyRelationshipArticleUnlocked = true;
                profile.digitalSafetyGuideUnlocked = true;
                return reward;
            }

            if (IsChapterTwo(chapter.id))
            {
                profile.completedChapterOne = true;
                profile.safeZoneUnlocked = true;
                if (profile.completedChapterTwo)
                    return 0;

                profile.Apply("xp", reward);
                profile.completedChapterTwo = true;
                profile.relationshipPathUnlocked = true;
                profile.bullyingSupportArticleUnlocked = true;
                return reward;
            }

            if (profile.completedChapterOne)
                return 0;

            profile.Apply("xp", reward);
            profile.completedChapterOne = true;
            profile.safeZoneUnlocked = true;
            return reward;
        }

        private static bool IsChapterTwo(string chapterId)
        {
            return string.Equals(chapterId, "chapter-2", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsChapterThree(string chapterId)
        {
            return string.Equals(chapterId, "chapter-3", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsChapterEight(string chapterId)
        {
            return string.Equals(chapterId, "chapter-8", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsChapterSeven(string chapterId)
        {
            return string.Equals(chapterId, "chapter-7", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsChapterSix(string chapterId)
        {
            return string.Equals(chapterId, "chapter-6", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsChapterFive(string chapterId)
        {
            return string.Equals(chapterId, "chapter-5", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsChapterFour(string chapterId)
        {
            return string.Equals(chapterId, "chapter-4", StringComparison.OrdinalIgnoreCase);
        }
    }
}
