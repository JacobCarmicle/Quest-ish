using UnityEngine;

public class SceneProgressReporter : MonoBehaviour
{
    [Tooltip("Tag to persist when this scene is active")]
    public string sceneTag = GameSceneTags.Intro;

    private void OnEnable()
    {
        ReportScene(sceneTag);
    }

    public static void ReportScene(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        var state = MainMenuFlow.LoadProgress();
        GameBootstrap.MarkScene(state, tag);
        MainMenuFlow.SaveProgress(state);
    }
}
