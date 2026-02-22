using System;
using NUnit.Framework;

public class EncounterDirectorEditModeTests
{
    private static Type T(string name)
    {
        var t = Type.GetType($"{name}, Assembly-CSharp");
        Assert.IsNotNull(t, $"Missing type {name}");
        return t;
    }

    [Test]
    public void EncounterDirector_CombatIntent_TriggersEncounter()
    {
        var t = T("EncounterDirector");
        var go = new UnityEngine.GameObject("EncounterDirector");
        var ed = go.AddComponent(t);

        var intentEnum = T("IntroIntentType");
        var combat = Enum.Parse(intentEnum, "COMBAT");
        t.GetMethod("ApplyIntent").Invoke(ed, new[] { combat });

        var active = (bool)t.GetField("encounterActive").GetValue(ed);
        Assert.IsTrue(active);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void EncounterDirector_InvalidIntent_IncrementsThreat()
    {
        var t = T("EncounterDirector");
        var go = new UnityEngine.GameObject("EncounterDirector");
        var ed = go.AddComponent(t);

        var intentEnum = T("IntroIntentType");
        var invalid = Enum.Parse(intentEnum, "INVALID");
        t.GetMethod("ApplyIntent").Invoke(ed, new[] { invalid });

        var threat = (int)t.GetField("threatClock").GetValue(ed);
        Assert.AreEqual(1, threat);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void EncounterDirector_ResolveEncounter_ResetsToSafeState()
    {
        var t = T("EncounterDirector");
        var go = new UnityEngine.GameObject("EncounterDirector");
        var ed = go.AddComponent(t);

        t.GetMethod("TriggerEncounter").Invoke(ed, null);
        t.GetMethod("ResolveEncounter").Invoke(ed, null);

        var active = (bool)t.GetField("encounterActive").GetValue(ed);
        var threat = (int)t.GetField("threatClock").GetValue(ed);

        Assert.IsFalse(active);
        Assert.AreEqual(1, threat);

        UnityEngine.Object.DestroyImmediate(go);
    }
}
