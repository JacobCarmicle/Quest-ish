using System;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroEditModeTests
{
    private static Type GetGameType(string typeName)
    {
        var t = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.IsNotNull(t, $"Type not found: {typeName} in Assembly-CSharp");
        return t;
    }

    [Test]
    public void TutorialController_Advance_StopsAtComplete()
    {
        var tType = GetGameType("TutorialController");
        var go = new GameObject("TutorialController");
        var tutorial = go.AddComponent(tType);

        var phaseEnum = tType.GetNestedType("TutorialPhase");
        var setPhase = tType.GetMethod("SetPhase");
        var advance = tType.GetMethod("Advance");
        var phaseProp = tType.GetProperty("Phase");

        var arrival = Enum.Parse(phaseEnum, "Arrival");
        var complete = Enum.Parse(phaseEnum, "Complete");

        setPhase.Invoke(tutorial, new[] { arrival });
        for (int i = 0; i < 6; i++) advance.Invoke(tutorial, null);

        var current = phaseProp.GetValue(tutorial);
        Assert.AreEqual(complete, current);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void GameState_AddRecentEvent_KeepLast10()
    {
        var gsType = GetGameType("GameState");
        var go = new GameObject("GameState");
        var gs = go.AddComponent(gsType);

        var addRecentEvent = gsType.GetMethod("AddRecentEvent");
        for (int i = 1; i <= 15; i++)
            addRecentEvent.Invoke(gs, new object[] { $"E{i}" });

        var worldField = gsType.GetField("world");
        var world = worldField.GetValue(gs);
        var worldType = world.GetType();
        var recentEventsField = worldType.GetField("recentEvents");
        var events = (string[])recentEventsField.GetValue(world);

        Assert.AreEqual(10, events.Length);
        Assert.AreEqual("E6", events[0]);
        Assert.AreEqual("E15", events[9]);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void PlayerInputController_SendButton_SubmitsAndClearsInput()
    {
        var root = new GameObject("UI");

        var inputGo = new GameObject("InputField", typeof(RectTransform));
        inputGo.transform.SetParent(root.transform);
        var input = inputGo.AddComponent<TMP_InputField>();

        var textArea = new GameObject("Text Area", typeof(RectTransform));
        textArea.transform.SetParent(inputGo.transform);

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(textArea.transform);
        var text = textGo.AddComponent<TextMeshProUGUI>();

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(textArea.transform);
        var placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();

        input.textViewport = textArea.GetComponent<RectTransform>();
        input.textComponent = text;
        input.placeholder = placeholder;

        var btnGo = new GameObject("SendButton");
        btnGo.transform.SetParent(root.transform);
        btnGo.AddComponent<Image>();
        var button = btnGo.AddComponent<Button>();

        var cType = GetGameType("PlayerInputController");
        var controllerGo = new GameObject("PlayerInputController");
        var controller = controllerGo.AddComponent(cType);

        cType.GetField("inputField").SetValue(controller, input);
        cType.GetField("sendButton").SetValue(controller, button);

        string submitted = null;
        var evt = cType.GetEvent("OnSubmit");
        Action<string> handler = s => submitted = s;
        evt.AddEventHandler(controller, handler);

        var awake = cType.GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        awake?.Invoke(controller, null);

        input.text = "hello dm";
        button.onClick.Invoke();

        Assert.AreEqual("hello dm", submitted);
        Assert.AreEqual(string.Empty, input.text);

        evt.RemoveEventHandler(controller, handler);
        UnityEngine.Object.DestroyImmediate(root);
        UnityEngine.Object.DestroyImmediate(controllerGo);
    }
}
