using System;
using UnityEngine;

namespace YouthRise
{
    public static class StoryRepository
    {
        private const string ChapterOneResource = "YouthRise/chapter1";

        public static StoryGraph LoadChapterOne()
        {
            TextAsset asset = Resources.Load<TextAsset>(ChapterOneResource);
            if (asset == null)
                throw new InvalidOperationException(
                    $"Missing story resource at Assets/YouthRise/Resources/{ChapterOneResource}.json");

            StoryChapter chapter = JsonUtility.FromJson<StoryChapter>(asset.text);
            if (chapter == null)
                throw new InvalidOperationException("Chapter 1 JSON could not be parsed.");

            return new StoryGraph(chapter);
        }
    }
}
