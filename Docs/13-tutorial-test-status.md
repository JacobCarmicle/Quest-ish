# 13) Tutorial Test Status

_Last updated: automated by OpenClaw_

## Current automated suite (EditMode)
Result: **PASS** (13/13)

1. `TutorialController_Advance_StopsAtComplete` ✅
2. `GameState_AddRecentEvent_KeepLast10` ✅
3. `PlayerInputController_SendButton_SubmitsAndClearsInput` ✅
4. `IntroFlowDirector_NameQuestion_SubmitSetsPlayerNameAndAdvancesPhase` ✅
5. `IntroFlowDirector_FirstAction_SubmitAddsEventAndAdvancesToOutcome` ✅
6. `IntroFlowDirector_Outcome_SubmitCompletesTutorial` ✅
7. `IntroFlowDirector_EmptySubmit_DoesNothing` ✅
8. `DialogueController_SetSpeaker_UsesDefaultPortraitWhenMissing` ✅
9. `IntroScene_PortraitLayout_IsTopRightOverflowAndBodyTextLower` ✅
10. `IntroFlowDirector_ParseIntent_Matrix_Works` ✅
11. `IntroFlowDirector_BuildTurnRequest_ContainsState` ✅
12. `IntroFlowDirector_UnknownDirective_IsSafelyIgnored` ✅
13. `WorldStatePersistence_SaveLoad_RoundTrip_Works` ✅

## Covered right now
- Tutorial phase progression
- State log ring-buffer behavior (`recentEvents`)
- Input submit plumbing and no-op empty submit
- Intro flow branching for key phases
- Dialogue portrait fallback behavior
- Intent parsing/routing matrix coverage
- Turn request construction from runtime state
- Directive allowlist safe-ignore behavior for unknown directives
- Persistence save/load round-trip for world state
- Core portrait/text layout guard in Intro scene YAML

## Still missing test coverage (next)
- Real network AI integration (with mocked HTTP + contract validation)
- Timeout + retry/fallback UI messaging contract
- PlayMode integration tests for runtime UI behavior and no-overlap across aspect ratios
- Directive side effects beyond logging (actual VFX/SFX/background systems)

## Tracking rule
- Any new tutorial feature should ship with at least one test in `Assets/Tests/EditMode/Intro` or `Assets/Tests/PlayMode/Intro`.
