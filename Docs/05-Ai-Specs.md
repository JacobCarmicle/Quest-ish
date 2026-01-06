# 5️⃣ AI Intent Classification Spec

## Purpose
Enable **free-form player input** while keeping the game:
- deterministic (Unity remains authoritative)
- consistent (campaign state stays coherent)
- fast (low latency decisions)
- safe (AI cannot invent mechanics or assets)

This spec defines how the AI (via the DM Gateway) classifies a player’s text input into a small set of **intent types** that drive the next step in the turn loop.

> The goal of intent classification is not storytelling.  
> It is to decide **what kind of turn this is** and what the engine should do next.

---

## Intent Categories (MVP)

### 1) `DIALOGUE`
Player is talking, asking questions, role-playing, or exploring without requiring mechanics.

**Examples**
- “I ask the innkeeper what’s on the menu.”
- “I look around the room for anything suspicious.”
- “I tell the goblin he has a nice hat.”

**Expected engine behavior**
- No roll required by default
- AI narrates and may offer options

---

### 2) `SKILL_CHECK`
Player attempts something that should be resolved by a roll (stealth, persuasion, lockpick, etc.)

**Examples**
- “I sneak past the guard.”
- “I try to convince the merchant to lower the price.”
- “I climb the wall.”
- “I attempt to pick the lock.”

**Expected engine behavior**
- Unity selects the final `skillTag` + `difficulty` (may override AI)
- Unity rolls and resolves outcome
- AI narrates using official results

---

### 3) `COMBAT`
Player initiates violence or triggers a hostile response likely leading to an encounter.

**Examples**
- “I attack the bandit.”
- “I cast fireball at the skeleton.”
- “I draw my sword and charge.”

**Expected engine behavior**
- Unity starts EncounterScene (if enemies exist/are valid)
- Combat resolves deterministically
- AI narrates key beats (optional) and outcome

---

### 4) `CLARIFY`
Player input is ambiguous or missing needed information (target, approach, intent).

**Examples**
- “I do the thing.”
- “I try to use it.”
- “I talk to him.” (multiple NPCs present)
- “I cast a spell.” (no spell chosen)

**Expected engine behavior**
- AI asks a single targeted clarification question
- No state mutation

---

### 5) `INVALID`
Player attempts something impossible, out-of-world, or that violates constraints.

**Examples**
- “I spawn 10,000 gold.”
- “I become a god.”
- “I delete the dungeon.”
- “I hack the game and win.”

**Expected engine behavior**
- AI rejects humorously
- Suggests 2–4 viable alternatives
- No state mutation (or optional tiny “time passes” tick)

---

## Confidence & Routing

### Confidence Score
Classifier returns `confidence` in range `0..1`.

**Recommended thresholds**
- `>= 0.75`: accept intent
- `0.50 – 0.74`: accept with caution (prefer CLARIFY if stakes are high)
- `< 0.50`: treat as `CLARIFY`

### High-stakes override
Even if confidence is high, if the proposed action would:
- start combat
- change location
- end a quest stage
then Unity/Gateway may require confirmation or a clarification step.

---

## Output Schema (IntentResult)

**IntentResult** is a structured object returned by the AI classifier step.

```json
{
  "intentType": "SKILL_CHECK",
  "confidence": 0.86,
  "notes": "Player wants to distract a guard with deception.",
  "proposed": {
    "skillTag": "deception",
    "difficultyBand": "medium",
    "target": "guard_captain"
  },
  "clarifyQuestion": null
}
