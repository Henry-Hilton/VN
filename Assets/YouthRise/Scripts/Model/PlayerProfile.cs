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
        public int trustSarah;
        public int trustTeacher;
        public int confidence;
        public int empathy;
        public int knowledge;
        public int socialSupport;
        public int anxiety;
        public int bystanderResponse;
        public int relationshipAwareness;
        public int digitalSafetyAwareness;
        public int boundaryAwareness;
        public int helpSeekingTendency;
        public int emotionalAwareness;
        public int copingTendency;
        public int resilienceIndicator;
        public int financialAwareness;
        public int spendingControl;
        public int impulseControl;
        public int scamAwareness;
        public int xp;
        public bool safeZoneUnlocked;
        public bool completedChapterOne;
        public bool completedChapterTwo;
        public bool completedChapterThree;
        public bool completedChapterFour;
        public bool completedChapterFive;
        public bool seasonOneCompleted;
        public bool relationshipPathUnlocked;
        public bool bullyingSupportArticleUnlocked;
        public bool healthyRelationshipArticleUnlocked;
        public bool digitalSafetyGuideUnlocked;
        public bool financialSafetyArticleUnlocked;
        public bool moneySmartGuideUnlocked;

        public int TrustScore => Mathf.Clamp(
            50 + trustParent + trustFriend + trustMaya + trustRina + trustLeo + trustSarah + trustTeacher,
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
            trustSarah = 0;
            trustTeacher = 0;
            confidence = 50;
            empathy = 50;
            knowledge = 50;
            socialSupport = 50;
            anxiety = 20;
            bystanderResponse = 50;
            relationshipAwareness = 50;
            digitalSafetyAwareness = 50;
            boundaryAwareness = 50;
            helpSeekingTendency = 50;
            emotionalAwareness = 50;
            copingTendency = 50;
            resilienceIndicator = 50;
            PrepareForChapterFive();
            xp = 0;
            safeZoneUnlocked = false;
            completedChapterOne = false;
            completedChapterTwo = false;
            completedChapterThree = false;
            completedChapterFour = false;
            completedChapterFive = false;
            seasonOneCompleted = false;
            relationshipPathUnlocked = false;
            bullyingSupportArticleUnlocked = false;
            healthyRelationshipArticleUnlocked = false;
            digitalSafetyGuideUnlocked = false;
            financialSafetyArticleUnlocked = false;
            moneySmartGuideUnlocked = false;
        }

        public void PrepareForChapterTwo()
        {
            bystanderResponse = 50;
        }

        public void PrepareForChapterThree()
        {
            relationshipAwareness = 50;
            digitalSafetyAwareness = 50;
            boundaryAwareness = 50;
            helpSeekingTendency = 50;
        }

        public void PrepareForChapterFour()
        {
            emotionalAwareness = 50;
            copingTendency = 50;
            helpSeekingTendency = 50;
            resilienceIndicator = 50;
        }

        public void PrepareForChapterFive()
        {
            // Initialize only the new chapter indicators, including on legacy saves.
            // Existing support, trust, and help-seeking progress carries forward.
            financialAwareness = 50;
            spendingControl = 50;
            impulseControl = 50;
            scamAwareness = 50;
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
                case "trustsarah":
                    trustSarah = Mathf.Clamp(trustSarah + amount, -50, 50);
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
                case "relationshipawareness":
                    relationshipAwareness = Mathf.Clamp(relationshipAwareness + amount, 0, 100);
                    break;
                case "digitalsafety":
                case "digitalsafetyawareness":
                    digitalSafetyAwareness = Mathf.Clamp(digitalSafetyAwareness + amount, 0, 100);
                    break;
                case "boundary":
                case "boundaryawareness":
                    boundaryAwareness = Mathf.Clamp(boundaryAwareness + amount, 0, 100);
                    break;
                case "helpseeking":
                case "helpseekingtendency":
                    helpSeekingTendency = Mathf.Clamp(helpSeekingTendency + amount, 0, 100);
                    break;
                case "emotionalawareness":
                    emotionalAwareness = Mathf.Clamp(emotionalAwareness + amount, 0, 100);
                    break;
                case "coping":
                case "copingtendency":
                    copingTendency = Mathf.Clamp(copingTendency + amount, 0, 100);
                    break;
                case "resilience":
                case "resilienceindicator":
                    resilienceIndicator = Mathf.Clamp(resilienceIndicator + amount, 0, 100);
                    break;
                case "financialawareness":
                    financialAwareness = Mathf.Clamp(financialAwareness + amount, 0, 100);
                    break;
                case "spendingcontrol":
                    spendingControl = Mathf.Clamp(spendingControl + amount, 0, 100);
                    break;
                case "impulsecontrol":
                    impulseControl = Mathf.Clamp(impulseControl + amount, 0, 100);
                    break;
                case "scamawareness":
                    scamAwareness = Mathf.Clamp(scamAwareness + amount, 0, 100);
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
                case "trustsarah": return trustSarah;
                case "trustteacher": return trustTeacher;
                case "confidence": return confidence;
                case "empathy": return empathy;
                case "knowledge": return knowledge;
                case "socialsupport": return socialSupport;
                case "anxiety": return anxiety;
                case "bystander": return bystanderResponse;
                case "bystanderresponse": return bystanderResponse;
                case "relationshipawareness": return relationshipAwareness;
                case "digitalsafety": return digitalSafetyAwareness;
                case "digitalsafetyawareness": return digitalSafetyAwareness;
                case "boundary": return boundaryAwareness;
                case "boundaryawareness": return boundaryAwareness;
                case "helpseeking": return helpSeekingTendency;
                case "helpseekingtendency": return helpSeekingTendency;
                case "emotionalawareness": return emotionalAwareness;
                case "coping": return copingTendency;
                case "copingtendency": return copingTendency;
                case "resilience": return resilienceIndicator;
                case "resilienceindicator": return resilienceIndicator;
                case "xp": return xp;
                case "financialawareness": return financialAwareness;
                case "spendingcontrol": return spendingControl;
                case "impulsecontrol": return impulseControl;
                case "scamawareness": return scamAwareness;
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
                bystanderResponse = bystanderResponse,
                relationshipAwareness = relationshipAwareness,
                digitalSafetyAwareness = digitalSafetyAwareness,
                boundaryAwareness = boundaryAwareness,
                helpSeekingTendency = helpSeekingTendency,
                emotionalAwareness = emotionalAwareness,
                copingTendency = copingTendency,
                resilienceIndicator = resilienceIndicator,
                financialAwareness = financialAwareness,
                spendingControl = spendingControl,
                impulseControl = impulseControl,
                scamAwareness = scamAwareness
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
        public int relationshipAwareness;
        public int digitalSafetyAwareness;
        public int boundaryAwareness;
        public int helpSeekingTendency;
        public int emotionalAwareness;
        public int copingTendency;
        public int resilienceIndicator;
        public int financialAwareness;
        public int spendingControl;
        public int impulseControl;
        public int scamAwareness;
    }
}
