using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroEditModeTests
{
    [Test]
    public void TutorialController_Advance_StopsAtComplete()
    {
        var go = new GameObject("TutorialController");
        var tutorial = go.AddComponent<TutorialController>();

        tutorial.SetPhase(TutorialController.TutorialPhase.Arrival);
        tutorial.Advance();
        tutorial.Advance();
        tutorial.Advance();
        tutorial.Advance();
        tutorial.Advance();
        tutorial.Advance();

        Assert.AreEqual(TutorialController.TutorialPhase.Complete, tutorial.Phase);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void GameState_AddRecentEvent_KeepLast10()
    {
        var go = new GameObject("GameState");
        var gs = go.AddComponent<GameState>();

        for (int i = 1; i <= 15; i++)
            gs.AddRecentEvent($"E{i}");

        Assert.AreEqual(10, gs.world.recentEvents.Length);
        Assert.AreEqual("E6", gs.world.recentEvents[0]);
        Assert.AreEqual("E15", gs.world.recentEvents[9]);

        Object.DestroyImmediate(go);
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

        var controllerGo = new GameObject("PlayerInputController");
        var controller = controllerGo.AddComponent<PlayerInputController>();
        controller.inputField = input;
        controller.sendButton = button;

        string submitted = null;
        controller.OnSubmit += s => submitted = s;

        input.text = "hello dm";
        button.onClick.Invoke();

        Assert.AreEqual("hello dm", submitted);
        Assert.AreEqual(string.Empty, input.text);

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(controllerGo);
    }
}
