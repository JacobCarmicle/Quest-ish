using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroOptionsUI : MonoBehaviour
{
    public RectTransform container;
    public Button buttonTemplate;
    public bool hideWhenEmpty = true;

    private readonly List<Button> _spawned = new List<Button>();

    public void ShowOptions(IntroOption[] options, Action<string> onPicked)
    {
        EnsureContainer();
        Clear();

        if (options == null || options.Length == 0)
        {
            if (container != null && hideWhenEmpty) container.gameObject.SetActive(false);
            return;
        }

        if (container != null) container.gameObject.SetActive(true);

        foreach (var opt in options)
        {
            var b = CreateButton();
            if (b == null) continue;

            var label = b.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = string.IsNullOrWhiteSpace(opt?.label) ? "Option" : opt.label;

            var id = string.IsNullOrWhiteSpace(opt?.id) ? label?.text ?? "option" : opt.id;
            b.onClick.AddListener(() => onPicked?.Invoke(id));
            _spawned.Add(b);
        }
    }

    public void Clear()
    {
        foreach (var b in _spawned)
            if (b != null) Destroy(b.gameObject);
        _spawned.Clear();
    }

    private void EnsureContainer()
    {
        if (container != null) return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("OptionsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(canvas.transform, false);

        container = go.GetComponent<RectTransform>();
        container.anchorMin = new Vector2(0.05f, 0.18f);
        container.anchorMax = new Vector2(0.95f, 0.35f);
        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;

        var v = go.GetComponent<VerticalLayoutGroup>();
        v.childControlHeight = true;
        v.childControlWidth = true;
        v.childForceExpandHeight = false;
        v.childForceExpandWidth = true;
        v.spacing = 8f;

        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private Button CreateButton()
    {
        if (container == null) return null;

        if (buttonTemplate != null)
        {
            var b = Instantiate(buttonTemplate, container);
            b.gameObject.SetActive(true);
            return b;
        }

        var go = new GameObject("OptionButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(container, false);

        var image = go.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);

        var le = go.GetComponent<LayoutElement>();
        le.preferredHeight = 56f;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);

        var lbl = labelGo.GetComponent<TextMeshProUGUI>();
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.fontSize = 24;
        lbl.color = Color.white;

        var rt = labelGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go.GetComponent<Button>();
    }
}
