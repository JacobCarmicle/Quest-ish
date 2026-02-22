using UnityEngine;

public class EncounterDirector : MonoBehaviour
{
    [Range(0, 6)] public int threatClock = 0;
    public bool encounterActive;
    public GameObject enemyVisual;

    public void ApplyRollOutcome(bool success)
    {
        threatClock += success ? 0 : 2;
        threatClock = Mathf.Clamp(threatClock, 0, 6);

        if (threatClock >= 4)
            TriggerEncounter();
    }

    public void ApplyIntent(IntroIntentType intent)
    {
        if (intent == IntroIntentType.COMBAT)
            TriggerEncounter();
        else if (intent == IntroIntentType.INVALID)
            threatClock = Mathf.Clamp(threatClock + 1, 0, 6);
    }

    public void TriggerEncounter()
    {
        encounterActive = true;
        if (enemyVisual != null)
            enemyVisual.SetActive(true);
    }

    public void ResolveEncounter()
    {
        encounterActive = false;
        threatClock = 1;
        if (enemyVisual != null)
            enemyVisual.SetActive(false);
    }
}
