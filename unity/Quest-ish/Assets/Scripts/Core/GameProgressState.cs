using System;

[Serializable]
public class GameProgressState
{
    public bool hasStartedGame;
    public string lastSceneTag = "intro_scene";
    public string playerName = string.Empty;
    public string playerClass = string.Empty;
}
