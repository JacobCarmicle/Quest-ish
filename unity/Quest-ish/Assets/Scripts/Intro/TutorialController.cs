using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public enum TutorialPhase
    {
        Arrival,
        NameQuestion,
        FirstAction,
        Outcome,
        Reinforcement,
        Complete
    }

    [SerializeField] private TutorialPhase _phase = TutorialPhase.Arrival;
    public TutorialPhase Phase => _phase;

    public bool IsActive => _phase != TutorialPhase.Complete;

    public void SetPhase(TutorialPhase phase) => _phase = phase;

    public void Advance()
    {
        if (_phase == TutorialPhase.Complete) return;
        _phase++;
    }
}
