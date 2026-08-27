using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace YouthRise
{
    public sealed class YouthRisePrototype : MonoBehaviour
    {
        private static readonly Color Ink = Hex("17233A");
        private static readonly Color Paper = Hex("F7F4EC");
        private static readonly Color Navy = Hex("14233E");
        private static readonly Color Blue = Hex("377DCE");
        private static readonly Color Cyan = Hex("3BBAC7");
        private static readonly Color Mint = Hex("BCE6D3");
        private static readonly Color Coral = Hex("EC6B62");
        private static readonly Color Gold = Hex("F4C95D");
        private static readonly Color White = new Color(1f, 1f, 1f, 1f);

        private StoryGraph story;
        private PlayerProfile profile;
        private IConversationGenerator conversationGenerator;
        private SafeZoneAssistant safeZoneAssistant;
        private DecisionTelemetry telemetry;
        private StoryNode currentNode;
        private SafeZoneAssessment currentAssessment;
        private Font font;
        private int sessionSeed;
        private float decisionStartedAt;
        private string branchPath = string.Empty;
        private bool chapterCompleted;

        private GameObject startScreen;
        private GameObject storyScreen;
        private GameObject completionScreen;
        private GameObject safeZoneScreen;
        private GameObject safeChatPanel;
        private GameObject safeArticlesPanel;
        private GameObject safeReportPanel;

        private Image sceneBackground;
        private Image riskFill;
        private Image trustFill;
        private Text riskValue;
        private Text trustValue;
        private Text locationText;
        private Text speakerName;
        private Text speakerInitials;
        private Text dialogueText;
        private Text toastText;
        private readonly Button[] choiceButtons = new Button[3];
        private readonly Text[] choiceLabels = new Text[3];
        private Button continueStoryButton;
        private Button continueMenuButton;
        private Button safeZoneMenuButton;
        private Text safeZoneMenuLabel;

        private InputField chatInput;
        private Text chatResponse;
        private InputField reportInput;
        private Text reportAssessment;
        private Button saveDraftButton;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<YouthRisePrototype>() != null)
                return;

            var host = new GameObject("YouthRise Prototype");
            host.AddComponent<YouthRisePrototype>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            conversationGenerator = new LocalConversationGenerator();
            safeZoneAssistant = new SafeZoneAssistant();
            sessionSeed = Guid.NewGuid().GetHashCode();

            profile = new PlayerProfile();
            profile.ResetForChapterOne();

            try
            {
                story = StoryRepository.LoadChapterOne();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            EnsureEventSystem();
            BuildInterface();
            ShowStartMenu();

            if (story == null)
            {
                ShowToast("Story data gagal dimuat. Periksa Console Unity.", true);
                SetButtonEnabled(continueMenuButton, false);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var eventSystemObject = new GameObject("YouthRise EventSystem");
            DontDestroyOnLoad(eventSystemObject);
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private void BuildInterface()
        {
            var canvasObject = new GameObject("YouthRise Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            startScreen = BuildStartScreen(canvasRect);
            storyScreen = BuildStoryScreen(canvasRect);
            completionScreen = BuildCompletionScreen(canvasRect);
            safeZoneScreen = BuildSafeZoneScreen(canvasRect);

            GameObject toast = CreateRect("Toast", canvasRect, new Vector2(0.27f, 0.89f), new Vector2(0.73f, 0.97f));
            AddImage(toast, new Color(Navy.r, Navy.g, Navy.b, 0.94f));
            toastText = AddText(toast, string.Empty, 22, White, TextAnchor.MiddleCenter, FontStyle.Bold);
            toast.SetActive(false);
        }

        private GameObject BuildStartScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Start Screen", parent, Vector2.zero, Vector2.one);
            AddImage(root, Navy);

            CreateDecorativeBlock(root.transform, "Sun", new Vector2(0.73f, 0.60f), new Vector2(0.96f, 0.98f), Gold);
            CreateDecorativeBlock(root.transform, "Sky", new Vector2(0.03f, 0.04f), new Vector2(0.20f, 0.36f), Cyan);
            CreateDecorativeBlock(root.transform, "Coral", new Vector2(0.82f, 0.08f), new Vector2(0.97f, 0.29f), Coral);

            GameObject eyebrow = CreateRect("Eyebrow", root.transform, new Vector2(0.18f, 0.79f), new Vector2(0.68f, 0.86f));
            AddText(eyebrow, "YOUTHRise • INTERACTIVE STORY", 24, Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject title = CreateRect("Title", root.transform, new Vector2(0.18f, 0.58f), new Vector2(0.79f, 0.80f));
            Text titleText = AddText(title, "THE FIRST\nDAY", 96, White, TextAnchor.MiddleLeft, FontStyle.Bold);
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 58;
            titleText.resizeTextMaxSize = 96;

            GameObject subtitle = CreateRect("Subtitle", root.transform, new Vector2(0.18f, 0.48f), new Vector2(0.68f, 0.59f));
            AddText(subtitle, "Chapter 1 • Hari pertama Alex di sekolah baru", 30, Paper, TextAnchor.MiddleLeft);

            GameObject feature = CreateRect("Features", root.transform, new Vector2(0.18f, 0.40f), new Vector2(0.75f, 0.48f));
            AddText(feature, "DIALOG PCG LOKAL   •   PILIHAN BERCABANG   •   SAFE ZONE", 19, new Color(1f, 1f, 1f, 0.65f), TextAnchor.MiddleLeft, FontStyle.Bold);

            Button start = CreateButton(root.transform, "Start", "MULAI CHAPTER", new Vector2(0.18f, 0.27f), new Vector2(0.39f, 0.36f), Blue, White, 24);
            start.onClick.AddListener(StartNewGame);

            continueMenuButton = CreateButton(root.transform, "Continue", "LANJUTKAN", new Vector2(0.41f, 0.27f), new Vector2(0.59f, 0.36f), Cyan, Navy, 22);
            continueMenuButton.onClick.AddListener(ContinueGame);

            safeZoneMenuButton = CreateButton(root.transform, "Safe Zone", "SAFE ZONE • TERKUNCI", new Vector2(0.61f, 0.27f), new Vector2(0.82f, 0.36f), Mint, Navy, 20);
            safeZoneMenuLabel = safeZoneMenuButton.GetComponentInChildren<Text>();
            safeZoneMenuButton.onClick.AddListener(ShowSafeZone);

            GameObject privacy = CreateRect("Privacy", root.transform, new Vector2(0.18f, 0.09f), new Vector2(0.82f, 0.21f));
            AddImage(privacy, new Color(1f, 1f, 1f, 0.07f));
            Text privacyText = AddText(privacy,
                "PROTOTYPE PRIVASI\nPilihan dan waktu respons disimpan secara pseudonim di perangkat ini. Draft bantuan tidak pernah dikirim otomatis.",
                18,
                new Color(1f, 1f, 1f, 0.78f),
                TextAnchor.MiddleLeft);
            SetTextPadding(privacyText, 26f, 26f, 8f, 8f);

            return root;
        }

        private GameObject BuildStoryScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Story Screen", parent, Vector2.zero, Vector2.one);
            sceneBackground = AddImage(root, Hex("6AA6D8"));

            CreateDecorativeBlock(root.transform, "Horizon", new Vector2(0f, 0f), new Vector2(1f, 0.43f), new Color(Navy.r, Navy.g, Navy.b, 0.16f));
            CreateDecorativeBlock(root.transform, "Light", new Vector2(0.66f, 0.34f), new Vector2(0.93f, 0.88f), new Color(1f, 1f, 1f, 0.10f));

            GameObject topBar = CreateRect("Top Bar", root.transform, new Vector2(0.025f, 0.90f), new Vector2(0.975f, 0.98f));
            AddImage(topBar, new Color(Navy.r, Navy.g, Navy.b, 0.95f));

            GameObject brand = CreateRect("Brand", topBar.transform, new Vector2(0.025f, 0.12f), new Vector2(0.20f, 0.88f));
            AddText(brand, "YOUTHRise", 25, White, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject location = CreateRect("Location", topBar.transform, new Vector2(0.20f, 0.12f), new Vector2(0.52f, 0.88f));
            locationText = AddText(location, "", 21, new Color(1f, 1f, 1f, 0.72f), TextAnchor.MiddleLeft);

            CreateMeter(topBar.transform, "Risk", "RISK", new Vector2(0.55f, 0.18f), new Vector2(0.75f, 0.82f), Coral, out riskFill, out riskValue);
            CreateMeter(topBar.transform, "Trust", "TRUST", new Vector2(0.77f, 0.18f), new Vector2(0.97f, 0.82f), Cyan, out trustFill, out trustValue);

            GameObject portraitCard = CreateRect("Speaker Card", root.transform, new Vector2(0.035f, 0.25f), new Vector2(0.245f, 0.86f));
            AddImage(portraitCard, new Color(Paper.r, Paper.g, Paper.b, 0.96f));

            GameObject portrait = CreateRect("Portrait", portraitCard.transform, new Vector2(0.12f, 0.36f), new Vector2(0.88f, 0.88f));
            AddImage(portrait, new Color(Blue.r, Blue.g, Blue.b, 0.20f));
            speakerInitials = AddText(portrait, "A", 92, Navy, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject name = CreateRect("Speaker Name", portraitCard.transform, new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.34f));
            speakerName = AddText(name, "NARASI", 28, Navy, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject cardCaption = CreateRect("Card Caption", portraitCard.transform, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.15f));
            AddText(cardCaption, "CHAPTER 01", 16, new Color(Navy.r, Navy.g, Navy.b, 0.45f), TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject dialogueCard = CreateRect("Dialogue Card", root.transform, new Vector2(0.27f, 0.25f), new Vector2(0.965f, 0.86f));
            AddImage(dialogueCard, new Color(Paper.r, Paper.g, Paper.b, 0.97f));

            GameObject dialogue = CreateRect("Dialogue", dialogueCard.transform, new Vector2(0.055f, 0.17f), new Vector2(0.945f, 0.88f));
            dialogueText = AddText(dialogue, "", 34, Ink, TextAnchor.MiddleLeft);
            dialogueText.resizeTextForBestFit = true;
            dialogueText.resizeTextMinSize = 24;
            dialogueText.resizeTextMaxSize = 34;

            continueStoryButton = CreateButton(dialogueCard.transform, "Continue Story", "LANJUT  →", new Vector2(0.70f, 0.04f), new Vector2(0.94f, 0.16f), Navy, White, 20);

            for (int index = 0; index < choiceButtons.Length; index++)
            {
                float startX = 0.035f + index * 0.315f;
                float endX = startX + 0.295f;
                choiceButtons[index] = CreateButton(root.transform, $"Choice {index + 1}", "", new Vector2(startX, 0.055f), new Vector2(endX, 0.205f), Navy, White, 21);
                choiceLabels[index] = choiceButtons[index].GetComponentInChildren<Text>();
                choiceLabels[index].alignment = TextAnchor.MiddleLeft;
                SetTextPadding(choiceLabels[index], 22f, 14f, 5f, 5f);
            }

            return root;
        }

        private GameObject BuildCompletionScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Completion Screen", parent, Vector2.zero, Vector2.one);
            AddImage(root, Navy);

            CreateDecorativeBlock(root.transform, "Gold", new Vector2(0.72f, 0.54f), new Vector2(0.96f, 0.96f), Gold);
            CreateDecorativeBlock(root.transform, "Mint", new Vector2(0.05f, 0.05f), new Vector2(0.23f, 0.37f), Mint);

            GameObject card = CreateRect("Reflection Card", root.transform, new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.88f));
            AddImage(card, Paper);

            GameObject kicker = CreateRect("Kicker", card.transform, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.91f));
            AddText(kicker, "TODAY REFLECTION", 22, Blue, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject heading = CreateRect("Heading", card.transform, new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.82f));
            AddText(heading, "HARI PERTAMA\nSELESAI", 55, Navy, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject reflection = CreateRect("Reflection", card.transform, new Vector2(0.12f, 0.35f), new Vector2(0.88f, 0.62f));
            AddText(reflection,
                "✓ Kamu bertemu teman baru\n✓ Kamu menghadapi tekanan teman sebaya\n✓ Kamu membuat pilihan yang sulit\n\nBesok adalah kesempatan baru.",
                25,
                Ink,
                TextAnchor.MiddleLeft);

            GameObject reward = CreateRect("Reward", card.transform, new Vector2(0.18f, 0.24f), new Vector2(0.82f, 0.34f));
            AddImage(reward, new Color(Gold.r, Gold.g, Gold.b, 0.32f));
            AddText(reward, "★ 100 XP   •   SAFE ZONE UNLOCKED", 23, Navy, TextAnchor.MiddleCenter, FontStyle.Bold);

            Button safe = CreateButton(card.transform, "Enter Safe Zone", "MASUK SAFE ZONE", new Vector2(0.12f, 0.07f), new Vector2(0.55f, 0.18f), Blue, White, 22);
            safe.onClick.AddListener(ShowSafeZone);

            Button menu = CreateButton(card.transform, "Back to Menu", "KEMBALI KE MENU", new Vector2(0.57f, 0.07f), new Vector2(0.88f, 0.18f), Cyan, Navy, 20);
            menu.onClick.AddListener(ShowStartMenu);

            return root;
        }

        private GameObject BuildSafeZoneScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Safe Zone Screen", parent, Vector2.zero, Vector2.one);
            AddImage(root, Hex("DDF2E8"));

            GameObject header = CreateRect("Header", root.transform, new Vector2(0f, 0.84f), new Vector2(1f, 1f));
            AddImage(header, Navy);

            GameObject title = CreateRect("Title", header.transform, new Vector2(0.045f, 0.18f), new Vector2(0.42f, 0.90f));
            AddText(title, "SAFE ZONE", 40, White, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject welcome = CreateRect("Welcome", header.transform, new Vector2(0.40f, 0.18f), new Vector2(0.82f, 0.90f));
            AddText(welcome, "“Tempat untuk berbicara tanpa takut dihakimi.”\n— Counselor", 18, new Color(1f, 1f, 1f, 0.74f), TextAnchor.MiddleLeft);

            Button close = CreateButton(header.transform, "Close", "KEMBALI", new Vector2(0.84f, 0.24f), new Vector2(0.96f, 0.78f), Coral, White, 17);
            close.onClick.AddListener(ShowStartMenu);

            GameObject tabs = CreateRect("Tabs", root.transform, new Vector2(0.045f, 0.735f), new Vector2(0.955f, 0.82f));
            Button chatTab = CreateButton(tabs.transform, "Chat Tab", "CHAT PENDAMPING", new Vector2(0f, 0f), new Vector2(0.30f, 1f), Blue, White, 19);
            chatTab.onClick.AddListener(() => ShowSafeTab("chat"));
            Button articleTab = CreateButton(tabs.transform, "Article Tab", "ARTIKEL SINGKAT", new Vector2(0.35f, 0f), new Vector2(0.65f, 1f), Cyan, Navy, 19);
            articleTab.onClick.AddListener(() => ShowSafeTab("articles"));
            Button reportTab = CreateButton(tabs.transform, "Report Tab", "NEED EXTRA HELP?", new Vector2(0.70f, 0f), new Vector2(1f, 1f), Coral, White, 19);
            reportTab.onClick.AddListener(() => ShowSafeTab("report"));

            safeChatPanel = BuildChatPanel(root.transform);
            safeArticlesPanel = BuildArticlesPanel(root.transform);
            safeReportPanel = BuildReportPanel(root.transform);

            GameObject disclaimer = CreateRect("Disclaimer", root.transform, new Vector2(0.045f, 0.025f), new Vector2(0.955f, 0.075f));
            AddText(disclaimer,
                "Prototype edukasi • Bukan layanan darurat • Tidak menggantikan Guru BK atau tenaga profesional • Data draft tersimpan lokal",
                16,
                new Color(Navy.r, Navy.g, Navy.b, 0.62f),
                TextAnchor.MiddleCenter,
                FontStyle.Bold);

            return root;
        }

        private GameObject BuildChatPanel(Transform parent)
        {
            GameObject panel = CreateRect("Chat Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);

            GameObject intro = CreateRect("Intro", panel.transform, new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.94f));
            AddText(intro,
                "PENDAMPING LOKAL\nCeritakan apa yang kamu rasakan. Respons dibuat dari aturan aman di perangkat ini—bukan diagnosis dan bukan manusia.",
                21,
                Navy,
                TextAnchor.MiddleLeft);

            GameObject response = CreateRect("Response", panel.transform, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.73f));
            AddImage(response, new Color(Mint.r, Mint.g, Mint.b, 0.55f));
            chatResponse = AddText(response,
                "Halo, Alex. Aku siap mendengarkan. Kamu bisa mulai dari hal yang paling nyaman untuk diceritakan.",
                23,
                Ink,
                TextAnchor.MiddleLeft);
            SetTextPadding(chatResponse, 26f, 26f, 16f, 16f);

            chatInput = CreateInputField(panel.transform, "Chat Input", "Tulis perasaan atau situasimu...", new Vector2(0.05f, 0.08f), new Vector2(0.76f, 0.30f), false);
            Button send = CreateButton(panel.transform, "Send Chat", "KIRIM", new Vector2(0.79f, 0.08f), new Vector2(0.95f, 0.30f), Blue, White, 21);
            send.onClick.AddListener(SendSafeZoneChat);

            return panel;
        }

        private GameObject BuildArticlesPanel(Transform parent)
        {
            GameObject panel = CreateRect("Articles Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);

            CreateArticleCard(panel.transform, 0.05f, 0.31f, "TEKANAN TEMAN SEBAYA", "Kamu boleh menolak tanpa menjelaskan panjang. Cari teman atau orang dewasa yang mendukung keputusan amanmu.", Blue);
            CreateArticleCard(panel.transform, 0.35f, 0.61f, "MENGELOLA CEMAS", "Tarik napas perlahan, beri nama pada perasaanmu, lalu pilih satu langkah kecil yang bisa dilakukan sekarang.", Cyan);
            CreateArticleCard(panel.transform, 0.65f, 0.95f, "HUBUNGAN SEHAT", "Hubungan yang sehat menghormati batas, tidak memaksa, dan memberi ruang untuk berkata tidak.", Coral);

            return panel;
        }

        private GameObject BuildReportPanel(Transform parent)
        {
            GameObject panel = CreateRect("Report Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);

            GameObject intro = CreateRect("Intro", panel.transform, new Vector2(0.045f, 0.76f), new Vector2(0.955f, 0.94f));
            AddText(intro,
                "NEED EXTRA HELP?\nJelaskan kejadian tanpa menulis nama lengkap jika tidak diperlukan. Analisis dilakukan lokal dan dapat kamu tinjau sebelum menyimpan.",
                20,
                Navy,
                TextAnchor.MiddleLeft);

            reportInput = CreateInputField(panel.transform, "Report Input", "Apa yang terjadi? Kapan? Apakah kamu merasa aman sekarang?", new Vector2(0.045f, 0.30f), new Vector2(0.56f, 0.73f), true);

            GameObject assessment = CreateRect("Assessment", panel.transform, new Vector2(0.59f, 0.30f), new Vector2(0.955f, 0.73f));
            AddImage(assessment, new Color(Mint.r, Mint.g, Mint.b, 0.45f));
            reportAssessment = AddText(assessment,
                "Belum dianalisis.\n\nPrototype ini hanya membuat draft lokal; tidak ada laporan yang dikirim otomatis.",
                20,
                Ink,
                TextAnchor.UpperLeft);
            SetTextPadding(reportAssessment, 24f, 24f, 18f, 18f);

            Button analyze = CreateButton(panel.transform, "Analyze", "ANALISIS LOKAL", new Vector2(0.045f, 0.08f), new Vector2(0.28f, 0.23f), Blue, White, 19);
            analyze.onClick.AddListener(AnalyzeReport);

            saveDraftButton = CreateButton(panel.transform, "Save Draft", "SIMPAN DRAFT LOKAL", new Vector2(0.31f, 0.08f), new Vector2(0.57f, 0.23f), Coral, White, 18);
            saveDraftButton.onClick.AddListener(SaveReportDraft);
            SetButtonEnabled(saveDraftButton, false);

            Button clear = CreateButton(panel.transform, "Clear", "HAPUS FORM", new Vector2(0.60f, 0.08f), new Vector2(0.79f, 0.23f), Cyan, Navy, 18);
            clear.onClick.AddListener(ClearReportForm);

            GameObject localOnly = CreateRect("Local Only", panel.transform, new Vector2(0.81f, 0.08f), new Vector2(0.955f, 0.23f));
            AddText(localOnly, "LOCAL\nONLY", 17, Coral, TextAnchor.MiddleCenter, FontStyle.Bold);

            return panel;
        }

        private void StartNewGame()
        {
            if (story == null)
                return;

            PrototypeSaveService.Clear();
            profile = new PlayerProfile();
            profile.ResetForChapterOne();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void ContinueGame()
        {
            if (story == null || !PrototypeSaveService.TryLoad(out PrototypeSave save))
            {
                StartNewGame();
                return;
            }

            profile = save.profile;
            branchPath = save.branchPath ?? string.Empty;
            chapterCompleted = save.chapterCompleted;
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);

            if (chapterCompleted)
                ShowCompletion(false);
            else
                ShowNode(story.Contains(save.currentNodeId) ? save.currentNodeId : story.Chapter.startNodeId);
        }

        private void ShowNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || nodeId.Equals("END", StringComparison.OrdinalIgnoreCase))
            {
                CompleteChapter();
                return;
            }

            StoryNode node = story.Get(nodeId);
            if (node == null)
            {
                Debug.LogError($"YouthRise could not find story node '{nodeId}'.");
                ShowToast("Alur cerita terputus. Periksa data chapter.", true);
                return;
            }

            currentNode = node;
            SetScreen(storyScreen);
            ApplyBackground(node.background);
            locationText.text = (node.location ?? string.Empty).ToUpperInvariant();
            speakerName.text = (node.speaker ?? "Narasi").ToUpperInvariant();
            speakerInitials.text = GetInitials(node.speaker);
            dialogueText.text = conversationGenerator.Generate(node, profile, sessionSeed);
            decisionStartedAt = Time.unscaledTime;

            bool hasChoices = node.choices != null && node.choices.Length > 0;
            for (int index = 0; index < choiceButtons.Length; index++)
            {
                if (!hasChoices || index >= node.choices.Length)
                {
                    choiceButtons[index].gameObject.SetActive(false);
                    continue;
                }

                StoryChoice selectedChoice = node.choices[index];
                choiceButtons[index].gameObject.SetActive(true);
                choiceButtons[index].onClick.RemoveAllListeners();
                choiceButtons[index].onClick.AddListener(() => SelectChoice(selectedChoice));
                choiceLabels[index].text = $"{(char)('A' + index)}   {selectedChoice.label}";
            }

            continueStoryButton.gameObject.SetActive(!hasChoices);
            continueStoryButton.onClick.RemoveAllListeners();
            if (!hasChoices)
            {
                string next = node.nextNodeId;
                continueStoryButton.onClick.AddListener(() => ShowNode(next));
            }

            UpdateMeters();
            SaveProgress(node.id, false);
        }

        private void SelectChoice(StoryChoice choice)
        {
            if (choice == null || currentNode == null)
                return;

            int beforeRisk = profile.risk;
            int beforeTrust = profile.TrustScore;
            profile.Apply(choice.effects);
            branchPath = string.IsNullOrEmpty(branchPath)
                ? $"{currentNode.id}:{choice.id}"
                : branchPath + ">" + currentNode.id + ":" + choice.id;

            float latency = Time.unscaledTime - decisionStartedAt;
            telemetry?.RecordDecision(currentNode, choice, latency, branchPath, profile);
            UpdateMeters();
            ShowChoiceFeedback(beforeRisk, beforeTrust);

            string next = choice.nextNodeId;
            SaveProgress(next, false);
            ShowNode(next);
        }

        private void CompleteChapter()
        {
            if (!chapterCompleted)
            {
                chapterCompleted = true;
                profile.Apply("xp", 100);
                profile.safeZoneUnlocked = true;
                telemetry?.RecordChapterCompleted(branchPath, profile);
                SaveProgress("END", true);
            }

            ShowCompletion(false);
        }

        private void ShowCompletion(bool grantReward)
        {
            if (grantReward && !chapterCompleted)
                CompleteChapter();
            else
                SetScreen(completionScreen);
        }

        private void ShowStartMenu()
        {
            SetScreen(startScreen);

            bool hasSave = PrototypeSaveService.TryLoad(out PrototypeSave save);
            if (hasSave && save.profile != null)
                profile = save.profile;

            continueMenuButton.gameObject.SetActive(hasSave);
            bool unlocked = hasSave && save.profile != null && save.profile.safeZoneUnlocked;
            SetButtonEnabled(safeZoneMenuButton, unlocked);
            safeZoneMenuLabel.text = unlocked ? "SAFE ZONE • TERBUKA" : "SAFE ZONE • TERKUNCI";
        }

        private void ShowSafeZone()
        {
            if ((profile == null || !profile.safeZoneUnlocked) &&
                PrototypeSaveService.TryLoad(out PrototypeSave save) &&
                save.profile != null && save.profile.safeZoneUnlocked)
            {
                profile = save.profile;
            }

            if (profile == null || !profile.safeZoneUnlocked)
                return;

            SetScreen(safeZoneScreen);
            telemetry?.RecordSafeZoneOpened(profile);
            ShowSafeTab("chat");
        }

        private void ShowSafeTab(string tab)
        {
            safeChatPanel.SetActive(tab == "chat");
            safeArticlesPanel.SetActive(tab == "articles");
            safeReportPanel.SetActive(tab == "report");
        }

        private void SendSafeZoneChat()
        {
            string input = chatInput.text;
            chatResponse.text = safeZoneAssistant.CreateChatResponse(input);
            if (!string.IsNullOrWhiteSpace(input))
                chatInput.text = string.Empty;
        }

        private void AnalyzeReport()
        {
            currentAssessment = safeZoneAssistant.Assess(reportInput.text);
            reportAssessment.text =
                $"AI TRIAGE LOKAL\nKategori: {currentAssessment.category}\nPrioritas: {currentAssessment.urgency}\n\n" +
                currentAssessment.supportiveResponse + "\n\n" + currentAssessment.suggestedAction;
            reportAssessment.color = currentAssessment.immediateSafetyConcern ? Hex("9C2F2F") : Ink;
            SetButtonEnabled(saveDraftButton, !string.IsNullOrWhiteSpace(reportInput.text));
        }

        private void SaveReportDraft()
        {
            if (currentAssessment == null)
                AnalyzeReport();

            ReportSaveResult result = safeZoneAssistant.SaveLocalDraft(reportInput.text, currentAssessment);
            reportAssessment.text = result.message;
            reportAssessment.color = result.success ? Hex("22684A") : Hex("9C2F2F");

            if (result.success)
            {
                telemetry?.RecordReportDraft(currentAssessment.category, currentAssessment.urgency);
                SetButtonEnabled(saveDraftButton, false);
            }
        }

        private void ClearReportForm()
        {
            reportInput.text = string.Empty;
            reportAssessment.text = "Form dibersihkan. Tidak ada data yang dikirim.";
            reportAssessment.color = Ink;
            currentAssessment = null;
            SetButtonEnabled(saveDraftButton, false);
        }

        private void SaveProgress(string nodeId, bool completed)
        {
            PrototypeSaveService.Save(new PrototypeSave
            {
                chapterId = story?.Chapter.id,
                currentNodeId = nodeId,
                branchPath = branchPath,
                chapterCompleted = completed,
                profile = profile
            });
        }

        private void UpdateMeters()
        {
            if (profile == null)
                return;

            riskFill.fillAmount = profile.risk / 100f;
            trustFill.fillAmount = profile.TrustScore / 100f;
            riskValue.text = profile.risk.ToString("00");
            trustValue.text = profile.TrustScore.ToString("00");
        }

        private void ShowChoiceFeedback(int previousRisk, int previousTrust)
        {
            int riskDelta = profile.risk - previousRisk;
            int trustDelta = profile.TrustScore - previousTrust;

            if (riskDelta != 0 || trustDelta != 0)
            {
                string risk = riskDelta == 0 ? string.Empty : $"Risk {(riskDelta > 0 ? "↑" : "↓")}  ";
                string trust = trustDelta == 0 ? string.Empty : $"Trust {(trustDelta > 0 ? "↑" : "↓")}";
                ShowToast((risk + trust).Trim(), false);
            }
            else
            {
                ShowToast("Pilihanmu membentuk perjalanan Alex.", false);
            }
        }

        private void ApplyBackground(string background)
        {
            switch ((background ?? string.Empty).ToLowerInvariant())
            {
                case "home": sceneBackground.color = Hex("D99C79"); break;
                case "school-gate": sceneBackground.color = Hex("68A7C9"); break;
                case "classroom": sceneBackground.color = Hex("7FB58F"); break;
                case "hallway": sceneBackground.color = Hex("D6B36B"); break;
                case "back-school": sceneBackground.color = Hex("657A6B"); break;
                case "street": sceneBackground.color = Hex("C78973"); break;
                case "bedroom": sceneBackground.color = Hex("5B6289"); break;
                default: sceneBackground.color = Hex("6AA6D8"); break;
            }
        }

        private void ShowToast(string message, bool error)
        {
            if (toastText == null)
                return;

            toastText.text = message;
            toastText.color = error ? Gold : White;
            toastText.transform.parent.gameObject.SetActive(true);
            CancelInvoke(nameof(HideToast));
            Invoke(nameof(HideToast), error ? 5f : 1.8f);
        }

        private void HideToast()
        {
            if (toastText != null)
                toastText.transform.parent.gameObject.SetActive(false);
        }

        private void SetScreen(GameObject active)
        {
            startScreen.SetActive(active == startScreen);
            storyScreen.SetActive(active == storyScreen);
            completionScreen.SetActive(active == completionScreen);
            safeZoneScreen.SetActive(active == safeZoneScreen);
        }

        private void CreateMeter(Transform parent, string name, string label, Vector2 min, Vector2 max, Color color, out Image fill, out Text value)
        {
            GameObject container = CreateRect(name, parent, min, max);

            GameObject labelObject = CreateRect("Label", container.transform, new Vector2(0f, 0f), new Vector2(0.30f, 1f));
            AddText(labelObject, label, 15, new Color(1f, 1f, 1f, 0.72f), TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject track = CreateRect("Track", container.transform, new Vector2(0.30f, 0.31f), new Vector2(0.82f, 0.69f));
            AddImage(track, new Color(1f, 1f, 1f, 0.13f));
            GameObject fillObject = CreateRect("Fill", track.transform, Vector2.zero, Vector2.one);
            fill = AddImage(fillObject, color);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;

            GameObject valueObject = CreateRect("Value", container.transform, new Vector2(0.84f, 0f), new Vector2(1f, 1f));
            value = AddText(valueObject, "00", 18, White, TextAnchor.MiddleRight, FontStyle.Bold);
        }

        private void CreateArticleCard(Transform parent, float minX, float maxX, string title, string body, Color accent)
        {
            GameObject card = CreateRect(title, parent, new Vector2(minX, 0.10f), new Vector2(maxX, 0.90f));
            AddImage(card, new Color(accent.r, accent.g, accent.b, 0.13f));
            GameObject stripe = CreateRect("Stripe", card.transform, new Vector2(0f, 0f), new Vector2(0.035f, 1f));
            AddImage(stripe, accent);
            GameObject titleObject = CreateRect("Title", card.transform, new Vector2(0.10f, 0.66f), new Vector2(0.90f, 0.90f));
            AddText(titleObject, title, 21, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            GameObject bodyObject = CreateRect("Body", card.transform, new Vector2(0.10f, 0.12f), new Vector2(0.90f, 0.64f));
            Text bodyText = AddText(bodyObject, body, 20, Ink, TextAnchor.UpperLeft);
            bodyText.resizeTextForBestFit = true;
            bodyText.resizeTextMinSize = 16;
            bodyText.resizeTextMaxSize = 20;
        }

        private InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 min, Vector2 max, bool multiline)
        {
            GameObject fieldObject = CreateRect(name, parent, min, max);
            AddImage(fieldObject, White);
            InputField input = fieldObject.AddComponent<InputField>();
            input.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            input.characterLimit = multiline ? 2500 : 500;

            GameObject placeholderObject = CreateRect("Placeholder", fieldObject.transform, new Vector2(0.035f, 0.08f), new Vector2(0.965f, 0.92f));
            Text placeholderText = AddText(placeholderObject, placeholder, 19, new Color(Navy.r, Navy.g, Navy.b, 0.38f), multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, FontStyle.Italic);
            placeholderText.raycastTarget = false;

            GameObject textObject = CreateRect("Text", fieldObject.transform, new Vector2(0.035f, 0.08f), new Vector2(0.965f, 0.92f));
            Text inputText = AddText(textObject, string.Empty, 19, Ink, multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);
            inputText.raycastTarget = false;

            input.textComponent = inputText;
            input.placeholder = placeholderText;
            input.targetGraphic = fieldObject.GetComponent<Image>();
            input.caretColor = Navy;
            input.selectionColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);
            return input;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color background, Color foreground, int fontSize)
        {
            GameObject buttonObject = CreateRect(name, parent, min, max);
            Image image = AddImage(buttonObject, background);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, White, 0.18f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.12f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(background.r, background.g, background.b, 0.28f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            GameObject labelObject = CreateRect("Label", buttonObject.transform, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
            Text text = AddText(labelObject, label, fontSize, foreground, TextAnchor.MiddleCenter, FontStyle.Bold);
            text.raycastTarget = false;
            return button;
        }

        private GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return gameObject;
        }

        private Image AddImage(GameObject target, Color color)
        {
            Image image = target.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text AddText(GameObject target, string content, int size, Color color, TextAnchor alignment, FontStyle style = FontStyle.Normal)
        {
            // A uGUI GameObject can only render one Graphic through its CanvasRenderer.
            // Put text on a full-size child whenever the target already owns an Image.
            GameObject textTarget = target.GetComponent<Graphic>() == null
                ? target
                : CreateRect("Text", target.transform, Vector2.zero, Vector2.one);

            Text text = textTarget.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = true;
            return text;
        }

        private void CreateDecorativeBlock(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            GameObject block = CreateRect(name, parent, min, max);
            AddImage(block, color);
            block.transform.SetAsFirstSibling();
        }

        private static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button != null)
                button.interactable = enabled;
        }

        private static void SetTextPadding(Text text, float left, float right, float bottom, float top)
        {
            RectTransform rect = text.rectTransform;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static string GetInitials(string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker) || speaker.Equals("Narasi", StringComparison.OrdinalIgnoreCase))
                return "✦";

            string clean = speaker.Split('•')[0].Trim();
            string[] words = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return "?";
            if (words.Length == 1)
                return words[0].Substring(0, 1).ToUpperInvariant();
            return (words[0].Substring(0, 1) + words[1].Substring(0, 1)).ToUpperInvariant();
        }

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? color : Color.magenta;
        }
    }
}
