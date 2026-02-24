using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [Header("Wired by Builder (or assign manually)")]
    public Animator animator;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public string fadeOutTriggerName = "FadeOut";

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndSwitch(sceneName));
    }

    private IEnumerator FadeAndSwitch(string sceneName)
    {
        if (animator != null)
            animator.SetTrigger(fadeOutTriggerName);

        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(sceneName);
    }
}
