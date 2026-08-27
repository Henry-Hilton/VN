using System;
using System.IO;
using UnityEngine;

namespace YouthRise
{
    [Serializable]
    internal sealed class TelemetryEvent
    {
        public string eventType;
        public string utc;
        public string sessionId;
        public string chapterId;
        public string nodeId;
        public string choiceId;
        public string choiceLabel;
        public string tendency;
        public string branchPath;
        public float decisionLatencySeconds;
        public MetricSnapshot metrics;
        public string category;
        public string urgency;
    }

    /// <summary>
    /// Writes pseudonymous, local-only JSON Lines. No name, account id, device id, chat text,
    /// or whistleblower narrative is included in behavioral telemetry.
    /// </summary>
    public sealed class DecisionTelemetry
    {
        private readonly string sessionId;
        private readonly string chapterId;
        private readonly string outputPath;

        public DecisionTelemetry(string chapter)
        {
            chapterId = chapter;
            sessionId = Guid.NewGuid().ToString("N");
            string directory = Path.Combine(Application.persistentDataPath, "YouthRise", "Telemetry");
            outputPath = Path.Combine(directory, $"{chapter}_{DateTime.UtcNow:yyyyMMdd-HHmmss}.jsonl");

            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"YouthRise could not create telemetry directory: {exception.Message}");
            }
        }

        public void RecordSessionStarted(PlayerProfile profile)
        {
            Write(new TelemetryEvent
            {
                eventType = "session_started",
                metrics = profile?.Snapshot()
            });
        }

        public void RecordDecision(
            StoryNode node,
            StoryChoice choice,
            float latencySeconds,
            string branchPath,
            PlayerProfile profile)
        {
            Write(new TelemetryEvent
            {
                eventType = "decision",
                nodeId = node?.id,
                choiceId = choice?.id,
                choiceLabel = choice?.label,
                tendency = choice?.tendency,
                branchPath = branchPath,
                decisionLatencySeconds = Mathf.Max(0f, latencySeconds),
                metrics = profile?.Snapshot()
            });
        }

        public void RecordChapterCompleted(string branchPath, PlayerProfile profile)
        {
            Write(new TelemetryEvent
            {
                eventType = "chapter_completed",
                branchPath = branchPath,
                metrics = profile?.Snapshot()
            });
        }

        public void RecordSafeZoneOpened(PlayerProfile profile)
        {
            Write(new TelemetryEvent
            {
                eventType = "safe_zone_opened",
                metrics = profile?.Snapshot()
            });
        }

        public void RecordReportDraft(string category, string urgency)
        {
            Write(new TelemetryEvent
            {
                eventType = "local_report_draft_saved",
                category = category,
                urgency = urgency
            });
        }

        private void Write(TelemetryEvent entry)
        {
            try
            {
                entry.utc = DateTime.UtcNow.ToString("O");
                entry.sessionId = sessionId;
                entry.chapterId = chapterId;
                File.AppendAllText(outputPath, JsonUtility.ToJson(entry) + Environment.NewLine);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"YouthRise telemetry write failed: {exception.Message}");
            }
        }
    }
}
