using System;

[Serializable]
public class GameProgressState
{
    public bool hasStartedGame;
    public string lastSceneTag = GameSceneTags.Intro;
    public string playerName = string.Empty;
    public string playerClass = string.Empty;
    public int saveVersion = 1;
}
