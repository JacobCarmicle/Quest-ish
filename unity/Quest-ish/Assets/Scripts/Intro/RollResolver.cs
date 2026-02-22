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
}

public static class RollResolver
{
    public static SkillRollResult Resolve(string playerText, int level = 1)
    {
        var skill = InferSkill(playerText);
        var dc = InferDC(playerText);
        var modifier = SkillModifier(skill, level);
        var d20 = UnityEngine.Random.Range(1, 21);
        var total = d20 + modifier;

        return new SkillRollResult
        {
            skill = skill,
            dc = dc,
            d20 = d20,
            modifier = modifier,
            total = total,
            success = total >= dc
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

    public static int InferDC(string text)
    {
        var t = (text ?? string.Empty).ToLowerInvariant();
        if (t.Contains("impossible") || t.Contains("legendary")) return 20;
        if (t.Contains("hard") || t.Contains("complex") || t.Contains("careful")) return 16;
        if (t.Contains("easy") || t.Contains("simple")) return 10;
        return 14;
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
