# Chapter 5: Easy Money?

Chapter 5 continues after the existing four-chapter Season 1 milestone. It contains an opening, ten three-choice decisions, and a closing reflection. First completion grants 250 XP (1,000 XP across Chapters 1–5) and unlocks Safe Zone > Finansial: Financial Safety and Money Smart Guide.

## Assessment and saves

All requested base effects are preserved. The additional authored indicators are game-design heuristics, not validated financial-literacy scores or psychological assessments:

- Financial Awareness: the exact scenario deltas.
- Spending Control: budgeting, prioritizing essentials and saving increase it; spending beyond means or ignoring costs decreases it.
- Impulse Control: pausing and verifying increase it; rushing into purchases, loans and transfers decreases it.
- Scam Awareness: checking sources and prize offers increases it; unverified payments and forwarding decrease it.
- Help-Seeking Tendency: choosing trusted support increases the existing indicator; secrecy or dismissing support decreases it.

New financial indicators start at 50 when beginning/replaying Chapter 5, including from old saves. Existing Risk, Trust, Social Support and Help-Seeking carry forward. Continue resumes saved values without resetting them. All indicators are clamped to 0–100 and included in metric snapshots and local decision telemetry. Trust is recorded but Chapter 5 does not add Trust deltas absent from the scenario.

Authored PCG variants at Kevin's debt disclosure and Rina's advice adapt to Financial Awareness and Help-Seeking. Variants retain scene intent and do not alter choice effects. Risky options remain playable fictional choices; no real application, payment, account signup, or personal-data collection occurs.

## Educational references

The Indonesian story follows the supplied scenario. The repayment and budget amounts are explicitly fictional arithmetic examples, not market rates, lending offers, or personal financial advice. This prototype does not determine borrowing eligibility.

- [OJK: financial education and protection following the IPB scam](https://ojk.go.id/id/berita-dan-kegiatan/siaran-pers/Pages/OJK-Tingkatkan-Edukasi-dan-Perlindungan-Konsumen-dalam-Kasus-Penipuan-Berkedok-Investasi-di-IPB.aspx): verify legality, interest, fees, terms and data safety.
- [OJK: saving from an early age, 2025](https://ojk.go.id/id/berita-dan-kegiatan/siaran-pers/Pages/Hari-Indonesia-Menabung-dan-Bulan-Literasi-Keuangan-2025.aspx): education encouraging students to save for purchases rather than borrow.
- [OJK financial planning booklet](https://sikapiuangmu.ojk.go.id/FrontEnd/images/FileDownload/25_Buku_Perencanaan_Keuangan.pdf): needs, wants and budgeting.

References checked 5 September 2026. Before research or school deployment, have an appropriate educator review wording and the assessment rubric; the prototype does not claim validated learning outcomes.

## Validation (5 September 2026)

- 28/28 Unity Edit Mode tests passed after the final script changes.
- Played Chapter 4 completion into Chapter 5, resumed at node 6 without resetting scores, and completed the first-choice route.
- Completed a second-choice replay: total XP remained 1,000, with no duplicate reward.
- Visually inspected the menu, Mr. Arman, longer repayment dialogue, completion screen, locked/unlocked financial guides, and existing articles at 1280×720.
- All 20 tested decisions recorded nonnegative latency and all four financial indicators in local telemetry.
- Final Unity console had zero errors; Play mode was stopped. No standalone player build was tested.
- Original save restored byte-for-byte. Test telemetry and screenshots retained separately in the ignored `Logs/Chapter5QA/` folder.

## Mr. Arman artwork

Generated with the built-in image-generation tool (single new illustration, no CLI). Saved at `Resources/YouthRise/Art/Characters/char_mr_arman_chroma.png`. Existing backgrounds and cast art remain unchanged. The generated green background is removed by the existing runtime chroma-key shader.

Final generation prompt:

> Use case: illustration-story. Asset type: Indonesian high-school visual novel character sprite for YouthRise. Create a single new character, Mr. Arman, a friendly Indonesian male teacher and financial-literacy mentor in his early forties, medium warm brown skin, neat short black hair with subtle gray at temples, clean shaven, thin dark rectangular glasses, a calm encouraging closed-mouth smile. Semi-realistic painted digital illustration with crisp contours, detailed gently shaded face and fabric, matching a polished school visual novel. Brown and muted cream short-sleeved batik shirt, dark trousers, simple blank ID badge, holding a plain small notebook at waist level with both hands. Eye-level front view, head through upper thighs only, portrait 2:3 canvas, character fills about 88 percent of canvas height, complete head and both arms within frame, small margin above head. Flat solid vivid pure chroma green #00FF00 background for the game's existing green-screen shader, no shadows on background, no green on character or clothing. Soft neutral front lighting, no scenery, no other people, no lettering, no watermark. This is a new distinct character, not Mr. Daniel.
