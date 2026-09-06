using System;
using UnityEngine;

namespace YouthRise
{
    public static class StoryRepository
    {
        private const string ChapterOneResource = "YouthRise/chapter1";
        private const string ChapterTwoResource = "YouthRise/chapter2";
        private const string ChapterThreeResource = "YouthRise/chapter3";
        private const string ChapterFourResource = "YouthRise/chapter4";
        private const string ChapterFiveResource = "YouthRise/chapter5";
        private const string ChapterSixResource = "YouthRise/chapter6";
        private const string ChapterSevenResource = "YouthRise/chapter7";
        private const string ChapterEightResource = "YouthRise/chapter8";

        public static StoryGraph LoadChapterOne()
        {
            return Load(ChapterOneResource, "Chapter 1");
        }

        public static StoryGraph LoadChapterTwo()
        {
            return Load(ChapterTwoResource, "Chapter 2");
        }

        public static StoryGraph LoadChapterThree()
        {
            return Load(ChapterThreeResource, "Chapter 3");
        }

        public static StoryGraph LoadChapterFour()
        {
            return Load(ChapterFourResource, "Chapter 4");
        }

        public static StoryGraph LoadChapterFive()
        {
            return Load(ChapterFiveResource, "Chapter 5");
        }

        public static StoryGraph LoadChapterSix()
        {
            return Load(ChapterSixResource, "Chapter 6");
        }

        public static StoryGraph LoadById(string chapterId)
        {
            if (string.Equals(chapterId, "chapter-8", StringComparison.OrdinalIgnoreCase))
                return LoadChapterEight();
            if (string.Equals(chapterId, "chapter-7", StringComparison.OrdinalIgnoreCase))
                return LoadChapterSeven();
            if (string.Equals(chapterId, "chapter-6", StringComparison.OrdinalIgnoreCase))
                return LoadChapterSix();
            if (string.Equals(chapterId, "chapter-5", StringComparison.OrdinalIgnoreCase))
                return LoadChapterFive();
            if (string.Equals(chapterId, "chapter-4", StringComparison.OrdinalIgnoreCase))
                return LoadChapterFour();
            if (string.Equals(chapterId, "chapter-3", StringComparison.OrdinalIgnoreCase))
                return LoadChapterThree();
            if (string.Equals(chapterId, "chapter-2", StringComparison.OrdinalIgnoreCase))
                return LoadChapterTwo();
            return LoadChapterOne();
        }

        public static StoryGraph LoadChapterSeven()
        {
            return Load(ChapterSevenResource, "Chapter 7");
        }

        public static StoryGraph LoadChapterEight()
        {
            return Load(ChapterEightResource, "Chapter 8");
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
