using System;
using UnityEngine;

namespace YouthRise
{
    public static class StoryRepository
    {
        private const string ChapterOneResource = "YouthRise/chapter1";
        private const string ChapterTwoResource = "YouthRise/chapter2";

        public static StoryGraph LoadChapterOne()
        {
            return Load(ChapterOneResource, "Chapter 1");
        }

        public static StoryGraph LoadChapterTwo()
        {
            return Load(ChapterTwoResource, "Chapter 2");
        }

        public static StoryGraph LoadById(string chapterId)
        {
            return string.Equals(chapterId, "chapter-2", StringComparison.OrdinalIgnoreCase)
                ? LoadChapterTwo()
                : LoadChapterOne();
        }

        private static StoryGraph Load(string resourcePath, string displayName)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
                throw new InvalidOperationException(
                    $"Missing story resource at Assets/YouthRise/Resources/{resourcePath}.json");

            StoryChapter chapter = JsonUtility.FromJson<StoryChapter>(asset.text);
            if (chapter == null)
                throw new InvalidOperationException($"{displayName} JSON could not be parsed.");

            return new StoryGraph(chapter);
        }
    }
}
