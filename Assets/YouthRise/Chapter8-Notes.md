# Chapter 8: Home Is Complicated

Chapter 8 closes the expanded eight-chapter Season 1. It contains an opening, ten three-choice decisions, and the supplied home conversation (12 nodes, 30 choices). First completion awards 300 XP, bringing Chapters 1–8 to 1,850 XP, and unlocks **Safe Zone > Keluarga: Family & Support**.

## Story and presentation

The complete corrected scenario is localized into Indonesian, consistent with the earlier chapters. It covers parental disagreement, school expectations, comparison, support from friends, financial stress, self-blame, family communication, seeking help, and a modest first conversation with Mom. The ending does not claim that one conversation fixes everything. Kevin and Leo's family experiences are recalled in Rina's conversation to connect the cast without adding scored decisions.

All supplied base effects are retained. Trust Parent uses the existing parent relationship; generic Trust in Maya's scene uses her relationship score. Dad has his own new illustrated portrait, but there is no invented separate Dad-trust meter. Parent conversations at nodes 4 and 11 have bounded, authored variants selected using Family Communication. Both variants are reachable through normal earlier choices. They do not alter the choices or base effects, and neither version withholds support from Alex.

Existing home, bedroom, hallway and counselor backgrounds, portraits, HUD, chroma-key material and scene transitions are reused. The menu now has two rows of four chapter buttons, followed by Continue and Safe Zone. The Safe Zone has a Keluarga tab with a Family & Support card and an Orang Dewasa Aman card.

After the six-line reflection, **AKHIR SEASON 1** opens the three supplied English closing sentences. Continue crossfades to **YOUR JOURNEY IS JUST BEGINNING.**, a short journey recap, and buttons for Family & Support and the menu. The exact closing messages are stored as optional chapter data (`endingLines` and `endingHeading`), not embedded in the dialogue generator.

## Hidden indicators and saves

Three new hidden fields initialize to 50 when starting/replaying Chapter 8 and clamp to 0–100:

- Family Communication: an authored tendency toward expressing feelings, listening, and discussing manageable plans, versus avoidance or aggressive responses.
- Family Support: a fictional indicator of the interactions written for Alex and his parents. It is not a rating of the player's real family, perceived safety, worth, or responsibility for adult behavior.
- Emotional Regulation: an authored indicator of pausing, naming feelings and seeking support during fictional conflict. It is not a diagnosis or a measure of whether an emotional response is justified.

Help-Seeking reuses `helpSeekingTendency`. The earlier Self-Control, parent/friend relationships, anxiety, health, digital indicators, XP and unlocks carry forward. Continue resumes all saved values and the branch path without reinitializing these fields.

The additional rubric rewards requests for safe support, thoughtful communication and manageable steps. Avoidance, dismissal, secrecy and attempts to solve adult problems alone can reduce fictional indicators. Every added delta is explicit in [chapter8.json](Resources/YouthRise/chapter8.json), separately from the supplied base deltas. These are game-design heuristics, not validated psychological assessments; research or school use requires educator, safeguarding and ethics review. They must not be used to label a player or infer abuse from gameplay choices.

Decision telemetry includes Risk, Trust, Confidence, Empathy, Social Support, Anxiety, Family Communication, Family Support, Emotional Regulation and Help-Seeking, alongside prior indicators. Existing local latency, selected choice, branch and tendency recording is reused. Chat and report text are not added to telemetry.

Starting requires completed Chapter 7. First completion sets `completedChapterEight`, `seasonOneCompleted` and `familySupportArticleUnlocked`. Replays retain all earned XP and the guide unlock without adding the 300 XP again. Legacy saves that marked Season 1 complete after Chapter 4 are normalized to the expanded Chapter 8 finale without removing prior chapter completion, XP, or guides. Chapter 4's reflection is preserved but no longer labels itself the season finale.

## Educational boundaries

The content distinguishes ordinary disagreement from threats or violence. Calm conversation is suggested only when safe; it is not presented as a way to guarantee another person's behavior. Alex is not asked to mediate a dangerous argument. Support may come from a safe adult outside the family, and another adult can be approached if the first does not help. The guide explicitly says the game's chat and drafts do not contact real assistance.

