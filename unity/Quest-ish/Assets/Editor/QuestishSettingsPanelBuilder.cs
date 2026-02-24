using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestishSettingsPanelBuilder
{
    [MenuItem("Tools/Quest-ish/Create Settings Panel (UGUI)")]
    public static void CreateSettingsPanel()
    {
        // Find a Canvas (prefer selected's parent Canvas)
        Canvas canvas = null;

        if (Selection.activeGameObject != null)
        {
            canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
        }
        if (canvas == null)
        {
            canvas = Object.FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            // Create a Canvas if none exists
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Create EventSystem if missing
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        // Avoid duplicates
        if (canvas.transform.Find("SettingsPanel") != null)
        {
            EditorUtility.DisplayDialog("SettingsPanel already exists",
                "Canvas already contains a 'SettingsPanel'. Delete/rename it first if you want to regenerate.",
                "OK");
            return;
        }

        // Root SettingsPanel
        var settingsPanel = CreateUIObject("SettingsPanel", canvas.transform);
        var settingsImage = settingsPanel.AddComponent<Image>();
        settingsImage.color = new Color(0f, 0f, 0f, 0.55f); // dark overlay
        StretchToParent(settingsPanel.GetComponent<RectTransform>());

        // Background image holder (optional: assign your sprite later)
        var bg = CreateUIObject("SettingsBackground", settingsPanel.transform);
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.white;
        bgImage.raycastTarget = false; // IMPORTANT: don't block clicks
        StretchToParent(bg.GetComponent<RectTransform>());
        bg.transform.SetAsFirstSibling(); // behind everything

        // Extra dark overlay (optional)
        var overlay = CreateUIObject("DarkOverlay", settingsPanel.transform);
        var overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.35f);
        overlayImage.raycastTarget = false;
        StretchToParent(overlay.GetComponent<RectTransform>());
        overlay.transform.SetSiblingIndex(1); // above background, below card

        // Card
        var card = CreateUIObject("Card", settingsPanel.transform);
        var cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
        cardImage.raycastTarget = false;

        var cardRT = card.GetComponent<RectTransform>();
        AnchorMiddleCenter(cardRT);
        cardRT.sizeDelta = new Vector2(900, 700);

        // Layout on Card
        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 22;
        vlg.padding = new RectOffset(60, 60, 50, 50);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Title
        var titleGO = CreateUIObject("TitleText", card.transform);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "Settings";
        title.fontSize = 56;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;

        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 90;

        // Rows
        var musicRow = CreateRow(card.transform, "MusicRow", "Music");
        var musicSlider = CreateSlider(musicRow.transform, "MusicSlider");

        var sfxRow = CreateRow(card.transform, "SfxRow", "SFX");
        var sfxSlider = CreateSlider(sfxRow.transform, "SfxSlider");

        // Back button
        var backButtonGO = CreateButton(card.transform, "BackButton", "Back");
        var backLE = backButtonGO.AddComponent<LayoutElement>();
        backLE.preferredHeight = 80;
        backLE.preferredWidth = 360;

        // Start disabled (you can enable when needed)
        settingsPanel.SetActive(false);

        // Select it for convenience
        Selection.activeGameObject = settingsPanel;
        EditorGUIUtility.PingObject(settingsPanel);

        EditorUtility.DisplayDialog(
            "SettingsPanel created ✅",
            "Generated SettingsPanel under Canvas.\n\nNext:\n1) Assign your background sprite to SettingsBackground Image.\n2) Wire buttons (Settings opens panel / Back closes).\n3) Drag MusicSlider & SfxSlider into your SimpleSettings script.",
            "OK"
        );
    }

    // -------- Helpers --------

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchToParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static void AnchorMiddleCenter(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static GameObject CreateRow(Transform parent, string rowName, string labelText)
    {
        var row = CreateUIObject(rowName, parent);
        var rowRT = row.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 70);

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.spacing = 20;
        hlg.padding = new RectOffset(0, 0, 0, 0);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = false;

        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 70;

        // Label
        var labelGO = CreateUIObject("Label", row.transform);
        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 34;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;

        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 160;
        labelLE.preferredHeight = 60;

        return row;
    }

    static Slider CreateSlider(Transform parent, string name)
    {
        // Create a standard Unity slider, then fix layout
        var sliderGO = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderGO.name = name;
        sliderGO.transform.SetParent(parent, false);

        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;
        slider.wholeNumbers = false;

        // Prevent it from blocking layout weirdly
        var le = sliderGO.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        le.preferredWidth = 520;
        le.preferredHeight = 40;

        // Optional: remove any child text if Unity's template added one in your version
        var tmpTexts = sliderGO.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmpTexts)
        {
            Object.DestroyImmediate(t.gameObject);
        }

        return slider;
    }

    static GameObject CreateButton(Transform parent, string name, string text)
    {
        var btnGO = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnGO.name = name;
        btnGO.transform.SetParent(parent, false);

        // Button text (swap to TMP)
        var textGO = btnGO.GetComponentInChildren<Text>();
        if (textGO != null)
        {
            // Replace legacy Text with TMP
            var oldTextObj = textGO.gameObject;
            Object.DestroyImmediate(textGO);

            var tmp = oldTextObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var rt = tmp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Make button look a bit nicer by default
        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0.85f, 0.35f, 0.40f, 1f);

        return btnGO;
    }
}
