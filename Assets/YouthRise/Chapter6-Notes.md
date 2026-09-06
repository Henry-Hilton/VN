# Chapter 6: Take Care of You

Chapter 6 follows “Easy Money?” within the expanded eight-chapter Season 1. It includes an opening, ten three-choice decisions, and the supplied five-line reflection. First completion grants 250 XP, bringing Chapters 1–6 to 1,250 XP, and unlocks Safe Zone > Gaya Hidup: Healthy Lifestyle and My Healthy Routine.

## Scenario and assessment

All supplied base score deltas are retained. The three “No change” choices (3B, 4B, 10C) have empty effect lists: they do not modify any hidden metric. Breakfast occurs on the following school morning so the preceding choice to miss school does not immediately contradict the setting.

The new Health score and six awareness/habit indicators are hidden, clamped 0–100, and initialized to 50 when starting or replaying Chapter 6. Continue preserves saved values. Existing confidence, risk, relationships, financial indicators and support progress carry forward.

- Health: only the Health deltas supplied in the scenario. This is a fictional gameplay indicator, not a measurement of physical health, fitness, body composition, disability or illness.
- Health Awareness: only the awareness deltas supplied in the scenario.
- Sleep Awareness: added authored deltas for recognizing sleep needs, wind-down and boundaries around late-night gaming.
- Physical Activity Awareness: added deltas for recognizing accessible movement; observing the activity is not penalized.
- Nutrition Awareness: added deltas for regular, varied meals. Agreeing to fast food causes no stat penalty.
- Self-Care: added deltas for responding to everyday needs, manageable habits and support.
- Routine Consistency: added deltas for workable plans and repeatable actions versus all-at-once changes.

These are transparent game-design heuristics, not validated health or psychological assessments. The exact rubric is visible in `Resources/YouthRise/chapter6.json`. Metric snapshots and decision telemetry include all seven new fields. Tests independently check every supplied base effect, four complete routes, no-change choices, clamping, save round-trips and legacy initialization.

Authored PCG variants at nodes 6 and 7 respond to Sleep Awareness and Self-Care. They retain the scene and choices and make no diagnosis. The story and guides use nonjudgmental language, adaptable movement (including seated options), regular meals, hydration and hygiene. No weight, calorie, body-size, extreme diet, or punitive exercise targets are introduced.

## Educational references

General guidance was checked against primary public-health sources on 6 September 2026:

