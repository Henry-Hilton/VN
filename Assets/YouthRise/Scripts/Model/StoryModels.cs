using System;
using System.Collections.Generic;

namespace YouthRise
{
    [Serializable]
    public sealed class StoryChapter
    {
        public string id;
        public string title;
        public string subtitle;
        public int number;
        public int rewardXp;
        public string completionHeading;
        public string[] reflectionLines;
        public string[] unlockLabels;
        public string startNodeId;
        public StoryNode[] nodes;
    }

    [Serializable]
    public sealed class StoryNode
    {
        public string id;
        public string location;
        public string background;
        public string speaker;
        public string body;
        public string nextNodeId;
        public string branch;
        public StoryVariant[] variants;
        public StoryChoice[] choices;
    }

    [Serializable]
    public sealed class StoryVariant
    {
        public string stat;
        public int minInclusive;
        public int maxInclusive = 100;
        public string text;
    }

    [Serializable]
    public sealed class StoryChoice
    {
        public string id;
        public string label;
        public string nextNodeId;
        public string tendency;
        public StatDelta[] effects;
    }

    [Serializable]
    public sealed class StatDelta
    {
        public string stat;
        public int amount;

        public StatDelta()
        {
        }

        public StatDelta(string statId, int delta)
        {
            stat = statId;
            amount = delta;
        }
    }

    public sealed class StoryGraph
    {
        private readonly Dictionary<string, StoryNode> nodes;

        public StoryChapter Chapter { get; }

        public StoryGraph(StoryChapter chapter)
        {
            Chapter = chapter ?? throw new ArgumentNullException(nameof(chapter));
            nodes = new Dictionary<string, StoryNode>(StringComparer.OrdinalIgnoreCase);

            if (chapter.nodes == null || chapter.nodes.Length == 0)
                throw new InvalidOperationException("The chapter contains no story nodes.");

            foreach (StoryNode node in chapter.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                    throw new InvalidOperationException("Every story node must have an id.");

                if (!nodes.TryAdd(node.id, node))
                    throw new InvalidOperationException($"Duplicate story node id: {node.id}");
            }

            if (!nodes.ContainsKey(chapter.startNodeId))
                throw new InvalidOperationException($"Start node '{chapter.startNodeId}' does not exist.");

            ValidateLinks();
        }

        public StoryNode Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return nodes.TryGetValue(id, out StoryNode node) ? node : null;
        }

        public bool Contains(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && nodes.ContainsKey(id);
        }

        private void ValidateLinks()
        {
            foreach (StoryNode node in nodes.Values)
            {
                ValidateTarget(node.id, node.nextNodeId);

                if (node.choices == null)
                    continue;

                foreach (StoryChoice choice in node.choices)
                {
                    if (choice == null || string.IsNullOrWhiteSpace(choice.id))
                        throw new InvalidOperationException($"Node '{node.id}' has a choice without an id.");

                    ValidateTarget($"{node.id}/{choice.id}", choice.nextNodeId);
                }
            }
        }

        private void ValidateTarget(string source, string target)
        {
            if (string.IsNullOrWhiteSpace(target) || target.Equals("END", StringComparison.OrdinalIgnoreCase))
                return;

            if (!nodes.ContainsKey(target))
                throw new InvalidOperationException($"Story link '{source}' points to missing node '{target}'.");
        }
    }
}
