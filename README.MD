# Project: (Working Title) Goofy AI-DM Campaign Game

A goofy 2D mobile RPG built in **Unity** where the player can type **free-form actions** and an **AI Dungeon Master** drives the campaign flow—while **Unity remains authoritative** for all mechanics (rolls, combat, state).

---

## What This Is
- **Unity 2D game** (mobile-first: iOS + Android)
- **AI DM** that:
  - interprets player intent
  - narrates outcomes in a comedic tone
  - suggests scene changes using a controlled asset vocabulary
- **Deterministic rules engine** in Unity:
  - skill checks / dice rolls
  - combat resolution
  - inventory, stats, quests, flags
- **DM Gateway backend**:
  - builds prompts from compact state
  - validates AI responses and directives
  - streams narration to the client for low latency
  - caches/prefetches likely next turns

---

## Core Pillars (Non-Negotiable)
1. **AI is narrator/director, not rule arbiter**
2. **Unity is the source of truth**
3. **Free-form player input is allowed**
4. **Mobile performance + perceived latency matter** (streaming is mandatory)

---

## Repository Docs (Start Here)

### 1) Product Vision & Constraints
Sets tone, promise, and hard constraints to prevent scope creep.
- `docs/01-product-vision-and-constraints.md`

### 2) System Architecture Overview
Defines components, responsibilities, and trust boundaries.
- `docs/02-system-architecture-overview.md`

### 3) Game State & Campaign Model
Defines `WorldState`, quest/campaign modeling, and save/load requirements.
- `docs/03-game-state-and-campaign-model.md`

### 4) Player Action → AI → Engine Loop
Defines the turn lifecycle for dialogue, checks, combat, clarify, invalid.
- `docs/04-turn-loop-player-ai-engine.md`

### 5) AI Intent Classification Spec
Defines intent categories and output schema for fast routing.
- `docs/05-ai-intent-classification-spec.md`

### 6) AI Response Contract
Defines the JSON response schema (narration, directives, options).
- `docs/06-ai-response-contract.md`

### 7) Asset Taxonomy
Defines allowed tags for backgrounds, characters, poses, FX, music, SFX.
- `docs/07-asset-taxonomy.md`

### 8) AI Hosting & Latency Spec
Defines streaming protocol, caching/prefetch, latency targets, reliability.
- `docs/08-ai-hosting-and-latency-spec.md`

---

## Suggested Folder Structure (Target)
This is the intended long-term layout (can evolve):

