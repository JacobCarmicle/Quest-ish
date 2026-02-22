# 12) Tutorial Screen Test Checklist

Use this as the acceptance checklist for Intro/Tutorial scene.

## A. Core Turn Loop
- [ ] Player can submit free-form text via button and Enter key.
- [ ] `TurnRequest` payload is constructed and sent.
- [ ] Intent routes to one of: DIALOGUE / SKILL_CHECK / COMBAT / CLARIFY / INVALID.
- [ ] Branch fallback works when response is invalid.

## B. Deterministic Engine Rules
- [ ] Skill check branch resolves in Unity (not AI).
- [ ] Combat branch starts Unity encounter flow.
- [ ] AI narration uses official outcome from Unity resolution.

## C. AI Contract + Directives
- [ ] AIResponse schema validates before application.
- [ ] Unknown directives are ignored safely (no crash).
- [ ] Allowed directives apply visual/audio changes correctly.
- [ ] Asset tags are allowlisted and validated.

## D. UI/UX Tutorial
- [ ] DM portrait appears in intended slot (top-right overflow from dialogue container).
- [ ] Character name is anchored next to portrait.
- [ ] Dialogue body text does not overlap portrait.
- [ ] Input row remains visible on target aspect ratios.
- [ ] Clarify/invalid responses display correctly.

## E. Persistence
- [ ] Player name persists in state.
- [ ] `recentEvents` records and caps correctly.
- [ ] State saves after each turn and reloads cleanly.

## F. Latency + Failure Handling
- [ ] TTFT feels responsive (streaming visible quickly).
- [ ] Timeout fallback message appears if AI unavailable.
- [ ] Retry path works without losing state.

## G. Regression Smoke (per build)
- [ ] Intro scene enters Play Mode with 0 compile errors.
- [ ] No missing script references in scene/prefabs.
- [ ] No blocking exceptions in Console during first 3 turns.
