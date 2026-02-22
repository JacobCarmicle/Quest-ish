using UnityEngine;

public class IntroFlowDirector : MonoBehaviour
{
    [Header("Wired by Builder")]
    public TutorialController tutorial;
    public GameState gameState;
    public DialogueController dialogue;
    public PlayerInputController input;
    public HintToast hint;

    [Header("Speaker Portraits (Optional)")]
    public Sprite dmPortrait;
    public Sprite enemyPortrait;

    [Header("Speaker Names")]
    public string dmName = "DM";
    public string enemyName = "Enemy";

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

        // Phase 0: Arrival
        if (tutorial != null)
            tutorial.SetPhase(TutorialController.TutorialPhase.Arrival);

        SetSpeaker(dmName, dmPortrait);

        if (dialogue != null)
        {
            dialogue.Say(
                "You wake up on a forest path. That’s concerning… but not unusual.\n\n" +
                "Before you do anything else — what do people call you?"
            );
        }

        if (tutorial != null)
            tutorial.SetPhase(TutorialController.TutorialPhase.NameQuestion);

        if (input != null)
            input.SetInteractable(true);

        // First-time micro-hint (non-blocking)
        if (hint != null)
        {
            hint.HideImmediate();
            hint.ShowForSeconds("Tip: You can type anything.", 2.5f);
        }
    }

    private void HandleSubmit(string playerText)
    {
        playerText = (playerText ?? "").Trim();
        if (string.IsNullOrEmpty(playerText)) return;

        var phase = tutorial != null ? tutorial.Phase : TutorialController.TutorialPhase.Arrival;

        switch (phase)
        {
            case TutorialController.TutorialPhase.NameQuestion:
            {
                if (gameState != null)
                {
                    gameState.SetPlayerName(playerText);
                    gameState.AddRecentEvent($"Chose name: {playerText}");
                }

                SetSpeaker(dmName, dmPortrait);

                if (dialogue != null)
                {
                    dialogue.Say(
                        $"Alright, {playerText}. Strong name.\n\n" +
                        "What do you do?"
                    );
                }

                if (tutorial != null)
                    tutorial.SetPhase(TutorialController.TutorialPhase.FirstAction);

                if (hint != null)
                    hint.ShowForSeconds("Tip: Try something silly. Failure is part of the fun.", 3.0f);

                break;
            }

            case TutorialController.TutorialPhase.FirstAction:
            {
                if (gameState != null)
                    gameState.AddRecentEvent($"Tried: {playerText}");

                // You can swap to an enemy portrait/name here later if the story introduces one.
                SetSpeaker(dmName, dmPortrait);

                if (dialogue != null)
                {
                    dialogue.Say(
                        $"You try to \"{playerText}\".\n\n" +
                        "It almost works. Almost.\n\n" +
                        "What do you do next?"
                    );
                }

                if (tutorial != null)
                    tutorial.SetPhase(TutorialController.TutorialPhase.Outcome);

                break;
            }

            case TutorialController.TutorialPhase.Outcome:
            {
                SetSpeaker(dmName, dmPortrait);

                if (tutorial != null)
                    tutorial.SetPhase(TutorialController.TutorialPhase.Complete);

                if (dialogue != null)
                {
                    dialogue.Say(
                        "Okay. You seem ready. The path ahead looks… questionably safe.\n\n" +
                        "What do you do?"
                    );
                }

                break;
            }

            default:
            {
                if (gameState != null)
                    gameState.AddRecentEvent($"Did: {playerText}");

                SetSpeaker(dmName, dmPortrait);

                if (dialogue != null)
                    dialogue.Say($"Noted: \"{playerText}\".\n\n(Next: connect AI DM + engine rules.)");

                break;
            }
        }
    }

    private void SetSpeaker(string speakerName, Sprite portrait)
    {
        if (dialogue == null) return;
        dialogue.SetSpeaker(speakerName, portrait);
    }

    private void EnsureAudioListener()
    {
        // Your console screenshot shows: "There are no audio listeners in the scene"
        // Unity needs exactly 1 AudioListener to hear ANY AudioSource.
        if (FindObjectOfType<AudioListener>() != null) return;

        var cam = Camera.main;
        if (cam != null)
        {
            cam.gameObject.AddComponent<AudioListener>();
            return;
        }

        // Fallback: add one to this object so you at least get sound.
        gameObject.AddComponent<AudioListener>();
    }
}
