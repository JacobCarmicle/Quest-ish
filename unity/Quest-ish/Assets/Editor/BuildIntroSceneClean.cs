#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildIntroSceneClean
{
    private const string SceneName = "IntroScene";
    private const string ScenesFolder = "Assets/Scenes";
    private const string FadeFolder = "Assets/_SceneFadeClean";
    private const string FadeTrigger = "FadeOut";

    [MenuItem("Tools/Quest-ish/Build Intro Scene (CLEAN WORKING)")]
    public static void Build()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "_SceneFadeClean");

        // Create a brand-new empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SceneManager.SetActiveScene(scene);

        // Root
        var root = NewGO("IntroScene_Root");

        // Core groups
        var world = NewGO("World", root.transform);
        var ui = NewGO("UI", root.transform);
        var systems = NewGO("Systems", root.transform);
        var audio = NewGO("Audio", root.transform);

        // Camera (orthographic, centered)
        var camGO = NewGO("Main Camera", root.transform);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.orthographicSize = 10f; // you can tweak later

        // World Background SpriteRenderer (no UI background image masking!)
        var bgGO = NewGO("Background", world.transform);
        var sr = bgGO.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;
        bgGO.transform.position = Vector3.zero;

        // If you want, assign a sprite later in Inspector: World/Background (Sprite Renderer)

        // UI Root Canvas
        var canvasGO = NewGO("Canvas", ui.transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Dialogue Panel (bottom)
        var dialoguePanel = NewGO("DialoguePanel", canvasGO.transform);
        var panelImg = dialoguePanel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.55f);

        var panelRT = dialoguePanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.05f, 0.05f);
        panelRT.anchorMax = new Vector2(0.95f, 0.40f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // DM Name
        var dmName = NewTMP("DMNameText", dialoguePanel.transform, "DM", 40);
        dmName.alignment = TextAlignmentOptions.TopLeft;
        dmName.rectTransform.anchorMin = new Vector2(0.04f, 0.78f);
        dmName.rectTransform.anchorMax = new Vector2(0.96f, 0.96f);
        dmName.rectTransform.offsetMin = Vector2.zero;
        dmName.rectTransform.offsetMax = Vector2.zero;

        // DM Dialogue
        var dmDialogue = NewTMP("DMDialogueText", dialoguePanel.transform,
            "You wake up on a forest path. That’s concerning… but not unusual.\n\nBefore you do anything else — what do people call you?",
            32);
        dmDialogue.enableWordWrapping = true;
        dmDialogue.alignment = TextAlignmentOptions.TopLeft;

        dmDialogue.rectTransform.anchorMin = new Vector2(0.04f, 0.10f);
        dmDialogue.rectTransform.anchorMax = new Vector2(0.96f, 0.78f);
        dmDialogue.rectTransform.offsetMin = Vector2.zero;
        dmDialogue.rectTransform.offsetMax = Vector2.zero;

        // Input Row
        var inputRow = NewGO("InputRow", canvasGO.transform);
        var inputRT = inputRow.AddComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0.05f, 0.42f);
        inputRT.anchorMax = new Vector2(0.95f, 0.50f);
        inputRT.offsetMin = Vector2.zero;
        inputRT.offsetMax = Vector2.zero;

        var hLayout = inputRow.AddComponent<HorizontalLayoutGroup>();
        hLayout.childControlHeight = true;
        hLayout.childControlWidth = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.spacing = 16;
        hLayout.padding = new RectOffset(16, 16, 10, 10);

        // TMP InputField
        var inputFieldGO = NewGO("PlayerInputField", inputRow.transform);
        var inputBg = inputFieldGO.AddComponent<Image>();
        inputBg.color = new Color(1f, 1f, 1f, 0.95f);

        var inputLE = inputFieldGO.AddComponent<LayoutElement>();
        inputLE.flexibleWidth = 1f;
        inputLE.preferredHeight = 90;

        var tmpInput = inputFieldGO.AddComponent<TMP_InputField>();

        // Text Area
        var textArea = NewGO("TextArea", inputFieldGO.transform);
        var textAreaRT = textArea.AddComponent<RectTransform>();
        StretchFull(textAreaRT);
        textArea.AddComponent<RectMask2D>();

        var text = NewTMP("Text", textArea.transform, "", 32);
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        StretchFull(text.rectTransform);
        tmpInput.textComponent = text;

        var placeholder = NewTMP("Placeholder", textArea.transform, "Type what you do…", 32);
        placeholder.color = new Color(0f, 0f, 0f, 0.35f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        StretchFull(placeholder.rectTransform);
        tmpInput.placeholder = placeholder;

        // Send Button
        var sendBtnGO = NewGO("SendButton", inputRow.transform);
        var sendImg = sendBtnGO.AddComponent<Image>();
        sendImg.color = new Color(0.2f, 0.8f, 0.3f, 0.95f);

        var sendBtn = sendBtnGO.AddComponent<Button>();

        var sendLE = sendBtnGO.AddComponent<LayoutElement>();
        sendLE.preferredWidth = 220;
        sendLE.preferredHeight = 90;

        var sendText = NewTMP("Text", sendBtnGO.transform, "Send", 34);
        sendText.alignment = TextAlignmentOptions.Center;
        StretchFull(sendText.rectTransform);

        // Hint Toast (non-blocking)
        var hintGO = NewGO("HintToast", canvasGO.transform);
        var hintImg = hintGO.AddComponent<Image>();
        hintImg.color = new Color(0f, 0f, 0f, 0.70f);
        hintImg.raycastTarget = false;

        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.10f, 0.52f);
        hintRT.anchorMax = new Vector2(0.90f, 0.60f);
        hintRT.offsetMin = Vector2.zero;
        hintRT.offsetMax = Vector2.zero;

        var hintGroup = hintGO.AddComponent<CanvasGroup>();
        hintGroup.alpha = 0f;

        var hintText = NewTMP("HintText", hintGO.transform, "", 28);
        hintText.alignment = TextAlignmentOptions.Center;
        StretchFull(hintText.rectTransform);

        // Fade Canvas (TOP overlay)
        var (fadeController, _, _) = EnsureFadeAssets();

        var fadeCanvasGO = NewGO("FadeCanvas", ui.transform);
        var fadeCanvas = fadeCanvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.overrideSorting = true;
        fadeCanvas.sortingOrder = 999;
        fadeCanvasGO.AddComponent<GraphicRaycaster>();

        var fadeScaler = fadeCanvasGO.AddComponent<CanvasScaler>();
        fadeScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        fadeScaler.referenceResolution = new Vector2(1080, 1920);
        fadeScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        fadeScaler.matchWidthOrHeight = 0.5f;

        var fadeImageGO = NewGO("FadeImage", fadeCanvasGO.transform);
        var fadeImage = fadeImageGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        fadeImage.raycastTarget = false;
        StretchFull(fadeImageGO.GetComponent<RectTransform>());

        var fadeAnimator = fadeImageGO.AddComponent<Animator>();
        fadeAnimator.runtimeAnimatorController = fadeController;

        // Systems (requires your runtime scripts to exist)
        // If you already have these scripts from earlier, this will wire them.
        // If you don't, you'll get compile errors until you add them.
        var tutorial = NewGO("TutorialController", systems.transform).AddComponent<TutorialController>();
        var gs = NewGO("GameState", systems.transform).AddComponent<GameState>();

        var dialogueCtrl = NewGO("DialogueController", systems.transform).AddComponent<DialogueController>();
        dialogueCtrl.dmNameText = dmName;
        dialogueCtrl.dmDialogueText = dmDialogue;

        var inputCtrl = NewGO("PlayerInputController", systems.transform).AddComponent<PlayerInputController>();
        inputCtrl.inputField = tmpInput;
        inputCtrl.sendButton = sendBtn;

        var hintCtrl = NewGO("HintToastController", systems.transform).AddComponent<HintToast>();
        hintCtrl.canvasGroup = hintGroup;
        hintCtrl.hintText = hintText;

        var director = NewGO("IntroFlowDirector", systems.transform).AddComponent<IntroFlowDirector>();
        director.tutorial = tutorial;
        director.gameState = gs;
        director.dialogue = dialogueCtrl;
        director.input = inputCtrl;
        director.hint = hintCtrl;

        var fader = NewGO("SceneFader", systems.transform).AddComponent<SceneFader>();
        fader.animator = fadeAnimator;
        fader.fadeDuration = 0.5f;
        fader.fadeOutTriggerName = FadeTrigger;

        // Audio
        var music = NewGO("MusicSource", audio.transform).AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = true;
        music.volume = 0.25f;

        var sfx = NewGO("SFXSource", audio.transform).AddComponent<AudioSource>();
        sfx.loop = false;
        sfx.playOnAwake = false;
        sfx.volume = 0.8f;

        var amb = NewGO("AmbienceSource", audio.transform).AddComponent<AudioSource>();
        amb.loop = true;
        amb.playOnAwake = false;
        amb.volume = 0.35f;

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = NewGO("EventSystem", root.transform);
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Save scene
        var path = $"{ScenesFolder}/{SceneName}.unity";
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Built IntroScene (Clean)",
            $"✅ Built and saved {SceneName} at:\n{path}\n\nAssign your sprite on World/Background and adjust camera size if needed.",
            "OK");
    }

    // ---------------- Fade Assets ----------------
    private static (AnimatorController controller, AnimationClip fadeIn, AnimationClip fadeOut) EnsureFadeAssets()
    {
        string controllerPath = $"{FadeFolder}/FadeController.controller";
        string fadeInPath = $"{FadeFolder}/FadeIn.anim";
        string fadeOutPath = $"{FadeFolder}/FadeOut.anim";

        var fadeIn = AssetDatabase.LoadAssetAtPath<AnimationClip>(fadeInPath);
        var fadeOut = AssetDatabase.LoadAssetAtPath<AnimationClip>(fadeOutPath);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (fadeIn == null) fadeIn = CreateAlphaClip(fadeInPath, "FadeIn", 1f, 0f, 0.5f);
        if (fadeOut == null) fadeOut = CreateAlphaClip(fadeOutPath, "FadeOut", 0f, 1f, 0.5f);
        if (controller == null) controller = CreateController(controllerPath, fadeIn, fadeOut);

        // Enforce correct transitions: FadeIn default; FadeIn -> FadeOut on Trigger
        var sm = controller.layers[0].stateMachine;
        var sIn = FindState(sm, "FadeIn");
        var sOut = FindState(sm, "FadeOut");
        sm.defaultState = sIn;

        EnsureTrigger(controller, FadeTrigger);
        RemoveTransitions(sOut, sIn);               // remove wrong FadeOut->FadeIn
        EnsureTransition(sIn, sOut, FadeTrigger);   // correct FadeIn->FadeOut
        EnsureTransition(sOut, sOut, FadeTrigger);  // optional re-trigger

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        return (controller, fadeIn, fadeOut);
    }

    private static AnimationClip CreateAlphaClip(string assetPath, string clipName, float a0, float a1, float duration)
    {
        var clip = new AnimationClip { name = clipName, frameRate = 60f };

        var binding = new EditorCurveBinding
        {
            type = typeof(Image),
            path = "",
            propertyName = "m_Color.a"
        };

        var curve = new AnimationCurve(
            new Keyframe(0f, a0),
            new Keyframe(duration, a1)
        );

        for (int i = 0; i < curve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);
        AssetDatabase.CreateAsset(clip, assetPath);
        return clip;
    }

    private static AnimatorController CreateController(string path, AnimationClip fadeIn, AnimationClip fadeOut)
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter(FadeTrigger, AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;
        var sIn = sm.AddState("FadeIn");
        sIn.motion = fadeIn;

        var sOut = sm.AddState("FadeOut");
        sOut.motion = fadeOut;

        sm.defaultState = sIn;

        var t = sIn.AddTransition(sOut);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0f;
        t.AddCondition(AnimatorConditionMode.If, 0f, FadeTrigger);

        return controller;
    }

    private static void EnsureTrigger(AnimatorController controller, string name)
    {
        foreach (var p in controller.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Trigger)
                return;
        controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
    }

    private static AnimatorState FindState(AnimatorStateMachine sm, string stateName)
    {
        foreach (var child in sm.states)
            if (child.state != null && child.state.name == stateName)
                return child.state;
        return null;
    }

    private static void RemoveTransitions(AnimatorState from, AnimatorState to)
    {
        var transitions = from.transitions;
        for (int i = transitions.Length - 1; i >= 0; i--)
        {
            if (transitions[i].destinationState == to)
                from.RemoveTransition(transitions[i]);
        }
    }

    private static void EnsureTransition(AnimatorState from, AnimatorState to, string triggerName)
    {
        foreach (var tr in from.transitions)
        {
            if (tr.destinationState != to) continue;
            foreach (var c in tr.conditions)
            {
                if (c.mode == AnimatorConditionMode.If && c.parameter == triggerName)
                {
                    tr.hasExitTime = false;
                    tr.hasFixedDuration = true;
                    tr.duration = 0f;
                    return;
                }
            }
        }

        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0f;
        t.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
    }

    // ---------------- Helpers ----------------
    private static GameObject NewGO(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TMP_Text NewTMP(string name, Transform parent, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;

        // Default stretch; callers can override anchors
        StretchFull(tmp.rectTransform);

        return tmp;
    }

    private static void EnsureFolder(string parent, string folder)
    {
        var path = $"{parent}/{folder}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
