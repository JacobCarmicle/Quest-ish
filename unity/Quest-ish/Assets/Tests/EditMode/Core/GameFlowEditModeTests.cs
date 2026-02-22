using System;
using NUnit.Framework;

public class GameFlowEditModeTests
{
    private static Type T(string name)
    {
        var t = Type.GetType($"{name}, Assembly-CSharp");
        Assert.IsNotNull(t, $"Missing type {name}");
        return t;
    }

    [Test]
    public void SceneRoutePlanner_NewGame_GoesToIntro()
    {
        var t = T("SceneRoutePlanner");
        var scene = (string)t.GetMethod("GetNewGameScene").Invoke(null, null);
        Assert.AreEqual("IntroScene", scene);
    }

    [Test]
    public void SceneRoutePlanner_Continue_UsesKnownTag()
    {
        var t = T("SceneRoutePlanner");
        Assert.AreEqual("IntroScene", (string)t.GetMethod("GetContinueScene").Invoke(null, new object[] { "intro_scene" }));
        Assert.AreEqual("CharacterSelection", (string)t.GetMethod("GetContinueScene").Invoke(null, new object[] { "character_selection" }));
    }

    [Test]
    public void SceneRoutePlanner_Continue_FallbacksToIntro_WhenUnknown()
    {
        var t = T("SceneRoutePlanner");
        Assert.AreEqual("IntroScene", (string)t.GetMethod("GetContinueScene").Invoke(null, new object[] { "unknown_tag" }));
        Assert.AreEqual("IntroScene", (string)t.GetMethod("GetContinueScene").Invoke(null, new object[] { null }));
    }

    [Test]
    public void GameBootstrap_ResetForNewGame_ClearsProgress()
    {
        var stateType = T("GameProgressState");
        var state = Activator.CreateInstance(stateType);
        stateType.GetField("hasStartedGame").SetValue(state, true);
        stateType.GetField("lastSceneTag").SetValue(state, "intro_scene");
        stateType.GetField("playerName").SetValue(state, "Jacob");
        stateType.GetField("playerClass").SetValue(state, "thief");

        var bootstrap = T("GameBootstrap");
        bootstrap.GetMethod("ResetForNewGame").Invoke(null, new[] { state });

        Assert.IsFalse((bool)stateType.GetField("hasStartedGame").GetValue(state));
        Assert.AreEqual(string.Empty, (string)stateType.GetField("playerName").GetValue(state));
        Assert.AreEqual(string.Empty, (string)stateType.GetField("playerClass").GetValue(state));
        Assert.AreEqual("intro_scene", (string)stateType.GetField("lastSceneTag").GetValue(state));
    }
}
