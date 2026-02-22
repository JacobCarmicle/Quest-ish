using System;
using System.Collections;
using UnityEngine;

public class IntroFlowDirector : MonoBehaviour
{
    [Header("Wired by Builder")]
    public TutorialController tutorial;
    public GameState gameState;
    public DialogueController dialogue;
    public PlayerInputController input;
    public HintToast hint;
    public IntroOptionsUI optionsUI;
    public IntroDirectiveExecutor directiveExecutor;
    public TutorialBeatManager beatManager;
    public EncounterDirector encounterDirector;
    public DiceRollAnimator diceRollAnimator;
    public Sprite npcPortrait;
    public string npcName = "Caretaker";

    [Header("AI (Optional)")]
    public bool enableAI = false;
    public OpenAIIntroClient aiClient;

    [Header("Speaker Portraits (Optional)")]
    public Sprite dmPortrait;
    public Sprite enemyPortrait;

    [Header("Speaker Names")]
    public string dmName = "DM";
    public string enemyName = "Enemy";

    private bool _isHandling;
    private bool _awaitingClassSelection;

    private void Awake()
    {
        tutorial ??= FindFirstObjectByType<TutorialController>();
        gameState ??= FindFirstObjectByType<GameState>();
        dialogue ??= FindFirstObjectByType<DialogueController>();
        input ??= FindFirstObjectByType<PlayerInputController>();
        hint ??= FindFirstObjectByType<HintToast>();
        optionsUI ??= FindFirstObjectByType<IntroOptionsUI>();
        directiveExecutor ??= FindFirstObjectByType<IntroDirectiveExecutor>();
        beatManager ??= FindFirstObjectByType<TutorialBeatManager>();
        encounterDirector ??= FindFirstObjectByType<EncounterDirector>();
        diceRollAnimator ??= FindFirstObjectByType<DiceRollAnimator>();
        aiClient ??= FindFirstObjectByType<OpenAIIntroClient>();

        if (optionsUI == null)
        {
            var go = new GameObject("IntroOptionsUI");
            optionsUI = go.AddComponent<IntroOptionsUI>();
        }

        if (directiveExecutor == null)
        {
            var go = new GameObject("IntroDirectiveExecutor");
            directiveExecutor = go.AddComponent<IntroDirectiveExecutor>();
        }

        if (aiClient == null)
        {
            var go = new GameObject("OpenAIIntroClient");
            aiClient = go.AddComponent<OpenAIIntroClient>();
        }

        if (beatManager == null)
        {
            var go = new GameObject("TutorialBeatManager");
            beatManager = go.AddComponent<TutorialBeatManager>();
        }

        if (encounterDirector == null)
        {
            var go = new GameObject("EncounterDirector");
            encounterDirector = go.AddComponent<EncounterDirector>();
        }

        if (diceRollAnimator == null)
        {
            var go = new GameObject("DiceRollAnimator");
            var cg = go.AddComponent<CanvasGroup>();
            var txtGo = new GameObject("RollText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.GetComponent<TMPro.TextMeshProUGUI>();
            txt.fontSize = 28;
            txt.alignment = TMPro.TextAlignmentOptions.Center;
            var rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.78f);
            rt.anchorMax = new Vector2(0.8f, 0.92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            diceRollAnimator = go.AddComponent<DiceRollAnimator>();
            diceRollAnimator.canvasGroup = cg;
            diceRollAnimator.rollText = txt;
            cg.alpha = 0f;
        }

        EnsurePortraitPlaceholders();

        if (!enableAI && aiClient != null && aiClient.HasConfiguredApiKey)
            enableAI = true;
    }

    private static readonly string[] ImpossibleHints = { "god", "infinite", "hack", "cheat", "spawn 10000", "delete the game" };
    private static readonly string[] CombatHints = { "attack", "hit", "stab", "shoot", "fight", "kill", "punch" };
    private static readonly string[] SkillHints = { "sneak", "stealth", "persuade", "convince", "climb", "pick", "lock", "search", "inspect", "roll" };

    private void OnEnable()
    {
        if (input != null) input.OnSubmit += HandleSubmit;
    }

    private void OnDisable()
    {
        if (input != null) input.OnSubmit -= HandleSubmit;
    }

    private void Start()
    {
        EnsureAudioListener();

        if (gameState != null && gameState.world == null)
            gameState.world = WorldStatePersistence.Load();

        if (tutorial != null)
            tutorial.SetPhase(TutorialController.TutorialPhase.Arrival);

        SetSpeaker(dmName, dmPortrait);
        dialogue?.ClearTranscript();

        dialogue?.Say(
            "You wake in a wizard tower, the air humming with unstable magic.\n\n" +
            "Before we continue — what do people call you?"
        );

        tutorial?.SetPhase(TutorialController.TutorialPhase.NameQuestion);
        input?.SetInteractable(true);

        if (hint != null)
        {
            hint.HideImmediate();
            if (enableAI && (aiClient == null || !aiClient.HasConfiguredApiKey))
                hint.ShowForSeconds("AI enabled but API key missing. Using fallback DM.", 2.8f);
            else
                hint.ShowForSeconds("Tip: You can type anything.", 2.5f);
        }

        optionsUI?.Clear();
        gameState?.AddRecentEvent("Tutorial started in wizard tower");
    }

    private void HandleSubmit(string playerText)
    {
        if (_isHandling) return;
        StartCoroutine(HandleSubmitRoutine(playerText));
    }

    private IEnumerator HandleSubmitRoutine(string playerText)
    {
        playerText = (playerText ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(playerText)) yield break;

        _isHandling = true;
        input?.SetInteractable(false);
        dialogue?.AddPlayerLine(playerText);

        var phase = tutorial != null ? tutorial.Phase : TutorialController.TutorialPhase.Arrival;

        if (phase == TutorialController.TutorialPhase.NameQuestion)
        {
            gameState?.SetPlayerName(playerText);
            gameState?.AddRecentEvent($"Chose name: {playerText}");
            var pstate = MainMenuFlow.LoadProgress();
            pstate.playerName = playerText;
            pstate.lastSceneTag = "intro_scene";
            MainMenuFlow.SaveProgress(pstate);

            SetSpeaker(dmName, dmPortrait);
            dialogue?.Say($"Alright, {playerText}. Strong name.\n\nChoose your class: Thief, Fighter, or Mage.");
            tutorial?.SetPhase(TutorialController.TutorialPhase.FirstAction);
            _awaitingClassSelection = true;
            hint?.ShowForSeconds("Pick a class to lock in your starter stats and items.", 3.0f);

            SaveState();
            input?.SetInteractable(true);
            _isHandling = false;
            yield break;
        }

        if (_awaitingClassSelection)
        {
            if (gameState?.world?.player != null && CharacterLoadout.TryApplyClass(gameState.world.player, playerText, out var className))
            {
                _awaitingClassSelection = false;
                var pstate2 = MainMenuFlow.LoadProgress();
                pstate2.hasStartedGame = true;
                pstate2.playerClass = gameState.world.player.classTag;
                pstate2.lastSceneTag = "intro_scene";
                MainMenuFlow.SaveProgress(pstate2);
                SetSpeaker(dmName, dmPortrait);
                dialogue?.Say($"Excellent. {className} it is.\n\n{CharacterLoadout.StarterSummary(gameState.world.player)}\n\nYou stand in a wizard tower study with a sealed door, a glowing rune circle, and a whispering spellbook. What do you do?");
                hint?.ShowForSeconds("Now take your first meaningful action.", 2.5f);
                SaveState();
                input?.SetInteractable(true);
                _isHandling = false;
                yield break;
            }

            dialogue?.Say("Pick one class exactly: Thief, Fighter, or Mage.");
            input?.SetInteractable(true);
            _isHandling = false;
            yield break;
        }

        IntroAIResponse aiResponse = null;
        string aiError = null;

        if (enableAI && aiClient != null)
        {
            var req = BuildTurnRequest(playerText);
            yield return aiClient.RequestTurn(req, (resp, err) =>
            {
                aiResponse = resp;
                aiError = err;
            });
        }

        if (aiResponse == null)
        {
            if (!string.IsNullOrEmpty(aiError))
            {
                gameState?.AddRecentEvent($"AI fallback: {aiError}");
                hint?.ShowForSeconds("Network hiccup. Using local DM fallback.", 2.2f);
            }

            var fallbackIntent = ClassifyInputHeuristic(playerText);
            ApplyIntent(fallbackIntent, playerText, null);
            optionsUI?.Clear();
        }
        else
        {
            if (!IntroAIResponseValidator.TryNormalize(aiResponse, out var validatedIntent))
                validatedIntent = ClassifyInputHeuristic(playerText);

            ApplyIntent(validatedIntent, playerText, aiResponse);
            ApplyDirectives(aiResponse.directives);
            optionsUI?.Clear();
        }

        if (tutorial != null && tutorial.Phase == TutorialController.TutorialPhase.Complete)
            encounterDirector?.ResolveEncounter();

        SaveState();
        input?.SetInteractable(true);
        _isHandling = false;
    }

    public IntroTurnRequest BuildTurnRequest(string playerText)
    {
        var world = gameState != null ? gameState.world : new WorldStateModel();
        return new IntroTurnRequest
        {
            sessionId = world.sessionId,
            playerInput = playerText,
            phase = tutorial != null ? tutorial.Phase.ToString() : "Unknown",
            playerName = world.player != null ? world.player.name : string.Empty,
            storySummary = world.storySummary,
            recentEvents = world.recentEvents ?? Array.Empty<string>(),
            currentBeat = beatManager != null ? beatManager.currentBeat.ToString() : "WakeInTower",
            goalHint = beatManager != null ? beatManager.GoalHint() : "Stabilize the tower",
            threatClock = encounterDirector != null ? encounterDirector.threatClock : 0,
            encounterActive = encounterDirector != null && encounterDirector.encounterActive
        };
    }

    public IntroIntentType ParseIntent(string raw, string playerText)
    {
        if (Enum.TryParse(raw ?? string.Empty, true, out IntroIntentType parsed))
            return parsed;
        return ClassifyInputHeuristic(playerText);
    }

    public IntroIntentType ClassifyInputHeuristic(string text)
    {
        var lower = (text ?? string.Empty).ToLowerInvariant();

        foreach (var k in ImpossibleHints)
            if (lower.Contains(k)) return IntroIntentType.INVALID;

        foreach (var k in CombatHints)
            if (lower.Contains(k)) return IntroIntentType.COMBAT;

        foreach (var k in SkillHints)
            if (lower.Contains(k)) return IntroIntentType.SKILL_CHECK;

        if (lower.Split(' ').Length <= 2 || lower == "do it" || lower == "that")
            return IntroIntentType.CLARIFY;

        return IntroIntentType.DIALOGUE;
    }

    public void ApplyIntent(IntroIntentType intent, string playerText, IntroAIResponse ai)
    {
        var narration = ai != null && !string.IsNullOrWhiteSpace(ai.narration) ? ai.narration : null;
        var clarify = ai != null && !string.IsNullOrWhiteSpace(ai.clarifyQuestion) ? ai.clarifyQuestion : null;
        beatManager?.AdvanceByIntent(intent);
        encounterDirector?.ApplyIntent(intent);

        var currentPhase = tutorial != null ? tutorial.Phase : TutorialController.TutorialPhase.Arrival;
        var nextResolvedPhase = currentPhase == TutorialController.TutorialPhase.Outcome
            ? TutorialController.TutorialPhase.Complete
            : TutorialController.TutorialPhase.Outcome;

        switch (intent)
        {
            case IntroIntentType.DIALOGUE:
                var lower = (playerText ?? string.Empty).ToLowerInvariant();
                var npcTalk = lower.Contains("npc") || lower.Contains("caretaker") || lower.Contains("librarian") || lower.Contains("talk");
                if (encounterDirector != null && encounterDirector.encounterActive)
                    SetSpeaker(enemyName, enemyPortrait != null ? enemyPortrait : dmPortrait);
                else if (npcTalk && npcPortrait != null)
                    SetSpeaker(npcName, npcPortrait);
                else
                    SetSpeaker(dmName, dmPortrait);
                dialogue?.Say(narration ?? $"You attempt: \"{playerText}\".\n\nThe tower reacts — faint runes pulse near the sealed door.\nGoal: {beatManager?.GoalHint() ?? "Keep exploring"}");
                tutorial?.SetPhase(nextResolvedPhase);
                gameState?.AddRecentEvent($"Dialogue: {playerText}");
                break;

            case IntroIntentType.SKILL_CHECK:
                if (encounterDirector != null && encounterDirector.encounterActive)
                    SetSpeaker(enemyName, enemyPortrait != null ? enemyPortrait : dmPortrait);
                else
                    SetSpeaker(dmName, dmPortrait);
                var level = gameState?.world?.player != null ? gameState.world.player.level : 1;
                var roll = RollResolver.Resolve(playerText, level);
                var rollLine = $"Roll: d20({roll.d20}) + {roll.modifier} [{roll.skill}] = {roll.total} vs DC {roll.dc} → {(roll.success ? "SUCCESS" : "FAIL")}";
                diceRollAnimator?.Play(roll);
                encounterDirector?.ApplyRollOutcome(roll.success);
                if (roll.success && encounterDirector != null && encounterDirector.encounterActive && beatManager != null && beatManager.currentBeat >= TutorialBeat.FirstEncounter)
                    encounterDirector.ResolveEncounter();
                var outcomeText = roll.success
                    ? "Your move works, but stirs unstable energy in the room."
                    : "You miss the mark; the tower pushes back with a crackle of arcane force.";
                var encounterText = encounterDirector != null && encounterDirector.encounterActive
                    ? "\nAn Arcane Sentinel manifests from the rune circle!"
                    : "";
                dialogue?.Say((narration ?? $"You attempt \"{playerText}\".") + $"\n\n{rollLine}\n{outcomeText}{encounterText}\nGoal: {beatManager?.GoalHint() ?? "Stabilize the tower"}");
                tutorial?.SetPhase(nextResolvedPhase);
                gameState?.AddRecentEvent($"Skill check: {playerText} | {rollLine}");
                break;

            case IntroIntentType.COMBAT:
                SetSpeaker(enemyName, enemyPortrait != null ? enemyPortrait : dmPortrait);
                encounterDirector?.TriggerEncounter();
                dialogue?.Say(narration ?? $"You escalate with \"{playerText}\".\n\nA hostile construct animates near the rune circle. Stay focused and describe your next move.\nGoal: {beatManager?.GoalHint() ?? "Survive"}");
                tutorial?.SetPhase(nextResolvedPhase);
                gameState?.AddRecentEvent($"Combat intent: {playerText}");
                break;

            case IntroIntentType.CLARIFY:
                SetSpeaker(dmName, dmPortrait);
                dialogue?.Say(clarify ?? "Do you want to talk, sneak, or attack?");
                gameState?.AddRecentEvent("Clarify requested");
                break;

            case IntroIntentType.INVALID:
                SetSpeaker(dmName, dmPortrait);
                dialogue?.Say(narration ?? "That breaks the rules of this world. Try something in-world and chaotic instead.");
                gameState?.AddRecentEvent($"Invalid intent: {playerText}");
                break;
        }
    }

    public void ApplyDirectives(IntroDirective[] directives)
    {
        if (directives == null || directives.Length == 0) return;

        foreach (var d in directives)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.type)) continue;

