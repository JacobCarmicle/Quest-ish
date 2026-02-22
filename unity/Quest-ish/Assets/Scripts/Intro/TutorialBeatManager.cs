using System;
using UnityEngine;

public enum TutorialBeat
{
    WakeInTower,
    IdentifyObjective,
    InvestigateRune,
    TriggerThreat,
    FirstEncounter,
    ResolveEncounter,
    ExitTower,
    Complete
}

public class TutorialBeatManager : MonoBehaviour
{
    public TutorialBeat currentBeat = TutorialBeat.WakeInTower;

    public void Advance(TutorialBeat next)
    {
        if ((int)next > (int)currentBeat)
            currentBeat = next;
    }

    public void AdvanceByIntent(IntroIntentType intent)
    {
        switch (currentBeat)
        {
            case TutorialBeat.WakeInTower:
                Advance(TutorialBeat.IdentifyObjective);
                break;
            case TutorialBeat.IdentifyObjective:
                if (intent == IntroIntentType.DIALOGUE || intent == IntroIntentType.SKILL_CHECK)
                    Advance(TutorialBeat.InvestigateRune);
                break;
            case TutorialBeat.InvestigateRune:
                if (intent == IntroIntentType.SKILL_CHECK || intent == IntroIntentType.COMBAT)
                    Advance(TutorialBeat.TriggerThreat);
                break;
            case TutorialBeat.TriggerThreat:
                Advance(TutorialBeat.FirstEncounter);
                break;
            case TutorialBeat.FirstEncounter:
                if (intent == IntroIntentType.SKILL_CHECK || intent == IntroIntentType.COMBAT)
                    Advance(TutorialBeat.ResolveEncounter);
                break;
            case TutorialBeat.ResolveEncounter:
                Advance(TutorialBeat.ExitTower);
                break;
        }
    }

    public string GoalHint()
    {
        return currentBeat switch
        {
            TutorialBeat.WakeInTower => "Orient yourself in the wizard tower.",
            TutorialBeat.IdentifyObjective => "Find what is destabilizing the tower wards.",
            TutorialBeat.InvestigateRune => "Inspect and interact with the glowing rune circle.",
            TutorialBeat.TriggerThreat => "The ward backlash is building; contain it.",
            TutorialBeat.FirstEncounter => "Survive the first hostile arcane manifestation.",
            TutorialBeat.ResolveEncounter => "Stabilize the room and secure the door.",
            TutorialBeat.ExitTower => "Unlock the exit and leave the tutorial zone.",
            _ => "Continue your adventure."
        };
    }
}
