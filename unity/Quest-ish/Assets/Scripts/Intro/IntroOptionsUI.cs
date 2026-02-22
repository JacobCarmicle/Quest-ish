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

    private Button CreateButton()
    {
        if (container == null) return null;

        if (buttonTemplate != null)
        {
            var b = Instantiate(buttonTemplate, container);
            b.gameObject.SetActive(true);
            return b;
        }

        var go = new GameObject("OptionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(container, false);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);

        var lbl = labelGo.GetComponent<TextMeshProUGUI>();
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.fontSize = 24;

        var rt = labelGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go.GetComponent<Button>();
    }
}
