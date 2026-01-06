# 4️⃣ Player Action → AI → Engine Loop

## Purpose
Define the **authoritative turn lifecycle** for free-form player input in a way that is:
- deterministic (Unity remains the source of truth)
- safe (AI cannot corrupt state)
- fast (low perceived latency with streaming)
- extensible (supports dialogue, skill checks, and combat)

This loop is the core “engine contract” between:
- the player experience (Unity UI)
- the AI DM (narrative + flow control)
- the rules engine (mechanics + outcomes)

---

## Key Principles

### Unity is authoritative
Unity decides:
- whether a roll is required
- the outcome of rolls/combat
- inventory/stat changes
- quest/state transitions

### AI is narrative + flow control
AI decides:
- how to interpret player intent
- what kind of scene should happen next
- narration, NPC dialogue, comedic framing
- suggested directives using allowed tags

### The DM Gateway is the enforcer
Gateway ensures:
- schema compliance
- directive validation
- prompt size control (summary strategy)
- caching/prefetch
- streaming to Unity

---

## Turn Types (MVP)
Every player input resolves into one of these turn types:

1. **Dialogue / Exploration**
   - no mechanics required
   - AI narrates + offers options or continues story

2. **Skill Check**
   - engine rolls against difficulty
   - AI narrates outcome using official result

3. **Combat**
   - Unity runs encounter deterministically
   - AI narrates major beats and outcomes

4. **Clarification Needed**
   - AI asks player to clarify ambiguous input

5. **Invalid / Impossible**
   - AI rejects action humorously and offers alternatives
   - no state change (or minimal “time passes” if desired)

---

## High-Level Sequence (One Turn)

### Step 0 — Player Input (Unity)
Player types a free-form action:
> “I try to distract the guard by pretending to be a lost tourist.”

Unity collects:
- `playerInput` (string)
- `WorldState` snapshot (compact)
- optional recent conversation snippet (short)

---

### Step 1 — TurnRequest (Unity → DM Gateway)
Unity sends a **TurnRequest**.

**Goal:** include enough context for correct interpretation, but keep prompt small.

Gateway receives:
- `sessionId`
- `playerInput`
- `worldStateCompact`
- `recentMessages` (optional, short)

---

### Step 2 — AI Pass #1: Intent + Flow Proposal (Gateway → Model)
Gateway asks the model to produce a **structured intent classification**.

Output includes:
- `intentType`: dialogue | skill_check | combat | clarify | invalid
- `confidence`: 0..1
- if `skill_check`: suggested `skillTag` + `difficultyBand`
- if `combat`: suggested enemy tags (allowed only)
- optional clarification question if needed

> This can be done with a smaller/faster model or a constrained response mode.

---

### Step 3 — Engine Validation (Gateway + Unity)
Before committing to anything:
- Validate directive schema
- Validate asset tags via `AssetRegistry`
- Validate state transitions (beat/quest rules)
- Validate feasibility (if needed)

If invalid:
- Gateway requests a corrected response OR falls back to a safe narration-only path.

---

### Step 4 — Branch Based on Intent

## Branch A: Dialogue / Exploration
1. Gateway requests narrative response (streaming)
2. Unity displays narration as it streams
3. AI returns:
   - narration text
   - optional directives (background, actor pose, etc.)
   - optional choices
4. Unity applies validated directives
5. Unity updates `WorldState` only if there are safe, validated changes
6. Save state

---

## Branch B: Skill Check
### B1 — AI proposes a check
Example proposed check:
- `skillTag`: `deception`
- `difficultyBand`: `medium`

### B2 — Unity resolves deterministically
Unity:
- selects final skill/stat (may override AI suggestion)
- computes DC from difficulty band
- rolls RNG + modifiers
- produces official result:
  - success / partial / fail
  - numeric roll totals
  - state deltas (flags, relationship changes, etc.)

### B3 — Resolution (Unity → Gateway)
Unity sends a **Resolution** payload.

### B4 — AI narrates outcome (Gateway → Model → Unity)
AI receives only the official result and narrates accordingly (streaming).
Unity applies any validated narrative directives (FX, reaction animations).
Unity saves `WorldState`.

---

## Branch C: Combat
### C1 — AI requests encounter start
AI proposes:
- enemies: allowed tags
- background: allowed tag
- optional difficulty hint

### C2 — Unity starts EncounterScene
Unity spawns:
- player + party
- enemy prefabs
- background + music

### C3 — Combat loop is engine-only
Unity runs turns:
- player chooses actions (buttons or quick input)
- engine resolves hit/damage/status
- character animations + FX

### C4 — CombatResult summary (Unity → Gateway)
At major beats or end:
- send a compact `CombatResult` summary:
  - who won
  - key moments (crit fail, funny event)
  - loot/rewards (engine-applied)

### C5 — AI narrates the combat outcome
AI describes what happened, comedic framing, and continues campaign flow.
Unity saves state.

---

## Branch D: Clarification Needed
AI asks a targeted question:
- “Do you mean you want to sneak past the guard, or distract him with conversation?”

Unity presents the clarification prompt and does **not** mutate state.

---

## Branch E: Invalid / Impossible
AI rejects gracefully:
- humorous response
- explains why it won’t work in-world
- proposes 2–4 alternatives
No state change (or minimal time tick).

---

## Turn Timeline Diagram

```mermaid
sequenceDiagram
  participant P as Player
  participant U as Unity Client
  participant G as DM Gateway
  participant L as LLM

  P->>U: Free-form action text
  U->>G: TurnRequest (compact state + input)

  G->>L: Intent/Flow request (structured)
  L-->>G: intentType + params + confidence

  alt Dialogue
    G->>L: Narrative response (stream)
    L-->>G: AIResponse stream
    G-->>U: Stream narration + validated directives
    U->>U: Apply directives + update state
  else Skill Check
    G-->>U: Proposed check (skill + difficulty)
    U->>U: Roll + resolve (authoritative)
    U->>G: Resolution (official outcome)
    G->>L: Narrate outcome (stream)
    L-->>G: Outcome narration stream
    G-->>U: Stream outcome text + directives
    U->>U: Apply directives + save
  else Combat
    G-->>U: Start encounter (validated tags)
    U->>U: Run combat loop (authoritative)
    U->>G: CombatResult summary
    G->>L: Narrate summary + next beat
    L-->>G: Narration stream
    G-->>U: Stream narration + directives
    U->>U: Apply directives + save
  end
