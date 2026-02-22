using System;
using UnityEngine;

[Serializable]
public class WorldStateModel
{
    public int schemaVersion = 1;
    public string sessionId = "sess_local";
    public string sceneTag = "intro_scene";
    public string storySummary = "";
    public string[] recentEvents = Array.Empty<string>();

    public PlayerModel player = new PlayerModel();

    [Serializable]
    public class PlayerModel
    {
        public string name = "";
        public string classTag = "unknown";
        public int level = 1;
        public int xp = 0;

        public int maxHealth = 20;
        public int health = 20;
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public string[] startingItems = Array.Empty<string>();
    }
}
