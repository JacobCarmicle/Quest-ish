using System;
using System.Collections.Generic;

[Serializable]
public class IntroTurnRequest
{
    public string sessionId;
    public string playerInput;
    public string phase;
    public string playerName;
    public string storySummary;
    public string[] recentEvents;
    public string currentBeat;
    public string goalHint;
    public int threatClock;
    public bool encounterActive;
}

[Serializable]
public class IntroAIResponse
{
    public string intentType; // DIALOGUE|SKILL_CHECK|COMBAT|CLARIFY|INVALID
    public string narration;
    public string clarifyQuestion;
    public IntroDirective[] directives;
    public IntroOption[] options;
}

[Serializable]
public class IntroDirective
{
    public string type;
    public string dataJson;
}

[Serializable]
public class IntroOption
{
    public string id;
    public string label;
}

public enum IntroIntentType
{
    DIALOGUE,
    SKILL_CHECK,
    COMBAT,
    CLARIFY,
    INVALID
}
