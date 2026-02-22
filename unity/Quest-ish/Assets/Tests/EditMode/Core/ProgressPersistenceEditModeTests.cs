using System;
using NUnit.Framework;
using UnityEngine;

public class ProgressPersistenceEditModeTests
{
    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteKey("questish.progress");
    }

    [Test]
    public void MainMenuFlow_SaveAndLoadProgress_RoundTrips()
    {
        var stateType = Type.GetType("GameProgressState, Assembly-CSharp");
        var flowType = Type.GetType("MainMenuFlow, Assembly-CSharp");
        Assert.IsNotNull(stateType);
        Assert.IsNotNull(flowType);

        var state = Activator.CreateInstance(stateType);
        stateType.GetField("hasStartedGame").SetValue(state, true);
        stateType.GetField("playerName").SetValue(state, "Jacob");
        stateType.GetField("playerClass").SetValue(state, "mage");

        flowType.GetMethod("SaveProgress").Invoke(null, new[] { state });
        var loaded = flowType.GetMethod("LoadProgress").Invoke(null, null);

        Assert.AreEqual("Jacob", stateType.GetField("playerName").GetValue(loaded));
        Assert.AreEqual("mage", stateType.GetField("playerClass").GetValue(loaded));
        Assert.IsTrue((bool)stateType.GetField("hasStartedGame").GetValue(loaded));
    }

    [Test]
    public void SceneProgressReporter_ReportScene_UpdatesLastSceneTag()
    {
        var stateType = Type.GetType("GameProgressState, Assembly-CSharp");
        var flowType = Type.GetType("MainMenuFlow, Assembly-CSharp");
        var reporterType = Type.GetType("SceneProgressReporter, Assembly-CSharp");
        Assert.IsNotNull(stateType);
        Assert.IsNotNull(flowType);
        Assert.IsNotNull(reporterType);

        var state = Activator.CreateInstance(stateType);
        flowType.GetMethod("SaveProgress").Invoke(null, new[] { state });

        reporterType.GetMethod("ReportScene").Invoke(null, new object[] { "hub_scene" });
        var loaded = flowType.GetMethod("LoadProgress").Invoke(null, null);

        Assert.AreEqual("hub_scene", stateType.GetField("lastSceneTag").GetValue(loaded));
    }
}
