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

    private void Awake()
    {
        tutorial ??= FindFirstObjectByType<TutorialController>();
        gameState ??= FindFirstObjectByType<GameState>();
        dialogue ??= FindFirstObjectByType<DialogueController>();
        input ??= FindFirstObjectByType<PlayerInputController>();
        hint ??= FindFirstObjectByType<HintToast>();
        optionsUI ??= FindFirstObjectByType<IntroOptionsUI>();
        directiveExecutor ??= FindFirstObjectByType<IntroDirectiveExecutor>();
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

        var phase = tutorial != null ? tutorial.Phase : TutorialController.TutorialPhase.Arrival;

        if (phase == TutorialController.TutorialPhase.NameQuestion)
        {
            gameState?.SetPlayerName(playerText);
            gameState?.AddRecentEvent($"Chose name: {playerText}");

            SetSpeaker(dmName, dmPortrait);
            dialogue?.Say($"Alright, {playerText}. Strong name.\n\nYou’re in a wizard tower study with a sealed door, a glowing rune circle, and a whispering spellbook. What do you do?");
            tutorial?.SetPhase(TutorialController.TutorialPhase.FirstAction);
            hint?.ShowForSeconds("Tip: Try something silly. Failure is part of the fun.", 3.0f);

            SaveState();
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
            recentEvents = world.recentEvents ?? Array.Empty<string>()
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

        var currentPhase = tutorial != null ? tutorial.Phase : TutorialController.TutorialPhase.Arrival;
        var nextResolvedPhase = currentPhase == TutorialController.TutorialPhase.Outcome
            ? TutorialController.TutorialPhase.Complete
            : TutorialController.TutorialPhase.Outcome;

        switch (intent)
        {
            case IntroIntentType.DIALOGUE:
                SetSpeaker(dmName, dmPortrait);
                dialogue?.Say(narration ?? $"You attempt: \"{playerText}\".\n\nThe tower reacts — faint runes pulse near the sealed door. What do you try next?");
                tutorial?.SetPhase(nextResolvedPhase);
                gameState?.AddRecentEvent($"Dialogue: {playerText}");
                break;

            case IntroIntentType.SKILL_CHECK:
                SetSpeaker(dmName, dmPortrait);
                var level = gameState?.world?.player != null ? gameState.world.player.level : 1;
                var roll = RollResolver.Resolve(playerText, level);
                var rollLine = $"Roll: d20({roll.d20}) + {roll.modifier} [{roll.skill}] = {roll.total} vs DC {roll.dc} → {(roll.success ? "SUCCESS" : "FAIL")}";
                var outcomeText = roll.success
                    ? "Your move works, but stirs unstable energy in the room."
                    : "You miss the mark; the tower pushes back with a crackle of arcane force.";
                dialogue?.Say((narration ?? $"You attempt \"{playerText}\".") + $"\n\n{rollLine}\n{outcomeText}");
                tutorial?.SetPhase(nextResolvedPhase);
                gameState?.AddRecentEvent($"Skill check: {playerText} | {rollLine}");
                break;

            case IntroIntentType.COMBAT:
                SetSpeaker(enemyName, enemyPortrait != null ? enemyPortrait : dmPortrait);
                dialogue?.Say(narration ?? $"You escalate with \"{playerText}\".\n\nA hostile construct animates near the rune circle. Stay focused and describe your next move.");
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
