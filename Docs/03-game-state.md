# 3️⃣ Game State & Campaign Model

## Purpose
Define the **authoritative game state** and **campaign progression model** that enables:
- Free-form player actions without chaos
- Deterministic mechanics (Unity remains truth)
- AI-driven narrative flow that stays consistent
- Save/load, debugging, and future content expansion

This state is the backbone of the entire experience. The AI DM reads it and proposes story flow, but **Unity enforces and mutates it**.

---

## Design Principles

### 1) Unity is authoritative
- All mechanical changes (HP, inventory, quest completion, XP) are applied by Unity.
- AI can *suggest* changes via directives, but Unity validates and applies.

### 2) State must be compact + stable
- Store “facts” and “flags,” not full conversation history.
- Keep a rolling `storySummary` and short `recentEvents` list for context.

### 3) Campaign is a state machine
- Quests, scenes, and encounters are modeled as states + transitions.
- The AI chooses among valid transitions; Unity enforces constraints.

---

## Data Model Overview

### WorldState (Saved Game)
`WorldState` is serialized locally (JSON) and can be mirrored server-side later.

#### Required Fields (MVP)
- `schemaVersion`: integer
- `sessionId`: string (stable per campaign)
- `player`: PlayerState
- `party`: PartyState
- `location`: LocationState
- `campaign`: CampaignState
- `quests`: QuestState[]
- `relationships`: RelationshipState
- `inventory`: InventoryState
- `flags`: object (string -> primitive)
- `storySummary`: string (rolling, compact)
- `recentEvents`: string[] (last ~10 short bullets)
- `time`: TimeState (optional MVP)
- `rngSeed`: integer (optional; useful for reproducible debugging)

---

## Detailed Schemas (Documentation-Level)

### PlayerState
- `name`: string
- `classTag`: string (e.g., `warrior`, `mage`)
- `level`: int
- `xp`: int
- `stats`: object
  - `hp`: { `current`: int, `max`: int }
  - `mp`: { `current`: int, `max`: int } (optional MVP)
  - `attack`: int
  - `defense`: int
  - `speed`: int
- `skills`: SkillRef[]
- `statusEffects`: StatusEffect[]

### PartyState (MVP-lite)
- `members`: CharacterRef[] (player + allies)
- `activeMemberIds`: string[] (for encounter slots)

### LocationState
- `regionTag`: string (e.g., `starter_realm`)
- `sceneTag`: string (e.g., `tavern_day`, `forest_path`)
- `subLocation`: string (optional)
- `knownNpcsHere`: string[] (npc tags)

### CampaignState
- `chapterId`: string (e.g., `chapter_01`)
- `beatId`: string (e.g., `beat_meet_innkeeper`)
- `difficulty`: int (1..N)
- `toneModifiers`: object (optional: goofy knobs like `chaos`, `sarcasm`)
- `activeThreads`: CampaignThread[]

### CampaignThread
Represents running story threads the AI can pull on.
- `id`: string
- `title`: string
- `status`: `active | resolved | dormant`
- `priority`: int (1..5)
- `notes`: string (short)

### QuestState
- `questId`: string
- `title`: string
- `status`: `inactive | active | completed | failed`
- `stage`: int
- `objectives`: ObjectiveState[]
- `rewards`: RewardState[] (engine-applied)
- `lastUpdated`: string (ISO timestamp optional)

### ObjectiveState
- `id`: string
- `description`: string
- `status`: `pending | complete`
- `progress`: { `current`: int, `target`: int } (optional)

### RelationshipState
Tracks faction and NPC reputation.
- `factions`: object (string -> int)  // -100..100
- `npcs`: object (string -> int)      // -100..100

### InventoryState
- `gold`: int
- `items`: ItemStack[]

### ItemStack
- `itemId`: string
- `count`: int

---

## Flags & Facts (Key-Value)
Use `flags` for lightweight campaign facts like:
- `met_innkeeper`: true
- `bandits_angry`: true
- `bridge_destroyed`: true
- `player_is_wanted`: false

**Rules**
- Flags must be stable primitives (bool/int/string).
- Avoid storing long prose or logs here.

---

## Story Summary Strategy (Critical for AI + Latency)

### storySummary
A rolling summary (200–600 tokens target) that describes:
- current goal
- recent major events
- important NPC relationships
- unresolved threads

### recentEvents
Short bullet list (max 10–15). Example:
- “Accidentally insulted the guard captain.”
- “Promised to retrieve the missing pie shipment.”
- “Bandit escaped after slipping on a banana peel.”

**Why:** Keeps prompts small, responses consistent, and costs down.

---

## Campaign Progression Model

### Campaign Beats (State Machine)
Campaign flow is modeled as:
- `chapterId` + `beatId`
- Each `beatId` has:
  - preconditions (flags/quest stages)
  - allowed next beats
  - optional encounter hooks

AI can *suggest* the next beat, but Unity must validate:
- are prerequisites met?
- is this transition allowed?

#### Example Beat Transition Rules (Illustrative)
- `beat_meet_innkeeper` -> { `beat_first_job_offer`, `beat_bar_fight`, `beat_suspicious_stranger` }
- `beat_bar_fight` requires `sceneTag == tavern_*`

---

## Deterministic vs Narrative Fields

### Deterministic (Engine-owned)
- stats, hp/mp
- inventory
- quest status/stage/objectives
- gold/xp/loot
- encounter outcomes

### Narrative (AI-influenced but stored safely)
- storySummary
- campaign threads
- flavor descriptions (kept out of core truth)

---

## State Mutation Rules

### Allowed mutations by Unity
- Any deterministic update
- Flag updates (validated keys)
- Quest progression updates
- Location changes

### Allowed suggestions by AI (must be validated)
- location/scene change (tags only)
- starting an encounter (using allowed enemy tags)
- proposing quest thread activation
- proposing flag changes (limited allowlist)

**Recommendation:** Maintain allowlists:
- allowed flag keys AI may request
- allowed quest operations AI may propose

---

## Save/Load Requirements (MVP)
- Save after each successful turn resolution
- Save after combat ends
- Load must be resilient to missing fields:
  - use `schemaVersion` migration defaults

---

## Example WorldState (Minimal, Example Only)

```json
{
  "schemaVersion": 1,
  "sessionId": "sess_abc123",
  "player": {
    "name": "Bonkley",
    "classTag": "warrior",
    "level": 1,
    "xp": 0,
    "stats": {
      "hp": { "current": 20, "max": 20 },
      "attack": 5,
      "defense": 3,
      "speed": 2
    },
    "skills": [],
    "statusEffects": []
  },
  "party": { "members": ["player_warrior"], "activeMemberIds": ["player_warrior"] },
  "location": { "regionTag": "starter_realm", "sceneTag": "tavern_day", "subLocation": "bar" },
  "campaign": {
    "chapterId": "chapter_01",
    "beatId": "beat_meet_innkeeper",
    "difficulty": 1,
    "activeThreads": []
  },
  "quests": [],
  "relationships": { "factions": {}, "npcs": { "innkeeper": 10 } },
  "inventory": { "gold": 5, "items": [] },
  "flags": { "met_innkeeper": true },
  "storySummary": "You are in a goofy tavern. The innkeeper trusts you. You need a first job.",
  "recentEvents": ["You tripped into the bar and somehow looked heroic."]
}
