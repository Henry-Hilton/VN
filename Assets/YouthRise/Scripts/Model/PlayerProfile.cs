using System;
using UnityEngine;

namespace YouthRise
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public int risk;
        public int trustParent;
        public int trustFriend;
        public int trustMaya;
        public int trustRina;
        public int trustLeo;
        public int trustTeacher;
        public int confidence;
        public int empathy;
        public int knowledge;
        public int socialSupport;
        public int anxiety;
        public int bystanderResponse;
        public int xp;
        public bool safeZoneUnlocked;
        public bool completedChapterOne;
        public bool completedChapterTwo;
        public bool relationshipPathUnlocked;
        public bool bullyingSupportArticleUnlocked;

        public int TrustScore => Mathf.Clamp(
            50 + trustParent + trustFriend + trustMaya + trustRina + trustLeo + trustTeacher,
            0,
            100);

        public void ResetForChapterOne()
        {
            risk = 30;
            trustParent = 0;
            trustFriend = 0;
            trustMaya = 0;
            trustRina = 0;
            trustLeo = 0;
            trustTeacher = 0;
            confidence = 50;
            empathy = 50;
            knowledge = 50;
            socialSupport = 50;
            anxiety = 20;
            bystanderResponse = 50;
            xp = 0;
            safeZoneUnlocked = false;
            completedChapterOne = false;
            completedChapterTwo = false;
            relationshipPathUnlocked = false;
            bullyingSupportArticleUnlocked = false;
        }

        public void PrepareForChapterTwo()
        {
            bystanderResponse = 50;
        }

        public void Apply(StatDelta[] effects)
        {
            if (effects == null)
                return;

            foreach (StatDelta effect in effects)
            {
                if (effect == null || string.IsNullOrWhiteSpace(effect.stat))
                    continue;

                Apply(effect.stat, effect.amount);
            }
        }

        public void Apply(string stat, int amount)
        {
            switch (stat.Trim().ToLowerInvariant())
            {
                case "risk":
                    risk = Mathf.Clamp(risk + amount, 0, 100);
                    break;
                case "trustparent":
                    trustParent = Mathf.Clamp(trustParent + amount, -50, 50);
                    break;
                case "trustfriend":
                    trustFriend = Mathf.Clamp(trustFriend + amount, -50, 50);
                    break;
                case "trustmaya":
                    trustMaya = Mathf.Clamp(trustMaya + amount, -50, 50);
                    break;
                case "trustrina":
                    trustRina = Mathf.Clamp(trustRina + amount, -50, 50);
                    break;
                case "trustleo":
                    trustLeo = Mathf.Clamp(trustLeo + amount, -50, 50);
                    break;
                case "trustteacher":
                    trustTeacher = Mathf.Clamp(trustTeacher + amount, -50, 50);
                    break;
                case "confidence":
                    confidence = Mathf.Clamp(confidence + amount, 0, 100);
                    break;
                case "empathy":
                    empathy = Mathf.Clamp(empathy + amount, 0, 100);
                    break;
                case "knowledge":
                    knowledge = Mathf.Clamp(knowledge + amount, 0, 100);
                    break;
                case "socialsupport":
                    socialSupport = Mathf.Clamp(socialSupport + amount, 0, 100);
                    break;
                case "anxiety":
                    anxiety = Mathf.Clamp(anxiety + amount, 0, 100);
                    break;
                case "bystander":
                case "bystanderresponse":
                    bystanderResponse = Mathf.Clamp(bystanderResponse + amount, 0, 100);
                    break;
                case "xp":
                    xp = Mathf.Max(0, xp + amount);
                    break;
                default:
                    Debug.LogWarning($"YouthRise ignored unknown stat '{stat}'.");
                    break;
            }
        }

        public int GetStat(string stat)
        {
            if (string.IsNullOrWhiteSpace(stat))
                return 0;

            switch (stat.Trim().ToLowerInvariant())
            {
                case "risk": return risk;
                case "trust": return TrustScore;
                case "trustparent": return trustParent;
                case "trustfriend": return trustFriend;
                case "trustmaya": return trustMaya;
                case "trustrina": return trustRina;
                case "trustleo": return trustLeo;
                case "trustteacher": return trustTeacher;
                case "confidence": return confidence;
                case "empathy": return empathy;
                case "knowledge": return knowledge;
                case "socialsupport": return socialSupport;
                case "anxiety": return anxiety;
                case "bystander": return bystanderResponse;
                case "bystanderresponse": return bystanderResponse;
                case "xp": return xp;
                default: return 0;
            }
        }

        public MetricSnapshot Snapshot()
        {
            return new MetricSnapshot
            {
                risk = risk,
                trust = TrustScore,
                confidence = confidence,
                empathy = empathy,
                knowledge = knowledge,
                socialSupport = socialSupport,
                anxiety = anxiety,
                bystanderResponse = bystanderResponse
            };
        }
    }

    [Serializable]
    public sealed class MetricSnapshot
    {
        public int risk;
        public int trust;
        public int confidence;
        public int empathy;
        public int knowledge;
        public int socialSupport;
        public int anxiety;
        public int bystanderResponse;
    }
}
