#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

public static class BuildSpeakerPortraitUI
{
    private const string ROOT_NAME = "IntroScene_Root";
    private const string UI_NAME = "UI";
    private const string CANVAS_NAME = "Canvas";
    private const string DIALOGUE_PANEL_NAME = "DialoguePanel";

    private const string SPEAKER_HEADER_NAME = "SpeakerHeader";
    private const string SPEAKER_PORTRAIT_NAME = "SpeakerPortrait";
    private const string SPEAKER_NAME_TEXT = "SpeakerName";

    [MenuItem("Tools/Tutorial/Build Speaker Portrait UI")]
    public static void Build()
    {
        var root = FindOrCreate(ROOT_NAME);
        var ui = FindOrCreate(UI_NAME, root.transform);

        // Ensure we have a Canvas root under UI
        var canvasGO = FindOrCreate(CANVAS_NAME, ui.transform);
        var canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (canvasGO.GetComponent<CanvasScaler>() == null)
        {
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (canvasGO.GetComponent<GraphicRaycaster>() == null)
            canvasGO.AddComponent<GraphicRaycaster>();

        // Ensure DialoguePanel exists under Canvas
        var dialoguePanel = FindOrCreate(DIALOGUE_PANEL_NAME, canvasGO.transform);

        // Ensure DialoguePanel has a RectTransform configured (non-destructive)
        var dpRt = dialoguePanel.GetComponent<RectTransform>();
        if (dpRt == null) dpRt = dialoguePanel.AddComponent<RectTransform>();

        // If DialoguePanel has no Image, add a transparent one so layout behaves nicely (optional)
        if (dialoguePanel.GetComponent<Image>() == null)
        {
            var img = dialoguePanel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // transparent
            img.raycastTarget = false;
        }

        // Add SpeakerHeader under DialoguePanel (above dialogue)
        var speakerHeader = dialoguePanel.transform.Find(SPEAKER_HEADER_NAME)?.gameObject;
        if (speakerHeader == null)
        {
            speakerHeader = new GameObject(SPEAKER_HEADER_NAME, typeof(RectTransform));
            speakerHeader.transform.SetParent(dialoguePanel.transform, false);
        }

        // Put SpeakerHeader as the FIRST child so it appears above dialogue in most setups
        speakerHeader.transform.SetSiblingIndex(0);

        var shRt = speakerHeader.GetComponent<RectTransform>();
        ConfigureSpeakerHeaderRect(shRt);

        // Background (optional): add Image with low alpha so text is readable
        var shBg = speakerHeader.GetComponent<Image>();
        if (shBg == null) shBg = speakerHeader.AddComponent<Image>();
        shBg.color = new Color(0f, 0f, 0f, 0.20f);
        shBg.raycastTarget = false;

        // Layout: portrait left, name right
        var hlg = speakerHeader.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = speakerHeader.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(24, 24, 12, 12);
        hlg.spacing = 16;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        var fitter = speakerHeader.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = speakerHeader.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // SpeakerPortrait Image
        var portraitGO = speakerHeader.transform.Find(SPEAKER_PORTRAIT_NAME)?.gameObject;
        if (portraitGO == null)
        {
            portraitGO = new GameObject(SPEAKER_PORTRAIT_NAME, typeof(RectTransform), typeof(Image));
            portraitGO.transform.SetParent(speakerHeader.transform, false);
        }

        var portraitImg = portraitGO.GetComponent<Image>();
        portraitImg.color = Color.white;
        portraitImg.preserveAspect = true;
        portraitImg.raycastTarget = false;

        var portraitLE = portraitGO.GetComponent<LayoutElement>();
        if (portraitLE == null) portraitLE = portraitGO.AddComponent<LayoutElement>();
        portraitLE.preferredWidth = 128;
        portraitLE.preferredHeight = 128;
        portraitLE.minWidth = 96;
        portraitLE.minHeight = 96;
        portraitLE.flexibleWidth = 0;
        portraitLE.flexibleHeight = 0;

        // SpeakerName text
        var nameGO = speakerHeader.transform.Find(SPEAKER_NAME_TEXT)?.gameObject;
        if (nameGO == null)
        {
#if TMP_PRESENT
            nameGO = new GameObject(SPEAKER_NAME_TEXT, typeof(RectTransform), typeof(TextMeshProUGUI));
#else
            nameGO = new GameObject(SPEAKER_NAME_TEXT, typeof(RectTransform), typeof(Text));
#endif
            nameGO.transform.SetParent(speakerHeader.transform, false);
        }

#if TMP_PRESENT
        var tmp = nameGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "DM";
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
#else
        var txt = nameGO.GetComponent<Text>();
        txt.text = "DM";
        txt.fontSize = 32;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.raycastTarget = false;
#endif

        var nameLE = nameGO.GetComponent<LayoutElement>();
        if (nameLE == null) nameLE = nameGO.AddComponent<LayoutElement>();
        nameLE.preferredHeight = 128;
        nameLE.flexibleWidth = 1;
        nameLE.flexibleHeight = 0;

        // Add a tiny helper component that your runtime can use to set portrait/name (optional)
        var helper = speakerHeader.GetComponent<SpeakerHeaderUI>();
        if (helper == null) helper = speakerHeader.AddComponent<SpeakerHeaderUI>();
        helper.Portrait = portraitImg;
#if TMP_PRESENT
        helper.NameTMP = nameGO.GetComponent<TextMeshProUGUI>();
#else
        helper.NameLegacy = nameGO.GetComponent<Text>();
#endif

        EditorUtility.SetDirty(dialoguePanel);
        EditorUtility.SetDirty(speakerHeader);
        EditorUtility.SetDirty(canvasGO);

        Debug.Log("✅ SpeakerHeader UI built under UI/Canvas/DialoguePanel.\n" +
                  "Next: assign portrait sprites at runtime using SpeakerHeaderUI.SetSpeaker(name, sprite).");
    }

    private static void ConfigureSpeakerHeaderRect(RectTransform rt)
    {
        // This makes it stretch horizontally and sit at the TOP of DialoguePanel
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);

        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, 160f);
        rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
        rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
    }

    private static GameObject FindOrCreate(string name, Transform parent = null)
    {
        GameObject existing = null;

        if (parent == null)
        {
            existing = GameObject.Find(name);
        }
        else
        {
            var t = parent.Find(name);
            if (t != null) existing = t.gameObject;
        }

        if (existing != null) return existing;

        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }
}

// Runtime helper component (safe to keep in Editor file; Unity will compile it for runtime too)
public class SpeakerHeaderUI : MonoBehaviour
{
    public Image Portrait;

#if TMP_PRESENT
    public TMPro.TextMeshProUGUI NameTMP;
#else
    public UnityEngine.UI.Text NameLegacy;
#endif

    /// <summary>
    /// Call this from your dialogue system when the speaker changes.
    /// </summary>
    public void SetSpeaker(string speakerName, Sprite portraitSprite)
    {
        if (Portrait != null)
        {
            Portrait.sprite = portraitSprite;
            Portrait.enabled = portraitSprite != null;
        }

#if TMP_PRESENT
        if (NameTMP != null) NameTMP.text = speakerName ?? "";
#else
        if (NameLegacy != null) NameLegacy.text = speakerName ?? "";
#endif
    }
}
#endif
