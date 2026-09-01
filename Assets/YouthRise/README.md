# YouthRise — Chapters 1 & 2 Prototype

YouthRise is a playable Indonesian high-school visual novel prototype. Open the project in Unity 6000.5.10f1 and press Play in `Assets/Scenes/SampleScene.unity`; the runtime bootstrap builds the complete interface without requiring scene wiring.

## Implemented flow

- Chapter 1, “The First Day,” and Chapter 2, “Behind the Smile,” with 21 three-choice decisions across 25 story nodes.
- Hidden evolving metrics: risk, trust, confidence, empathy, knowledge, social support, anxiety, and bystander response.
- Visible Risk and Trust meters, decision latency, tendency classification, branch history, and local autosave.
- A bounded PCG conversation provider that selects authored dialogue variants from hidden player state. It is deterministic, offline, and replaceable through `IConversationGenerator`.
- Chapter-specific reflections, persistent 100/150 XP rewards, and progression from Safe Zone access into the Relationship Path.
- Safe Zone chat, short wellbeing articles—including unlocked bullying support—and a discreet **Need Extra Help?** reporting tab.
- Local, explainable report triage for prototype use, including immediate-safety guidance.
- Nine hand-painted Indonesian school, home, and support environments plus seven illustrated cast portraits.
- Crossfaded scene changes, sliding character entrances, dialogue fades, staggered choice reveals, and smooth screen transitions.

## Visual direction

Both chapters use a warm, hand-painted visual-novel style grounded in an Indonesian school setting. Runtime art lives under `Resources/YouthRise/Art/`. Character renders use a project-local chroma-key UI shader so their generated green backplates become transparent in game without destructive source-image processing.

The **YouthRise > QA** editor menu can start the chapter, continue dialogue, or choose the first option while the game is running. These controls are intended only for rapid visual review and are not included in player builds.

## Safety and privacy boundaries

This prototype does not diagnose users, contact emergency services, or submit reports. Report text can only be saved as an unencrypted local draft, and the interface says so before saving. Telemetry is pseudonymous and local; it records choice metadata and metric snapshots but deliberately excludes chat and report text.

Runtime data is written below `Application.persistentDataPath/YouthRise/`:

- `prototype-save.json` — current story/profile save.
- `Telemetry/*.jsonl` — one local event stream per session.
- `Reports/*.json` — explicitly saved, unencrypted local drafts.

Production deployment needs authentication, encrypted transport and storage, consent/retention controls, trained human review, local safeguarding escalation policies, and an approved AI provider. The local PCG and triage implementations are safe placeholders, not production AI services.

## Content authoring

Chapter content lives in `Resources/YouthRise/chapter1.json` and `chapter2.json`. Each node contains speaker, scene, dialogue, choices, effects, and optional stat-gated variants. All next-node references are validated when the graph loads.

Core separation:

- `Model/` — story and player-state data.
- `Services/` — story loading, PCG, saves, telemetry, and Safe Zone triage.
- `UI/YouthRisePrototype.cs` — runtime UI and interaction flow.
- `Tests/EditMode/` — integrity, stat, PCG, and safety tests.

## Replacing the PCG provider

Implement `IConversationGenerator` and inject it in the bootstrap. Keep generated text bounded by the authored scene intent and apply input/output moderation, timeouts, an offline fallback, and no direct mutation of player metrics. A remote model should receive the minimum necessary derived state rather than the full telemetry history.
