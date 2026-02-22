using System;
using NUnit.Framework;

public class RollResolverEditModeTests
{
    private static Type T(string name)
    {
        var t = Type.GetType($"{name}, Assembly-CSharp");
        Assert.IsNotNull(t, $"Missing type {name}");
        return t;
    }

    [Test]
    public void InferDC_RespectsDifficultyBands()
    {
        var t = T("RollResolver");
        var infer = t.GetMethod("InferDC");

        var impossible = (int)infer.Invoke(null, new object[] { "an impossible legendary ritual", 0 });
        var easy = (int)infer.Invoke(null, new object[] { "easy simple task", 0 });
        var hard = (int)infer.Invoke(null, new object[] { "hard complex lock", 0 });

        Assert.GreaterOrEqual(impossible, 19);
        Assert.AreEqual(10, easy);
        Assert.GreaterOrEqual(hard, 16);
    }

    [Test]
    public void InferDC_RisesWithThreatClock()
    {
        var t = T("RollResolver");
        var infer = t.GetMethod("InferDC");

        var low = (int)infer.Invoke(null, new object[] { "inspect rune", 0 });
        var high = (int)infer.Invoke(null, new object[] { "inspect rune", 6 });
        Assert.Greater(high, low);
    }

    [Test]
    public void Resolve_ReturnsOutcomeLabel()
    {
        var t = T("RollResolver");
        var resolve = t.GetMethod("Resolve", new[] { typeof(string), typeof(int), typeof(int), typeof(bool), typeof(bool) });
        var result = resolve.Invoke(null, new object[] { "inspect rune", 1, 0, false, false });
        var outcome = (string)result.GetType().GetField("outcome").GetValue(result);

        Assert.IsFalse(string.IsNullOrWhiteSpace(outcome));
        var valid = outcome == "critical_fail" || outcome == "fail" || outcome == "mixed" || outcome == "success" || outcome == "critical_success";
        Assert.IsTrue(valid);
    }
}
