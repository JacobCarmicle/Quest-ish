# 7️⃣ Asset Taxonomy (Visual Vocabulary)

## Purpose
Define a **controlled vocabulary** of assets the AI DM is allowed to reference via directives:
- backgrounds
- characters
- animation states (poses)
- expressions (optional)
- FX
- music
- SFX

This taxonomy is essential for:
- scalability (AI cannot invent art)
- safety (unknown tags are rejected)
- consistent visuals (no random mismatches)
- simple content expansion (add tags without changing AI rules)

> The AI can only “direct” the game using tags from this taxonomy.  
> Unity validates tags through `AssetRegistry`.

---

## Naming Conventions (Rules)

### General
- Use **lowercase snake_case**
- Tags must be stable and never repurposed once shipped
- Prefer descriptive names over clever ones

✅ Good:
- `tavern_day`
- `forest_path`
- `bandit_male_01`

❌ Avoid:
- `TavernDay`
- `bg1`
- `cool_place`
- `banditBUTANGRY`

### Versioning / Variants
Use suffixes for variants:
- `tavern_day` / `tavern_night`
- `forest_path` / `forest_path_rain`
- `bandit_male_01` / `bandit_male_02`

---

## Category Lists (MVP)

> These are the minimum recommended starter sets for a goofy 2D campaign MVP.  
> Expand later by appending new tags.

---

## 1) Backgrounds (`backgroundTag`)
Used by directives: `set_background`, `start_encounter`

### MVP Set (6–10)
- `tavern_day`
- `tavern_night`
- `forest_path`
- `village_square`
- `dungeon_hall`
- `camp_night`
- (optional) `road_day`
- (optional) `road_night`
- (optional) `castle_gate`
- (optional) `market_day`

**Rules**
- Backgrounds should be usable across many encounters
- Avoid making every unique story beat require a new background

---

## 2) Characters (`characterTag`)
Used by directives: `show_actor`, `update_actor`, `start_encounter`

### Character Slots (suggested)
- `player`
- `ally1`
- `ally2`
- `npc`
- `enemy1`
- `enemy2`
- `enemy3`

### MVP Characters (6–10)
**Player + allies**
- `player_warrior`
- `player_mage` (optional if class swap)
- `ally_rogue`
- `ally_cleric`

**Enemies**
- `bandit_male_01`
- `goblin_01`
- `skeleton_01`
- `slime_01` (goofy staple)

**NPCs**
- `innkeeper_01`
- `guard_01`

**Rules**
- Enemy tags are also “actors” for dialogue scenes (if needed)
- Prefer reusable archetypes over one-off NPC sprites for MVP

---

## 3) Animation States / Poses (`poseTag`)
Used by directives: `show_actor`, `update_actor`

### Required Core States (per character)
- `idle`
- `talk`
- `attack`
- `hurt`
- `die`

### Optional States (adds personality)
- `laugh`
- `confused`
- `panic`
- `victory`
- `defeat`
- `cast` (for magic users)

**Rules**
- Not every character needs every optional state
- Unity must fall back to `idle` if a state doesn’t exist

---

## 4) Expressions (`expressionTag`) *(Optional MVP)*
If you implement expressions as sprite swaps or animator layers.

### Common Expressions
- `neutral`
- `happy`
- `angry`
- `sad`
- `surprised`
- `smirk`
- `panic`
- `deadpan`

**Rules**
- Expressions should be compatible with poses (if layered)
- Fallback expression: `neutral`

---

## 5) Visual Effects (`fxTag`)
Used by directives: `play_fx`, `camera_shake`

### MVP FX (8–12)
- `hit_slash`
- `hit_bonk` *(goofy impact)*
- `hit_spark`
- `spell_glow`
- `heal_sparkle`
- `smoke_puff`
- `confetti_pop`
- `question_marks`
- (optional) `banana_peel_slip`
- (optional) `dramatic_gasp_lines`

**Rules**
- FX should be generic and reusable
- Prefer comedic FX to reinforce tone

---

## 6) Music (`musicTag`)
Used by directives: `play_music`, `stop_music`, `start_encounter`

### MVP Music (4–6)
- `tavern_bop`
- `forest_whimsy`
- `dungeon_sneaky`
- `combat_bop`
- `victory_jingle`
- `defeat_womp` *(optional)*

**Rules**
- Keep loops short and light for mobile
- Provide a “no music” safe default

---

## 7) Sound Effects (`sfxTag`)
Used by directives: `play_sfx`

### MVP SFX (10–20)
- `bonk`
- `slash`
- `whoosh`
- `sparkle`
- `coin_drop`
- `door_creak`
- `record_scratch`
- `dramatic_sting`
- `womp_womp`
- `goblin_giggle`
- `level_up`

**Rules**
- SFX labels should describe the sound
- Unity should rate-limit repeated SFX triggers per second

---

## AssetRegistry Requirements (Unity)

### AssetRegistry responsibilities
- Provide lookups by tag for each category
- Validate existence
- Provide fallbacks

### Must-have APIs (conceptual)
- `HasBackground(tag)`
- `GetBackground(tag)` -> sprite/prefab
- `HasCharacter(tag)`
- `GetCharacterPrefab(tag)`
- `HasFx(tag)`
- `GetFxPrefab(tag)`
- `HasMusic(tag)`
- `GetMusicClip(tag)`
- `HasSfx(tag)`
- `GetSfxClip(tag)`

### Fallback rules
- Unknown `backgroundTag` -> `tavern_day` (or last known)
- Unknown `characterTag` -> do not spawn actor
- Unknown `poseTag` -> `idle`
- Unknown `expressionTag` -> `neutral`
- Unknown `fxTag`/`sfxTag` -> ignore

---

## AI Usage Rules (Enforced by Gateway)
- AI may only reference tags from the taxonomy
- AI may not request new tags
- AI may not embed raw filenames or paths
- AI may not force multiple rapid FX/SFX spam

---

## Expansion Guidelines
When adding new assets:
1. Add tag to taxonomy list
2. Add asset to Unity registry mapping
3. (Optional) update AI “allowed tags” version hash
4. No change to directive schema needed

---

## Acceptance Criteria (MVP)
- All AI directives reference tags from this taxonomy
- Unity can validate every tag via AssetRegistry
- Missing/unknown tags never crash the game
- Tone is reinforced by FX/SFX options (bonk, womp, confetti)

