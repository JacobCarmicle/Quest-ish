#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public static class SceneFadeBuilder
{
    private const string DefaultFolder = "Assets/_SceneFade";
    private const string ControllerName = "FadeController.controller";
    private const string FadeInClipName = "FadeIn.anim";
    private const string FadeOutClipName = "FadeOut.anim";
    private const string TriggerName = "FadeOut";

    [MenuItem("Tools/Scene Fade/Build Fade Assets + Scene Objects")]
    public static void BuildAll()
    {
        // 1) Ensure folders
        EnsureFolder("Assets", "_SceneFade");

        // 2) Create/Load assets (clips + controller)
        string controllerPath = $"{DefaultFolder}/{ControllerName}";
        string fadeInPath = $"{DefaultFolder}/{FadeInClipName}";
        string fadeOutPath = $"{DefaultFolder}/{FadeOutClipName}";

        AnimationClip fadeIn = AssetDatabase.LoadAssetAtPath<AnimationClip>(fadeInPath);
        AnimationClip fadeOut = AssetDatabase.LoadAssetAtPath<AnimationClip>(fadeOutPath);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (fadeIn == null) fadeIn = CreateFadeClip(fadeInPath, "FadeIn", startAlpha: 1f, endAlpha: 0f, durationSeconds: 0.5f);
        if (fadeOut == null) fadeOut = CreateFadeClip(fadeOutPath, "FadeOut", startAlpha: 0f, endAlpha: 1f, durationSeconds: 0.5f);
        if (controller == null) controller = CreateController(controllerPath, fadeIn, fadeOut);

        // 3) Build scene objects (Canvas + Image + Animator + SceneFader)
        var canvasGO = GameObject.Find("FadeCanvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("FadeCanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create FadeCanvas");
        }

        var canvas = GetOrAdd<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GetOrAdd<CanvasScaler>(canvasGO);
        GetOrAdd<GraphicRaycaster>(canvasGO);

        var fadeImageGO = GameObject.Find("FadeImage");
        if (fadeImageGO == null)
        {
            fadeImageGO = new GameObject("FadeImage");
            Undo.RegisterCreatedObjectUndo(fadeImageGO, "Create FadeImage");
            fadeImageGO.transform.SetParent(canvasGO.transform, false);
        }
        else
        {
            // ensure parented to FadeCanvas
            fadeImageGO.transform.SetParent(canvasGO.transform, false);
        }

        var img = GetOrAdd<Image>(fadeImageGO);
        img.color = new Color(0f, 0f, 0f, 1f); // start black (alpha 1) so FadeIn works

        var rt = fadeImageGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var animator = GetOrAdd<Animator>(fadeImageGO);
        animator.runtimeAnimatorController = controller;

        // 4) Create/Setup SceneFader object
        var faderGO = GameObject.Find("SceneFader");
        if (faderGO == null)
        {
            faderGO = new GameObject("SceneFader");
            Undo.RegisterCreatedObjectUndo(faderGO, "Create SceneFader");
        }

        var sceneFader = GetOrAdd<SceneFader>(faderGO);
        sceneFader.animator = animator;
        sceneFader.fadeDuration = 0.5f;
        sceneFader.fadeOutTriggerName = TriggerName;

        // 5) Save + select
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(fadeIn);
        EditorUtility.SetDirty(fadeOut);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = faderGO;

        Debug.Log("✅ Scene Fade built: Canvas + Image + Animations + Controller + SceneFader");
    }

    private static AnimationClip CreateFadeClip(string assetPath, string clipName, float startAlpha, float endAlpha, float durationSeconds)
    {
        var clip = new AnimationClip
        {
            name = clipName,
            frameRate = 60f
        };

        // Animate: FadeImage -> Image.color.a
        // Property path: "m_Color.a" on UnityEngine.UI.Image
        var binding = new EditorCurveBinding
        {
            type = typeof(Image),
            path = "",               // same GameObject as Image
            propertyName = "m_Color.a"
        };

        var curve = new AnimationCurve(
            new Keyframe(0f, startAlpha),
            new Keyframe(durationSeconds, endAlpha)
        );

        // Linear easing (no bounce): make tangents linear
        for (int i = 0; i < curve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);

        AssetDatabase.CreateAsset(clip, assetPath);
        return clip;
    }

    private static AnimatorController CreateController(string controllerPath, AnimationClip fadeIn, AnimationClip fadeOut)
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Parameter
        controller.AddParameter(TriggerName, AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        // States
        var fadeInState = sm.AddState("FadeIn");
        fadeInState.motion = fadeIn;

        var fadeOutState = sm.AddState("FadeOut");
        fadeOutState.motion = fadeOut;

        // Default
        sm.defaultState = fadeInState;

        // Transition: FadeIn -> FadeOut on Trigger, no exit time
        var t = fadeInState.AddTransition(fadeOutState);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0f;
        t.AddCondition(AnimatorConditionMode.If, 0f, TriggerName);

        // Optional: allow FadeOut to restart if triggered again (self transition)
        var self = fadeOutState.AddTransition(fadeOutState);
        self.hasExitTime = false;
        self.duration = 0f;
        self.AddCondition(AnimatorConditionMode.If, 0f, TriggerName);

        return controller;
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string path = $"{parent}/{folderName}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = Undo.AddComponent<T>(go);
        return c;
    }
}
#endif
