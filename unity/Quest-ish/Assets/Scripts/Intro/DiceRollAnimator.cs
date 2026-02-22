using System.Collections;
using TMPro;
using UnityEngine;

public class DiceRollAnimator : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_Text rollText;

    public void Play(SkillRollResult result)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(PlayRoutine(result));
    }

    private IEnumerator PlayRoutine(SkillRollResult result)
    {
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        for (int i = 0; i < 6; i++)
        {
            var r = Random.Range(1, 21);
            if (rollText != null) rollText.text = $"Rolling... d20: {r}";
            yield return new WaitForSeconds(0.08f);
        }

        if (rollText != null)
            rollText.text = $"d20({result.d20}) + {result.modifier} [{result.skill}] = {result.total} vs DC {result.dc} → {(result.success ? "SUCCESS" : "FAIL")}";

        yield return new WaitForSeconds(1.2f);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}