These general communication and support principles were checked against [Childline's family problems guidance](https://www.childline.org.uk/info-advice/home-families/family-relationships/family-relationships/) on 7 September 2026 (Asia/Jakarta).

The story also states that adult finances are not Alex's responsibility to fix, and preserves the user's reminder that young people do not have to solve every family problem. The safeguarding distinction and emphasis on getting support, rather than intervening in unsafe conflict, are consistent with [Childline's domestic abuse guidance](https://www.childline.org.uk/info-advice/home-families/family-relationships/domestic-abuse/). These are general educational sources, not an Indonesian emergency protocol. UK contact numbers and legal rules are not transplanted into the game. Local safeguarding and referral procedures still need review before deployment.

## Dad artwork

Generated using the **built-in image generation tool**, not the fallback CLI. The selected PNG is saved in the project as [char_dad_chroma.png](Resources/YouthRise/Art/Characters/char_dad_chroma.png). The existing shader removes its green backplate at runtime; the source image is not destructively processed. Speaker aliases Ayah and Dad resolve to this asset; Ibu and Mom continue to use Mom's existing portrait.

Final generation prompt:

> Use case: illustration-story. Asset type: new character sprite for YouthRise, an Indonesian high-school visual novel, matching a warm semi-realistic hand-painted cast with crisp natural contours and softly shaded faces. Subject: Alex's father, an Indonesian man in his mid-forties, warm medium-brown skin, short black hair with a little gray at the temples, dark brown eyes, clean-shaven, slightly tired but thoughtful neutral expression, approachable not threatening. Wearing a plain sand-beige button-up shirt with sleeves rolled to the forearms, dark brown trousers and a modest watch. Standing relaxed, shoulders level, arms relaxed with one hand loosely near the waist, natural anatomy. Front three-quarter view, head to upper thighs, portrait 2:3 composition, small margin above head, full arms and hands in frame, fill about 90 percent of canvas height. Soft even daylight with subtle painterly fabric texture. Solid flat pure vivid chroma green #00FF00 background for the existing game's chroma-key shader; no green clothing, no background shadows. No scenery, no other people, no lettering, logos or watermark. Adult father in ordinary home clothes, not a school uniform or teacher's costume.

## Validation — 7 September 2026

- Unity 6000.5.10f1 compiled with no errors. All 92 Edit Mode tests passed after the final ending-transition change, with zero failures or skipped tests. The 22 Chapter 8 cases cover graph/art validity, every supplied base delta, all-A/B/C totals, gating, first/repeat rewards, old-save migration, save round-trips, clamping, parent aliases, dialogue thresholds and real-route reachability. Report: `Logs/Chapter8QA/final-test-results.json`.
- Live Play mode via editor QA helpers invoking the actual UI button callbacks: completed Chapter 7 from the existing Chapter 6 save, checked the Chapter 8 handoff, completed all ten A decisions, and replayed all ten C decisions. XP progressed from 1,250 to 1,550 to 1,850, then remained 1,850 on replay.
- Stopped/restarted Play mode at Chapter 8 node 4; Continue preserved the entire profile, current node and branch. Completed-save Continue returned to the reflection. Both ending stages, repeat entry, Family & Support navigation, and the final menu returned correctly.
- Inspected 1280×720 screenshots of the locked/completed menus, locked/unlocked family cards, opening, Dad, resumed comparison scene, both home-conversation variants, six-line reflection, closing message, and final journey screen. Text fit without clipping in these views. No standalone player build or device-specific layout testing was performed.
- All 20 live Chapter 8 decisions contained positive decision latency, branch, tendency and all ten requested assessment fields.
- Final console: zero errors. One unrelated Unity AI Toolkit warning reported unavailable account-service connectivity. SampleScene remained clean; Play mode was stopped.
- The original Chapter 6 save was restored byte-for-byte, verified by SHA-256 `A6C21F4AD1255572C0ACC618F4C08BF6B8D15458629B2C37E2EA781AD476535B`. Five QA-only telemetry sessions were moved into `Logs/Chapter8QA/` for recoverable review; all existing user telemetry was preserved. QA saves, screenshots and the test report are also under that ignored local directory. No commit or push was performed.

To play with the restored save: complete Chapter 7, then select **MULAI CHAPTER 8**. Completing Chapter 8 opens the family guide and Season 1 ending.
