# Chapter 7: Always Connected

Chapter 7 follows Take Care of You. It includes an opening and eleven three-choice decisions (12 nodes, 33 choices), followed directly by the five supplied reflection lines. First completion awards 300 XP; Chapters 1–7 total 1,550 XP. No additional guide unlocks were specified or added. Completion now continues into Chapter 8; the existing Safe Zone remains accessible from the menu.

## Story and presentation

The story is localized into Indonesian to match Chapters 1–6. It covers late-night scrolling, the next morning, FOMO, comparison, notifications during study, gaming boundaries, offline friendship, a phone-free pause, loneliness, screen-time review and a final routine choice. Leo appears with the group at nodes 4 and 7. Existing character portraits, day/night bedroom variants, school locations, HUD and transitions are reused.

The expanded menu uses two rows of four chapter buttons, followed by Continue and Safe Zone. Chapter 6 completion leads into Chapter 7; starting requires completed Chapter 6. Replay initializes only the six new indicators and never grants the reward twice. Continue preserves current metrics and branch history.

The numbers in node 10 are the supplied fictional Alex example: social media 4h20m, gaming 3h10m, entertainment 2h15m and school 1h30m. They are not obtained from the player's phone, operating system, or game-session duration. No device-monitoring permissions, timers, app deletion, or actual daily-limit enforcement are implemented.

## Hidden assessment rubric

Every supplied base delta is retained. Generic Trust at node 7 maps to Maya's existing relationship score. Digital Awareness and Self-Control are separate from Chapter 3's Digital Safety Awareness and Chapter 5's Impulse Control.

Six new fields initialize to 50 and clamp to 0–100:

- Digital Awareness: only the awareness changes supplied by the scenario.
- Self-Control: only the self-control changes supplied by the scenario.
- Screen-Time Management: authored deltas for reviewing use, managing interruptions, stopping at a planned time and making realistic routines.
- FOMO Index: higher values mean more fictional pressure to stay connected; lower values reflect practicing boundaries.
- Social Media Dependency Tendency: higher values mean more reliance on scrolling or posting in the story; lower values reflect trying alternatives. Gaming alone is not scored as social-media dependency.
- Offline Social Support: authored deltas for in-person connection and support. Choosing online conversation at node 7B retains its +1 Trust and has no extra hidden penalty.

Existing Health, Sleep Awareness, Anxiety and all earlier progress carry forward. Only Health/Anxiety changes requested in this chapter are applied. Sleep Awareness has additional authored deltas for bedtime choices and the final balanced routine.

All additional deltas are explicit in [chapter7.json](Resources/YouthRise/chapter7.json); the tests independently transcribe the supplied base deltas and expected complete-route totals. Metric snapshots in saves and local decision telemetry include the six new fields alongside Sleep Awareness and Anxiety.

These scores are game-design heuristics, not validated measures of addiction, dependency, mental health, or personal worth. They must not be used to label or diagnose a player. Higher scores are not always better (FOMO and dependency run in the opposite direction). Authored PCG variants at nodes 3 and 9 use Self-Control and Digital Awareness without changing available choices or making clinical claims. Both variants at each node are reachable through normal prior choices, not just artificially assigned test scores.

## Validation — 6 September 2026

- Unity 6000.5.10f1: 70/70 Edit Mode tests passed after the final dialogue adjustment; zero failures or skipped tests. The 18 Chapter 7 cases cover graph/art validity, all supplied base deltas, three complete routes, progression, one-time rewards, old and current save round-trips, clamping, dialogue thresholds and real-path variant reachability. Local report: `Logs/Chapter7QA/final-test-results.json`.
- Live Play mode: continued from the user's completed Chapter 6 save into Chapter 7; completed the all-A route; stopped and restarted Play mode at node 6 and verified the entire profile, node and branch were preserved. XP increased from 1,250 to 1,550.
- Replayed all eleven C choices and completed again. XP stayed at 1,550; replay reset only the six new indicators. All 22 live decisions recorded latency, branch, tendency and the eight requested assessment fields.
- Visual inspection at 1280×720 covered the seven-chapter menu, night opening, choice cards, screen-time example, final Maya scene, five-line reflection, completed menu and retained lifestyle guides. Existing artwork and transitions were reused; no new artwork was generated.
- Final console: zero errors; one unrelated Unity AI Toolkit account-service connectivity warning. No standalone player build was performed. The final dialogue condition change was verified by the complete automated suite after the playthroughs.
- Original Chapter 6 save restored byte-for-byte. Four QA-only telemetry sessions archived under `Logs/Chapter7QA/`; the user's own Chapter 6 telemetry was preserved. SampleScene remains unchanged and Play mode is stopped.

## Educational references

Guidance was checked on 6 September 2026 against the American Academy of Pediatrics:

- [How to Make a Family Media Plan](https://www.healthychildren.org/English/family-life/Media/Pages/How-to-Make-a-Family-Media-Use-Plan.aspx): personalized plans, optional notification/autoplay controls, revisiting routines and protecting other activities.
- [How to Connect with Your Teen about Smart & Safe Media Use](https://www.healthychildren.org/English/family-life/Media/Pages/Points-to-Make-With-Your-Teen-About-Media.aspx): online connection can be useful; social-media portrayals are not the whole of real life.

The chapter does not prescribe a universal daily screen-time limit for ages 11–18. The one-hour break is the user's optional scenario choice, not a diagnostic threshold or mandatory treatment. Mr. Daniel preserves access for important needs, invites support, and rejects a punitive delete-everything approach. Offline friendship is an additional source of connection, not a claim that online relationships are worthless. Educator and safeguarding review remains necessary before research or school deployment.
