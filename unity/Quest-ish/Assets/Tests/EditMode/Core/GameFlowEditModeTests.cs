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
        var tags = T("GameSceneTags");
        var introTag = (string)tags.GetField("Intro").GetValue(null);
        var classTag = (string)tags.GetField("CharacterSelection").GetValue(null);

        Assert.AreEqual("IntroScene", (string)t.GetMethod("GetContinueScene").Invoke(null, new object[] { introTag }));
        Assert.AreEqual("CharacterSelection", (string)t.GetMethod("GetContinueScene").Invoke(null, new object[] { classTag }));
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
        var tags = T("GameSceneTags");
        var introTag = (string)tags.GetField("Intro").GetValue(null);
        Assert.AreEqual(introTag, (string)stateType.GetField("lastSceneTag").GetValue(state));
    }


    [Test]
    public void MainMenuFlow_PrepareNewGameState_ResetsState()
    {
        var stateType = T("GameProgressState");
        var state = Activator.CreateInstance(stateType);
        stateType.GetField("hasStartedGame").SetValue(state, true);
        stateType.GetField("playerName").SetValue(state, "Jacob");

        var flow = T("MainMenuFlow");
        var reset = flow.GetMethod("PrepareNewGameState").Invoke(null, new[] { state });

        Assert.IsFalse((bool)stateType.GetField("hasStartedGame").GetValue(reset));
        Assert.AreEqual(string.Empty, (string)stateType.GetField("playerName").GetValue(reset));
    }

    [Test]
    public void MainMenuFlow_ResolveContinueScene_UsesProgressTag()
    {
        var stateType = T("GameProgressState");
        var state = Activator.CreateInstance(stateType);

        var tags = T("GameSceneTags");
        var classTag = (string)tags.GetField("CharacterSelection").GetValue(null);
        stateType.GetField("lastSceneTag").SetValue(state, classTag);

        var flow = T("MainMenuFlow");
        var scene = (string)flow.GetMethod("ResolveContinueScene").Invoke(null, new[] { state });
        Assert.AreEqual("CharacterSelection", scene);
    }

}