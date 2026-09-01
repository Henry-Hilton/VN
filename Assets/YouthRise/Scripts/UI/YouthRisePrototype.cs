using System;
using System.Collections;
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
        private Image nextSceneBackground;
        private Image characterPortrait;
        private Image completionBackground;
        private Image riskFill;
        private Image trustFill;
        private Text riskValue;
        private Text trustValue;
        private Text locationText;
        private Text speakerName;
        private Text speakerInitials;
        private Text dialogueText;
        private Text toastText;
        private Text menuTitleText;
        private Text menuSubtitleText;
        private Text menuFeatureText;
        private Text storyChapterCaption;
        private Text completionHeadingText;
        private Text completionReflectionText;
        private Text completionRewardText;
        private Text completionPrimaryLabel;
        private Text chapterTwoMenuLabel;
        private Text bullyingArticleBody;
        private GameObject toastRoot;
        private GameObject speakerPlaceholder;
        private CanvasGroup dialogueGroup;
        private CanvasGroup characterGroup;
        private CanvasGroup storyInteractionGroup;
        private CanvasGroup toastGroup;
        private RectTransform dialogueRect;
        private RectTransform characterRect;
        private RectTransform toastRect;
        private Image toastBackground;
        private Image toastAccent;
        private Material chromaKeyMaterial;
        private Coroutine storyTransition;
        private Coroutine screenTransition;
        private Coroutine meterTransition;
        private Coroutine toastTransition;
        private float displayedRisk;
        private float displayedTrust;
        private bool metersInitialized;
        private readonly Dictionary<string, Sprite> artSprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private readonly Button[] choiceButtons = new Button[3];
        private readonly Text[] choiceLabels = new Text[3];
        private readonly CanvasGroup[] choiceGroups = new CanvasGroup[3];
        private Button continueStoryButton;
        private Button continueMenuButton;
        private Button chapterTwoMenuButton;
        private Button safeZoneMenuButton;
        private Button completionPrimaryButton;
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

            toastRoot = CreateRect("Toast", canvasRect, new Vector2(0.365f, 0.862f), new Vector2(0.635f, 0.897f));
            toastRect = toastRoot.GetComponent<RectTransform>();
            toastBackground = AddImage(toastRoot, new Color(Navy.r, Navy.g, Navy.b, 0.96f));
            toastBackground.raycastTarget = false;
            toastGroup = toastRoot.AddComponent<CanvasGroup>();
            toastGroup.interactable = false;
            toastGroup.blocksRaycasts = false;
            toastText = AddText(toastRoot, string.Empty, 17, White, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetTextPadding(toastText, 28f, 20f, 4f, 4f);
            GameObject toastStripe = CreateRect("Accent", toastRoot.transform, Vector2.zero, new Vector2(0.018f, 1f));
            toastAccent = AddImage(toastStripe, Cyan);
            toastAccent.raycastTarget = false;
            toastRoot.SetActive(false);
        }

        private GameObject BuildStartScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Start Screen", parent, Vector2.zero, Vector2.one);
            Image menuBackground = AddImage(root, Navy);
            menuBackground.sprite = LoadArtSprite("YouthRise/Art/Backgrounds/bg_school_gate");
            menuBackground.color = menuBackground.sprite != null ? White : Navy;

            GameObject menuWash = CreateRect("Menu Wash", root.transform, Vector2.zero, Vector2.one);
            AddImage(menuWash, new Color(Navy.r, Navy.g, Navy.b, 0.82f)).raycastTarget = false;

            CreateDecorativeBlock(root.transform, "Sun", new Vector2(0.945f, 0.60f), new Vector2(0.957f, 0.96f), Gold);
            CreateDecorativeBlock(root.transform, "Sky", new Vector2(0.03f, 0.06f), new Vector2(0.042f, 0.34f), Cyan);
            CreateDecorativeBlock(root.transform, "Coral", new Vector2(0.945f, 0.10f), new Vector2(0.957f, 0.28f), Coral);

            GameObject eyebrow = CreateRect("Eyebrow", root.transform, new Vector2(0.18f, 0.79f), new Vector2(0.68f, 0.86f));
            AddText(eyebrow, "YOUTHRise • INTERACTIVE STORY", 24, Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject title = CreateRect("Title", root.transform, new Vector2(0.18f, 0.58f), new Vector2(0.79f, 0.80f));
            menuTitleText = AddText(title, "THE FIRST\nDAY", 96, White, TextAnchor.MiddleLeft, FontStyle.Bold);
            menuTitleText.resizeTextForBestFit = true;
            menuTitleText.resizeTextMinSize = 58;
            menuTitleText.resizeTextMaxSize = 96;

            GameObject subtitle = CreateRect("Subtitle", root.transform, new Vector2(0.18f, 0.48f), new Vector2(0.68f, 0.59f));
            menuSubtitleText = AddText(subtitle, "Chapter 1 • Hari pertama Alex di sekolah baru", 30, Paper, TextAnchor.MiddleLeft);

            GameObject feature = CreateRect("Features", root.transform, new Vector2(0.18f, 0.40f), new Vector2(0.75f, 0.48f));
            menuFeatureText = AddText(feature, "DIALOG PCG LOKAL   •   PILIHAN BERCABANG   •   SAFE ZONE", 19, new Color(1f, 1f, 1f, 0.65f), TextAnchor.MiddleLeft, FontStyle.Bold);

            Button start = CreateButton(root.transform, "Start", "MULAI CHAPTER 1", new Vector2(0.18f, 0.30f), new Vector2(0.49f, 0.37f), Blue, White, 21);
            start.onClick.AddListener(StartNewGame);

            chapterTwoMenuButton = CreateButton(root.transform, "Start Chapter 2", "CHAPTER 2 • TERKUNCI", new Vector2(0.51f, 0.30f), new Vector2(0.82f, 0.37f), Coral, White, 21);
            chapterTwoMenuLabel = chapterTwoMenuButton.GetComponentInChildren<Text>();
            chapterTwoMenuButton.onClick.AddListener(StartChapterTwo);

            continueMenuButton = CreateButton(root.transform, "Continue", "LANJUTKAN", new Vector2(0.18f, 0.22f), new Vector2(0.49f, 0.29f), Cyan, Navy, 20);
            continueMenuButton.onClick.AddListener(ContinueGame);

            safeZoneMenuButton = CreateButton(root.transform, "Safe Zone", "SAFE ZONE • TERKUNCI", new Vector2(0.51f, 0.22f), new Vector2(0.82f, 0.29f), Mint, Navy, 20);
            safeZoneMenuLabel = safeZoneMenuButton.GetComponentInChildren<Text>();
            safeZoneMenuButton.onClick.AddListener(ShowSafeZone);

            GameObject privacy = CreateRect("Privacy", root.transform, new Vector2(0.18f, 0.06f), new Vector2(0.82f, 0.18f));
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
            AddImage(root, Navy).raycastTarget = false;
            storyInteractionGroup = root.AddComponent<CanvasGroup>();

            GameObject background = CreateRect("Background", root.transform, Vector2.zero, Vector2.one);
            sceneBackground = AddImage(background, White);
            sceneBackground.raycastTarget = false;

            GameObject nextBackground = CreateRect("Background Crossfade", root.transform, Vector2.zero, Vector2.one);
            nextSceneBackground = AddImage(nextBackground, new Color(1f, 1f, 1f, 0f));
            nextSceneBackground.raycastTarget = false;

            GameObject atmosphere = CreateRect("Atmosphere", root.transform, Vector2.zero, Vector2.one);
            AddImage(atmosphere, new Color(Navy.r, Navy.g, Navy.b, 0.24f)).raycastTarget = false;

            GameObject characterBackdrop = CreateRect("Character Backdrop", root.transform, new Vector2(0.015f, 0.20f), new Vector2(0.37f, 0.90f));
            AddImage(characterBackdrop, new Color(Navy.r, Navy.g, Navy.b, 0.18f)).raycastTarget = false;

            GameObject character = CreateRect("Character Stage", root.transform, new Vector2(0.018f, 0.19f), new Vector2(0.38f, 0.90f));
            characterRect = character.GetComponent<RectTransform>();
            characterGroup = character.AddComponent<CanvasGroup>();

            GameObject portraitVisual = CreateRect("Character Portrait", character.transform, Vector2.zero, Vector2.one);
            characterPortrait = AddImage(portraitVisual, new Color(1f, 1f, 1f, 0f));
            characterPortrait.preserveAspect = true;
            characterPortrait.raycastTarget = false;

            Shader chromaShader = Shader.Find("YouthRise/UI Chroma Key");
            if (chromaShader != null)
            {
                chromaKeyMaterial = new Material(chromaShader);
                chromaKeyMaterial.SetColor("_KeyColor", Color.green);
                chromaKeyMaterial.SetFloat("_Threshold", 0.36f);
                chromaKeyMaterial.SetFloat("_Softness", 0.16f);
                characterPortrait.material = chromaKeyMaterial;
            }
            else
            {
                Debug.LogWarning("YouthRise chroma-key shader was not found. Character art will use its source background.");
            }

            speakerPlaceholder = CreateRect("Narrator Mark", character.transform, new Vector2(0.30f, 0.45f), new Vector2(0.70f, 0.65f));
            AddImage(speakerPlaceholder, new Color(Navy.r, Navy.g, Navy.b, 0.78f)).raycastTarget = false;
            speakerInitials = AddText(speakerPlaceholder, "✦", 56, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject hudShadow = CreateRect("HUD Shadow", root.transform, new Vector2(0.025f, 0.892f), new Vector2(0.975f, 0.902f));
            AddImage(hudShadow, new Color(0f, 0f, 0f, 0.24f)).raycastTarget = false;

            GameObject topBar = CreateRect("Top Bar", root.transform, new Vector2(0.025f, 0.90f), new Vector2(0.975f, 0.982f));
            AddImage(topBar, new Color(Navy.r, Navy.g, Navy.b, 0.95f));

            GameObject brandAccent = CreateRect("Brand Accent", topBar.transform, new Vector2(0.018f, 0.22f), new Vector2(0.024f, 0.78f));
            AddImage(brandAccent, Cyan).raycastTarget = false;

            GameObject brand = CreateRect("Brand", topBar.transform, new Vector2(0.035f, 0.12f), new Vector2(0.19f, 0.88f));
            AddText(brand, "YOUTHRise", 25, White, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject separator = CreateRect("Separator", topBar.transform, new Vector2(0.193f, 0.25f), new Vector2(0.195f, 0.75f));
            AddImage(separator, new Color(1f, 1f, 1f, 0.14f)).raycastTarget = false;

            GameObject locationDot = CreateRect("Location Dot", topBar.transform, new Vector2(0.207f, 0.12f), new Vector2(0.225f, 0.88f));
            AddText(locationDot, "●", 13, Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            GameObject location = CreateRect("Location", topBar.transform, new Vector2(0.228f, 0.12f), new Vector2(0.52f, 0.88f));
            locationText = AddText(location, "", 20, new Color(1f, 1f, 1f, 0.76f), TextAnchor.MiddleLeft, FontStyle.Bold);

            CreateMeter(topBar.transform, "Risk", "RISK", new Vector2(0.55f, 0.18f), new Vector2(0.75f, 0.82f), Coral, out riskFill, out riskValue);
            CreateMeter(topBar.transform, "Trust", "TRUST", new Vector2(0.77f, 0.18f), new Vector2(0.97f, 0.82f), Cyan, out trustFill, out trustValue);

            GameObject dialogueShadow = CreateRect("Dialogue Shadow", root.transform, new Vector2(0.315f, 0.258f), new Vector2(0.97f, 0.848f));
            AddImage(dialogueShadow, new Color(0f, 0f, 0f, 0.20f)).raycastTarget = false;

            GameObject dialogueCard = CreateRect("Dialogue Card", root.transform, new Vector2(0.31f, 0.27f), new Vector2(0.965f, 0.86f));
            dialogueRect = dialogueCard.GetComponent<RectTransform>();
            AddImage(dialogueCard, new Color(Paper.r, Paper.g, Paper.b, 0.96f));
            dialogueGroup = dialogueCard.AddComponent<CanvasGroup>();

            GameObject accent = CreateRect("Dialogue Accent", dialogueCard.transform, new Vector2(0f, 0f), new Vector2(0.012f, 1f));
            AddImage(accent, Cyan).raycastTarget = false;

            GameObject name = CreateRect("Speaker Name", dialogueCard.transform, new Vector2(0.055f, 0.81f), new Vector2(0.55f, 0.94f));
            speakerName = AddText(name, "NARASI", 27, Blue, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject cardCaption = CreateRect("Card Caption", dialogueCard.transform, new Vector2(0.69f, 0.82f), new Vector2(0.94f, 0.93f));
            storyChapterCaption = AddText(cardCaption, "CHAPTER 01  •  THE FIRST DAY", 15, new Color(Navy.r, Navy.g, Navy.b, 0.42f), TextAnchor.MiddleRight, FontStyle.Bold);

            GameObject dialogue = CreateRect("Dialogue", dialogueCard.transform, new Vector2(0.055f, 0.18f), new Vector2(0.945f, 0.79f));
            dialogueText = AddText(dialogue, "", 32, Ink, TextAnchor.MiddleLeft);
            dialogueText.resizeTextForBestFit = true;
            dialogueText.resizeTextMinSize = 22;
            dialogueText.resizeTextMaxSize = 32;

            continueStoryButton = CreateButton(dialogueCard.transform, "Continue Story", "LANJUT  →", new Vector2(0.70f, 0.04f), new Vector2(0.94f, 0.16f), Navy, White, 20);

            Color[] choiceAccents = { Coral, Gold, Cyan };
            for (int index = 0; index < choiceButtons.Length; index++)
            {
                float startX = 0.035f + index * 0.315f;
                float endX = startX + 0.295f;
                choiceButtons[index] = CreateButton(root.transform, $"Choice {index + 1}", "", new Vector2(startX, 0.045f), new Vector2(endX, 0.225f), new Color(Navy.r, Navy.g, Navy.b, 0.96f), White, 21);
                choiceLabels[index] = choiceButtons[index].GetComponentInChildren<Text>();
                choiceLabels[index].alignment = TextAnchor.MiddleLeft;
                SetTextPadding(choiceLabels[index], 88f, 18f, 5f, 5f);

                GameObject choiceAccent = CreateRect("Accent", choiceButtons[index].transform, Vector2.zero, new Vector2(0.016f, 1f));
                AddImage(choiceAccent, choiceAccents[index]).raycastTarget = false;

                GameObject keycap = CreateRect("Keycap", choiceButtons[index].transform, new Vector2(0.052f, 0.31f), new Vector2(0.145f, 0.69f));
                AddImage(keycap, new Color(choiceAccents[index].r, choiceAccents[index].g, choiceAccents[index].b, 0.18f)).raycastTarget = false;
                Text keyText = AddText(keycap, ((char)('A' + index)).ToString(), 20, choiceAccents[index], TextAnchor.MiddleCenter, FontStyle.Bold);
                keyText.raycastTarget = false;
                choiceGroups[index] = choiceButtons[index].gameObject.AddComponent<CanvasGroup>();
            }

            return root;
        }

        private GameObject BuildCompletionScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Completion Screen", parent, Vector2.zero, Vector2.one);
            completionBackground = AddImage(root, Navy);
            completionBackground.sprite = LoadArtSprite("YouthRise/Art/Backgrounds/bg_bedroom");
            completionBackground.color = completionBackground.sprite != null ? White : Navy;

            GameObject completionWash = CreateRect("Completion Wash", root.transform, Vector2.zero, Vector2.one);
            AddImage(completionWash, new Color(Navy.r, Navy.g, Navy.b, 0.78f)).raycastTarget = false;

            CreateDecorativeBlock(root.transform, "Gold", new Vector2(0.945f, 0.58f), new Vector2(0.957f, 0.94f), Gold);
            CreateDecorativeBlock(root.transform, "Mint", new Vector2(0.043f, 0.08f), new Vector2(0.055f, 0.35f), Mint);

            GameObject card = CreateRect("Reflection Card", root.transform, new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.88f));
            AddImage(card, Paper);

            GameObject kicker = CreateRect("Kicker", card.transform, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.91f));
            AddText(kicker, "TODAY REFLECTION", 22, Blue, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject heading = CreateRect("Heading", card.transform, new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.82f));
            completionHeadingText = AddText(heading, "HARI PERTAMA\nSELESAI", 55, Navy, TextAnchor.MiddleCenter, FontStyle.Bold);

            GameObject reflection = CreateRect("Reflection", card.transform, new Vector2(0.12f, 0.35f), new Vector2(0.88f, 0.62f));
            completionReflectionText = AddText(reflection,
                "✓ Kamu bertemu teman baru\n✓ Kamu menghadapi tekanan teman sebaya\n✓ Kamu membuat pilihan yang sulit\n\nBesok adalah kesempatan baru.",
                23,
                Ink,
                TextAnchor.MiddleLeft);
            completionReflectionText.resizeTextForBestFit = true;
            completionReflectionText.resizeTextMinSize = 18;
            completionReflectionText.resizeTextMaxSize = 23;

            GameObject reward = CreateRect("Reward", card.transform, new Vector2(0.18f, 0.24f), new Vector2(0.82f, 0.34f));
            AddImage(reward, new Color(Gold.r, Gold.g, Gold.b, 0.32f));
            completionRewardText = AddText(reward, "★ 100 XP   •   SAFE ZONE UNLOCKED", 21, Navy, TextAnchor.MiddleCenter, FontStyle.Bold);
            completionRewardText.resizeTextForBestFit = true;
            completionRewardText.resizeTextMinSize = 16;
            completionRewardText.resizeTextMaxSize = 21;

            completionPrimaryButton = CreateButton(card.transform, "Completion Primary", "MULAI CHAPTER 2", new Vector2(0.12f, 0.07f), new Vector2(0.55f, 0.18f), Blue, White, 22);
            completionPrimaryLabel = completionPrimaryButton.GetComponentInChildren<Text>();
            completionPrimaryButton.onClick.AddListener(HandleCompletionPrimary);

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

            CreateArticleCard(panel.transform, 0.05f, 0.48f, 0.53f, 0.93f, "TEKANAN TEMAN SEBAYA", "Kamu boleh menolak tanpa menjelaskan panjang. Cari teman atau orang dewasa yang mendukung keputusan amanmu.", Blue);
            CreateArticleCard(panel.transform, 0.52f, 0.95f, 0.53f, 0.93f, "MENGELOLA CEMAS", "Tarik napas perlahan, beri nama pada perasaanmu, lalu pilih satu langkah kecil yang bisa dilakukan sekarang.", Cyan);
            CreateArticleCard(panel.transform, 0.05f, 0.48f, 0.07f, 0.47f, "HUBUNGAN SEHAT", "Hubungan yang sehat menghormati batas, tidak memaksa, dan memberi ruang untuk berkata tidak.", Coral);
            bullyingArticleBody = CreateArticleCard(panel.transform, 0.52f, 0.95f, 0.07f, 0.47f, "DUKUNGAN BULLYING", "TERKUNCI • Selesaikan Chapter 2 untuk membuka artikel ini.", Gold);

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
            try
            {
                story = StoryRepository.LoadChapterOne();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 1 gagal dimuat.", true);
                return;
            }

            PrototypeSaveService.Clear();
            profile = new PlayerProfile();
            profile.ResetForChapterOne();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void StartChapterTwo()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }

            if (!CampaignProgression.CanStartChapterTwo(profile))
            {
                ShowToast("Selesaikan Chapter 1 untuk membuka Chapter 2.", false);
                return;
            }

            try
            {
                story = StoryRepository.LoadChapterTwo();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 2 gagal dimuat.", true);
                return;
            }

            profile.PrepareForChapterTwo();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void ContinueGame()
        {
            if (!PrototypeSaveService.TryLoad(out PrototypeSave save))
            {
                StartNewGame();
                return;
            }

            NormalizeLoadedProgress(save);
            profile = save.profile;
            ResetMeterAnimation();
            branchPath = save.branchPath ?? string.Empty;
            chapterCompleted = save.chapterCompleted;

            try
            {
                story = StoryRepository.LoadById(save.chapterId);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Progress tersimpan, tetapi chapter gagal dimuat.", true);
                return;
            }

            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);

            if (chapterCompleted)
                ShowCompletion(false);
            else
                ShowNode(story.Contains(save.currentNodeId) ? save.currentNodeId : story.Chapter.startNodeId);
        }

        private static void NormalizeLoadedProgress(PrototypeSave save)
        {
            CampaignProgression.Normalize(save);
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

            bool animateBetweenNodes = storyScreen.activeInHierarchy && currentNode != null;
            if (storyTransition != null)
                StopCoroutine(storyTransition);

            storyTransition = StartCoroutine(TransitionToNode(node, animateBetweenNodes));
        }

        private IEnumerator TransitionToNode(StoryNode node, bool animateBetweenNodes)
        {
            SetScreen(storyScreen);
            storyInteractionGroup.interactable = false;
            storyInteractionGroup.blocksRaycasts = false;

            Sprite nextBackground = LoadArtSprite(
                $"YouthRise/Art/Backgrounds/bg_{(node.background ?? string.Empty).Replace('-', '_')}");
            Color fallback = BackgroundFallback(node.background);

            if (!animateBetweenNodes)
            {
                sceneBackground.sprite = nextBackground;
                sceneBackground.color = nextBackground != null ? White : fallback;
                nextSceneBackground.sprite = null;
                nextSceneBackground.color = new Color(1f, 1f, 1f, 0f);
                PopulateNode(node);
                SetCharacterArt(node.speaker);

                dialogueGroup.alpha = 0f;
                characterGroup.alpha = 0f;
                SetChoiceAnimationState(0f, 0.97f);
                yield return AnimateNodeEntrance(0.42f);
            }
            else
            {
                nextSceneBackground.sprite = nextBackground;
                Color nextBase = nextBackground != null ? White : fallback;
                nextSceneBackground.color = WithAlpha(nextBase, 0f);

                const float exitDuration = 0.18f;
                for (float elapsed = 0f; elapsed < exitDuration; elapsed += Time.unscaledDeltaTime)
                {
                    float eased = Ease01(elapsed / exitDuration);
                    dialogueGroup.alpha = 1f - eased;
                    characterGroup.alpha = 1f - eased;
                    nextSceneBackground.color = WithAlpha(nextBase, eased * 0.46f);
                    yield return null;
                }

                PopulateNode(node);
                SetCharacterArt(node.speaker);
                dialogueGroup.alpha = 0f;
                characterGroup.alpha = 0f;
                SetChoiceAnimationState(0f, 0.97f);

                const float entranceDuration = 0.38f;
                Vector2 dialogueRest = Vector2.zero;
                Vector2 characterRest = Vector2.zero;
                dialogueRect.anchoredPosition = dialogueRest + new Vector2(44f, -8f);
                characterRect.anchoredPosition = characterRest + new Vector2(-42f, -12f);

                for (float elapsed = 0f; elapsed < entranceDuration; elapsed += Time.unscaledDeltaTime)
                {
                    float normalized = Mathf.Clamp01(elapsed / entranceDuration);
                    float eased = Ease01(normalized);
                    nextSceneBackground.color = WithAlpha(nextBase, Mathf.Lerp(0.46f, 1f, eased));
                    dialogueGroup.alpha = eased;
                    characterGroup.alpha = eased;
                    dialogueRect.anchoredPosition = Vector2.Lerp(dialogueRest + new Vector2(44f, -8f), dialogueRest, eased);
                    characterRect.anchoredPosition = Vector2.Lerp(characterRest + new Vector2(-42f, -12f), characterRest, eased);
                    AnimateChoiceCards(normalized);
                    yield return null;
                }

                sceneBackground.sprite = nextBackground;
                sceneBackground.color = nextBase;
                nextSceneBackground.sprite = null;
                nextSceneBackground.color = new Color(1f, 1f, 1f, 0f);
                dialogueRect.anchoredPosition = dialogueRest;
                characterRect.anchoredPosition = characterRest;
            }

            dialogueGroup.alpha = 1f;
            characterGroup.alpha = HasCharacterVisual() ? 1f : 0f;
            SetChoiceAnimationState(1f, 1f);
            storyInteractionGroup.interactable = true;
            storyInteractionGroup.blocksRaycasts = true;
            decisionStartedAt = Time.unscaledTime;
            storyTransition = null;
        }

        private IEnumerator AnimateNodeEntrance(float duration)
        {
            Vector2 dialogueRest = Vector2.zero;
            Vector2 characterRest = Vector2.zero;
            dialogueRect.anchoredPosition = dialogueRest + new Vector2(52f, -10f);
            characterRect.anchoredPosition = characterRest + new Vector2(-48f, -14f);

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float normalized = Mathf.Clamp01(elapsed / duration);
                float eased = Ease01(normalized);
                dialogueGroup.alpha = eased;
                characterGroup.alpha = HasCharacterVisual() ? eased : 0f;
                dialogueRect.anchoredPosition = Vector2.Lerp(dialogueRest + new Vector2(52f, -10f), dialogueRest, eased);
                characterRect.anchoredPosition = Vector2.Lerp(characterRest + new Vector2(-48f, -14f), characterRest, eased);
                AnimateChoiceCards(normalized);
                yield return null;
            }

            dialogueRect.anchoredPosition = dialogueRest;
            characterRect.anchoredPosition = characterRest;
        }

        private void PopulateNode(StoryNode node)
        {
            currentNode = node;
            locationText.text = (node.location ?? string.Empty).ToUpperInvariant();
            speakerName.text = (node.speaker ?? "Narasi").ToUpperInvariant();
            speakerInitials.text = GetInitials(node.speaker);
            storyChapterCaption.text = $"CHAPTER {Mathf.Max(1, story.Chapter.number):00}  •  {(story.Chapter.title ?? string.Empty).ToUpperInvariant()}";
            dialogueText.text = conversationGenerator.Generate(node, profile, sessionSeed);
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
                choiceLabels[index].text = selectedChoice.label;
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

        private void SetCharacterArt(string speaker)
        {
            string resource = CharacterResource(speaker);
            Sprite sprite = string.IsNullOrEmpty(resource) ? null : LoadArtSprite(resource);
            characterPortrait.sprite = sprite;
            characterPortrait.color = sprite != null ? White : new Color(1f, 1f, 1f, 0f);
            speakerPlaceholder.SetActive(sprite == null);
        }

        private bool HasCharacterVisual()
        {
            return characterPortrait != null &&
                   (characterPortrait.sprite != null ||
                    (speakerPlaceholder != null && speakerPlaceholder.activeSelf));
        }

        private Sprite LoadArtSprite(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                return null;

            if (artSprites.TryGetValue(resourcePath, out Sprite cached))
                return cached;

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogWarning($"YouthRise art asset was not found at Resources/{resourcePath}.");
                artSprites[resourcePath] = null;
                return null;
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = texture.name + " Runtime Sprite";
            artSprites[resourcePath] = sprite;
            return sprite;
        }

        private static string CharacterResource(string speaker)
        {
            string normalized = (speaker ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized.StartsWith("maya")) return "YouthRise/Art/Characters/char_maya_chroma";
            if (normalized.StartsWith("kevin")) return "YouthRise/Art/Characters/char_kevin_chroma";
            if (normalized.StartsWith("rina")) return "YouthRise/Art/Characters/char_rina_chroma";
            if (normalized.StartsWith("leo")) return "YouthRise/Art/Characters/char_leo_chroma";
            if (normalized.StartsWith("ibu")) return "YouthRise/Art/Characters/char_ibu_chroma";
            if (normalized.StartsWith("senior")) return "YouthRise/Art/Characters/char_senior_chroma";
            if (normalized.Contains("wali kelas") || normalized.Contains("daniel") || normalized.Contains("guru bk"))
                return "YouthRise/Art/Characters/char_mr_daniel_chroma";
            return null;
        }

        private void SetChoiceAnimationState(float alpha, float scale)
        {
            for (int index = 0; index < choiceGroups.Length; index++)
            {
                if (choiceGroups[index] == null)
                    continue;

                choiceGroups[index].alpha = alpha;
                choiceButtons[index].transform.localScale = Vector3.one * scale;
            }
        }

        private void AnimateChoiceCards(float normalized)
        {
            for (int index = 0; index < choiceGroups.Length; index++)
            {
                if (choiceGroups[index] == null || !choiceButtons[index].gameObject.activeSelf)
                    continue;

                float delayed = Mathf.Clamp01((normalized - index * 0.11f) / 0.78f);
                float eased = Ease01(delayed);
                choiceGroups[index].alpha = eased;
                choiceButtons[index].transform.localScale = Vector3.one * Mathf.Lerp(0.97f, 1f, eased);
            }
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
                CampaignProgression.Complete(story.Chapter, profile);
                telemetry?.RecordChapterCompleted(branchPath, profile);
                SaveProgress("END", true);
            }

            ShowCompletion(false);
        }

        private void ShowCompletion(bool grantReward)
        {
            if (toastTransition != null)
            {
                StopCoroutine(toastTransition);
                toastTransition = null;
            }
            HideToast();

            if (grantReward && !chapterCompleted)
                CompleteChapter();
            else
            {
                UpdateCompletionContent();
                ShowScreenSmooth(completionScreen);
            }
        }

        private void HandleCompletionPrimary()
        {
            if (IsChapterTwo())
                ShowSafeZone();
            else
                StartChapterTwo();
        }

        private void UpdateCompletionContent()
        {
            if (story?.Chapter == null)
                return;

            StoryChapter chapter = story.Chapter;
            completionHeadingText.text = string.IsNullOrWhiteSpace(chapter.completionHeading)
                ? $"CHAPTER {chapter.number}\nSELESAI"
                : chapter.completionHeading;

            if (chapter.reflectionLines != null && chapter.reflectionLines.Length > 0)
            {
                string[] lines = new string[chapter.reflectionLines.Length];
                for (int index = 0; index < chapter.reflectionLines.Length; index++)
                    lines[index] = "✓ " + chapter.reflectionLines[index];
                completionReflectionText.text = string.Join("\n", lines);
            }

            string unlocks = chapter.unlockLabels != null && chapter.unlockLabels.Length > 0
                ? "   •   " + string.Join("   •   ", chapter.unlockLabels)
                : string.Empty;
            completionRewardText.text = $"★ {chapter.rewardXp} XP{unlocks}";

            bool chapterTwo = IsChapterTwo();
            completionPrimaryLabel.text = chapterTwo ? "MASUK SAFE ZONE" : "MULAI CHAPTER 2";
            completionBackground.sprite = LoadArtSprite(chapterTwo
                ? "YouthRise/Art/Backgrounds/bg_classroom"
                : "YouthRise/Art/Backgrounds/bg_bedroom");
            completionBackground.color = completionBackground.sprite != null ? White : Navy;
        }

        private void ShowStartMenu()
        {
            ShowScreenSmooth(startScreen);

            bool hasSave = PrototypeSaveService.TryLoad(out PrototypeSave save);
            if (hasSave && save.profile != null)
            {
                NormalizeLoadedProgress(save);
                profile = save.profile;
            }

            continueMenuButton.gameObject.SetActive(hasSave);
            bool unlocked = hasSave && save.profile != null && save.profile.safeZoneUnlocked;
            SetButtonEnabled(safeZoneMenuButton, unlocked);
            safeZoneMenuLabel.text = unlocked ? "SAFE ZONE • TERBUKA" : "SAFE ZONE • TERKUNCI";

            bool chapterTwoUnlocked = hasSave && save.profile != null && save.profile.completedChapterOne;
            SetButtonEnabled(chapterTwoMenuButton, chapterTwoUnlocked);
            chapterTwoMenuLabel.text = !chapterTwoUnlocked
                ? "CHAPTER 2 • TERKUNCI"
                : save.profile.completedChapterTwo
                    ? "ULANGI CHAPTER 2"
                    : "MULAI CHAPTER 2";

            if (chapterTwoUnlocked)
            {
                menuTitleText.text = "BEHIND THE\nSMILE";
                menuSubtitleText.text = "Chapter 2 • Bullying, keberanian, dan mencari bantuan";
            }
            else
            {
                menuTitleText.text = "THE FIRST\nDAY";
                menuSubtitleText.text = "Chapter 1 • Hari pertama Alex di sekolah baru";
            }

            menuFeatureText.text = hasSave && save.profile != null && save.profile.relationshipPathUnlocked
                ? "RELATIONSHIP PATH • TERBUKA   •   SAFE ZONE   •   2 CHAPTER"
                : "DIALOG PCG LOKAL   •   PILIHAN BERCABANG   •   SAFE ZONE";
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

            ShowScreenSmooth(safeZoneScreen);
            telemetry?.RecordSafeZoneOpened(profile);
            UpdateSafeZoneArticles();
            ShowSafeTab("chat");
        }

        private void ShowSafeTab(string tab)
        {
            UpdateSafeZoneArticles();
            safeChatPanel.SetActive(tab == "chat");
            safeArticlesPanel.SetActive(tab == "articles");
            safeReportPanel.SetActive(tab == "report");
        }

        private void UpdateSafeZoneArticles()
        {
            if (bullyingArticleBody == null)
                return;

            bullyingArticleBody.text = profile != null && profile.bullyingSupportArticleUnlocked
                ? "Simpan bukti, dekati korban dengan aman, dan libatkan orang dewasa tepercaya. Diam juga merupakan sebuah pilihan."
                : "TERKUNCI • Selesaikan Chapter 2 untuk membuka artikel ini.";
        }

        private bool IsChapterTwo()
        {
            return string.Equals(story?.Chapter?.id, "chapter-2", StringComparison.OrdinalIgnoreCase);
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

            float targetRisk = profile.risk;
            float targetTrust = profile.TrustScore;
            if (!metersInitialized)
            {
                displayedRisk = targetRisk;
                displayedTrust = targetTrust;
                metersInitialized = true;
                ApplyMeterVisuals();
                return;
            }

            if (Mathf.Approximately(displayedRisk, targetRisk) && Mathf.Approximately(displayedTrust, targetTrust))
            {
                ApplyMeterVisuals();
                return;
            }

            if (meterTransition != null)
                StopCoroutine(meterTransition);
            meterTransition = StartCoroutine(AnimateMeters(targetRisk, targetTrust));
        }

        private IEnumerator AnimateMeters(float targetRisk, float targetTrust)
        {
            float startRisk = displayedRisk;
            float startTrust = displayedTrust;
            const float duration = 0.38f;

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float eased = Ease01(elapsed / duration);
                displayedRisk = Mathf.Lerp(startRisk, targetRisk, eased);
                displayedTrust = Mathf.Lerp(startTrust, targetTrust, eased);
                ApplyMeterVisuals();
                yield return null;
            }

            displayedRisk = targetRisk;
            displayedTrust = targetTrust;
            ApplyMeterVisuals();
            meterTransition = null;
        }

        private void ApplyMeterVisuals()
        {
            riskFill.fillAmount = displayedRisk / 100f;
            trustFill.fillAmount = displayedTrust / 100f;
            riskValue.text = $"{Mathf.RoundToInt(displayedRisk):00}%";
            trustValue.text = $"{Mathf.RoundToInt(displayedTrust):00}%";
        }

        private void ResetMeterAnimation()
        {
            if (meterTransition != null)
                StopCoroutine(meterTransition);
            meterTransition = null;
            metersInitialized = false;
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

        private static Color BackgroundFallback(string background)
        {
            switch ((background ?? string.Empty).ToLowerInvariant())
            {
                case "home": return Hex("D99C79");
                case "school-gate": return Hex("68A7C9");
                case "classroom": return Hex("7FB58F");
                case "hallway": return Hex("D6B36B");
                case "back-school": return Hex("657A6B");
                case "street": return Hex("C78973");
                case "bedroom": return Hex("5B6289");
                default: return Hex("6AA6D8");
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static float Ease01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private void ShowToast(string message, bool error)
        {
            if (toastText == null)
                return;

            toastText.text = message;
            toastText.color = error ? Gold : White;
            toastBackground.color = new Color(Navy.r, Navy.g, Navy.b, 0.97f);
            toastAccent.color = error ? Coral : Cyan;

            if (toastTransition != null)
                StopCoroutine(toastTransition);
            toastTransition = StartCoroutine(AnimateToast(error ? 4.5f : 1.45f));
        }

        private IEnumerator AnimateToast(float holdDuration)
        {
            toastRoot.SetActive(true);
            toastGroup.alpha = 0f;
            toastRect.localScale = Vector3.one * 0.97f;

            const float fadeInDuration = 0.14f;
            for (float elapsed = 0f; elapsed < fadeInDuration; elapsed += Time.unscaledDeltaTime)
            {
                float eased = Ease01(elapsed / fadeInDuration);
                toastGroup.alpha = eased;
                toastRect.localScale = Vector3.one * Mathf.Lerp(0.97f, 1f, eased);
                yield return null;
            }

            toastGroup.alpha = 1f;
            toastRect.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(holdDuration);

            const float fadeOutDuration = 0.20f;
            for (float elapsed = 0f; elapsed < fadeOutDuration; elapsed += Time.unscaledDeltaTime)
            {
                toastGroup.alpha = 1f - Ease01(elapsed / fadeOutDuration);
                yield return null;
            }

            HideToast();
            toastTransition = null;
        }

        private void HideToast()
        {
            if (toastRoot != null)
                toastRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (chromaKeyMaterial != null)
                Destroy(chromaKeyMaterial);

            foreach (Sprite sprite in artSprites.Values)
            {
                if (sprite != null)
                    Destroy(sprite);
            }

            artSprites.Clear();
        }

        private void SetScreen(GameObject active)
        {
            startScreen.SetActive(active == startScreen);
            storyScreen.SetActive(active == storyScreen);
            completionScreen.SetActive(active == completionScreen);
            safeZoneScreen.SetActive(active == safeZoneScreen);
        }

        private void ShowScreenSmooth(GameObject target)
        {
            GameObject current = null;
            int activeCount = 0;
            foreach (GameObject screen in AllScreens())
            {
                if (!screen.activeSelf)
                    continue;

                current = screen;
                activeCount++;
            }

            if (activeCount != 1 || current == null)
            {
                SetScreen(target);
                return;
            }

            if (current == target)
                return;

            if (screenTransition != null)
                StopCoroutine(screenTransition);

            screenTransition = StartCoroutine(CrossfadeScreens(current, target));
        }

        private IEnumerator CrossfadeScreens(GameObject current, GameObject target)
        {
            CanvasGroup from = GetOrAddCanvasGroup(current);
            CanvasGroup to = GetOrAddCanvasGroup(target);
            to.alpha = 0f;
            to.interactable = false;
            to.blocksRaycasts = false;
            target.SetActive(true);

            from.interactable = false;
            from.blocksRaycasts = false;
            const float duration = 0.32f;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float eased = Ease01(elapsed / duration);
                from.alpha = 1f - eased;
                to.alpha = eased;
                yield return null;
            }

            foreach (GameObject screen in AllScreens())
                screen.SetActive(screen == target);

            from.alpha = 1f;
            to.alpha = 1f;
            to.interactable = true;
            to.blocksRaycasts = true;
            screenTransition = null;
        }

        private IEnumerable<GameObject> AllScreens()
        {
            yield return startScreen;
            yield return storyScreen;
            yield return completionScreen;
            yield return safeZoneScreen;
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            return group != null ? group : target.AddComponent<CanvasGroup>();
        }

        private void CreateMeter(Transform parent, string name, string label, Vector2 min, Vector2 max, Color color, out Image fill, out Text value)
        {
            GameObject container = CreateRect(name, parent, min, max);
            AddImage(container, new Color(1f, 1f, 1f, 0.045f)).raycastTarget = false;

            GameObject accent = CreateRect("Accent", container.transform, new Vector2(0f, 0.18f), new Vector2(0.014f, 0.82f));
            AddImage(accent, color).raycastTarget = false;

            GameObject labelObject = CreateRect("Label", container.transform, new Vector2(0.055f, 0f), new Vector2(0.30f, 1f));
            AddText(labelObject, label, 14, new Color(1f, 1f, 1f, 0.76f), TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject track = CreateRect("Track", container.transform, new Vector2(0.31f, 0.34f), new Vector2(0.77f, 0.66f));
            AddImage(track, new Color(1f, 1f, 1f, 0.17f)).raycastTarget = false;
            GameObject fillObject = CreateRect("Fill", track.transform, Vector2.zero, Vector2.one);
            fill = AddImage(fillObject, color);
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;

            GameObject valueObject = CreateRect("Value", container.transform, new Vector2(0.79f, 0f), new Vector2(0.96f, 1f));
            value = AddText(valueObject, "00%", 16, White, TextAnchor.MiddleRight, FontStyle.Bold);
        }

        private Text CreateArticleCard(
            Transform parent,
            float minX,
            float maxX,
            float minY,
            float maxY,
            string title,
            string body,
            Color accent)
        {
            GameObject card = CreateRect(title, parent, new Vector2(minX, minY), new Vector2(maxX, maxY));
            AddImage(card, new Color(accent.r, accent.g, accent.b, 0.13f));
            GameObject stripe = CreateRect("Stripe", card.transform, new Vector2(0f, 0f), new Vector2(0.035f, 1f));
            AddImage(stripe, accent);
            GameObject titleObject = CreateRect("Title", card.transform, new Vector2(0.10f, 0.66f), new Vector2(0.90f, 0.90f));
            AddText(titleObject, title, 19, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            GameObject bodyObject = CreateRect("Body", card.transform, new Vector2(0.10f, 0.12f), new Vector2(0.90f, 0.64f));
            Text bodyText = AddText(bodyObject, body, 18, Ink, TextAnchor.UpperLeft);
            bodyText.resizeTextForBestFit = true;
            bodyText.resizeTextMinSize = 14;
            bodyText.resizeTextMaxSize = 18;
            return bodyText;
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
            colors.fadeDuration = 0.08f;
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
