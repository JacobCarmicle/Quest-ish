public static class SceneRoutePlanner
{
    public static string GetNewGameScene() => "IntroScene";

    public static string GetContinueScene(string lastSceneTag)
    {
        return lastSceneTag switch
        {
            GameSceneTags.Intro => "IntroScene",
            GameSceneTags.CharacterSelection => "CharacterSelection",
            GameSceneTags.Hub => "CharacterSelection", // temporary hub target until dedicated hub scene exists
            GameSceneTags.Encounter => "IntroScene",    // safe fallback to active gameplay scene
            GameSceneTags.Inventory => "CharacterSelection",
            _ => "IntroScene"
        };
    }
}