- [CDC: About Sleep](https://www.cdc.gov/sleep/about/index.html): regular sleep schedules, reducing screens before bedtime, and avoiding caffeine toward evening.
- [CDC: Sleep and Health](https://www.cdc.gov/physical-activity-education/staying-healthy/sleep.html): sleep, attention and concentration for students.
- [WHO: Physical activity](https://www.who.int/news-room/fact-sheets/detail/physical-activity): activity includes different forms of movement and can be adapted to abilities.
- [Kementerian Kesehatan: Isi Piringku](https://ayosehat.kemkes.go.id/isi-piringku-kebutuhan-gizi-harian-seimbang): varied meals, drinking water and washing hands with soap.

The guides intentionally avoid universal sleep-hour, exercise-minute or water-volume prescriptions for the mixed 11–18 age range. My Healthy Routine is a short planning guide, not a medical plan or a live habit tracker. Persistent difficulty sleeping, eating or caring for oneself is directed to a trusted adult or healthcare professional. Educator and safeguarding review remain necessary before school or research deployment.

## Validation — 6 September 2026

- Unity 6000.5.10f1: 52/52 Edit Mode tests passed after the final artwork import; zero failures or skipped tests. Local report: `Logs/Chapter6QA/final-test-results.json`.
- Live Play-mode checks: Chapter 5 completion opens Chapter 6; the six-button menu and both locked/unlocked lifestyle cards fit at 1280×720. Coach Sarah, the sports court, the morning bedroom, completion reflection and existing financial guides were visually inspected.
- Completed the supportive route (1B, then 2A–10A), stopping and restarting Play mode at node 6. Continue preserved the entire saved profile. First completion increased XP from 1,000 to 1,250 and unlocked both guides.
- Replayed all ten C choices, including the no-change final plan, and completed again. XP stayed at 1,250. All 20 live decisions recorded latency, branch, tendency and all seven new metric fields.
- Final console check: zero errors. Unity's AI Toolkit logged an unrelated account-service connectivity warning. No standalone player build was performed.
- Original Chapter 5 save restored byte-for-byte after testing; four QA-only telemetry sessions archived under `Logs/Chapter6QA/` instead of mixing with user gameplay data. SampleScene is unchanged and Play mode is stopped.

## Generated artwork

Three illustrations were produced with the built-in image-generation tool, without CLI use: two new assets and a morning-light edit of the existing bedroom. Existing source assets were preserved.

- `Resources/YouthRise/Art/Characters/char_coach_sarah_chroma.png`: Coach Sarah, an adult PE teacher; a separate identity and resource from the Chapter 3 student Sarah. Uses the existing chroma-key shader.
- `Resources/YouthRise/Art/Backgrounds/bg_school_court.png`: an Indonesian school sports court used at nodes 3 and 8.
- `Resources/YouthRise/Art/Backgrounds/bg_bedroom_morning.png`: a daylight variant of `bg_bedroom.png` used only for the opening and node 1. Late-night gaming scenes retain the original bedroom.

Final portrait prompt:

> Use case: illustration-story. Asset: a new character sprite for YouthRise, an Indonesian high-school visual novel. Coach Sarah is an adult Indonesian female physical-education teacher in her mid-thirties, warm medium-brown skin, dark brown eyes, black hair in a practical low ponytail, calm encouraging smile. Wearing a modest blue and white zip-up sports jacket, a whistle on a short dark lanyard, and dark navy track pants. Relaxed upright posture with hands loosely together at waist, a friendly teacher, not a student. Semi-realistic hand-painted digital illustration with crisp contours, gently shaded detailed face and fabric, natural proportions, soft even daylight. Front view, head through upper thighs only, portrait 2:3 canvas. Fill about 90% of the height, small margin above head, both arms and hands fully in frame. Solid flat vivid pure chroma green #00FF00 background for the existing game's chroma-key shader, no background shadow, no green on the person. No writing, logos, watermark, scenery, or other people.

Final sports-court prompt:

> Use case: illustration-story. Asset: a new background for YouthRise, a warm semi-realistic hand-painted Indonesian high-school visual novel. Empty outdoor multipurpose school sports court in soft morning sunshine, eye-level wide landscape 16:9 composition. Clean modest red-and-blue painted basketball court with subtle worn white lines, one basketball hoop at the far end, low two-storey Indonesian school classroom blocks with clay-tile roofs and open corridors, tropical shade trees, a small red-over-white Indonesian flag near the courtyard edge. Believable welcoming everyday SMA campus, natural perspective, detailed painted surfaces and gentle leafy shadows, no people, no lettering, no UI, no watermark. Keep foreground open for character sprites, middle-right area visually calm for a dialogue panel. No gym equipment in foreground. 1920x1080 landscape framing.

Final morning-bedroom edit prompt:

> Use case: lighting-weather edit. Image 1 is the existing YouthRise bedroom background, an edit target. Create a morning-light variant for the alarm/waking-up scene. Change only lighting and time of day: clear pale morning sky through the window, soft warm morning sunlight entering the room, exterior houses and foliage naturally visible in daylight, desk lamp switched off and phone screen dark. Preserve the same room geometry, camera view, landscape aspect ratio, furniture, bed, curtains, objects and their positions, and the semi-realistic hand-painted visual novel style. Do not add any people, objects, text, UI or watermark. This must remain recognizably the exact same bedroom.
