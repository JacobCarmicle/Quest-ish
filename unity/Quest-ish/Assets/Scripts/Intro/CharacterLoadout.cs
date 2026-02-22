using System;

public static class CharacterLoadout
{
    public static bool TryApplyClass(WorldStateModel.PlayerModel player, string rawClass, out string className)
    {
        className = null;
        if (player == null) return false;

        var c = (rawClass ?? string.Empty).Trim().ToLowerInvariant();
        if (c == "thief" || c == "rogue")
        {
            className = "Thief";
            player.classTag = "thief";
            player.maxHealth = player.health = 18;
            player.strength = 10; player.dexterity = 14; player.intelligence = 12;
            player.startingItems = new[] { "Lockpick Set", "Dagger", "Smoke Bomb" };
            return true;
        }

        if (c == "fighter" || c == "warrior")
        {
            className = "Fighter";
            player.classTag = "fighter";
            player.maxHealth = player.health = 26;
            player.strength = 14; player.dexterity = 10; player.intelligence = 8;
            player.startingItems = new[] { "Longsword", "Buckler", "Bandage Kit" };
            return true;
        }

        if (c == "mage" || c == "wizard")
        {
            className = "Mage";
            player.classTag = "mage";
            player.maxHealth = player.health = 16;
            player.strength = 8; player.dexterity = 10; player.intelligence = 15;
            player.startingItems = new[] { "Spellbook", "Focus Crystal", "Mana Tonic" };
            return true;
        }

        return false;
    }

    public static string StarterSummary(WorldStateModel.PlayerModel p)
    {
        if (p == null) return string.Empty;
        return $"Class: {p.classTag}\nHP: {p.health}/{p.maxHealth}\nSTR {p.strength}  DEX {p.dexterity}  INT {p.intelligence}\nItems: {string.Join(", ", p.startingItems ?? Array.Empty<string>())}";
    }
}
