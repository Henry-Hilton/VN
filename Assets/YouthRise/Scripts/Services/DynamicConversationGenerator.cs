using System;
using System.Collections.Generic;

namespace YouthRise
{
    public interface IConversationGenerator
    {
        string Generate(StoryNode node, PlayerProfile profile, int sessionSeed);
    }

    /// <summary>
    /// Local, deterministic PCG provider. Content authors provide bounded variants in JSON;
    /// the provider selects a matching line from the current hidden player state. It performs
    /// no network request and can later be replaced with an approved remote provider.
    /// </summary>
    public sealed class LocalConversationGenerator : IConversationGenerator
    {
        public string Generate(StoryNode node, PlayerProfile profile, int sessionSeed)
        {
            if (node == null)
                return string.Empty;

            var matching = new List<StoryVariant>();
            if (node.variants != null)
            {
                foreach (StoryVariant variant in node.variants)
                {
                    if (variant == null || string.IsNullOrWhiteSpace(variant.text))
                        continue;

                    int value = profile?.GetStat(variant.stat) ?? 0;
                    if (value >= variant.minInclusive && value <= variant.maxInclusive)
                        matching.Add(variant);
                }
            }

            string selected = node.body ?? string.Empty;
            if (matching.Count > 0)
            {
                int index = PositiveModulo(sessionSeed + StableHash(node.id), matching.Count);
                selected = matching[index].text;
            }

            return ExpandTokens(selected, profile);
        }

        private static string ExpandTokens(string text, PlayerProfile profile)
        {
            string result = text ?? string.Empty;
            result = result.Replace("{player}", "Alex");

            if (profile == null)
                return result;

            result = result.Replace("{riskMood}", profile.risk >= 60 ? "nekat" : "berhati-hati");
            result = result.Replace("{trustMood}", profile.TrustScore >= 55 ? "mulai percaya" : "masih menjaga jarak");
            return result;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                if (value == null)
                    return hash;

                foreach (char character in value)
                    hash = hash * 31 + character;

                return hash;
            }
        }

        private static int PositiveModulo(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
