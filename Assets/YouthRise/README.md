# YouthRise — Season 1 and Chapter 5 Prototype

YouthRise is a playable Indonesian high-school visual novel prototype. Open the project in Unity 6000.5.10f1 and press Play in `Assets/Scenes/SampleScene.unity`; the runtime bootstrap builds the complete interface without requiring scene wiring.

## Implemented flow

- Five connected chapters with 51 three-choice decisions across 61 story nodes. Chapter 4 remains the Season 1 finale; “Easy Money?” continues afterward.
- Hidden evolving metrics include risk, trust, confidence, empathy, knowledge, social support, anxiety, bystander response, relationship awareness, digital safety, boundaries, emotional awareness, coping, help-seeking, and resilience.
- Visible Risk and Trust meters, decision latency, tendency classification, branch history, and local autosave.
- A bounded PCG conversation provider that selects authored dialogue variants from hidden player state. It is deterministic, offline, and replaceable through `IConversationGenerator`.
- Chapter-specific reflections, one-time 100/150/200/300/250 XP rewards (1,000 total), and a complete Season 1 progression path followed by Chapter 5.
- Safe Zone chat, unlockable bullying, healthy-relationship, digital-safety, Financial Safety and Money Smart guidance, plus a discreet **Need Extra Help?** reporting tab.
- Local, explainable report triage for prototype use, including immediate-safety guidance.
- Ten hand-painted Indonesian school, home, and support environments plus nine illustrated cast portraits.
- Crossfaded scene changes, sliding character entrances, dialogue fades, staggered choice reveals, and smooth screen transitions.

## Visual direction

All five chapters use a warm, hand-painted visual-novel style grounded in an Indonesian school setting. Runtime art lives under `Resources/YouthRise/Art/`. Character renders use a project-local chroma-key UI shader so their generated green backplates become transparent in game without destructive source-image processing.

The **YouthRise > QA** editor menu can start the chapter, continue dialogue, or choose the first option while the game is running. These controls are intended only for rapid visual review and are not included in player builds.

## Safety and privacy boundaries

This prototype does not diagnose users, contact emergency services, or submit reports. Report text can only be saved as an unencrypted local draft, and the interface says so before saving. Telemetry is pseudonymous and local; it records choice metadata and metric snapshots but deliberately excludes chat and report text.

Runtime data is written below `Application.persistentDataPath/YouthRise/`:

- `prototype-save.json` — current story/profile save.
- `Telemetry/*.jsonl` — one local event stream per session.
- `Reports/*.json` — explicitly saved, unencrypted local drafts.

Production deployment needs authentication, encrypted transport and storage, consent/retention controls, trained human review, local safeguarding escalation policies, and an approved AI provider. The local PCG and triage implementations are safe placeholders, not production AI services.

## Content authoring

Chapter content lives in `Resources/YouthRise/chapter1.json` through `chapter5.json`. Each node contains speaker, scene, dialogue, choices, effects, and optional stat-gated variants. All next-node references are validated when the graph loads.

Chapter 5 adds four financial indicators and uses the existing Help-Seeking score. Its new indicators initialize on chapter start; saved in-progress values resume unchanged. These are authored game heuristics, not validated assessments. [Chapter 5 notes](Chapter5-Notes.md) document scoring, sources, safe educational boundaries, and Mr. Arman's artwork.

Core separation:

- `Model/` — story and player-state data.
- `Services/` — story loading, PCG, saves, telemetry, and Safe Zone triage.
- `UI/YouthRisePrototype.cs` — runtime UI and interaction flow.
- `Tests/EditMode/` — integrity, stat, PCG, and safety tests.

## Replacing the PCG provider

Implement `IConversationGenerator` and inject it in the bootstrap. Keep generated text bounded by the authored scene intent and apply input/output moderation, timeouts, an offline fallback, and no direct mutation of player metrics. A remote model should receive the minimum necessary derived state rather than the full telemetry history.
