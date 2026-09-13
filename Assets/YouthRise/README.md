# YouthRise — Eight-Chapter Season 1 Prototype

YouthRise is a playable Indonesian high-school visual novel prototype. Open the project in Unity 6000.5.10f1 and press Play in `Assets/Scenes/SampleScene.unity`; the runtime bootstrap builds the complete interface without requiring scene wiring.

## Implemented flow

- Eight connected chapters with 82 three-choice decisions across 97 story nodes. “Home Is Complicated” (Chapter 8) is the Season 1 finale; Chapter 4 remains an emotional well-being milestone.
- Hidden evolving metrics include risk, trust, confidence, empathy, knowledge, social support, anxiety, bystander response, relationship awareness, digital safety, boundaries, emotional awareness, coping, help-seeking, and resilience.
- Risk and Trust meters hidden during story decisions, revealed in a chapter review before reflection; decision latency, tendency classification, branch history, and local autosave remain.
- A bounded PCG conversation provider that selects authored dialogue variants from hidden player state. It is deterministic, offline, and replaceable through `IConversationGenerator`.
- Chapter-specific reflections, one-time 100/150/200/300/250/250/300/300 XP rewards (1,850 total), and an eight-chapter Season 1 progression path.
- Safe Zone chat, unlockable bullying, healthy-relationship, digital-safety, Financial Safety, Money Smart, Healthy Lifestyle, My Healthy Routine and Family & Support guidance, plus a discreet **Need Extra Help?** reporting tab.
- Local, explainable report triage for prototype use, including immediate-safety guidance.
- Twelve hand-painted Indonesian school, home, and support environments plus eleven illustrated cast portraits.
- Crossfaded scene changes, sliding character entrances, dialogue fades, staggered choice reveals, and smooth screen transitions.
- Rounded rectangle buttons, original ambient background music and optional local Windows voice narration; Indonesian pronunciation depends on installed voices or authored recordings.
- Previewable eight-issue choice summaries and disabled-by-default bot/WhatsApp-to-Guru-BK connectors. See [lecturer revision notes](Lecturer-Changes.md).

## Visual direction

All eight chapters use a warm, hand-painted visual-novel style grounded in an Indonesian school setting. Runtime art lives under `Resources/YouthRise/Art/`. Character renders use a project-local chroma-key UI shader so their generated green backplates become transparent in game without destructive source-image processing.

The **YouthRise > QA** editor menu can start the chapter, continue dialogue, or choose the first option while the game is running. These controls are intended only for rapid visual review and are not included in player builds.

## Safety and privacy boundaries

The shipped configuration does not diagnose users, contact emergency services, submit reports or call online AI. Optional server connections can enable consent-based bot chat and reviewed WhatsApp reports to a configured Guru BK. No report is sent automatically. Game-choice summaries are unvalidated descriptions, not findings about the player's real life. Telemetry is pseudonymous and local; it records choice metadata and metric snapshots but deliberately excludes chat and report text.

Runtime data is written below `Application.persistentDataPath/YouthRise/`:

- `prototype-save.json` — current story/profile save.
- `Telemetry/*.jsonl` — one local event stream per session.
- `Reports/*.json` — explicitly saved, unencrypted local drafts.
- `Reports/*.txt` — explicitly exported preview text, not submitted.
- `Receipts/*.json` — attempted-report IDs and API acceptance metadata when confirmed; not proof of delivery/read.

Production deployment needs authentication, encrypted transport and storage, age-appropriate consent/retention controls, trained human review, local safeguarding escalation policies, and an approved AI provider. The local PCG, keyword triage and optional connector are prototypes, not production counseling services. See [backend setup and safety limits](../../Backend/README.md).

## Content authoring

Chapter content lives in `Resources/YouthRise/chapter1.json` through `chapter8.json`. Each node contains speaker, scene, dialogue, choices, effects, and optional stat-gated variants. All next-node references are validated when the graph loads.

Chapter 5 adds four financial indicators and uses the existing Help-Seeking score. Its new indicators initialize on chapter start; saved in-progress values resume unchanged. These are authored game heuristics, not validated assessments. [Chapter 5 notes](Chapter5-Notes.md) document scoring, sources, safe educational boundaries, and Mr. Arman's artwork.

Chapter 6 adds a fictional Health score and Health Awareness, Sleep Awareness, Physical Activity Awareness, Nutrition Awareness, Self-Care and Routine Consistency. It preserves every supplied base delta and all no-change options, supports existing saves, and unlocks two guides under **Safe Zone > Gaya Hidup**. Coach Sarah is a separate adult character from student Sarah. [Chapter 6 notes](Chapter6-Notes.md) document the rubric, non-weight-focused guidance, primary sources and three artwork prompts.

Chapter 7 adds Digital Awareness, Self-Control, Screen-Time Management, FOMO Index, Social Media Dependency Tendency and Offline Social Support. It includes eleven decisions and a one-time 300 XP reward; existing Health, Sleep Awareness and Anxiety carry forward. No extra unlock or actual device monitoring is added. [Chapter 7 notes](Chapter7-Notes.md) explain the fictional screen-time example, scoring rubric and educational boundaries.

Chapter 8 adds three fictional family indicators and reuses Help-Seeking. It preserves the supplied base effects, awards 300 XP once, and unlocks **Safe Zone > Keluarga: Family & Support**. The two-stage ending uses the supplied English closing messages; dialogue remains localized. Old Season 1 flags are migrated to Chapter 8 without removing earned XP or earlier completion flags. [Chapter 8 notes](Chapter8-Notes.md) cover safety boundaries, Dad's portrait, scoring and validation.

Core separation:

- `Model/` — story and player-state data.
- `Services/` — story loading, PCG, saves, telemetry, and Safe Zone triage.
- `UI/YouthRisePrototype.cs` — runtime UI and interaction flow.
- `Tests/EditMode/` — integrity, stat, PCG, and safety tests.

## Replacing the PCG provider

Implement `IConversationGenerator` and inject it in the bootstrap. Keep generated text bounded by the authored scene intent and apply input/output moderation, timeouts, an offline fallback, and no direct mutation of player metrics. A remote model should receive the minimum necessary derived state rather than the full telemetry history.
