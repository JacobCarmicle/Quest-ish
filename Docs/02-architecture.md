# 2️⃣ System Architecture Overview

## Purpose
This document describes the **high-level system architecture** for the game, including:
- Major components and responsibilities
- Data flow between Unity, AI services, and storage
- Trust/authority boundaries (what is “source of truth”)
- Where latency optimizations live

This is the reference for implementation planning and prevents “AI-driven spaghetti.”

---

## High-Level Architecture (Components)

### A) Unity Mobile Client (Authoritative Game)
**Responsibility:** Presentation + rules + state updates  
**Owns (source of truth):**
- Combat resolution (rolls, hit/miss, damage)
- Player stats, inventory, HP/MP
- Quest progression flags
- Visuals (characters, animations, FX, backgrounds)
- Asset availability (tags/registries)

**Does not rely on AI for:**
- Rules
- Outcomes
- Direct state mutation without validation

---

### B) DM Gateway (Backend Service You Control)
**Responsibility:** The “AI integration brain” between Unity and the model(s)

**Core jobs:**
- Build prompts from structured state (keep prompts small)
- Enforce response schema + validate directives
- Maintain rolling story summaries (token control)
- Streaming responses to Unity
- Cache/prefetch likely next turns (perceived latency)
- Provider abstraction (swap LLMs without rewriting the client)

> The DM Gateway is where you win on latency, cost, and stability.

---

### C) LLM Provider / Inference Layer
This can be one of:
- Hosted LLM API (fastest to ship MVP)
- Self-hosted inference (best long-term latency/cost control)
- Hybrid (recommended: start hosted, move heavy traffic to self-host)

**LLM responsibilities:**
- Interpret player intent (classification)
- Narrate the scene / role-play NPCs
- Suggest “directives” within allowed vocabulary (background changes, encounter start, etc.)
- Offer player-facing options

**LLM is NOT authoritative** for mechanics or state.

---

### D) Persistence (Save + Session)
Two levels of storage are common:

**Client Save (MVP)**
- `WorldState.json` stored locally on device
- Fast, offline-friendly for everything except AI turns

**Server Session (Recommended)**
- Session summary cache (rolling summary, recent turns)
- Prefetched responses for offered choices
- Telemetry and abuse protection (rate limits)

---

## Data Flow (Turn Lifecycle)

### Turn Flow (Free-Form Input)
1. Player types an action in Unity
2. Unity sends `TurnRequest` to DM Gateway (small, structured)
3. DM Gateway:
   - fetches/updates session summary
   - calls model for intent + narrative (streaming)
4. Model returns:
   - narration (streamed)
   - directives (structured JSON)
   - options (choices)
5. DM Gateway validates directives against allowed schema + asset tags
6. Unity executes valid directives:
   - show background/actors/FX
   - if encounter requested, Unity starts combat
7. If mechanics required (skill check / combat):
   - Unity resolves deterministically
   - Unity sends `Resolution` back
   - AI narrates outcome using the official result

---

## Architecture Diagram

```mermaid
flowchart LR
  U[Unity Mobile Client\n- UI + Animations\n- Combat Engine (authoritative)\n- WorldState (authoritative)] -->|TurnRequest\n(player text + compact state)| G[DM Gateway\n- Prompt builder\n- Directive validator\n- Summary + cache\n- Streaming adapter]
  G -->|LLM call\n(prompt)| L[LLM Provider / Inference\n- Intent classify\n- Narrate\n- Suggest directives]
  L -->|Streamed AIResponse\n(narration + directives + options)| G
  G -->|Validated stream\n+ directives| U

  U -->|Resolution\n(roll results/combat log)| G
  G -->|Narrate outcome\n(using official results)| L
  L -->|Outcome narration| G
  G -->|Outcome text| U

  U -->|Save/Load| S[(Local Save\nWorldState.json)]
  G -->|Session summary + cache| C[(Server Cache\nSummary/Prefetch)]
