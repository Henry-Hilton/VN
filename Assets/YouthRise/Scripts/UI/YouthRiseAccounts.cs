using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace YouthRise
{
    public sealed partial class YouthRisePrototype
    {
        private PlayerAccount account;
        private string accountToken;
        private AccountServiceSettings accountSettings;
        private GameObject accountScreen, registerFields, incidentScreen;
        private InputField accountName, accountNickname, accountAge, accountPassword;
        private Toggle accountConsent;
        private Text accountHeading, accountStatus, accountIdentity, syncStatus, incidentText, incidentStatus;
        private Button authenticateButton, accountModeButton, genderButton, incidentSend, incidentBack, logoutButton;
        private bool registering, female, accountBusy, incidentBusy, resultsDirty, resultsBusy;
        private int accountEpoch;
        private float nextSync;
        private string incidentId, incidentBody;

        private string CharacterText(string value) => account?.gender == "female" ? (value ?? "").Replace("Alex", "Anita").Replace("ALEX", "ANITA") : value;

        private void BuildAccountScreens(RectTransform canvas)
        {
            accountSettings = AccountServiceSettings.Load();
            accountScreen = CreateRect("Account Screen", canvas, Vector2.zero, Vector2.one);
            AddImage(accountScreen, Paper);
            var left = CreateRect("Welcome", accountScreen.transform, Vector2.zero, new Vector2(.43f,1));
            AddImage(left, Navy);
            AddText(CreateRect("Brand", left.transform, new Vector2(.12f,.77f), new Vector2(.9f,.9f)), "YouthRise", 60, Mint, TextAnchor.MiddleLeft, FontStyle.Bold);
            AddText(CreateRect("Welcome Copy", left.transform, new Vector2(.12f,.35f), new Vector2(.88f,.72f)), "Cerita baru.\nPilihanmu.\nRuang untuk tumbuh.", 43, White, TextAnchor.MiddleLeft);
            AddText(CreateRect("Local Notice", left.transform, new Vector2(.12f,.10f), new Vector2(.88f,.29f)), "DEMO LOKAL\nProfil dan hasil pilihan tersimpan di server lokal untuk konselor. Chat biasa tetap lokal. Pesan bantuan hanya dikirim setelah konfirmasi.", 22, Paper, TextAnchor.MiddleLeft);
            accountHeading = AddText(CreateRect("Title", accountScreen.transform, new Vector2(.49f,.85f), new Vector2(.94f,.95f)), "Masuk ke ceritamu", 39, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            accountNickname = AccountInput(accountScreen.transform, "Nickname", "Nickname (3–30 huruf / angka)", .74f);
            accountNickname.characterLimit = 30;
            accountPassword = AccountInput(accountScreen.transform, "Password", "Password (minimal 8 karakter)", .65f);
            accountPassword.contentType = InputField.ContentType.Password; accountPassword.characterLimit = 128;
            registerFields = CreateRect("Register Fields", accountScreen.transform, Vector2.zero, Vector2.one);
            accountName = AccountInput(registerFields.transform, "Full Name", "Nama lengkap", .56f); accountName.characterLimit = 80;
            accountAge = CreateInputField(registerFields.transform, "Age", "Usia (11–18)", new Vector2(.49f,.47f), new Vector2(.68f,.54f), false);
            StyleAccountInput(accountAge);
            accountAge.contentType = InputField.ContentType.IntegerNumber; accountAge.characterLimit = 2;
            genderButton = CreateButton(registerFields.transform, "Gender", "Laki-laki • Alex", new Vector2(.70f,.47f), new Vector2(.94f,.54f), Mint, Navy, 23);
            genderButton.onClick.AddListener(() => { female = !female; genderButton.GetComponentInChildren<Text>().text = female ? "Perempuan • Anita" : "Laki-laki • Alex"; });
            accountConsent = CreateConsent(registerFields.transform, "Account Consent", "Saya setuju profil dan hasil pilihan game disimpan di server lokal dan dapat dilihat konselor. Gunakan data fiktif untuk demo.", .33f,.44f);
            accountConsent.GetComponent<RectTransform>().anchorMin = new Vector2(.49f,.33f);
            accountConsent.GetComponent<RectTransform>().anchorMax = new Vector2(.94f,.44f);
            authenticateButton = CreateButton(accountScreen.transform, "Authenticate", "MASUK →", new Vector2(.49f,.23f), new Vector2(.94f,.31f), Blue, White, 25);
            authenticateButton.onClick.AddListener(Authenticate);
            accountModeButton = CreateButton(accountScreen.transform, "Account Mode", "Belum punya akun? Daftar", new Vector2(.49f,.14f), new Vector2(.94f,.21f), Mint, Navy, 22);
            accountModeButton.onClick.AddListener(() => { if (accountBusy) return; registering = !registering; RefreshAccountMode(); });
            accountStatus = AddText(CreateRect("Account Status", accountScreen.transform, new Vector2(.49f,.025f), new Vector2(.94f,.13f)), "", 20, Navy, TextAnchor.MiddleLeft);
            accountIdentity = AddText(CreateRect("Player Identity", startScreen.transform, new Vector2(.065f,.925f), new Vector2(.50f,.985f)), "", 20, Mint, TextAnchor.MiddleLeft);
            logoutButton = CreateButton(startScreen.transform, "Logout", "KELUAR AKUN", new Vector2(.49f,.925f), new Vector2(.64f,.975f), Paper, Navy, 16);
            logoutButton.onClick.AddListener(LogoutAccount);
            syncStatus = AddText(CreateRect("Sync Status", startScreen.transform, new Vector2(.065f,.005f), new Vector2(.75f,.04f)), "", 16, Paper, TextAnchor.MiddleLeft);
            CreateButton(startScreen.transform, "Retry Sync", "SINKRON ULANG", new Vector2(.77f,.005f), new Vector2(.94f,.045f), Mint, Navy, 15).onClick.AddListener(() => { resultsDirty = true; nextSync = 0; });

            incidentScreen = CreateRect("Counselor Preview", canvas, Vector2.zero, Vector2.one); AddImage(incidentScreen, Paper);
            AddText(CreateRect("Incident Heading", incidentScreen.transform, new Vector2(.1f,.83f), new Vector2(.9f,.95f)), "Bagikan ke konselor?", 43, Navy, TextAnchor.MiddleLeft, FontStyle.Bold);
            AddText(CreateRect("Incident Notice", incidentScreen.transform, new Vector2(.1f,.64f), new Vector2(.9f,.82f)), "Jika ada bahaya sekarang, cari tempat aman dan hubungi orang dewasa tepercaya atau layanan darurat setempat. Jangan menunggu aplikasi.\n\nHanya pesan di bawah ini beserta profil akunmu akan masuk ke dashboard konselor lokal. Belum dikirim dan tidak anonim.", 24, Navy, TextAnchor.MiddleLeft);
            incidentText = CreateScrollableText(incidentScreen.transform, new Vector2(.1f,.31f), new Vector2(.9f,.61f));

            incidentStatus = AddText(CreateRect("Incident Status", incidentScreen.transform, new Vector2(.1f,.19f), new Vector2(.9f,.29f)), "", 22, Navy, TextAnchor.MiddleLeft);
            incidentSend = CreateButton(incidentScreen.transform, "Confirm Incident", "SETUJU, KIRIM KE KONSELOR", new Vector2(.1f,.07f), new Vector2(.65f,.16f), Blue, White, 23); incidentSend.onClick.AddListener(SubmitIncident);
            incidentBack = CreateButton(incidentScreen.transform, "Cancel Incident", "KEMBALI", new Vector2(.68f,.07f), new Vector2(.9f,.16f), Mint, Navy, 23); incidentBack.onClick.AddListener(ShowSafeZone);
            RefreshAccountMode();
        }
        private InputField AccountInput(Transform parent, string name, string placeholder, float y)
        {
            var input = CreateInputField(parent, name, placeholder, new Vector2(.49f,y), new Vector2(.94f,y+.07f), false);
            StyleAccountInput(input); return input;
        }
        private void StyleAccountInput(InputField input)
        {
            input.textComponent.fontSize = 25;
            if (input.placeholder is Text label) { label.fontSize = 23; label.color = new Color(.28f,.35f,.43f,1); label.fontStyle = FontStyle.Normal; }
        }
        private void RefreshAccountMode()
        {
            registerFields.SetActive(registering); accountStatus.text = "";
            accountHeading.text = registering ? "Buat akun pemain" : "Masuk ke ceritamu";
            authenticateButton.GetComponentInChildren<Text>().text = registering ? "DAFTAR & MULAI →" : "MASUK →";
            accountModeButton.GetComponentInChildren<Text>().text = registering ? "Sudah punya akun? Masuk" : "Belum punya akun? Daftar";
        }
        private void Authenticate()
        {
            if (accountBusy) return;
            int.TryParse(accountAge.text, out int age);
            var request = new AccountRequest { name = accountName.text.Trim(), nickname = accountNickname.text.Trim(), password = accountPassword.text, age = age, gender = female ? "female" : "male", consent = accountConsent.isOn };
            if (registering && !request.consent) { accountStatus.text = "Persetujuan penyimpanan profil dan hasil diperlukan."; return; }
            accountBusy = true; SetButtonEnabled(authenticateButton, false); SetButtonEnabled(accountModeButton, false); accountStatus.text = "Menghubungi server lokal…";
            StartCoroutine(PlayerAccountClient.Post(accountSettings.baseUrl, registering ? "/api/register" : "/api/login", null, JsonUtility.ToJson(request), result => {
                accountBusy = false; SetButtonEnabled(authenticateButton, true); SetButtonEnabled(accountModeButton, true);
                if (!result.success || result.user == null || string.IsNullOrEmpty(result.token)) { accountStatus.text = result.error ?? "Login gagal."; return; }
                account = result.user; accountToken = result.token; accountEpoch++;
                PrototypeSaveService.SetAccount(account.id); accountPassword.text = ""; accountName.text = ""; accountAge.text = ""; accountConsent.isOn = false;
                profile = new PlayerProfile(); profile.ResetForChapterOne(); story = StoryRepository.LoadChapterOne(); currentNode = null; branchPath = ""; chapterCompleted = false; telemetry = null;
                accountIdentity.text = $"Halo, {account.nickname}  •  Karakter: {account.CharacterName}";
                accountIdentity.supportRichText = false;
                chatResponse.text = $"Halo, {account.nickname}. Aku siap mendengarkan. Kamu bisa mulai dari hal yang paling nyaman untuk diceritakan.";
                ShowStartMenu(); ApplyCharacterText();
                resultsDirty = PrototypeSaveService.Exists; nextSync = 0;
                syncStatus.text = "Hasil pilihan akan tersinkron ke dashboard konselor lokal.";
            }));
        }
        private void ApplyCharacterText()
        {
            foreach (Text label in GetComponentsInChildren<Text>(true))
            {
                if (label.GetComponentInParent<InputField>() != null || label == chatResponse || label == reportAssessment || label == previewText ||
                    label == accountIdentity || label == genderButton.GetComponentInChildren<Text>() ||
                    label.transform.IsChildOf(accountScreen.transform) || label.transform.IsChildOf(incidentScreen.transform)) continue;
                label.text = account?.gender == "female" ? label.text.Replace("Alex", "Anita").Replace("ALEX", "ANITA")
                    : label.text.Replace("Anita", "Alex").Replace("ANITA", "ALEX");
            }
        }
        private void LogoutAccount()
        {
            if (resultsBusy || incidentBusy) { ShowToast("Tunggu pengiriman selesai sebelum keluar.", true); return; }
            string token = accountToken;
            StartCoroutine(PlayerAccountClient.Post(accountSettings.baseUrl, "/api/logout", token, "{}", _ => { }));
            accountEpoch++; account = null; accountToken = null; resultsDirty = false;
            if (screenTransition != null) { StopCoroutine(screenTransition); screenTransition = null; }
            if (storyTransition != null) { StopCoroutine(storyTransition); storyTransition = null; }
            PrototypeSaveService.SetAccount(null); profile = new PlayerProfile(); profile.ResetForChapterOne(); currentNode = null; telemetry = null;
            ClearSupportSession(); reportInput.text = ""; reportAssessment.text = ""; currentAssessment = null; previewRequest = null; previewText.text = ""; incidentBody = null; incidentId = null; incidentText.text = "";
            storyAudio.StopNarration(); ApplyCharacterText(); SetScreen(accountScreen);
            accountStatus.text = "Anda sudah keluar. Login untuk melanjutkan.";
        }
        private void Update()
        {
            if (account == null || !resultsDirty || resultsBusy || Time.unscaledTime < nextSync) return;
            resultsDirty = false; resultsBusy = true; SetButtonEnabled(logoutButton, false);
            int epoch = accountEpoch;
            var upload = new ResultUpload { choices = (profile.recordedChoices ?? new System.Collections.Generic.List<RecordedChoice>()).ToArray() };
            syncStatus.text = "Menyinkronkan hasil…";
            StartCoroutine(PlayerAccountClient.Post(accountSettings.baseUrl, "/api/results", accountToken, JsonUtility.ToJson(upload), result => {
                if (epoch != accountEpoch) return;
                resultsBusy = false; SetButtonEnabled(logoutButton, true);
                if (result.success) { syncStatus.text = "Hasil tersinkron ke dashboard konselor."; nextSync = Time.unscaledTime + 2; }
                else { resultsDirty = true; nextSync = Time.unscaledTime + 30; syncStatus.text = "Hasil belum tersinkron; save lokal aman. " + result.error; }
            }));
        }
        private void PreviewIncident(string text)
        {
            if (incidentBusy) return;
            if (incidentBody != text || string.IsNullOrEmpty(incidentId)) incidentId = "YR-" + Guid.NewGuid().ToString("N");
            incidentBody = text;
            incidentText.supportRichText = false; incidentText.text = text; incidentText.rectTransform.anchoredPosition = Vector2.zero;
            incidentStatus.text = "Tujuan: dashboard konselor lokal. Tekan Kembali jika belum ingin berbagi.";
            SetButtonEnabled(incidentSend, true); ShowScreenSmooth(incidentScreen);
        }
        private void SubmitIncident()
        {
            if (account == null || incidentBusy || string.IsNullOrEmpty(incidentId)) return;
            incidentBusy = true; SetButtonEnabled(incidentSend, false); SetButtonEnabled(incidentBack, false);
            incidentStatus.text = "Mengirim ke dashboard…";
            var request = new SupportReportRequest { reportId = incidentId, text = incidentBody, consent = true };
            StartCoroutine(PlayerAccountClient.Post(accountSettings.baseUrl, "/api/incidents", accountToken, JsonUtility.ToJson(request), result => {
                incidentBusy = false; SetButtonEnabled(incidentBack, true);
                if (result.success && result.status == "received_by_dashboard" && result.reportId == incidentId) {
                    incidentStatus.text = "Diterima dashboard konselor. Belum berarti sudah dibaca atau ditindaklanjuti. ID: " + incidentId;
                    incidentId = null; chatInput.text = "";
                } else { incidentStatus.text = "Belum terkonfirmasi. Coba lagi dengan ID yang sama. " + result.error; SetButtonEnabled(incidentSend, true); }
            }));
        }
    }
}
