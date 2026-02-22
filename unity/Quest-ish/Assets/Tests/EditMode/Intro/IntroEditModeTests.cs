using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
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

        var awake = cType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
        awake?.Invoke(controller, null);

        input.text = "hello dm";
        button.onClick.Invoke();

        Assert.AreEqual("hello dm", submitted);
        Assert.AreEqual(string.Empty, input.text);

        evt.RemoveEventHandler(controller, handler);
        UnityEngine.Object.DestroyImmediate(root);
        UnityEngine.Object.DestroyImmediate(controllerGo);
    }

    [Test]
    public void IntroFlowDirector_NameQuestion_SubmitSetsPlayerNameAndAdvancesPhase()
    {
        var fixture = BuildIntroFixture();

        var phaseEnum = fixture.tutorialType.GetNestedType("TutorialPhase");
        var nameQuestion = Enum.Parse(phaseEnum, "NameQuestion");
        fixture.tutorialType.GetMethod("SetPhase").Invoke(fixture.tutorial, new[] { nameQuestion });

        fixture.handleSubmit.Invoke(fixture.director, new object[] { "Jacob" });

        var world = fixture.gameStateType.GetField("world").GetValue(fixture.gameState);
        var player = world.GetType().GetField("player").GetValue(world);
        var playerName = (string)player.GetType().GetField("name").GetValue(player);
        Assert.AreEqual("Jacob", playerName);

        var phase = fixture.tutorialType.GetProperty("Phase").GetValue(fixture.tutorial).ToString();
        Assert.AreEqual("FirstAction", phase);

        var dialogueText = fixture.dialogueText.text;
        StringAssert.Contains("Strong name", dialogueText);

        CleanupFixture(fixture);
    }

    [Test]
    public void IntroFlowDirector_Outcome_SubmitCompletesTutorial()
    {
        var fixture = BuildIntroFixture();

        var phaseEnum = fixture.tutorialType.GetNestedType("TutorialPhase");
        var outcome = Enum.Parse(phaseEnum, "Outcome");
        fixture.tutorialType.GetMethod("SetPhase").Invoke(fixture.tutorial, new[] { outcome });

        fixture.handleSubmit.Invoke(fixture.director, new object[] { "attack the shade" });

        var phase = fixture.tutorialType.GetProperty("Phase").GetValue(fixture.tutorial).ToString();
        Assert.AreEqual("Complete", phase);
        Assert.IsTrue(fixture.dialogueText.text.Contains("ready") || fixture.dialogueText.text.Contains("Encounter hook") || fixture.dialogueText.text.Contains("world listens") || fixture.dialogueText.text.Contains("Roll resolves"));

        CleanupFixture(fixture);
    }

    [Test]
    public void IntroScene_PortraitLayout_IsTopRightOverflowAndBodyTextLower()
    {
        var scenePath = Path.GetFullPath(Path.Combine(Application.dataPath, "Scenes/IntroScene.unity"));
        var yaml = File.ReadAllText(scenePath);

        var speakerBlock = Regex.Match(yaml, @"--- !u!224 &1888209735[\s\S]*?(?=\n--- !u!|\z)").Value;
        Assert.IsNotEmpty(speakerBlock, "SpeakerHeader rect block not found");
        StringAssert.Contains("m_AnchorMin: {x: 0.98, y: 1}", speakerBlock);
        StringAssert.Contains("m_AnchorMax: {x: 0.98, y: 1}", speakerBlock);
        StringAssert.Contains("m_Pivot: {x: 1, y: 1}", speakerBlock);
        StringAssert.Contains("m_Father: {fileID: 1259061063}", speakerBlock); // DialoguePanel

        var bodyBlock = Regex.Match(yaml, @"--- !u!224 &1184021287[\s\S]*?(?=\n--- !u!|\z)").Value;
        Assert.IsNotEmpty(bodyBlock, "DMDialogueText rect block not found");
        StringAssert.IsMatch(@"m_AnchorMax: \{x: 0\.96, y: 0\.5(0)?\}", bodyBlock);
    }


    [Test]
    public void IntroFlowDirector_FirstAction_SubmitAddsEventAndAdvancesToOutcome()
    {
        var fixture = BuildIntroFixture();

        var phaseEnum = fixture.tutorialType.GetNestedType("TutorialPhase");
        var firstAction = Enum.Parse(phaseEnum, "FirstAction");
        fixture.tutorialType.GetMethod("SetPhase").Invoke(fixture.tutorial, new[] { firstAction });

        fixture.handleSubmit.Invoke(fixture.director, new object[] { "jump over the puddle" });

        var phase = fixture.tutorialType.GetProperty("Phase").GetValue(fixture.tutorial).ToString();
        Assert.AreEqual("Outcome", phase);

        var world = fixture.gameStateType.GetField("world").GetValue(fixture.gameState);
        var events = (string[])world.GetType().GetField("recentEvents").GetValue(world);
        Assert.IsTrue(events.Length > 0);
        StringAssert.Contains(":", events[events.Length - 1]);

        Assert.IsTrue(fixture.dialogueText.text.Contains("almost works") || fixture.dialogueText.text.Contains("world listens") || fixture.dialogueText.text.Contains("Roll resolves") || fixture.dialogueText.text.Contains("Encounter hook"));

        CleanupFixture(fixture);
    }

    [Test]
    public void IntroFlowDirector_EmptySubmit_DoesNothing()
    {
        var fixture = BuildIntroFixture();

        var phaseEnum = fixture.tutorialType.GetNestedType("TutorialPhase");
        var nameQuestion = Enum.Parse(phaseEnum, "NameQuestion");
        fixture.tutorialType.GetMethod("SetPhase").Invoke(fixture.tutorial, new[] { nameQuestion });

        var before = fixture.tutorialType.GetProperty("Phase").GetValue(fixture.tutorial).ToString();
        fixture.handleSubmit.Invoke(fixture.director, new object[] { "   " });
        var after = fixture.tutorialType.GetProperty("Phase").GetValue(fixture.tutorial).ToString();

        Assert.AreEqual(before, after);

        CleanupFixture(fixture);
    }

    [Test]
    public void DialogueController_SetSpeaker_UsesDefaultPortraitWhenMissing()
    {
        var dialogueType = GetGameType("DialogueController");
        var go = new GameObject("Dialogue");
        var dialogue = go.AddComponent(dialogueType);

        var nameGo = new GameObject("Name");
        nameGo.transform.SetParent(go.transform);
        var name = nameGo.AddComponent<TextMeshProUGUI>();

        var textGo = new GameObject("Body");
        textGo.transform.SetParent(go.transform);
        var body = textGo.AddComponent<TextMeshProUGUI>();

        var portraitGo = new GameObject("Portrait");
        portraitGo.transform.SetParent(go.transform);
        var portrait = portraitGo.AddComponent<Image>();

        dialogueType.GetField("dmNameText").SetValue(dialogue, name);
        dialogueType.GetField("dmDialogueText").SetValue(dialogue, body);
        dialogueType.GetField("speakerPortraitImage").SetValue(dialogue, portrait);

        var tex = new Texture2D(2, 2);
        var fallback = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        dialogueType.GetField("defaultPortrait").SetValue(dialogue, fallback);

        dialogueType.GetMethod("SetSpeaker").Invoke(dialogue, new object[] { "DM", null });

        Assert.AreEqual("DM", name.text);
        Assert.AreEqual(fallback, portrait.sprite);
        Assert.IsTrue(portrait.enabled);

        UnityEngine.Object.DestroyImmediate(go);
    }


    [Test]
    public void IntroFlowDirector_ParseIntent_Matrix_Works()
    {
        var fixture = BuildIntroFixture();

        Assert.AreEqual("DIALOGUE", fixture.directorType.GetMethod("ParseIntent").Invoke(fixture.director, new object[] { "DIALOGUE", "hello" }).ToString());
        Assert.AreEqual("SKILL_CHECK", fixture.directorType.GetMethod("ParseIntent").Invoke(fixture.director, new object[] { "SKILL_CHECK", "sneak" }).ToString());
        Assert.AreEqual("COMBAT", fixture.directorType.GetMethod("ParseIntent").Invoke(fixture.director, new object[] { "COMBAT", "attack" }).ToString());
        Assert.AreEqual("CLARIFY", fixture.directorType.GetMethod("ParseIntent").Invoke(fixture.director, new object[] { "CLARIFY", "do it" }).ToString());
        Assert.AreEqual("INVALID", fixture.directorType.GetMethod("ParseIntent").Invoke(fixture.director, new object[] { "INVALID", "become god" }).ToString());

        CleanupFixture(fixture);
    }

    [Test]
    public void IntroFlowDirector_BuildTurnRequest_ContainsState()
    {
        var fixture = BuildIntroFixture();
        fixture.gameStateType.GetMethod("SetPlayerName").Invoke(fixture.gameState, new object[] { "Jacob" });
        fixture.gameStateType.GetMethod("AddRecentEvent").Invoke(fixture.gameState, new object[] { "event1" });

        var req = fixture.directorType.GetMethod("BuildTurnRequest").Invoke(fixture.director, new object[] { "look around" });
        var reqType = req.GetType();

        Assert.AreEqual("look around", reqType.GetField("playerInput").GetValue(req));
        Assert.AreEqual("Jacob", reqType.GetField("playerName").GetValue(req));

        var events = (string[])reqType.GetField("recentEvents").GetValue(req);
        Assert.IsTrue(events.Length >= 1);

        CleanupFixture(fixture);
    }

    [Test]
    public void IntroFlowDirector_UnknownDirective_IsSafelyIgnored()
    {
        var fixture = BuildIntroFixture();

        var dType = Type.GetType("IntroDirective, Assembly-CSharp");
        var arr = Array.CreateInstance(dType, 1);
        var d = Activator.CreateInstance(dType);
        dType.GetField("type").SetValue(d, "teleport_player_to_mars");
        arr.SetValue(d, 0);

        fixture.directorType.GetMethod("ApplyDirectives").Invoke(fixture.director, new object[] { arr });

        var world = fixture.gameStateType.GetField("world").GetValue(fixture.gameState);
        var events = (string[])world.GetType().GetField("recentEvents").GetValue(world);
        StringAssert.Contains("Ignored directive", events[events.Length - 1]);

        CleanupFixture(fixture);
    }

    [Test]
    public void WorldStatePersistence_SaveLoad_RoundTrip_Works()
    {
        var worldType = Type.GetType("WorldStateModel, Assembly-CSharp");
        var world = Activator.CreateInstance(worldType);

        var player = worldType.GetField("player").GetValue(world);
        player.GetType().GetField("name").SetValue(player, "Jacob");
        worldType.GetField("recentEvents").SetValue(world, new[] { "a", "b" });

        var persistenceType = Type.GetType("WorldStatePersistence, Assembly-CSharp");
        var temp = Path.GetTempFileName();

        persistenceType.GetMethod("Save").Invoke(null, new object[] { world, temp });
        var loaded = persistenceType.GetMethod("Load").Invoke(null, new object[] { temp });

        var loadedPlayer = loaded.GetType().GetField("player").GetValue(loaded);
        Assert.AreEqual("Jacob", loadedPlayer.GetType().GetField("name").GetValue(loadedPlayer));

        var loadedEvents = (string[])loaded.GetType().GetField("recentEvents").GetValue(loaded);
        Assert.AreEqual(2, loadedEvents.Length);

        File.Delete(temp);
    }


    [Test]
    public void IntroAIResponseValidator_InvalidIntent_ReturnsFalse()
    {
        var respType = Type.GetType("IntroAIResponse, Assembly-CSharp");
        var resp = Activator.CreateInstance(respType);
        respType.GetField("intentType").SetValue(resp, "SOMETHING_ELSE");

        var validatorType = Type.GetType("IntroAIResponseValidator, Assembly-CSharp");
        var args = new object[] { resp, null };
        var ok = (bool)validatorType.GetMethod("TryNormalize").Invoke(null, args);

        Assert.IsFalse(ok);
    }

    [Test]
    public void IntroAIResponseValidator_ValidIntent_NormalizesFields()
    {
        var respType = Type.GetType("IntroAIResponse, Assembly-CSharp");
        var resp = Activator.CreateInstance(respType);
        respType.GetField("intentType").SetValue(resp, "combat");

        var validatorType = Type.GetType("IntroAIResponseValidator, Assembly-CSharp");
        var args = new object[] { resp, null };
        var ok = (bool)validatorType.GetMethod("TryNormalize").Invoke(null, args);

        Assert.IsTrue(ok);
        Assert.AreEqual("COMBAT", respType.GetField("intentType").GetValue(resp));
        Assert.IsNotNull(respType.GetField("directives").GetValue(resp));
        Assert.IsNotNull(respType.GetField("options").GetValue(resp));
    }

    [Test]
    public void IntroFlowDirector_AllowedDirective_IsTracked()
    {
        var fixture = BuildIntroFixture();

        var dType = Type.GetType("IntroDirective, Assembly-CSharp");
        var arr = Array.CreateInstance(dType, 1);
        var d = Activator.CreateInstance(dType);
        dType.GetField("type").SetValue(d, "play_music");
        arr.SetValue(d, 0);

        fixture.directorType.GetMethod("ApplyDirectives").Invoke(fixture.director, new object[] { arr });

        var world = fixture.gameStateType.GetField("world").GetValue(fixture.gameState);
        var events = (string[])world.GetType().GetField("recentEvents").GetValue(world);
        StringAssert.Contains("Directive:play_music", events[events.Length - 1]);

        CleanupFixture(fixture);
    }

    private static (GameObject root, Component director, Component tutorial, Component gameState, MethodInfo handleSubmit, Type tutorialType, Type gameStateType, Type directorType, TextMeshProUGUI dialogueText) BuildIntroFixture()
    {
        var root = new GameObject("FixtureRoot");

        var tutorialType = GetGameType("TutorialController");
        var gameStateType = GetGameType("GameState");
        var dialogueType = GetGameType("DialogueController");
        var inputType = GetGameType("PlayerInputController");
        var hintType = GetGameType("HintToast");
        var directorType = GetGameType("IntroFlowDirector");

        var tutorialGo = new GameObject("Tutorial");
        tutorialGo.transform.SetParent(root.transform);
        var tutorial = tutorialGo.AddComponent(tutorialType);

        var gsGo = new GameObject("GameState");
        gsGo.transform.SetParent(root.transform);
        var gameState = gsGo.AddComponent(gameStateType);

        var dialogueGo = new GameObject("Dialogue");
        dialogueGo.transform.SetParent(root.transform);
        var dialogue = dialogueGo.AddComponent(dialogueType);

        var dmNameGo = new GameObject("DMName");
        dmNameGo.transform.SetParent(dialogueGo.transform);
        var dmNameText = dmNameGo.AddComponent<TextMeshProUGUI>();

        var dmDialogueGo = new GameObject("DMText");
        dmDialogueGo.transform.SetParent(dialogueGo.transform);
        var dmDialogueText = dmDialogueGo.AddComponent<TextMeshProUGUI>();

        var portraitGo = new GameObject("Portrait");
        portraitGo.transform.SetParent(dialogueGo.transform);
        var portraitImage = portraitGo.AddComponent<Image>();

        dialogueType.GetField("dmNameText").SetValue(dialogue, dmNameText);
        dialogueType.GetField("dmDialogueText").SetValue(dialogue, dmDialogueText);
        dialogueType.GetField("speakerPortraitImage").SetValue(dialogue, portraitImage);

        var inputGo = new GameObject("Input");
        inputGo.transform.SetParent(root.transform);
        var input = inputGo.AddComponent(inputType);

        var hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(root.transform);
        var hint = hintGo.AddComponent(hintType);

        var directorGo = new GameObject("Director");
        directorGo.transform.SetParent(root.transform);
        var director = directorGo.AddComponent(directorType);

        directorType.GetField("tutorial").SetValue(director, tutorial);
        directorType.GetField("gameState").SetValue(director, gameState);
        directorType.GetField("dialogue").SetValue(director, dialogue);
        directorType.GetField("input").SetValue(director, input);
        directorType.GetField("hint").SetValue(director, hint);

        var tex = new Texture2D(2, 2);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        directorType.GetField("dmPortrait").SetValue(director, sprite);

        var handleSubmit = directorType.GetMethod("HandleSubmit", BindingFlags.Instance | BindingFlags.NonPublic);

        return (root, director, tutorial, gameState, handleSubmit, tutorialType, gameStateType, directorType, dmDialogueText);
    }

    private static void CleanupFixture((GameObject root, Component director, Component tutorial, Component gameState, MethodInfo handleSubmit, Type tutorialType, Type gameStateType, Type directorType, TextMeshProUGUI dialogueText) fixture)
    {
        UnityEngine.Object.DestroyImmediate(fixture.root);
    }
}