            if (d.type == "ask_clarification")
            {
                dialogue?.Say("Can you clarify that action?");
                gameState?.AddRecentEvent("Directive:ask_clarification");
                continue;
            }

            var isAllowed = d.type == "set_background" || d.type == "play_music" || d.type == "play_sfx" || d.type == "show_actor" || d.type == "hide_actor";
            var executed = directiveExecutor != null && directiveExecutor.Execute(d);

            if (isAllowed)
                gameState?.AddRecentEvent($"Directive:{d.type}");
            else if (executed)
                gameState?.AddRecentEvent($"Directive:{d.type}");
            else
                gameState?.AddRecentEvent($"Ignored directive:{d.type}");
        }
    }

    private void SaveState()
    {
        if (gameState == null || gameState.world == null) return;
        WorldStatePersistence.Save(gameState.world);
    }

    private void SetSpeaker(string speakerName, Sprite portrait)
    {
        if (dialogue == null) return;
        dialogue.SetSpeaker(speakerName, portrait);
    }

    private void EnsurePortraitPlaceholders()
    {
        if (dmPortrait == null) dmPortrait = GeneratePlaceholderSprite(new Color(0.85f, 0.75f, 0.25f));
        if (enemyPortrait == null) enemyPortrait = GeneratePlaceholderSprite(new Color(0.82f, 0.2f, 0.2f));
        if (npcPortrait == null) npcPortrait = GeneratePlaceholderSprite(new Color(0.2f, 0.6f, 0.9f));
    }

    private static Sprite GeneratePlaceholderSprite(Color color)
    {
        var size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var c = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var dx = x - size * 0.5f;
                var dy = y - size * 0.5f;
                var dist = Mathf.Sqrt(dx * dx + dy * dy) / (size * 0.5f);
                var alpha = dist <= 1f ? 1f : 0f;
                c[y * size + x] = new Color(color.r, color.g, color.b, alpha);
            }
        }
        tex.SetPixels(c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void EnsureAudioListener()
    {
        if (FindFirstObjectByType<AudioListener>() != null) return;

        var cam = Camera.main;
        if (cam != null)
        {
            cam.gameObject.AddComponent<AudioListener>();
            return;
        }

        gameObject.AddComponent<AudioListener>();
    }
}
