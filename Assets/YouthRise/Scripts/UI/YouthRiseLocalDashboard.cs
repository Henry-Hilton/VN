using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YouthRise
{
    public sealed partial class YouthRisePrototype
    {
        private GameObject localDashboardScreen;
        private Text dashboardText, dashboardStatus;
        private InputField dashboardPassword, dashboardReportId;
        private string dashboardToken;
        private void BuildLocalDashboard(RectTransform canvas)
        {
            localDashboardScreen = CreateRect("Local Dashboard", canvas, Vector2.zero, Vector2.one); AddImage(localDashboardScreen, Paper);
            AddText(CreateRect("Title", localDashboardScreen.transform, new Vector2(.08f,.87f), new Vector2(.92f,.97f)), "DASHBOARD LOKAL", 40, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            dashboardPassword = CreateInputField(localDashboardScreen.transform, "Dashboard Password", "Password pengelola lokal (minimal 12 karakter)", new Vector2(.08f,.77f), new Vector2(.66f,.85f), false);
            dashboardPassword.contentType = InputField.ContentType.Password; dashboardPassword.characterLimit = 128;
            CreateButton(localDashboardScreen.transform, "Open Dashboard", "BUKA / PERBARUI", new Vector2(.69f,.77f), new Vector2(.92f,.85f), Blue, White, 20).onClick.AddListener(RefreshLocalDashboard);
            dashboardStatus = AddText(CreateRect("Status", localDashboardScreen.transform, new Vector2(.08f,.66f), new Vector2(.92f,.76f)), "", 21, Navy, TextAnchor.MiddleLeft);
            dashboardText = CreateScrollableText(localDashboardScreen.transform, new Vector2(.08f,.25f), new Vector2(.92f,.65f));
            dashboardReportId = CreateInputField(localDashboardScreen.transform, "Report ID", "ID laporan dari daftar di atas", new Vector2(.08f,.15f), new Vector2(.48f,.23f), false);
            CreateButton(localDashboardScreen.transform, "Review Report", "DITINJAU", new Vector2(.50f,.15f), new Vector2(.63f,.23f), Mint, Navy, 18).onClick.AddListener(() => ReviewLocalReport("reviewed"));
            CreateButton(localDashboardScreen.transform, "Close Report", "TUTUP", new Vector2(.65f,.15f), new Vector2(.77f,.23f), Cyan, Navy, 18).onClick.AddListener(() => ReviewLocalReport("closed"));
            CreateButton(localDashboardScreen.transform, "Reopen Report", "BUKA LAGI", new Vector2(.79f,.15f), new Vector2(.92f,.23f), Mint, Navy, 18).onClick.AddListener(() => ReviewLocalReport("new"));
            CreateButton(localDashboardScreen.transform, "Dashboard Back", "KEMBALI & KUNCI", new Vector2(.08f,.04f), new Vector2(.48f,.12f), Blue, White, 20).onClick.AddListener(() => {
                LocalPlayerPortal.Logout(dashboardToken); dashboardToken = null; dashboardText.text = ""; dashboardPassword.text = ""; dashboardReportId.text = ""; ShowStartMenu();
            });
            CreateButton(accountScreen.transform, "Local Dashboard Entry", "DASHBOARD LOKAL", new Vector2(.05f,.02f), new Vector2(.38f,.09f), Mint, Navy, 19).onClick.AddListener(ShowLocalDashboard);
            CreateButton(startScreen.transform, "Local Dashboard Entry", "DASHBOARD", new Vector2(.065f,.875f), new Vector2(.25f,.918f), Mint, Navy, 17).onClick.AddListener(ShowLocalDashboard);
        }
        private void ShowLocalDashboard()
        {
            dashboardText.text = ""; dashboardPassword.text = "";
            dashboardStatus.text = LocalPlayerPortal.HasCounselorPassword ? "Masukkan password pengelola perangkat ini. Data tersimpan di PlayerPrefs lokal."
                : "Penggunaan pertama: tetapkan password pengelola (12+ karakter). Dashboard ini hanya untuk perangkat ini.";
            ShowScreenSmooth(localDashboardScreen);
        }
        private void RefreshLocalDashboard()
        {
            try
            {
                if (dashboardToken == null) dashboardToken = LocalPlayerPortal.OpenDashboard(dashboardPassword.text);
                dashboardPassword.text = "";
                if (dashboardToken == null) { dashboardStatus.text = "Password salah atau kurang dari 12 karakter saat pengaturan pertama."; return; }
                var data = LocalPlayerPortal.Dashboard(dashboardToken);
                var text = new StringBuilder();
                text.AppendLine($"{data.players.Count} PEMAIN  •  {data.reports.Count(r => r.status == "new")} LAPORAN BARU\n");
                var totals = new int[8, 4];
                foreach (var player in data.players)
                {
                    text.AppendLine($"{player.user.name} (@{player.user.nickname}) • {player.user.age} tahun • {player.user.CharacterName}");
                    text.AppendLine(player.results == null ? "Belum ada hasil pilihan.\n" : PlayerJourneyReport.CreateText(player.results) + "\n");
                    if (player.results == null) continue;
                    var rows = PlayerJourneyReport.Summarize(player.results);
                    for (int i = 0; i < rows.Length; i++) { totals[i,0] += rows[i].observed; totals[i,1] += rows[i].supportive; totals[i,2] += rows[i].pressure; totals[i,3] += rows[i].mixed; }
                }
                text.AppendLine("TREN PILIHAN SEMUA PEMAIN (BUKAN DIAGNOSIS)");
                for (int i = 0; i < 8; i++) text.AppendLine($"{PlayerJourneyReport.Issues[i]}: {totals[i,0]} keputusan; dukungan {totals[i,1]}, tekanan {totals[i,2]}, campuran {totals[i,3]}.");
                text.AppendLine("\nLAPORAN TERKONFIRMASI");
                foreach (var report in data.reports.AsEnumerable().Reverse())
                {
                    var player = data.players.FirstOrDefault(p => p.user.id == report.userId);
                    text.AppendLine($"\n{report.id}\n{player?.user.nickname ?? "Akun tidak tersedia"} • {report.createdUtc} • {report.status}\n{report.text}");
                }
                if (data.reports.Count == 0) text.AppendLine("Belum ada laporan.");
                dashboardText.text = text.ToString();
                dashboardStatus.text = "Data perangkat diperbarui. Chat biasa tidak disimpan. Salin ID laporan untuk mengubah status.";
            }
            catch { dashboardStatus.text = "Dashboard lokal tidak dapat dibaca. Periksa penyimpanan perangkat."; }
        }
        private void ReviewLocalReport(string status)
        {
            try { LocalPlayerPortal.Review(dashboardToken, dashboardReportId.text.Trim(), status); RefreshLocalDashboard(); }
            catch { dashboardStatus.text = "Masuk sebagai pengelola dan masukkan ID laporan yang valid."; }
        }
    }
}
