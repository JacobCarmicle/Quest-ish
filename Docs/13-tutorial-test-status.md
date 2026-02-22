# 13) Tutorial Test Status

_Last updated: automated by OpenClaw_

## Current automated suite (EditMode)
Result: **PASS** (18/18)

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
14. `IntroAIResponseValidator_InvalidIntent_ReturnsFalse` ✅
15. `IntroAIResponseValidator_ValidIntent_NormalizesFields` ✅
16. `IntroFlowDirector_AllowedDirective_IsTracked` ✅
17. `IntroDirectiveExecutor_SetBackground_Works` ✅
18. `IntroOptionsUI_ShowOptions_SpawnsButtons` ✅

## Covered right now
- Tutorial phase progression
- State log ring-buffer behavior (`recentEvents`)
- Input submit plumbing and no-op empty submit
- Intro flow branching for key phases
- Dialogue portrait fallback behavior
- Intent parsing/routing matrix coverage
- Turn request construction from runtime state
- AI response validation/normalization
- Directive allowlist behavior + unknown safe-ignore
- Directive execution path (background swap)
- Options UI spawn pipeline
- Persistence save/load round-trip for world state
- Core portrait/text layout guard in Intro scene YAML

## Still missing test coverage (next)
- Live API integration test harness (mock HTTP layer + contract fixture set)
- Timeout + retry/fallback UI messaging contract assertions
- PlayMode integration tests for runtime UI behavior and no-overlap across aspect ratios
- Directive side effects for audio/actor visibility in scene integration context

## Tracking rule
- Any new tutorial feature should ship with at least one test in `Assets/Tests/EditMode/Intro` or `Assets/Tests/PlayMode/Intro`.
