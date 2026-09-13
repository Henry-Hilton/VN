using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouthRise
{
    [Serializable]
    public sealed class RecordedChoice
    {
        public int chapter;
        public string nodeId;
        public string choiceId;
    }

    public sealed class IssueSummary
    {
        public string title;
        public int observed, total, supportive, pressure, mixed;
    }

    /// <summary>Describes authored choices, never diagnoses the player or infers real events.</summary>
    public static class PlayerJourneyReport
    {
        public static readonly string[] Issues = {
            "Tekanan teman & zat berisiko", "Bullying & cyberbullying", "Hubungan & keamanan pribadi",
            "Kesejahteraan emosional", "Keamanan finansial", "Gaya hidup sehat",
            "Keseimbangan digital", "Komunikasi & dukungan keluarga"
        };
        private static readonly HashSet<string> PressureStats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "risk", "anxiety", "fomoIndex", "socialMediaDependencyTendency" };

        public static void BeginChapter(PlayerProfile profile, int chapter)
        {
            if (profile == null) return;
            profile.recordedChoices ??= new List<RecordedChoice>();
            profile.recordedChoices.RemoveAll(c => c == null || c.chapter == chapter);
        }

        public static void Record(PlayerProfile profile, int chapter, StoryNode node, StoryChoice choice)
        {
            if (profile == null || node == null || choice == null || chapter < 1 || chapter > 8) return;
            profile.recordedChoices ??= new List<RecordedChoice>();
            profile.recordedChoices.RemoveAll(c => c == null || (c.chapter == chapter && c.nodeId == node.id));
            profile.recordedChoices.Add(new RecordedChoice { chapter = chapter, nodeId = node.id, choiceId = choice.id });
        }

        public static IssueSummary[] Summarize(PlayerProfile profile)
        {
            var result = new List<IssueSummary>();
            for (int chapter = 1; chapter <= 8; chapter++)
            {
                StoryGraph graph = StoryRepository.LoadById("chapter-" + chapter);
                var row = new IssueSummary { title = Issues[chapter - 1],
                    total = graph.Chapter.nodes.Count(n => n.choices != null && n.choices.Length > 0) };
                var unique = new HashSet<string>();
                foreach (RecordedChoice recorded in profile?.recordedChoices ?? new List<RecordedChoice>())
                {
                    if (recorded == null || recorded.chapter != chapter || !unique.Add(recorded.nodeId ?? "")) continue;
                    StoryChoice choice = graph.Get(recorded.nodeId)?.choices?.FirstOrDefault(c => c.id == recorded.choiceId);
                    if (choice == null) continue;
                    row.observed++;
                    bool protective = false, pressure = false;
                    foreach (StatDelta delta in choice.effects ?? new StatDelta[0])
                    {
                        if (delta == null || delta.amount == 0 || delta.stat == "xp") continue;
                        bool positive = PressureStats.Contains(delta.stat) ? delta.amount < 0 : delta.amount > 0;
                        protective |= positive;
                        pressure |= !positive;
                    }
                    if (protective && !pressure) row.supportive++;
                    else if (pressure && !protective) row.pressure++;
                    else row.mixed++;
                }
                result.Add(row);
            }
            return result.ToArray();
        }

        public static string CreateText(PlayerProfile profile)
        {
            var text = new StringBuilder("RINGKASAN PILIHAN GAME — 8 ISU\n");
            text.AppendLine("Bukan diagnosis, bukti kejadian nyata, atau penilaian kepribadian. Jangan digunakan untuk menghukum atau memberi label siswa.");
            text.AppendLine("Rubrik prototipe: dukungan = hanya delta protektif; tekanan = hanya delta tekanan; campuran/netral = keduanya atau tanpa delta. Belum tervalidasi.\n");
            IssueSummary[] rows = Summarize(profile);
            for (int i = 0; i < rows.Length; i++)
            {
                IssueSummary r = rows[i];
                text.AppendLine($"{i + 1}. {r.title}");
                text.AppendLine(r.observed == 0 ? "   Belum ada data pilihan (termasuk save lama)."
                    : $"   {r.observed}/{r.total} keputusan: dukungan {r.supportive}, tekanan {r.pressure}, campuran/netral {r.mixed}.");
            }
            text.Append("\nHanya pilihan yang direkam sejak pembaruan ini; chapter yang diulang mengganti data chapter tersebut. Percabangan dapat melewati keputusan. Bahan percakapan dengan Guru BK, bukan kesimpulan tentang kehidupan pemain. Tidak memuat chat, identitas, atau teks pengaduan.");
            return text.ToString();
        }
    }
}
