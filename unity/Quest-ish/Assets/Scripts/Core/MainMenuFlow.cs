using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFlow : MonoBehaviour
{
    private const string ProgressKey = "questish.progress";

    public void NewGame()
    {
        var progress = PrepareNewGameState(LoadProgress());
        SaveProgress(progress);
        SceneManager.LoadScene(SceneRoutePlanner.GetNewGameScene());
    }

    public void ContinueGame()
    {
        var progress = LoadProgress();
        var scene = ResolveContinueScene(progress);
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public static GameProgressState PrepareNewGameState(GameProgressState state)
    {
        state ??= new GameProgressState();
        GameBootstrap.ResetForNewGame(state);
        return state;
    }

    public static string ResolveContinueScene(GameProgressState state)
    {
        state ??= new GameProgressState();
        return SceneRoutePlanner.GetContinueScene(state.lastSceneTag);
    }

    public static GameProgressState LoadProgress()
    {
        if (!PlayerPrefs.HasKey(ProgressKey))
            return new GameProgressState();

        var json = PlayerPrefs.GetString(ProgressKey, "");
        if (string.IsNullOrWhiteSpace(json))
            return new GameProgressState();

        return JsonUtility.FromJson<GameProgressState>(json) ?? new GameProgressState();
    }

    public static void SaveProgress(GameProgressState state)
    {
        var json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(ProgressKey, json);
        PlayerPrefs.Save();
    }
}
