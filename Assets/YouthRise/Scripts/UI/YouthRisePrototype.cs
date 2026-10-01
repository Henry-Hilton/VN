using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace YouthRise
{
    public sealed partial class YouthRisePrototype : MonoBehaviour
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
        private GameObject seasonEndingScreen;
        private GameObject seasonClosingPanel;
        private GameObject seasonJourneyPanel;
        private GameObject safeChatPanel;
        private GameObject safeArticlesPanel;
        private GameObject safeFinancialPanel;
        private GameObject safeLifestylePanel;
        private GameObject safeFamilyPanel;
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
        private Text chapterThreeMenuLabel;
        private Text chapterFourMenuLabel;
        private Text chapterFiveMenuLabel;
        private Text chapterSixMenuLabel;
        private Text chapterSevenMenuLabel;
        private Text chapterEightMenuLabel;
        private Text familySupportArticleBody;
        private Text safeAdultArticleBody;
        private Text seasonClosingText;
        private Text seasonJourneyText;
        private Text healthyLifestyleArticleBody;
        private Text healthyRoutineGuideBody;
        private Text financialSafetyArticleBody;
        private Text moneySmartGuideBody;
        private Text bullyingArticleBody;
        private Text healthyRelationshipArticleBody;
        private Text digitalSafetyGuideBody;
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
        private Button chapterThreeMenuButton;
        private Button chapterFourMenuButton;
        private Button chapterFiveMenuButton;
        private Button chapterSixMenuButton;
        private Button chapterSevenMenuButton;
        private Button chapterEightMenuButton;
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
            storyAudio = gameObject.AddComponent<StoryAudio>();
            onlineSettings = OnlineSupportSettings.Load();
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
            seasonEndingScreen = BuildSeasonEndingScreen(canvasRect);
            BuildLecturerScreens(canvasRect);
            BuildAccountScreens(canvasRect);

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

            GameObject eyebrow = CreateRect("Eyebrow", root.transform, new Vector2(0.18f, 0.85f), new Vector2(0.82f, 0.92f));
            AddText(eyebrow, "YOUTHRise • INTERACTIVE STORY", 24, Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject title = CreateRect("Title", root.transform, new Vector2(0.18f, 0.63f), new Vector2(0.82f, 0.85f));
            menuTitleText = AddText(title, "THE FIRST\nDAY", 96, White, TextAnchor.MiddleLeft, FontStyle.Bold);
            menuTitleText.resizeTextForBestFit = true;
            menuTitleText.resizeTextMinSize = 58;
            menuTitleText.resizeTextMaxSize = 96;

            GameObject subtitle = CreateRect("Subtitle", root.transform, new Vector2(0.18f, 0.52f), new Vector2(0.82f, 0.63f));
            menuSubtitleText = AddText(subtitle, "Chapter 1 • Hari pertama Alex di sekolah baru", 30, Paper, TextAnchor.MiddleLeft);

            GameObject feature = CreateRect("Features", root.transform, new Vector2(0.18f, 0.43f), new Vector2(0.82f, 0.51f));
            menuFeatureText = AddText(feature, "DIALOG PCG LOKAL   •   PILIHAN BERCABANG   •   SAFE ZONE", 19, new Color(1f, 1f, 1f, 0.65f), TextAnchor.MiddleLeft, FontStyle.Bold);

            Button start = CreateButton(root.transform, "Start", "CHAPTER 1 • MULAI BARU", new Vector2(0.18f, 0.34f), new Vector2(0.332f, 0.41f), Blue, White, 16);
            start.onClick.AddListener(StartNewGame);

            chapterTwoMenuButton = CreateButton(root.transform, "Start Chapter 2", "CHAPTER 2 • TERKUNCI", new Vector2(0.343f, 0.34f), new Vector2(0.495f, 0.41f), Coral, White, 16);
            chapterTwoMenuLabel = chapterTwoMenuButton.GetComponentInChildren<Text>();
            chapterTwoMenuButton.onClick.AddListener(StartChapterTwo);

            chapterThreeMenuButton = CreateButton(root.transform, "Start Chapter 3", "CHAPTER 3 • TERKUNCI", new Vector2(0.505f, 0.34f), new Vector2(0.657f, 0.41f), Gold, Navy, 16);
            chapterThreeMenuLabel = chapterThreeMenuButton.GetComponentInChildren<Text>();
            chapterThreeMenuButton.onClick.AddListener(StartChapterThree);

            chapterFourMenuButton = CreateButton(root.transform, "Start Chapter 4", "CHAPTER 4 • TERKUNCI", new Vector2(0.668f, 0.34f), new Vector2(0.82f, 0.41f), Mint, Navy, 16);
            chapterFourMenuLabel = chapterFourMenuButton.GetComponentInChildren<Text>();
            chapterFourMenuButton.onClick.AddListener(StartChapterFour);

            chapterFiveMenuButton = CreateButton(root.transform, "Start Chapter 5", "CHAPTER 5 • TERKUNCI", new Vector2(0.18f, 0.26f), new Vector2(0.332f, 0.33f), Gold, Navy, 16);
            chapterFiveMenuLabel = chapterFiveMenuButton.GetComponentInChildren<Text>();
            chapterFiveMenuButton.onClick.AddListener(StartChapterFive);

            chapterSixMenuButton = CreateButton(root.transform, "Start Chapter 6", "CHAPTER 6 • TERKUNCI", new Vector2(0.343f, 0.26f), new Vector2(0.495f, 0.33f), Cyan, Navy, 16);
            chapterSixMenuLabel = chapterSixMenuButton.GetComponentInChildren<Text>();
            chapterSixMenuButton.onClick.AddListener(StartChapterSix);

            chapterSevenMenuButton = CreateButton(root.transform, "Start Chapter 7", "CHAPTER 7 • TERKUNCI", new Vector2(0.505f, 0.26f), new Vector2(0.657f, 0.33f), Blue, White, 16);
            chapterSevenMenuLabel = chapterSevenMenuButton.GetComponentInChildren<Text>();
            chapterSevenMenuButton.onClick.AddListener(StartChapterSeven);

            chapterEightMenuButton = CreateButton(root.transform, "Start Chapter 8", "CHAPTER 8 • TERKUNCI", new Vector2(0.668f, 0.26f), new Vector2(0.82f, 0.33f), Mint, Navy, 16);
            chapterEightMenuLabel = chapterEightMenuButton.GetComponentInChildren<Text>();
            chapterEightMenuButton.onClick.AddListener(StartChapterEight);

            continueMenuButton = CreateButton(root.transform, "Continue", "LANJUTKAN", new Vector2(0.18f, 0.18f), new Vector2(0.495f, 0.25f), Cyan, Navy, 20);
            continueMenuButton.onClick.AddListener(ContinueGame);

            safeZoneMenuButton = CreateButton(root.transform, "Safe Zone", "SAFE ZONE • TERKUNCI", new Vector2(0.505f, 0.18f), new Vector2(0.82f, 0.25f), Mint, Navy, 20);
            safeZoneMenuLabel = safeZoneMenuButton.GetComponentInChildren<Text>();
            safeZoneMenuButton.onClick.AddListener(ShowSafeZone);

            GameObject privacy = CreateRect("Privacy", root.transform, new Vector2(0.18f, 0.035f), new Vector2(0.82f, 0.145f));
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

            // Resources retains the shader in player builds, including Android.
            Shader chromaShader = Resources.Load<Shader>("YouthRise/UIColorKey");
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

            BuildAudioControls(topBar.transform);

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

                GameObject choiceAccent = CreateRect("Accent", choiceButtons[index].transform, new Vector2(0.02f, 0.15f), new Vector2(0.026f, 0.85f));
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

        private GameObject BuildSeasonEndingScreen(RectTransform parent)
        {
            GameObject root = CreateRect("Season Ending Screen", parent, Vector2.zero, Vector2.one);
            Image background = AddImage(root, Navy);
            background.sprite = LoadArtSprite("YouthRise/Art/Backgrounds/bg_home");
            background.color = background.sprite != null ? White : Navy;
            AddImage(CreateRect("Ending Wash", root.transform, Vector2.zero, Vector2.one),
                new Color(Navy.r, Navy.g, Navy.b, 0.92f)).raycastTarget = false;
            AddText(CreateRect("Ending Kicker", root.transform, new Vector2(0.14f, 0.81f), new Vector2(0.86f, 0.9f)),
                "YOUTHRise • SEASON 1", 25, Mint, TextAnchor.MiddleCenter, FontStyle.Bold);

            seasonClosingPanel = CreateRect("Closing Message", root.transform, Vector2.zero, Vector2.one);
            seasonClosingText = AddText(CreateRect("Closing Lines", seasonClosingPanel.transform, new Vector2(0.18f, 0.37f), new Vector2(0.82f, 0.78f)),
                string.Empty, 34, Paper, TextAnchor.MiddleCenter);
            Button next = CreateButton(seasonClosingPanel.transform, "Season Ending Continue", "LANJUT →", new Vector2(0.36f, 0.17f), new Vector2(0.64f, 0.27f), Mint, Navy, 22);
            next.onClick.AddListener(() => ShowSeasonEnding(true));

            seasonJourneyPanel = CreateRect("Journey Message", root.transform, Vector2.zero, Vector2.one);
            seasonJourneyText = AddText(CreateRect("Journey Heading", seasonJourneyPanel.transform, new Vector2(0.12f, 0.49f), new Vector2(0.88f, 0.77f)),
                string.Empty, 66, White, TextAnchor.MiddleCenter, FontStyle.Bold);
            seasonJourneyText.resizeTextForBestFit = true;
            seasonJourneyText.resizeTextMinSize = 38;
            seasonJourneyText.resizeTextMaxSize = 66;
            AddText(CreateRect("Journey Recap", seasonJourneyPanel.transform, new Vector2(0.19f, 0.32f), new Vector2(0.81f, 0.46f)),
                "Dari hari pertama, tekanan teman, bullying, dan batas pribadi, hingga emosi, uang, kebiasaan sehat, dunia digital, dan keluarga.\n\nKamu tidak harus menghadapi semuanya sendirian.",
                22, Paper, TextAnchor.MiddleCenter);
            Button support = CreateButton(seasonJourneyPanel.transform, "Ending Family Support", "FAMILY & SUPPORT", new Vector2(0.18f, 0.14f), new Vector2(0.49f, 0.25f), Mint, Navy, 22);
            support.onClick.AddListener(() => { ShowSafeZone(); ShowSafeTab("family"); });
            Button menu = CreateButton(seasonJourneyPanel.transform, "Ending Menu", "KEMBALI KE MENU", new Vector2(0.51f, 0.14f), new Vector2(0.82f, 0.25f), Cyan, Navy, 22);
            menu.onClick.AddListener(ShowStartMenu);
            return root;
        }

        private void ShowSeasonEnding(bool journey)
        {
            if (!IsChapterEight() || !chapterCompleted)
                return;
            seasonClosingText.text = string.Join("\n\n", story.Chapter.endingLines ?? new string[0]);
            seasonJourneyText.text = story.Chapter.endingHeading ?? string.Empty;
            if (journey && seasonEndingScreen.activeSelf && seasonClosingPanel.activeSelf)
            {
                if (screenTransition != null)
                    return;
                screenTransition = StartCoroutine(CrossfadeScreens(seasonClosingPanel, seasonJourneyPanel, false));
                return;
            }
            foreach (GameObject panel in new[] { seasonClosingPanel, seasonJourneyPanel })
            {
                CanvasGroup group = GetOrAddCanvasGroup(panel);
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
            seasonClosingPanel.SetActive(!journey);
            seasonJourneyPanel.SetActive(journey);
            ShowScreenSmooth(seasonEndingScreen);
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
            Button chatTab = CreateButton(tabs.transform, "Chat Tab", "CHAT PENDAMPING", new Vector2(0f, 0f), new Vector2(0.152f, 1f), Blue, White, 17);
            chatTab.onClick.AddListener(() => ShowSafeTab("chat"));
            Button articleTab = CreateButton(tabs.transform, "Article Tab", "ARTIKEL SINGKAT", new Vector2(0.17f, 0f), new Vector2(0.322f, 1f), Cyan, Navy, 17);
            articleTab.onClick.AddListener(() => ShowSafeTab("articles"));
            Button financialTab = CreateButton(tabs.transform, "Financial Tab", "FINANSIAL", new Vector2(0.34f, 0f), new Vector2(0.492f, 1f), Gold, Navy, 17);
            financialTab.onClick.AddListener(() => ShowSafeTab("financial"));
            Button lifestyleTab = CreateButton(tabs.transform, "Lifestyle Tab", "GAYA HIDUP", new Vector2(0.508f, 0f), new Vector2(0.66f, 1f), Mint, Navy, 17);
            lifestyleTab.onClick.AddListener(() => ShowSafeTab("lifestyle"));
            Button familyTab = CreateButton(tabs.transform, "Family Tab", "KELUARGA", new Vector2(0.678f, 0f), new Vector2(0.83f, 1f), Mint, Navy, 17);
            familyTab.onClick.AddListener(() => ShowSafeTab("family"));
            Button reportTab = CreateButton(tabs.transform, "Report Tab", "NEED EXTRA HELP?", new Vector2(0.848f, 0f), new Vector2(1f, 1f), Coral, White, 17);
            reportTab.onClick.AddListener(() => ShowSafeTab("report"));

            safeChatPanel = BuildChatPanel(root.transform);
            safeArticlesPanel = BuildArticlesPanel(root.transform);
            safeFinancialPanel = BuildFinancialPanel(root.transform);
            safeLifestylePanel = BuildLifestylePanel(root.transform);
            safeFamilyPanel = BuildFamilyPanel(root.transform);
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
                "RUANG CURHAT LOKAL — bukan konselor manusia atau layanan darurat. Chat biasa tidak dikirim. Pesan mendesak dapat dibagikan ke konselor setelah konfirmasi.",
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
            chatSendButton = send;
            send.onClick.AddListener(SendSafeZoneChat);
            Button connect = CreateButton(panel.transform, "Connection Settings", "KONEKSI / PRIVASI", new Vector2(0.64f, 0.91f), new Vector2(0.95f, 0.995f), Mint, Navy, 16);
            connect.onClick.AddListener(ShowConnectionSettings);
            chatResponse.supportRichText = false;
            chatResponse.resizeTextForBestFit = true;
            chatResponse.resizeTextMinSize = 18;
            chatResponse.resizeTextMaxSize = 23;

            return panel;
        }

        private GameObject BuildArticlesPanel(Transform parent)
        {
            GameObject panel = CreateRect("Articles Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);

            CreateArticleCard(panel.transform, 0.04f, 0.33f, 0.53f, 0.93f, "TEKANAN TEMAN SEBAYA", "Kamu boleh menolak tanpa menjelaskan panjang. Cari dukungan untuk keputusan yang aman.", Blue);
            CreateArticleCard(panel.transform, 0.355f, 0.645f, 0.53f, 0.93f, "MENGELOLA CEMAS", "Tarik napas, beri nama pada perasaanmu, lalu pilih satu langkah kecil yang aman.", Cyan);
            CreateArticleCard(panel.transform, 0.67f, 0.96f, 0.53f, 0.93f, "BATAS DAN RASA AMAN", "Rasa tidak nyaman adalah alasan yang cukup untuk berhenti, menjauh, atau berkata tidak.", Coral);
            bullyingArticleBody = CreateArticleCard(panel.transform, 0.04f, 0.33f, 0.07f, 0.47f, "DUKUNGAN BULLYING", "TERKUNCI • Selesaikan Chapter 2.", Gold);
            healthyRelationshipArticleBody = CreateArticleCard(panel.transform, 0.355f, 0.645f, 0.07f, 0.47f, "HUBUNGAN SEHAT", "TERKUNCI • Selesaikan Chapter 3.", Mint);
            digitalSafetyGuideBody = CreateArticleCard(panel.transform, 0.67f, 0.96f, 0.07f, 0.47f, "KEAMANAN DIGITAL", "TERKUNCI • Selesaikan Chapter 3.", Blue);

            return panel;
        }

        private GameObject BuildFinancialPanel(Transform parent)
        {
            GameObject panel = CreateRect("Financial Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);
            financialSafetyArticleBody = CreateArticleCard(panel.transform, 0.04f, 0.485f, 0.07f, 0.93f,
                "FINANCIAL SAFETY", "TERKUNCI • Selesaikan Chapter 5.", Gold, true);
            moneySmartGuideBody = CreateArticleCard(panel.transform, 0.515f, 0.96f, 0.07f, 0.93f,
                "MONEY SMART GUIDE", "TERKUNCI • Selesaikan Chapter 5.", Mint, true);
            return panel;
        }

        private void BuildFinancialContent()
        {
            financialSafetyArticleBody.text = profile != null && profile.financialSafetyArticleUnlocked
                ? "BERHENTI • PERIKSA • MINTA BANTUAN\n\nPeriksa legalitas penyedia melalui kanal resmi OJK bersama orang dewasa tepercaya. Pahami bunga, biaya, denda, dan total pembayaran.\n\nHadiah yang meminta transfer lebih dulu patut dicurigai. Jangan kirim uang atau data pribadi; verifikasi lewat kanal resmi yang kamu cari sendiri.\n\nJika sudah terlanjur, simpan bukti dan minta bantuan orang dewasa. Jangan menambah pinjaman untuk menutup utang."
                : "TERKUNCI • Selesaikan Chapter 5 untuk membuka Financial Safety.";
            moneySmartGuideBody.text = profile != null && profile.moneySmartGuideUnlocked
                ? "KEBUTUHAN DULU, KEINGINAN KEMUDIAN\n\nCatat uang yang tersedia. Dahulukan makan dan transportasi; sisihkan tabungan sesuai kemampuan. Beri jeda sebelum membeli barang karena tren.\n\nContoh anggaran fiktif: Rp50.000 = makan Rp20.000 + transportasi Rp15.000 + tabungan Rp10.000 + sisa Rp5.000. Sesuaikan dengan kebutuhanmu.\n\nCatat pengeluaran dan cek sisa anggaran. Untuk pelajar, menabung untuk keinginan lebih aman daripada terburu-buru berutang."
                : "TERKUNCI • Selesaikan Chapter 5 untuk membuka Money Smart Guide.";
        }

        private GameObject BuildLifestylePanel(Transform parent)
        {
            GameObject panel = CreateRect("Lifestyle Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);
            healthyLifestyleArticleBody = CreateArticleCard(panel.transform, 0.04f, 0.485f, 0.07f, 0.93f,
                "HEALTHY LIFESTYLE", "TERKUNCI • Selesaikan Chapter 6.", Mint, true);
            healthyRoutineGuideBody = CreateArticleCard(panel.transform, 0.515f, 0.96f, 0.07f, 0.93f,
                "MY HEALTHY ROUTINE", "TERKUNCI • Selesaikan Chapter 6.", Cyan, true);
            return panel;
        }

        private void UpdateLifestyleContent()
        {
            healthyLifestyleArticleBody.text = profile != null && profile.healthyLifestyleArticleUnlocked
                ? "RAWAT DIRI, TANPA HARUS SEMPURNA\n\nTidur cukup membantu perhatian dan konsentrasi. Usahakan waktu tidur teratur; beri jeda dari layar dan hindari kafein menjelang malam.\n\nPilih gerak yang nyaman dan sesuai kemampuan, termasuk gerak sambil duduk. Makan teratur dan beragam: makanan pokok, lauk, sayur, dan buah sesuai yang tersedia.\n\nSediakan air minum dan cuci tangan dengan sabun sebelum makan. Tujuannya mendukung energi dan kenyamanan, bukan mengubah bentuk tubuh."
                : "TERKUNCI • Selesaikan Chapter 6 untuk membuka Healthy Lifestyle.";
            healthyRoutineGuideBody.text = profile != null && profile.healthyRoutineGuideUnlocked
                ? "SATU LANGKAH KECIL MINGGU INI\n\nPilih satu kebiasaan: siapkan air minum, sisihkan waktu makan, bergerak dengan nyaman, atau akhiri game sebelum waktu tidur.\n\nTentukan kapan dan minta dukungan bila perlu. Contoh: setelah pulang sekolah, bergerak sebentar sesuai kemampuan. Catat apa yang terasa membantu, bukan mengejar rekor.\n\nTinjau di akhir minggu dan sesuaikan. Jika terlewat, mulai lagi tanpa menghukum diri. Bila sulit tidur, makan, atau merawat diri, bicaralah dengan orang dewasa tepercaya atau tenaga kesehatan."
                : "TERKUNCI • Selesaikan Chapter 6 untuk membuka My Healthy Routine.";
        }

        private GameObject BuildFamilyPanel(Transform parent)
        {
            GameObject panel = CreateRect("Family Panel", parent, new Vector2(0.045f, 0.095f), new Vector2(0.955f, 0.71f));
            AddImage(panel, Paper);
            familySupportArticleBody = CreateArticleCard(panel.transform, 0.04f, 0.485f, 0.07f, 0.93f,
                "FAMILY & SUPPORT", "TERKUNCI • Selesaikan Chapter 8.", Mint, true);
            safeAdultArticleBody = CreateArticleCard(panel.transform, 0.515f, 0.96f, 0.07f, 0.93f,
                "ORANG DEWASA AMAN", "TERKUNCI • Selesaikan Chapter 8.", Gold, true);
            return panel;
        }

        private void UpdateFamilyContent()
        {
            bool unlocked = profile != null && profile.familySupportArticleUnlocked;
            familySupportArticleBody.text = unlocked
                ? "KAMU TIDAK HARUS MEMPERBAIKI SEMUANYA\n\nPerbedaan pendapat tidak selalu berarti tidak ada kasih sayang. Namun, tekanan tidak membenarkan ancaman atau kekerasan. Masalah dan utang orang dewasa bukan tanggung jawabmu untuk diselesaikan.\n\nJika aman, pilih waktu tenang. Coba: 'Aku merasa tertekan saat dibandingkan. Aku ingin didengarkan.' Kamu boleh mengambil jeda atau minta didampingi.\n\nKamu tetap berhak mendapat dukungan, meski belum siap bicara dengan keluarga. Tidak semua masalah selesai dalam satu percakapan."
                : "TERKUNCI • Selesaikan Chapter 8 untuk membuka Family & Support.";
            safeAdultArticleBody.text = unlocked
                ? "CARI DUKUNGAN YANG MENJAGA KESELAMATANMU\n\nOrang dewasa aman mendengarkan, tidak menyalahkan, dan membantu melindungimu. Bisa Guru BK, guru lain, kerabat yang aman, atau tenaga profesional. Tidak harus orang tua.\n\nMulai dengan: 'Aku butuh bantuan. Aku merasa tidak aman atau kewalahan di rumah.' Jika orang pertama tidak membantu, cari orang aman lainnya.\n\nJika ada ancaman atau kekerasan, utamakan tempat aman dan bantuan langsung. Jangan mencoba menengahi sendirian. Chat dan draft di game ini tidak menghubungi bantuan atau layanan darurat."
                : "TERKUNCI • Panduan orang dewasa aman terbuka bersama Family & Support setelah Chapter 8.";
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
                "Belum dianalisis.\n\nMulai dari draft lokal. Tidak ada laporan dikirim otomatis. Tinjau / Kirim membuka konfirmasi pengiriman ke dashboard konselor lokal.",
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

            Button submit = CreateButton(panel.transform, "Preview Incident", "TINJAU / KIRIM", new Vector2(0.81f, 0.08f), new Vector2(0.955f, 0.23f), Gold, Navy, 16);
            submit.onClick.AddListener(() => PreviewSharing(false));
            Button journey = CreateButton(panel.transform, "Preview Journey", "RINGKASAN PILIHAN 8 ISU", new Vector2(0.60f, 0.93f), new Vector2(0.955f, 0.995f), Mint, Navy, 16);
            journey.onClick.AddListener(() => PreviewSharing(true));
            reportInput.characterLimit = 1500;
            reportInput.onValueChanged.AddListener(_ => { currentAssessment = null; SetButtonEnabled(saveDraftButton, false); });

            return panel;
        }

        private void StartNewGame()
        {
            ClearSupportSession();
            ClearReportForm();
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

        private void StartChapterThree()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }

            if (!CampaignProgression.CanStartChapterThree(profile))
            {
                ShowToast("Selesaikan Chapter 2 untuk membuka Chapter 3.", false);
                return;
            }

            try
            {
                story = StoryRepository.LoadChapterThree();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 3 gagal dimuat.", true);
                return;
            }

            profile.PrepareForChapterThree();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void StartChapterFour()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }

            if (!CampaignProgression.CanStartChapterFour(profile))
            {
                ShowToast("Selesaikan Chapter 3 untuk membuka Chapter 4.", false);
                return;
            }

            try
            {
                story = StoryRepository.LoadChapterFour();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 4 gagal dimuat.", true);
                return;
            }

            profile.PrepareForChapterFour();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void StartChapterFive()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }

            if (!CampaignProgression.CanStartChapterFive(profile))
            {
                ShowToast("Selesaikan Chapter 4 untuk membuka Chapter 5.", false);
                return;
            }

            try
            {
                story = StoryRepository.LoadChapterFive();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 5 gagal dimuat.", true);
                return;
            }

            profile.PrepareForChapterFive();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void StartChapterSix()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }

            if (!CampaignProgression.CanStartChapterSix(profile))
            {
                ShowToast("Selesaikan Chapter 5 untuk membuka Chapter 6.", false);
                return;
            }

            try
            {
                story = StoryRepository.LoadChapterSix();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 6 gagal dimuat.", true);
                return;
            }

            profile.PrepareForChapterSix();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void StartChapterSeven()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }

            if (!CampaignProgression.CanStartChapterSeven(profile))
            {
                ShowToast("Selesaikan Chapter 6 untuk membuka Chapter 7.", false);
                return;
            }

            try
            {
                story = StoryRepository.LoadChapterSeven();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 7 gagal dimuat.", true);
                return;
            }

            profile.PrepareForChapterSeven();
            ResetMeterAnimation();
            branchPath = string.Empty;
            chapterCompleted = false;
            sessionSeed = Guid.NewGuid().GetHashCode();
            telemetry = new DecisionTelemetry(story.Chapter.id);
            telemetry.RecordSessionStarted(profile);
            ShowNode(story.Chapter.startNodeId);
        }

        private void StartChapterEight()
        {
            if (PrototypeSaveService.TryLoad(out PrototypeSave saved) && saved.profile != null)
            {
                NormalizeLoadedProgress(saved);
                profile = saved.profile;
            }
            if (!CampaignProgression.CanStartChapterEight(profile))
            {
                ShowToast("Selesaikan Chapter 7 untuk membuka Chapter 8.", false);
                return;
            }
            try { story = StoryRepository.LoadChapterEight(); }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowToast("Chapter 8 gagal dimuat.", true);
                return;
            }
            profile.PrepareForChapterEight();
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
                ShowChapterReview();
            else
                ShowNode(story.Contains(save.currentNodeId) ? save.currentNodeId : story.Chapter.startNodeId);
        }

        private static void NormalizeLoadedProgress(PrototypeSave save)
        {
            CampaignProgression.Normalize(save);
        }

        private void ShowNode(string nodeId)
        {
            storyAudio.StopNarration();
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
            NarrateCurrentNode();
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
            if (node.id == story.Chapter.startNodeId && string.IsNullOrEmpty(branchPath))
                PlayerJourneyReport.BeginChapter(profile, story.Chapter.number);
            currentNode = node;
            locationText.text = (node.location ?? string.Empty).ToUpperInvariant();
            speakerName.text = (node.speaker ?? "Narasi").ToUpperInvariant();
            speakerInitials.text = GetInitials(node.speaker);
            storyChapterCaption.text = $"CHAPTER {Mathf.Max(1, story.Chapter.number):00}  •  {(story.Chapter.title ?? string.Empty).ToUpperInvariant()}";
            dialogueText.text = CharacterText(conversationGenerator.Generate(node, profile, sessionSeed));
            speakerName.text = CharacterText(speakerName.text);
            speakerInitials.text = GetInitials(CharacterText(node.speaker));
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
                choiceLabels[index].text = CharacterText(selectedChoice.label);
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
            bool anita = account?.gender == "female" && (speaker ?? "").StartsWith("Alex", StringComparison.OrdinalIgnoreCase);
            if (anita) resource = "YouthRise/Art/Characters/char_anita";
            characterPortrait.material = anita ? null : chromaKeyMaterial;
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
            if (normalized == "coach sarah") return "YouthRise/Art/Characters/char_coach_sarah_chroma";
            if (normalized.StartsWith("maya")) return "YouthRise/Art/Characters/char_maya_chroma";
            if (normalized.StartsWith("kevin")) return "YouthRise/Art/Characters/char_kevin_chroma";
            if (normalized.StartsWith("rina")) return "YouthRise/Art/Characters/char_rina_chroma";
            if (normalized.StartsWith("leo")) return "YouthRise/Art/Characters/char_leo_chroma";
            if (normalized.StartsWith("sarah")) return "YouthRise/Art/Characters/char_sarah_chroma";
            if (normalized.StartsWith("ibu") || normalized == "mom") return "YouthRise/Art/Characters/char_ibu_chroma";
            if (normalized.StartsWith("ayah") || normalized == "dad") return "YouthRise/Art/Characters/char_dad_chroma";
            if (normalized.StartsWith("senior")) return "YouthRise/Art/Characters/char_senior_chroma";
            if (normalized.Contains("arman")) return "YouthRise/Art/Characters/char_mr_arman_chroma";
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
            PlayerJourneyReport.Record(profile, story.Chapter.number, currentNode, choice);
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

            ShowChapterReview();
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
            if (IsChapterEight())
                ShowSeasonEnding(false);
            else if (IsChapterSeven())
                StartChapterEight();
            else if (IsChapterSix())
                StartChapterSeven();
            else if (IsChapterFive())
                StartChapterSix();
            else if (IsChapterFour())
                StartChapterFive();
            else if (IsChapterThree())
                StartChapterFour();
            else if (IsChapterTwo())
                StartChapterThree();
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
            bool chapterThree = IsChapterThree();
            bool chapterFour = IsChapterFour();
            completionPrimaryLabel.text = IsChapterEight()
                ? "AKHIR SEASON 1 →"
                : IsChapterSeven()
                    ? "MULAI CHAPTER 8"
                : IsChapterSix()
                    ? "MULAI CHAPTER 7"
                    : IsChapterFive()
                    ? "MULAI CHAPTER 6"
                    : chapterFour
                    ? "MULAI CHAPTER 5"
                    : chapterThree
                    ? "MULAI CHAPTER 4"
                    : chapterTwo
                        ? "MULAI CHAPTER 3"
                        : "MULAI CHAPTER 2";
            completionBackground.sprite = LoadArtSprite(chapterFour
                ? "YouthRise/Art/Backgrounds/bg_bedroom"
                : chapterThree
                    ? "YouthRise/Art/Backgrounds/bg_cafeteria"
                    : chapterTwo
                        ? "YouthRise/Art/Backgrounds/bg_classroom"
                        : "YouthRise/Art/Backgrounds/bg_bedroom");
            completionBackground.color = completionBackground.sprite != null ? White : Navy;
        }

        private void ShowStartMenu()
        {
            if (account == null) { SetScreen(accountScreen); return; }
            storyAudio.StopNarration();
            ShowScreenSmooth(startScreen);

            bool hasSave = PrototypeSaveService.TryLoad(out PrototypeSave save);
            if (hasSave && save.profile != null)
            {
                NormalizeLoadedProgress(save);
                profile = save.profile;
            }

            continueMenuButton.gameObject.SetActive(hasSave);
            bool unlocked = hasSave && save.profile != null && save.profile.safeZoneUnlocked;
            SetButtonEnabled(safeZoneMenuButton, true);
            safeZoneMenuLabel.text = "SAFE ZONE • CURHAT & BANTUAN";

            bool chapterTwoUnlocked = hasSave && save.profile != null && save.profile.completedChapterOne;
            SetButtonEnabled(chapterTwoMenuButton, chapterTwoUnlocked);
            chapterTwoMenuLabel.text = !chapterTwoUnlocked
                ? "CHAPTER 2 • TERKUNCI"
                : save.profile.completedChapterTwo
                    ? "ULANGI CHAPTER 2"
                    : "MULAI CHAPTER 2";

            bool chapterThreeUnlocked = hasSave && save.profile != null && save.profile.completedChapterTwo;
            SetButtonEnabled(chapterThreeMenuButton, chapterThreeUnlocked);
            chapterThreeMenuLabel.text = !chapterThreeUnlocked
                ? "CHAPTER 3 • TERKUNCI"
                : save.profile.completedChapterThree
                    ? "ULANGI CHAPTER 3"
                    : "MULAI CHAPTER 3";

            bool chapterFourUnlocked = hasSave && save.profile != null && save.profile.completedChapterThree;
            SetButtonEnabled(chapterFourMenuButton, chapterFourUnlocked);
            chapterFourMenuLabel.text = !chapterFourUnlocked
                ? "CHAPTER 4 • TERKUNCI"
                : save.profile.completedChapterFour
                    ? "ULANGI CHAPTER 4"
                    : "MULAI CHAPTER 4";

            bool chapterFiveUnlocked = hasSave && CampaignProgression.CanStartChapterFive(save.profile);
            SetButtonEnabled(chapterFiveMenuButton, chapterFiveUnlocked);
            chapterFiveMenuLabel.text = !chapterFiveUnlocked
                ? "CHAPTER 5 • TERKUNCI"
                : save.profile.completedChapterFive ? "ULANGI CHAPTER 5" : "MULAI CHAPTER 5";

            bool chapterSixUnlocked = hasSave && CampaignProgression.CanStartChapterSix(save.profile);
            SetButtonEnabled(chapterSixMenuButton, chapterSixUnlocked);
            chapterSixMenuLabel.text = !chapterSixUnlocked
                ? "CHAPTER 6 • TERKUNCI"
                : save.profile.completedChapterSix ? "ULANGI CHAPTER 6" : "MULAI CHAPTER 6";

            bool chapterSevenUnlocked = hasSave && CampaignProgression.CanStartChapterSeven(save.profile);
            SetButtonEnabled(chapterSevenMenuButton, chapterSevenUnlocked);
            chapterSevenMenuLabel.text = !chapterSevenUnlocked
                ? "CHAPTER 7 • TERKUNCI"
                : save.profile.completedChapterSeven ? "ULANGI CHAPTER 7" : "MULAI CHAPTER 7";

            bool chapterEightUnlocked = hasSave && CampaignProgression.CanStartChapterEight(save.profile);
            SetButtonEnabled(chapterEightMenuButton, chapterEightUnlocked);
            chapterEightMenuLabel.text = !chapterEightUnlocked
                ? "CHAPTER 8 • TERKUNCI"
                : save.profile.completedChapterEight ? "ULANGI CHAPTER 8" : "MULAI CHAPTER 8";

            if (chapterEightUnlocked)
            {
                menuTitleText.text = "HOME IS\nCOMPLICATED";
                menuSubtitleText.text = "Chapter 8 • Keluarga, komunikasi, dan dukungan aman";
            }
            else if (chapterSevenUnlocked)
            {
                menuTitleText.text = "ALWAYS\nCONNECTED";
                menuSubtitleText.text = "Chapter 7 • FOMO, waktu layar, dan keseimbangan digital";
            }
            else if (chapterSixUnlocked)
            {
                menuTitleText.text = "TAKE CARE\nOF YOU";
                menuSubtitleText.text = "Chapter 6 • Istirahat, gerak, dan kebiasaan sehat";
            }
            else if (chapterFiveUnlocked)
            {
                menuTitleText.text = "EASY\nMONEY?";
                menuSubtitleText.text = "Chapter 5 • Literasi finansial, pinjol, dan mengenali scam";
            }
            else if (chapterFourUnlocked)
            {
                menuTitleText.text = "YOU DON'T HAVE\nTO BE STRONG";
                menuSubtitleText.text = "Chapter 4 • Mental well-being, coping, dan mencari bantuan";
            }
            else if (chapterThreeUnlocked)
            {
                menuTitleText.text = "MORE THAN A\nCRUSH";
                menuSubtitleText.text = "Chapter 3 • Hubungan sehat, batas pribadi, dan keamanan digital";
            }
            else if (chapterTwoUnlocked)
            {
                menuTitleText.text = "BEHIND THE\nSMILE";
                menuSubtitleText.text = "Chapter 2 • Bullying, keberanian, dan mencari bantuan";
            }
            else
            {
                menuTitleText.text = "THE FIRST\nDAY";
                menuSubtitleText.text = "Chapter 1 • Hari pertama Alex di sekolah baru";
            }

            menuFeatureText.text = hasSave && save.profile != null && save.profile.completedChapterEight
                ? "SEASON 1 • SELESAI   •   FAMILY & SUPPORT TERBUKA"
                : chapterEightUnlocked
                    ? "SEASON 1 FINALE • TERBUKA   •   CHAPTER 8"
                : chapterSevenUnlocked
                    ? "CHAPTER 6 • SELESAI   •   CHAPTER 7 TERBUKA"
                : chapterSixUnlocked
                    ? "CHAPTER 5 • SELESAI   •   CHAPTER 6 TERBUKA"
                : chapterFiveUnlocked
                    ? "CHAPTER 4 • SELESAI   •   CHAPTER 5 TERBUKA"
                : hasSave && save.profile != null && save.profile.completedChapterThree
                    ? "CHAPTER 3 • SELESAI   •   CHAPTER 4 TERBUKA"
                : hasSave && save.profile != null && save.profile.relationshipPathUnlocked
                    ? "RELATIONSHIP PATH • TERBUKA   •   CHAPTER 3"
                    : "DIALOG PCG LOKAL   •   PILIHAN BERCABANG   •   SAFE ZONE";
            ApplyCharacterText();
        }

        private void ShowSafeZone()
        {
            storyAudio.StopNarration();
            if ((profile == null || !profile.safeZoneUnlocked) &&
                PrototypeSaveService.TryLoad(out PrototypeSave save) &&
                save.profile != null && save.profile.safeZoneUnlocked)
            {
                profile = save.profile;
            }

            if (profile == null || account == null)
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
            safeFinancialPanel.SetActive(tab == "financial");
            safeLifestylePanel.SetActive(tab == "lifestyle");
            safeFamilyPanel.SetActive(tab == "family");
            safeReportPanel.SetActive(tab == "report");
        }

        private void UpdateSafeZoneArticles()
        {
            if (familySupportArticleBody != null && safeAdultArticleBody != null)
                UpdateFamilyContent();
            if (healthyLifestyleArticleBody != null && healthyRoutineGuideBody != null)
                UpdateLifestyleContent();
            if (financialSafetyArticleBody != null && moneySmartGuideBody != null)
                BuildFinancialContent();
            if (bullyingArticleBody == null || healthyRelationshipArticleBody == null || digitalSafetyGuideBody == null)
                return;

            bullyingArticleBody.text = profile != null && profile.bullyingSupportArticleUnlocked
                ? "Simpan bukti, dekati korban dengan aman, dan libatkan orang dewasa tepercaya. Diam juga merupakan sebuah pilihan."
                : "TERKUNCI • Selesaikan Chapter 2 untuk membuka artikel ini.";

            healthyRelationshipArticleBody.text = profile != null && profile.healthyRelationshipArticleUnlocked
                ? "Hubungan sehat memberi ruang untuk berkata tidak, memiliki waktu sendiri, dan didengarkan tanpa ancaman atau paksaan."
                : "TERKUNCI • Selesaikan Chapter 3 untuk membuka artikel ini.";

            digitalSafetyGuideBody.text = profile != null && profile.digitalSafetyGuideUnlocked
                ? "Jangan bagikan data atau foto pribadi karena tekanan. Blokir dan laporkan akun mencurigakan, simpan bukti, lalu cari bantuan."
                : "TERKUNCI • Selesaikan Chapter 3 untuk membuka panduan ini.";
        }

        private bool IsChapterTwo()
        {
            return string.Equals(story?.Chapter?.id, "chapter-2", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChapterThree()
        {
            return string.Equals(story?.Chapter?.id, "chapter-3", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChapterEight()
        {
            return string.Equals(story?.Chapter?.id, "chapter-8", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChapterSeven()
        {
            return string.Equals(story?.Chapter?.id, "chapter-7", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChapterSix()
        {
            return string.Equals(story?.Chapter?.id, "chapter-6", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChapterFive()
        {
            return string.Equals(story?.Chapter?.id, "chapter-5", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChapterFour()
        {
            return string.Equals(story?.Chapter?.id, "chapter-4", StringComparison.OrdinalIgnoreCase);
        }

        private void SendSafeZoneChat()
        {
            SendSupportChat();
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
            if (account == null) return;
            PrototypeSaveService.Save(new PrototypeSave
            {
                chapterId = story?.Chapter.id,
                currentNodeId = nodeId,
                branchPath = branchPath,
                chapterCompleted = completed,
                profile = profile
            });
            resultsDirty = true;
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
            MeterFill.SetValue(riskFill, displayedRisk / 100f);
            MeterFill.SetValue(trustFill, displayedTrust / 100f);
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
            // No score arrows or deltas before the end-of-chapter review.
            ShowToast("Pilihanmu membentuk perjalanan Alex.", false);
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
            // A direct navigation during a crossfade must cancel its pending target.
            if (screenTransition != null) { StopCoroutine(screenTransition); screenTransition = null; }
            if (account == null && active != localDashboardScreen) active = accountScreen;
            accountScreen.SetActive(active == accountScreen);
            if (localDashboardScreen != null) localDashboardScreen.SetActive(active == localDashboardScreen);
            incidentScreen.SetActive(active == incidentScreen);
            startScreen.SetActive(active == startScreen);
            storyScreen.SetActive(active == storyScreen);
            completionScreen.SetActive(active == completionScreen);
            safeZoneScreen.SetActive(active == safeZoneScreen);
            seasonEndingScreen.SetActive(active == seasonEndingScreen);
            if (chapterReviewScreen != null) chapterReviewScreen.SetActive(active == chapterReviewScreen);
            if (sharingScreen != null) sharingScreen.SetActive(active == sharingScreen);
            if (connectionScreen != null) connectionScreen.SetActive(active == connectionScreen);
            CanvasGroup activeGroup = GetOrAddCanvasGroup(active);
            activeGroup.alpha = 1f;
            activeGroup.interactable = true;
            activeGroup.blocksRaycasts = true;
            ApplyCharacterText();
        }

        private void ShowScreenSmooth(GameObject target)
        {
            ApplyCharacterText();
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

        private IEnumerator CrossfadeScreens(GameObject current, GameObject target, bool fullScreen = true)
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

            if (fullScreen)
            {
                foreach (GameObject screen in AllScreens())
                    screen.SetActive(screen == target);
            }
            else
            {
                current.SetActive(false);
            }

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
            yield return seasonEndingScreen;
            yield return chapterReviewScreen;
            yield return sharingScreen;
            yield return connectionScreen;
            yield return accountScreen;
            if (localDashboardScreen != null) yield return localDashboardScreen;
            yield return incidentScreen;
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
            AddText(labelObject, label, 22, new Color(1f, 1f, 1f, 0.76f), TextAnchor.MiddleLeft, FontStyle.Bold);

            GameObject track = CreateRect("Track", container.transform, new Vector2(0.31f, 0.34f), new Vector2(0.77f, 0.66f));
            AddImage(track, new Color(1f, 1f, 1f, 0.17f)).raycastTarget = false;
            GameObject fillObject = CreateRect("Fill", track.transform, Vector2.zero, Vector2.one);
            fill = AddImage(fillObject, color);
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;

            GameObject valueObject = CreateRect("Value", container.transform, new Vector2(0.79f, 0f), new Vector2(0.96f, 1f));
            value = AddText(valueObject, "00%", 24, White, TextAnchor.MiddleRight, FontStyle.Bold);
        }

        private Text CreateArticleCard(
            Transform parent,
            float minX,
            float maxX,
            float minY,
            float maxY,
            string title,
            string body,
            Color accent,
            bool longForm = false)
        {
            GameObject card = CreateRect(title, parent, new Vector2(minX, minY), new Vector2(maxX, maxY));
            AddImage(card, new Color(accent.r, accent.g, accent.b, 0.13f));
            GameObject stripe = CreateRect("Stripe", card.transform, new Vector2(0f, 0f), new Vector2(0.035f, 1f));
            AddImage(stripe, accent);
            GameObject titleObject = CreateRect("Title", card.transform, new Vector2(0.10f, longForm ? 0.84f : 0.66f), new Vector2(0.90f, longForm ? 0.94f : 0.90f));
            AddText(titleObject, title, longForm ? 24 : 19, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            GameObject bodyObject = CreateRect("Body", card.transform, new Vector2(0.10f, longForm ? 0.08f : 0.12f), new Vector2(0.90f, longForm ? 0.80f : 0.64f));
            Text bodyText = AddText(bodyObject, body, longForm ? 22 : 18, Ink, TextAnchor.UpperLeft);
            bodyText.resizeTextForBestFit = true;
            bodyText.resizeTextMinSize = longForm ? 20 : 14;
            bodyText.resizeTextMaxSize = longForm ? 22 : 18;
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
            Image image = buttonObject.AddComponent<RoundedGraphic>();
            image.color = White;
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
