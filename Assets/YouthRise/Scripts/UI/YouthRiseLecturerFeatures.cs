using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YouthRise
{
    public sealed partial class YouthRisePrototype
    {
        private StoryAudio storyAudio;
        private OnlineSupportSettings onlineSettings;
        private GameObject chapterReviewScreen, sharingScreen, connectionScreen;
        private Text reviewHeading, reviewDetails, previewText, sharingStatus, connectionStatus;
        private Toggle onlineChatConsent, sharingConsent;
        private InputField accessCode;
        private Button chatSendButton, shareButton, localExportButton, sharingBack;
        private readonly List<Text> musicLabels = new List<Text>(), voiceLabels = new List<Text>();
        private readonly List<SupportMessage> chatHistory = new List<SupportMessage>();
        private bool chatBusy, sharingBusy, previewAttempted;
        private int chatEpoch;
        private SupportReportRequest previewRequest;

        private void BuildAudioControls(Transform parent, bool menu = false)
        {
            Vector2 min = menu ? new Vector2(.65f,.925f) : new Vector2(.55f,.12f);
            Vector2 max = menu ? new Vector2(.79f,.975f) : new Vector2(.69f,.88f);
            Button music = CreateButton(parent, menu ? "Menu Music" : "Music", "", min, max, Mint, Navy, 17);
            musicLabels.Add(music.GetComponentInChildren<Text>());
            music.onClick.AddListener(() => { storyAudio.ToggleMusic(); RefreshAudioLabels(); });
            min.x = menu ? .80f : .71f; max.x = menu ? .94f : .86f;
            Button voice = CreateButton(parent, menu ? "Menu Voice" : "Voice", "", min, max, Cyan, Navy, 17);
            voiceLabels.Add(voice.GetComponentInChildren<Text>());
            voice.onClick.AddListener(() => {
                storyAudio.ToggleVoice(); RefreshAudioLabels();
                if (storyAudio.VoiceEnabled) { NarrateCurrentNode(); ShowToast(storyAudio.VoiceStatus, false); }
            });
            if (!menu) CreateButton(parent, "Repeat Narration", "ULANG SUARA", new Vector2(.88f,.12f), new Vector2(.985f,.88f), Paper, Navy, 15).onClick.AddListener(NarrateCurrentNode);
        }
        private void RefreshAudioLabels()
        {
            foreach (Text label in musicLabels) label.text = storyAudio.MusicEnabled ? "MUSIK: ON" : "MUSIK: OFF";
            foreach (Text label in voiceLabels) label.text = storyAudio.VoiceEnabled ? "NARASI: ON" : "NARASI: OFF";
        }
        private void NarrateCurrentNode()
        {
            if (currentNode == null || !storyScreen.activeInHierarchy || storyTransition != null) return;
            string body = dialogueText.text;
            using (SHA256 hash = SHA256.Create())
            {
                string key = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(body))).Replace("-", "").Substring(0,12).ToLowerInvariant();
                storyAudio.PlayAuthoredNarration($"YouthRise/Audio/Narration/{story.Chapter.id}/{currentNode.id}-{key}", body);
            }
        }
        private void BuildLecturerScreens(RectTransform canvas)
        {
            BuildAudioControls(startScreen.transform, true); RefreshAudioLabels();
            chapterReviewScreen = CreateRect("Chapter Review Screen", canvas, Vector2.zero, Vector2.one);
            AddImage(chapterReviewScreen, Navy);
            reviewHeading = AddText(CreateRect("Heading", chapterReviewScreen.transform, new Vector2(.13f,.76f), new Vector2(.87f,.92f)), "", 48, White, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(CreateRect("Caution", chapterReviewScreen.transform, new Vector2(.17f,.64f), new Vector2(.83f,.75f)), "Indikator pilihan Alex dalam game, bukan diagnosis atau nilai dirimu.\nNilai dibawa dari chapter sebelumnya; tidak menentukan baik/buruknya seseorang.", 23, Paper, TextAnchor.MiddleCenter);
            CreateMeter(chapterReviewScreen.transform, "Risk", "RISK", new Vector2(.20f,.52f), new Vector2(.80f,.61f), Coral, out riskFill, out riskValue);
            CreateMeter(chapterReviewScreen.transform, "Trust", "TRUST", new Vector2(.20f,.40f), new Vector2(.80f,.49f), Cyan, out trustFill, out trustValue);
            reviewDetails = AddText(CreateRect("Details", chapterReviewScreen.transform, new Vector2(.20f,.26f), new Vector2(.80f,.38f)), "", 25, Paper, TextAnchor.MiddleCenter);
            CreateButton(chapterReviewScreen.transform, "Review Reflection", "LANJUT KE REFLECTION →", new Vector2(.30f,.12f), new Vector2(.70f,.22f), Mint, Navy, 23).onClick.AddListener(() => ShowCompletion(false));

            connectionScreen = CreateRect("Connection Screen", canvas, Vector2.zero, Vector2.one);
            AddImage(connectionScreen, Paper);
            AddText(CreateRect("Heading", connectionScreen.transform, new Vector2(.10f,.81f), new Vector2(.90f,.94f)), "KONEKSI & PERSETUJUAN", 42, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            connectionStatus = AddText(CreateRect("Status", connectionScreen.transform, new Vector2(.10f,.56f), new Vector2(.90f,.80f)), "", 25, Navy, TextAnchor.MiddleLeft);
            accessCode = CreateInputField(connectionScreen.transform, "Session Access Code", "Kode akses sesi dari pengelola (bukan API key)", new Vector2(.10f,.46f), new Vector2(.90f,.54f), false);
            accessCode.contentType = InputField.ContentType.Password; accessCode.characterLimit = 128;
            onlineChatConsent = CreateConsent(connectionScreen.transform, "Online Chat Consent", "Saya setuju mengirim pesan chat dan maksimal 6 pesan konteks sesi ke server serta penyedia AI. Jangan tulis identitas. Bot bukan konselor manusia; dukungan nyata tetap diperlukan.", .26f,.43f);
            onlineChatConsent.onValueChanged.AddListener(consented => {
                if (consented) return;
                // In-flight data cannot be recalled; ignore its reply and do not reuse context.
                chatEpoch++; chatHistory.Clear(); chatBusy = false;
                SetButtonEnabled(chatSendButton, true);
                chatResponse.text = "Persetujuan online dinonaktifkan. Chat berikutnya memakai respons lokal.";
            });
            CreateButton(connectionScreen.transform, "Connection Back", "KEMBALI KE SAFE ZONE", new Vector2(.10f,.10f), new Vector2(.60f,.20f), Blue, White, 23).onClick.AddListener(ShowSafeZone);
            CreateButton(connectionScreen.transform, "Clear Online Session", "HAPUS SESI", new Vector2(.64f,.10f), new Vector2(.90f,.20f), Coral, White, 22).onClick.AddListener(ClearSupportSession);

            sharingScreen = CreateRect("Sharing Preview Screen", canvas, Vector2.zero, Vector2.one);
            AddImage(sharingScreen, Paper);
            AddText(CreateRect("Heading", sharingScreen.transform, new Vector2(.08f,.86f), new Vector2(.92f,.96f)), "TINJAU SEBELUM BERBAGI", 38, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            previewText = CreateScrollableText(sharingScreen.transform, new Vector2(.08f,.36f), new Vector2(.92f,.85f));
            sharingStatus = AddText(CreateRect("Share Status", sharingScreen.transform, new Vector2(.08f,.24f), new Vector2(.92f,.35f)), "", 22, Navy, TextAnchor.MiddleLeft);
            sharingConsent = CreateConsent(sharingScreen.transform, "Sharing Consent", "Saya sudah meninjau teks dan setuju mengirimnya ke WhatsApp Guru BK melalui server. Pesan dapat tersimpan pada server/WhatsApp/penerima; tidak dapat ditarik kembali dari game.", .13f,.23f);
            localExportButton = CreateButton(sharingScreen.transform, "Export Preview", "SIMPAN LOKAL", new Vector2(.08f,.035f), new Vector2(.33f,.115f), Cyan, Navy, 20);
            localExportButton.onClick.AddListener(ExportPreview);
            shareButton = CreateButton(sharingScreen.transform, "Submit WhatsApp", "KIRIM KE WHATSAPP", new Vector2(.35f,.035f), new Vector2(.69f,.115f), Blue, White, 20);
            shareButton.onClick.AddListener(SubmitPreview);
            sharingBack = CreateButton(sharingScreen.transform, "Sharing Back", "KEMBALI", new Vector2(.71f,.035f), new Vector2(.92f,.115f), Coral, White, 20);
            sharingBack.onClick.AddListener(() => { ShowSafeZone(); ShowSafeTab("report"); });
            sharingConsent.onValueChanged.AddListener(_ => UpdateShareEnabled());
        }
        private Toggle CreateConsent(Transform parent, string name, string text, float bottom, float top)
        {
            GameObject root = CreateRect(name, parent, new Vector2(.10f,bottom), new Vector2(.90f,top));
            var toggle = root.AddComponent<Toggle>();
            AddImage(root, Color.clear);
            GameObject box = CreateRect("Box", root.transform, new Vector2(0,.32f), new Vector2(.035f,.68f));
            toggle.targetGraphic = AddImage(box, Mint);
            toggle.graphic = AddImage(CreateRect("Check", box.transform, new Vector2(.2f,.2f), new Vector2(.8f,.8f)), Navy);
            AddText(CreateRect("Label", root.transform, new Vector2(.05f,0), Vector2.one), text, 21, Ink, TextAnchor.MiddleLeft).raycastTarget = false;
            toggle.isOn = false; return toggle;
        }
        private Text CreateScrollableText(Transform parent, Vector2 min, Vector2 max)
        {
            GameObject viewport = CreateRect("Report Scroll", parent, min, max);
            AddImage(viewport, new Color(Mint.r, Mint.g, Mint.b, .45f)); viewport.AddComponent<RectMask2D>();
            ScrollRect scroll = viewport.AddComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            GameObject content = CreateRect("Report Text", viewport.transform, new Vector2(.025f,1), new Vector2(.975f,1));
            RectTransform rect = content.GetComponent<RectTransform>(); rect.pivot = new Vector2(.5f,1);
            Text text = AddText(content, "", 23, Ink, TextAnchor.UpperLeft); text.supportRichText = false; text.raycastTarget = false;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = rect; return text;
        }
        private void ShowChapterReview()
        {
            storyAudio.StopNarration(); HideToast();
            reviewHeading.text = $"CHAPTER {story.Chapter.number} SELESAI\nSEBELUM REFLECTION";
            reviewDetails.text = $"Confidence {profile.confidence}   •   Empathy {profile.empathy}\nSocial Support {profile.socialSupport}   •   Anxiety {profile.anxiety}";
            ResetMeterAnimation(); displayedRisk = displayedTrust = 0; metersInitialized = true;
            ApplyMeterVisuals(); ShowScreenSmooth(chapterReviewScreen); UpdateMeters();
        }
        private void ShowConnectionSettings()
        {
            connectionStatus.text = (onlineSettings.IsConfigured ? $"Server: {onlineSettings.baseUrl}\nTujuan laporan: {onlineSettings.recipientLabel}" : "Mode lokal. Backend belum dikonfigurasi; chat dan laporan tidak dikirim.") + "\n\nKode akses hanya berada dalam memori sesi. API key AI/WhatsApp tidak boleh dimasukkan ke game. Untuk demo terawasi; bukan pengganti bantuan profesional.";
            ShowScreenSmooth(connectionScreen);
        }
        private void SendSupportChat()
        {
            if (chatBusy || string.IsNullOrWhiteSpace(chatInput.text)) return;
            string input = chatInput.text.Trim(); SafeZoneAssessment assessment = safeZoneAssistant.Assess(input);
            if (!onlineSettings.IsConfigured || !onlineChatConsent.isOn || string.IsNullOrWhiteSpace(accessCode.text) || assessment.immediateSafetyConcern)
            { chatResponse.text = "[PENDAMPING LOKAL — tidak dikirim]\n" + safeZoneAssistant.CreateChatResponse(input); chatInput.text = ""; return; }
            chatBusy = true; SetButtonEnabled(chatSendButton, false);
            int epoch = chatEpoch;
            chatResponse.text = "Menghubungi bot AI… Jika ada bahaya sekarang, jangan menunggu aplikasi.";
            var context = new List<SupportMessage>(chatHistory) { new SupportMessage { role = "user", content = input } };
            while (context.Count > 6) context.RemoveAt(0);
            StartCoroutine(OnlineSupportClient.Post(onlineSettings, "/chat", accessCode.text, JsonUtility.ToJson(new SupportChatRequest { consent = true, messages = context.ToArray() }), response => {
                if (epoch != chatEpoch) return;
                chatBusy = false; SetButtonEnabled(chatSendButton, true);
                if (response.success && !string.IsNullOrWhiteSpace(response.reply))
                {
                    string answer = response.reply.Length > 900 ? response.reply.Substring(0,900) : response.reply;
                    chatResponse.text = "[BOT AI — bukan konselor manusia]\n" + answer;
                    chatHistory.Clear(); chatHistory.AddRange(context); chatHistory.Add(new SupportMessage { role = "assistant", content = answer });
                    while (chatHistory.Count > 5) chatHistory.RemoveAt(0);
                    chatInput.text = "";
                }
                else chatResponse.text = "[KONEKSI GAGAL — respons lokal]\n" + safeZoneAssistant.CreateChatResponse(input);
            }));
        }
        private void ClearSupportSession()
        {
            chatEpoch++; chatBusy = false; SetButtonEnabled(chatSendButton, true);
            accessCode.text = ""; onlineChatConsent.isOn = false; chatHistory.Clear(); chatInput.text = "";
            chatResponse.text = "Konteks lokal dibersihkan. Data yang sudah dikirim mengikuti kebijakan server/penyedia.";
        }
        private void PreviewSharing(bool journey)
        {
            if (sharingBusy) return;
            if (!journey && string.IsNullOrWhiteSpace(reportInput.text)) { ShowToast("Tuliskan kejadian terlebih dahulu.", true); return; }
            previewAttempted = false;
            previewRequest = new SupportReportRequest { reportId = "YR-" + Guid.NewGuid().ToString("N"), kind = journey ? "journey" : "incident", recipientLabel = onlineSettings.recipientLabel, text = journey ? PlayerJourneyReport.CreateText(profile) : "PENGADUAN SUKARELA PEMAIN\n" + reportInput.text.Trim() };
            previewText.text = previewRequest.text; previewText.rectTransform.anchoredPosition = Vector2.zero; sharingConsent.isOn = false;
            SetButtonEnabled(localExportButton, true);
            sharingStatus.text = "Tujuan: " + onlineSettings.recipientLabel + ". Gulir untuk membaca seluruh teks.\nSimpan lokal tidak terenkripsi. " + (onlineSettings.IsConfigured ? "Pengiriman memerlukan kode akses di Koneksi / Privasi." : "WhatsApp belum terhubung; tidak ada data dikirim.");
            UpdateShareEnabled(); ShowScreenSmooth(sharingScreen);
        }
        private void UpdateShareEnabled() { SetButtonEnabled(shareButton, !sharingBusy && previewRequest != null && sharingConsent.isOn && onlineSettings.IsConfigured && !string.IsNullOrWhiteSpace(accessCode.text)); }
        private void ExportPreview()
        {
            if (previewRequest == null) return;
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "YouthRise", "Reports"); Directory.CreateDirectory(directory);
                string state = previewAttempted ? "SALINAN LOKAL — PENGIRIMAN BELUM TERKONFIRMASI" : "DRAFT LOKAL — BELUM DIKIRIM";
                File.WriteAllText(Path.Combine(directory, previewRequest.reportId + ".txt"), state + "\n" + previewRequest.text);
                sharingStatus.text = "Salinan " + previewRequest.reportId + " tersimpan lokal, tidak terenkripsi.\n" +
                    (previewAttempted ? "Pengiriman belum terkonfirmasi; cek pengelola sebelum mencoba lagi." : "Belum dikirim.");
            }
            catch { sharingStatus.text = "Tidak dapat menyimpan draft lokal."; }
        }
        private void SubmitPreview()
        {
            if (sharingBusy || previewRequest == null || !sharingConsent.isOn || !onlineSettings.IsConfigured || string.IsNullOrWhiteSpace(accessCode.text)) return;
            if (!TryStoreReceipt(new SupportResponse { reportId = previewRequest.reportId, recipientLabel = previewRequest.recipientLabel, status = "unknown" }))
            {
                sharingStatus.text = "Tidak dikirim: catatan ID percobaan tidak dapat disimpan. Periksa izin penyimpanan lokal.";
                return;
            }
            sharingBusy = true; previewAttempted = true; previewRequest.consent = true;
            SetButtonEnabled(localExportButton, false); SetButtonEnabled(sharingBack, false); sharingConsent.interactable = false; UpdateShareEnabled();
            sharingStatus.text = "Mengirim permintaan. Ini bukan layanan darurat. Jangan menutup game sampai status tersedia.";
            SupportReportRequest sent = previewRequest;
            StartCoroutine(OnlineSupportClient.Post(onlineSettings, "/reports", accessCode.text, JsonUtility.ToJson(sent), response => {
                sharingBusy = false; SetButtonEnabled(localExportButton, true); SetButtonEnabled(sharingBack, true); sharingConsent.interactable = true;
                if (OnlineSupportClient.IsAcceptedReceipt(sent, response))
                {
                    sharingStatus.text = "Diterima API WhatsApp. ID: " + response.reportId + "\nBelum membuktikan pesan sampai/dibaca atau ditindaklanjuti Guru BK.";
                    if (!TryStoreReceipt(new SupportResponse { success = true, reportId = sent.reportId, recipientLabel = sent.recipientLabel,
                        status = response.status, messageId = response.messageId, acceptedUtc = response.acceptedUtc }))
                        sharingStatus.text += " Bukti lokal gagal disimpan; catatan percobaan masih berstatus belum terkonfirmasi.";
                    previewRequest = null;
                    SetButtonEnabled(localExportButton, false);
                }
                else sharingStatus.text = "Belum terkonfirmasi. ID: " + sent.reportId + "\nCek pengelola sebelum mencoba lagi. Tidak ada klaim terkirim/dibaca.";
                UpdateShareEnabled();
            }));
        }
        private static bool TryStoreReceipt(SupportResponse receipt)
        {
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "YouthRise", "Receipts");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, receipt.reportId + ".json"), JsonUtility.ToJson(receipt, true));
                return true;
            }
            catch { return false; }
        }
    }
}
