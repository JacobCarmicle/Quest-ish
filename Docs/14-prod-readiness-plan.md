# 14) Prod Readiness Plan (Tonight)

## Objective
Ship a stable, playable end-to-end vertical slice beyond tutorial, aligned to product/architecture docs:
- Unity authoritative mechanics
- AI narrative under strict contracts
- mobile-first responsiveness
- deterministic progression + recoverable saves

## Current Baseline
- Intro flow playable
- AI + fallback path
- Roll resolver + encounter/threat systems started
- Class selection/starter loadouts added
- EditMode tests passing (24/24)

---

## Phase 1 — Core Game Loop Completion (TDD)

### 1. Scene Flow + Routing
- [ ] Define canonical scene route map:
  - TitleScreen -> IntroScene -> Hub -> Encounter -> Hub
  - Continue -> last saved scene
- [ ] Wire main menu buttons to `MainMenuFlow` methods in scene
- [ ] Add tests for route decisions + progress persistence

**Done criteria:** all menu actions route to valid scene; no dead routes.

### 2. New Game Bootstrap
- [ ] New Game always prompts for Name + Class before free play
- [ ] Continue bypasses bootstrap if profile exists
- [ ] Save profile fields (name/class/stats/items/scene tag)

**Done criteria:** profile survives app restart; bootstrap appears only when needed.

### 3. Mechanics Authority
- [ ] Replace ad-hoc roll heuristics with explicit action->check mapping table
- [ ] Add DC bands by beat + threat
- [ ] Add result categories (fail / mixed / success / critical)
- [ ] Ensure AI cannot override roll outcomes

**Done criteria:** every SKILL_CHECK shows deterministic formula + outcome; tests cover mapping + bounds.

---

## Phase 2 — Narrative Progression + Encounters (TDD)

### 4. Beat Progression Engine
- [ ] Formal beat graph with allowed transitions
- [ ] Per-beat objectives and fail-forward outcomes
- [ ] Persist active beat and unresolved thread

**Done criteria:** story advances each turn; cannot stall at same beat without state change.

### 5. Encounter Director
- [ ] Threat clock escalation rules tuned
- [ ] Enemy encounter triggers from beats + failed checks
- [ ] Encounter resolve transitions back to story flow

**Done criteria:** encounter guaranteed by progression conditions; no soft-lock after battle.

### 6. Dynamic Speaker/Actor Presentation
- [ ] Speaker portrait switches DM/enemy/NPC correctly
- [ ] On encounter start, enemy portrait persists until encounter resolves
- [ ] NPC interactions use NPC portrait and name

**Done criteria:** portrait always matches active speaker context.

---

## Phase 3 — UX & Mobile Readiness (TDD + PlayMode)

### 7. Chat Transcript UX
- [ ] Scroll works with long transcripts
- [ ] Player + DM message history retained in session
- [ ] Overflow behavior verified across mobile aspect ratios

**Done criteria:** can read full history and manually scroll in Play Mode.

### 8. Dice Roll Feedback
- [ ] Roll animation guaranteed to appear in skill checks
- [ ] Final roll line always visible in transcript
- [ ] Animation non-blocking on low FPS devices

**Done criteria:** players always see roll process and result.

### 9. Failure Handling
- [ ] AI timeout/retry user feedback
- [ ] Fallback path keeps game playable
- [ ] Error logs summarized into recent events

**Done criteria:** no turn drops on network/model failure.

---

## Phase 4 — Content + Assets

### 10. Asset Pass (No blocker fallback + optional import)
- [ ] Keep generated placeholder portraits/icons as guaranteed fallback
- [ ] Add free-pack import checklist for enemy/NPC UI icons + lightweight VFX
- [ ] Apply only allowlisted asset tags

**Done criteria:** visual placeholders never break game; optional packs improve polish.

---

## Phase 5 — Test Gate for Tonight

## Required test suite
- [ ] EditMode: >= 30 tests, all passing
- [ ] New tests added for:
  - scene routing/menu flow
  - bootstrap/continue behavior
  - beat transitions
  - encounter trigger/resolve
  - roll mapping and deterministic outcomes
  - portrait switching by context
  - transcript scroll behavior logic
- [ ] PlayMode smoke tests for:
  - New Game full loop
  - Continue path
  - one encounter cycle

## Release checklist (tonight)
- [ ] 0 compile errors
- [ ] no missing script refs
- [ ] all critical button paths verified
- [ ] save/load survives restart
- [ ] fallback mode playable if AI unavailable

---

## Execution Style
Use strict TDD loop for each feature:
1) write failing test
2) implement minimum to pass
3) refactor
4) rerun full suite
5) commit

Each feature should include a passing test before moving on.
