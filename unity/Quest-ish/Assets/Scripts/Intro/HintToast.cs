using System.Collections;
using TMPro;
using UnityEngine;

public class HintToast : MonoBehaviour
{
    [Header("Wired by Builder")]
    public CanvasGroup canvasGroup;
    public TMP_Text hintText;

    private Coroutine _routine;

    public void HideImmediate()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    public void ShowForSeconds(string text, float seconds)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(text, seconds));
    }

    private IEnumerator ShowRoutine(string text, float seconds)
    {
        if (hintText != null) hintText.text = text;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(seconds);
            canvasGroup.alpha = 0f;
        }
    }
}
