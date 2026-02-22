using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonBinder : MonoBehaviour
{
    public MainMenuFlow flow;

    private void Awake()
    {
        if (flow == null) flow = FindFirstObjectByType<MainMenuFlow>();
        if (flow == null) return;

        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            var action = MapButtonAction(b.gameObject.name);
            if (action == MenuButtonAction.None) continue;

            b.onClick.RemoveAllListeners();
            switch (action)
            {
                case MenuButtonAction.NewGame: b.onClick.AddListener(flow.NewGame); break;
                case MenuButtonAction.Continue: b.onClick.AddListener(flow.ContinueGame); break;
                case MenuButtonAction.Quit: b.onClick.AddListener(flow.QuitGame); break;
            }
        }
    }

    public static MenuButtonAction MapButtonAction(string name)
    {
        var n = (name ?? string.Empty).Trim().ToLowerInvariant();
        if (n.Contains("new")) return MenuButtonAction.NewGame;
        if (n.Contains("continue")) return MenuButtonAction.Continue;
        if (n.Contains("quit") || n.Contains("exit")) return MenuButtonAction.Quit;
        return MenuButtonAction.None;
    }
}

public enum MenuButtonAction
{
    None,
    NewGame,
    Continue,
    Quit
}
