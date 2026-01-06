# 1️⃣ Product Vision & Constraints

## Purpose
This document defines the **core vision, tone, and non-negotiable constraints** of the project.  
Its goal is to keep **design, AI behavior, and engineering decisions aligned**, while preventing scope creep and unrealistic expectations of what AI should or should not do.

This is a *foundational document* — all future systems, features, and content should be traceable back to the principles defined here.

---

## Game Fantasy & Tone

**Tone:**  
- Goofy  
- Lighthearted  
- Self-aware  
- Comedic failure is part of the fun  

The game embraces **absurdity and chaos** rather than epic seriousness. Player mistakes, bad rolls, and unexpected outcomes are *entertaining*, not punishing.

Visually, the game uses:
- Stylized 2D art
- Expressive character animations
- Exaggerated reactions (bonks, flops, dramatic failures)

Narratively, the Dungeon Master:
- Has personality
- Sometimes jokes at the player’s expense
- Treats the campaign as a story *and* a game

This is not a grimdark fantasy or a simulation-heavy CRPG.

---

## Player Promise

**What the player should feel:**
> “I can try almost anything, and the game will *respond* — even if it goes horribly wrong.”

The game promises:
- **Free-form player input** (not just button presses)
- A campaign that reacts to *intent*, not keywords
- Meaningful consequences, but framed humorously
- A sense of agency without overwhelming complexity

What makes this game different:
- The AI Dungeon Master **guides the flow**, not just generates text
- The game feels *alive* while remaining mechanically fair
- Visual feedback reinforces narrative outcomes
- Failure is often more fun than success

---

## Hard Constraints (Non-Negotiable)

These constraints exist to ensure:
- stability
- performance
- profitability
- predictable gameplay

### AI Constraints
- The AI **is a narrator and director**, not a rules engine
- The AI **cannot**:
  - Decide dice outcomes
  - Modify stats directly
  - Invent items, abilities, or assets
  - Bypass game rules
- The AI **interprets intent and advances story flow**, but always defers to the engine for mechanics

### Engine Authority
- **Unity is the single source of truth**
- All mechanics are deterministic and engine-controlled:
  - Dice rolls
  - Combat resolution
  - HP, inventory, XP
  - Quest state
- The AI receives *results* from the engine and narrates them

### Player Input
- Free-form text input is allowed
- The AI must:
  - Classify intent
  - Reject or redirect impossible actions
  - Ask for clarification when needed
- Player freedom does **not** override world logic or constraints

### Platform & Performance
- Target platform: **Mobile (iOS & Android)**
- Performance goals:
  - Fast time-to-first-response from the AI
  - Streaming text preferred over long waits
  - Minimal UI blocking
- AI usage must be optimized for:
  - Low latency
  - Predictable cost per turn
  - Graceful degradation if AI is unavailable

---

## Design Guardrail

> **If a feature requires the AI to “just figure it out,” it is probably a bad feature.**

The AI is powerful, but it is treated as **infrastructure**, not magic.  
All systems must assume:
- the AI can be wrong
- the AI can be slow
- the AI must be constrained

This mindset prevents “AI magic thinking” and keeps the game shippable.

---

## Summary

This project is:
- A **goofy 2D fantasy campaign**
- Driven by an AI Dungeon Master with personality
- Grounded by a deterministic game engine
- Designed for player freedom *within* clear boundaries

These constraints are not limitations — they are what make the experience reliable, scalable, and fun.
