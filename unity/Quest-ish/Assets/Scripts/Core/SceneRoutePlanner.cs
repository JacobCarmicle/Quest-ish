public static class SceneRoutePlanner
{
    public static string GetNewGameScene() => "IntroScene";

    public static string GetContinueScene(string lastSceneTag)
    {
        return lastSceneTag switch
        {
            "intro_scene" => "IntroScene",
            "character_selection" => "CharacterSelection",
            _ => "IntroScene"
        };
    }
}
