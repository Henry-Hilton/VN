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

        public static void Normalize(PrototypeSave save)
        {
            if (save?.profile == null)
                return;

            bool chapterTwo = IsChapterTwo(save.chapterId);
            bool chapterThree = IsChapterThree(save.chapterId);
            bool chapterFour = IsChapterFour(save.chapterId);
            bool chapterFive = IsChapterFive(save.chapterId);
            if ((!chapterTwo && !chapterThree && !chapterFour && !chapterFive && save.chapterCompleted) || chapterTwo || chapterThree || chapterFour || chapterFive)
            {
                save.profile.completedChapterOne = true;
                save.profile.safeZoneUnlocked = true;
            }

            if ((chapterTwo && save.chapterCompleted) || chapterThree || chapterFour || chapterFive)
            {
                save.profile.completedChapterTwo = true;
                save.profile.relationshipPathUnlocked = true;
                save.profile.bullyingSupportArticleUnlocked = true;
            }

            if ((chapterThree && save.chapterCompleted) || chapterFour || chapterFive)
            {
                save.profile.completedChapterThree = true;
                save.profile.healthyRelationshipArticleUnlocked = true;
                save.profile.digitalSafetyGuideUnlocked = true;
            }

            if ((chapterFour && save.chapterCompleted) || chapterFive)
            {
                save.profile.completedChapterFour = true;
                save.profile.seasonOneCompleted = true;
            }

            if (chapterFive && save.chapterCompleted)
            {
                save.profile.completedChapterFive = true;
                save.profile.financialSafetyArticleUnlocked = true;
                save.profile.moneySmartGuideUnlocked = true;
            }
        }

        public static int Complete(StoryChapter chapter, PlayerProfile profile)
        {
            if (chapter == null || profile == null)
                return 0;

            int reward = Math.Max(0, chapter.rewardXp);
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
                profile.seasonOneCompleted = true;
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
