using System;
using System.IO;
using UnityEngine;

public static class WorldStatePersistence
{
    public static string DefaultPath => Path.Combine(Application.persistentDataPath, "worldstate.json");

    public static void Save(WorldStateModel world, string path = null)
    {
        if (world == null) return;
        path ??= DefaultPath;
        var json = JsonUtility.ToJson(world, true);
        File.WriteAllText(path, json);
    }

    public static WorldStateModel Load(string path = null)
    {
        path ??= DefaultPath;
        if (!File.Exists(path)) return new WorldStateModel();

        try
        {
            var json = File.ReadAllText(path);
            var state = JsonUtility.FromJson<WorldStateModel>(json);
            return state ?? new WorldStateModel();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load world state: {e.Message}");
            return new WorldStateModel();
        }
    }
}
