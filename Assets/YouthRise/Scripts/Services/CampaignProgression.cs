using System;

namespace YouthRise
{
    public static class CampaignProgression
    {
        public static bool CanStartChapterTwo(PlayerProfile profile)
        {
            return profile != null && profile.completedChapterOne;
        }

        public static void Normalize(PrototypeSave save)
        {
            if (save?.profile == null)
                return;

            bool chapterTwo = IsChapterTwo(save.chapterId);
            if ((!chapterTwo && save.chapterCompleted) || chapterTwo)
            {
                save.profile.completedChapterOne = true;
                save.profile.safeZoneUnlocked = true;
            }

            if (chapterTwo && save.chapterCompleted)
            {
                save.profile.completedChapterTwo = true;
                save.profile.relationshipPathUnlocked = true;
                save.profile.bullyingSupportArticleUnlocked = true;
            }
        }

        public static int Complete(StoryChapter chapter, PlayerProfile profile)
        {
            if (chapter == null || profile == null)
                return 0;

            int reward = Math.Max(0, chapter.rewardXp);
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
    }
}
