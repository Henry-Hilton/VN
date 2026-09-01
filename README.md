# YouthRise

**YouthRise** is a playable Indonesian high-school visual novel prototype built with Unity. Chapter 1, **“The First Day,”** introduces Alex's new school, while Chapter 2, **“Behind the Smile,”** explores bullying, cyberbullying, bystander choices, and reaching trusted support.

![YouthRise school setting](Assets/YouthRise/Resources/YouthRise/Art/Backgrounds/bg_school_gate.png)

> [!NOTE]
> YouthRise is an early prototype. Its local wellbeing and reporting features are demonstrations, not professional care, emergency services, or a production reporting system.

## Highlights

- Two complete chapters with 25 story nodes and 21 three-choice decisions.
- Branching outcomes influenced by risk, trust, confidence, empathy, knowledge, social support, anxiety, and bystander response.
- State-aware dialogue selected locally from bounded, authored variants.
- Autosave, decision-latency tracking, tendency classification, and branch history.
- Chapter-specific reflections, persistent XP rewards, Safe Zone access, and an unlockable Relationship Path.
- Safe Zone chat, short wellbeing articles—including Chapter 2 bullying support—and a discreet reporting-draft flow.
- Nine hand-painted environments and seven illustrated characters.
- Crossfaded scenes, animated character entrances, dialogue fades, and staggered choice reveals.

## Getting started

### Requirements

- [Unity Hub](https://unity.com/download)
- Unity Editor **6000.5.10f1**
- [Git LFS](https://git-lfs.com/)

### Run the prototype

```bash
git clone https://github.com/Henry-Hilton/VN.git
cd VN
git lfs pull
```

1. Add the cloned folder as a project in Unity Hub.
2. Open it with Unity **6000.5.10f1** and allow Unity to restore the packages.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play**.

The runtime bootstrap creates the complete interface automatically; the scene does not require manual wiring.

## Controls and testing

The prototype is mouse-driven: select menu actions, dialogue choices, and Safe Zone tabs with the on-screen interface.

For automated checks, open Unity's **Test Runner**, select **EditMode**, and run the `YouthRise.Tests.EditMode` test assembly. The suite checks:

- Chapter graph integrity and decision counts.
- Player-stat effects.
- State-aware dialogue selection.
- Immediate-safety language handling.
- Availability and minimum resolution of the visual-art catalog.

While the game is running, the **YouthRise > QA** editor menu can start the chapter, advance dialogue, or select the first choice for faster visual review. These helpers are editor-only and are not included in player builds.

## Project structure

```text
Assets/
├── Scenes/SampleScene.unity             # Entry scene
└── YouthRise/
    ├── Editor/                           # Editor-only QA helpers
    ├── Resources/YouthRise/
    │   ├── Art/                          # Runtime backgrounds and characters
    │   ├── chapter1.json                 # Chapter 1 story graph
    │   └── chapter2.json                 # Chapter 2 story graph
    ├── Scripts/
    │   ├── Model/                        # Story and player-state models
    │   ├── Services/                     # Story, dialogue, saves, telemetry, safety
    │   └── UI/                           # Runtime interface and interaction flow
    └── Tests/EditMode/                   # Core automated tests
```

## Content and architecture

Chapter content is authored in `Assets/YouthRise/Resources/YouthRise/chapter1.json` and `chapter2.json`. Each node can define a speaker, setting, dialogue, choices, stat effects, next-node references, and optional stat-gated dialogue variants. The repository validates each story graph when it loads.

Dynamic dialogue is intentionally offline and deterministic. `LocalConversationGenerator` implements `IConversationGenerator` by choosing among authored variants that match the player's hidden state. A future provider can replace it, but generated content should remain bounded by scene intent, moderated, resilient to timeouts, and unable to mutate player metrics directly.

## Local data and privacy

Runtime data is stored beneath `Application.persistentDataPath/YouthRise/`:

| Path | Purpose |
| --- | --- |
| `prototype-save.json` | Current story and player-profile save |
| `Telemetry/*.jsonl` | Pseudonymous choice metadata and metric snapshots for one session |
| `Reports/*.json` | Explicitly saved, unencrypted local report drafts |

The prototype does not submit reports, contact authorities, diagnose users, or call an online AI service. Telemetry deliberately excludes Safe Zone chat and report text. The interface warns users before saving an unencrypted local draft.

A production release would require authentication, encrypted transport and storage, consent and retention controls, trained human review, local safeguarding and escalation policies, and an approved AI provider.

## License

No project license has been added yet. Unless a license is added, the source code and assets remain under the copyright holder's default rights.
