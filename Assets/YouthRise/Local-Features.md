# Local login, dashboard and Gemini demo

## Login and dashboard

No Node server, localhost URL or backend setup is needed. Start Unity Play Mode,
register a player, then sign in. Accounts, salted PBKDF2 password hashes, choice
results and confirmed reports are stored under `YouthRise.localPortal.v1` in
Unity PlayerPrefs. Existing account-specific story saves remain local JSON files.
Accounts from the previous Node database are not automatically imported; create a
local account. The previous backend data is left intact.

Choose **DASHBOARD LOKAL** on the login screen or **DASHBOARD** on the main menu.
The first use sets a local manager password (at least 12 characters). Later visits
require that password. The dashboard shows player profiles, eight-topic choice
summaries, aggregate trends and confirmed incident reports. Enter a report ID to
mark it reviewed, closed or reopened. **KEMBALI & KUNCI** clears dashboard access.
Ordinary chat is not saved to the dashboard. Only a reviewed and explicitly
confirmed incident message is stored there.

PlayerPrefs can be inspected or modified by the device owner and is not encrypted.
Use fictional accounts/reports for this local demo. Clearing PlayerPrefs removes
local accounts and the dashboard password. Data is not shared between devices.
The older web dashboard and Node connectors remain optional legacy code.

## Gemini without a local backend

1. In Unity choose **YouthRise > Gemini > Open Private Config**.
2. Edit the revealed `UserSettings/YouthRise/gemini.json` file locally:

   ```json
   {
     "enabled": true,
     "apiKey": "YOUR_PRIVATE_KEY",
     "model": "YOUR_AVAILABLE_TEXT_MODEL_ID"
   }
   ```

   Use a model ID available in your Google AI Studio project without a `models/`
   prefix. Do not put the key in Resources, source files, PlayerPrefs or chat.
   `UserSettings/` is ignored by Git. Keep `enabled` false when not testing.
3. Open **Safe Zone > Koneksi / Privasi** and enable online-chat consent.
4. Send a fictional ordinary message. The game calls Google's HTTPS
   `generateContent` endpoint directly, with the key in `x-goog-api-key`.

Settings are reloaded per request; no restart is needed. In a standalone desktop
demo the config is read from `Application.persistentDataPath/YouthRise/gemini.json`.
The editor-only menu does not package the private file into a build. Do not ship
your developer API key in a public client; use a server-side proxy for a future
shared release.

Only up to six chat messages from the current session are sent. Account profiles,
scores, reports and story choices are not attached. Responses must finish normally
and pass the output checks; timeouts, provider failures, blocked or incomplete
responses use the authored local fallback. Urgent-keyword messages use local
safety guidance and open an incident preview without an API call. Clearing the
session, withdrawing consent or logging out prevents stale responses being shown.

Gemini remains disabled until configured. This is for adult developer testing
with fictional data: Google's current terms prohibit API clients directed at or
likely to be accessed by under-18s. Enabling a key does not approve use by the
game's teenage audience. Free-tier requests must not contain sensitive/personal
information. See https://ai.google.dev/gemini-api/terms and
https://ai.google.dev/api/generate-content.

## Verification

Run the Unity EditMode suite from **YouthRise > QA > Run EditMode Tests**.
Tests cover local registration/login, password hashing, duplicate nicknames,
account isolation, dashboard permissions, report confirmation/idempotency and
Gemini request/response parsing. Tests never contact Google.
The account integration smoke test now runs without a backend.
