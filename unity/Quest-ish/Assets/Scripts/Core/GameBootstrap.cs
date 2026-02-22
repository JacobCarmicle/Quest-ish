public static class GameBootstrap
{
    public static void ResetForNewGame(GameProgressState state)
    {
        if (state == null) return;
        state.hasStartedGame = false;
        state.playerName = string.Empty;
        state.playerClass = string.Empty;
        state.lastSceneTag = GameSceneTags.Intro;
    }

    public static void MarkScene(GameProgressState state, string sceneTag)
    {
        if (state == null || string.IsNullOrWhiteSpace(sceneTag)) return;
        state.lastSceneTag = sceneTag;
    }

    public static void MarkStarted(GameProgressState state)
    {
        if (state == null) return;
        state.hasStartedGame = true;
    }
}
