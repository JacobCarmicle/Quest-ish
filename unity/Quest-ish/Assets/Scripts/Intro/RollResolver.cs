using System;
using UnityEngine;

[Serializable]
public class SkillRollResult
{
    public string skill;
    public int dc;
    public int d20;
    public int modifier;
    public int total;
    public bool success;
    public string outcome; // critical_fail | fail | mixed | success | critical_success
}

public static class RollResolver
{
    public static SkillRollResult Resolve(string playerText, int level = 1)
    {
        return Resolve(playerText, level, 0, false, false);
    }

    public static SkillRollResult Resolve(string playerText, int level, int threatClock, bool advantage, bool disadvantage)
    {
        var skill = InferSkill(playerText);
        var dc = InferDC(playerText, threatClock);
        var modifier = SkillModifier(skill, level);

        var d20a = UnityEngine.Random.Range(1, 21);
        var d20b = UnityEngine.Random.Range(1, 21);
        var d20 = (advantage && !disadvantage) ? Mathf.Max(d20a, d20b)
                 : (disadvantage && !advantage) ? Mathf.Min(d20a, d20b)
                 : d20a;

        var total = d20 + modifier;
        var success = total >= dc;

        var outcome = "fail";
        if (d20 == 1) outcome = "critical_fail";
        else if (d20 == 20) outcome = "critical_success";
        else if (success && total >= dc + 5) outcome = "success";
        else if (success) outcome = "mixed";

        return new SkillRollResult
        {
            skill = skill,
            dc = dc,
            d20 = d20,
            modifier = modifier,
            total = total,
            success = success,
            outcome = outcome
        };
    }

    public static string InferSkill(string text)
    {
        var t = (text ?? string.Empty).ToLowerInvariant();
        if (t.Contains("sneak") || t.Contains("stealth") || t.Contains("quiet")) return "Stealth";
        if (t.Contains("persuade") || t.Contains("convince") || t.Contains("charm")) return "Persuasion";
        if (t.Contains("inspect") || t.Contains("search") || t.Contains("look")) return "Investigation";
        if (t.Contains("arcane") || t.Contains("rune") || t.Contains("magic")) return "Arcana";
        if (t.Contains("climb") || t.Contains("jump") || t.Contains("force")) return "Athletics";
        if (t.Contains("pick") || t.Contains("lock")) return "ThievesTools";
        return "Insight";
    }

    public static int InferDC(string text, int threatClock = 0)
    {
        var t = (text ?? string.Empty).ToLowerInvariant();
        var baseDc = 13;
        if (t.Contains("impossible") || t.Contains("legendary")) baseDc = 19;
        else if (t.Contains("hard") || t.Contains("complex") || t.Contains("careful")) baseDc = 16;
        else if (t.Contains("easy") || t.Contains("simple")) baseDc = 10;

        if (t.Contains("rune") || t.Contains("ward") || t.Contains("arcane"))
            baseDc += 1;

        baseDc += Mathf.Clamp(threatClock / 2, 0, 2);
        return Mathf.Clamp(baseDc, 8, 22);
    }

    public static int SkillModifier(string skill, int level)
    {
        var prof = Mathf.Clamp(2 + (level - 1) / 4, 2, 6);
        return skill switch
        {
            "Arcana" => prof + 2,
            "Stealth" => prof + 1,
            "Persuasion" => prof + 1,
            "Investigation" => prof,
            "Athletics" => prof,
            _ => prof - 1,
        };
    }
}
