using UnityEngine;

public class GameState : MonoBehaviour
{
    public WorldStateModel world = new WorldStateModel();

    public void SetPlayerName(string name)
    {
        world.player.name = name;
    }

    public void AddRecentEvent(string evt)
    {
        // Keep last 10 events max
        var old = world.recentEvents ?? System.Array.Empty<string>();
        var list = new System.Collections.Generic.List<string>(old);
        list.Add(evt);
        while (list.Count > 10) list.RemoveAt(0);
        world.recentEvents = list.ToArray();
    }
}
