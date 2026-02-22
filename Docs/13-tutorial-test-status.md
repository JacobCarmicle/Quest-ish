# 13) Tutorial Test Status

_Last updated: automated by OpenClaw_

## Current automated suite (EditMode)
Result: **PASS** (6/6)

1. `TutorialController_Advance_StopsAtComplete` ✅
2. `GameState_AddRecentEvent_KeepLast10` ✅
3. `PlayerInputController_SendButton_SubmitsAndClearsInput` ✅
4. `IntroFlowDirector_NameQuestion_SubmitSetsPlayerNameAndAdvancesPhase` ✅
5. `IntroFlowDirector_Outcome_SubmitCompletesTutorial` ✅
6. `IntroScene_PortraitLayout_IsTopRightOverflowAndBodyTextLower` ✅

## Covered right now
- Tutorial phase progression
- State log ring-buffer behavior (`recentEvents`)
- Input submit plumbing
- Intro flow branching for key phases
- Core portrait/text layout guard in Intro scene YAML

## Still missing test coverage (next)
- TurnRequest construction + outbound dispatch
- Intent routing matrix: `DIALOGUE`, `SKILL_CHECK`, `COMBAT`, `CLARIFY`, `INVALID`
- Directive allowlist validation and safe-ignore behavior
- Persistence save/load round-trip for world state
- Failure/timeout fallback behavior
- PlayMode integration tests for runtime UI behavior and no-overlap across aspect ratios

## Tracking rule
- Any new tutorial feature should ship with at least one test in `Assets/Tests/EditMode/Intro` or `Assets/Tests/PlayMode/Intro`.
