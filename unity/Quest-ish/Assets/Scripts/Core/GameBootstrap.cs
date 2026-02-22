public static class GameBootstrap
{
    public static void ResetForNewGame(GameProgressState state)
    {
        if (state == null) return;
        state.hasStartedGame = false;
        state.playerName = string.Empty;
        state.playerClass = string.Empty;
        state.lastSceneTag = "intro_scene";
    }
}
