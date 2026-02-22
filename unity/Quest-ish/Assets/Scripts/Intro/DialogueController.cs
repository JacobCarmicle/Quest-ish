using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [Header("Wired by Builder")]
    public TMP_Text dmNameText;
    public TMP_Text dmDialogueText;
    public ScrollRect dialogueScrollRect;

    private readonly StringBuilder _transcript = new StringBuilder();

    [Header("Optional Speaker Portrait")]
    public Image speakerPortraitImage;        // UI Image
    public Sprite defaultPortrait;            // Optional fallback sprite
    public bool hidePortraitWhenMissing = true;

    private void Awake()
    {
        if (dmDialogueText != null)
        {
            dmDialogueText.overflowMode = TextOverflowModes.Overflow;
            dmDialogueText.enableWordWrapping = true;
        }

        if (dialogueScrollRect == null)
            dialogueScrollRect = FindFirstObjectByType<ScrollRect>();

        if (dialogueScrollRect == null && dmDialogueText != null)
            EnsureRuntimeScrollView();

        EnsureScrollBindings();
    }

    private void EnsureRuntimeScrollView()
    {
        var textRT = dmDialogueText.rectTransform;
        var parent = textRT.parent as RectTransform;
        if (parent == null) return;

        var viewportGO = new GameObject("DialogueViewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        var viewport = viewportGO.GetComponent<RectTransform>();
        viewport.SetParent(parent, false);
        viewport.anchorMin = textRT.anchorMin;
        viewport.anchorMax = textRT.anchorMax;
        viewport.offsetMin = textRT.offsetMin;
        viewport.offsetMax = textRT.offsetMax;

        var img = viewportGO.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.18f);
        viewportGO.GetComponent<Mask>().showMaskGraphic = false;

        textRT.SetParent(viewport, false);

        dialogueScrollRect = viewportGO.GetComponent<ScrollRect>();
        dialogueScrollRect.viewport = viewport;
        dialogueScrollRect.horizontal = false;
        dialogueScrollRect.vertical = true;
        dialogueScrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    private void EnsureScrollBindings()
    {
        if (dmDialogueText == null || dialogueScrollRect == null) return;

        var textRT = dmDialogueText.rectTransform;

        if (dialogueScrollRect.viewport == null)
            dialogueScrollRect.viewport = dialogueScrollRect.GetComponent<RectTransform>();

        if (textRT.parent != dialogueScrollRect.viewport)
            textRT.SetParent(dialogueScrollRect.viewport, false);

        textRT.anchorMin = new Vector2(0f, 1f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.pivot = new Vector2(0.5f, 1f);
        textRT.anchoredPosition = new Vector2(0f, 0f);
        textRT.sizeDelta = new Vector2(-16f, textRT.sizeDelta.y);

        var fitter = dmDialogueText.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = dmDialogueText.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        dialogueScrollRect.content = textRT;
        dialogueScrollRect.scrollSensitivity = 24f;
    }

    public void SetDMName(string name)
    {
        if (dmNameText != null) dmNameText.text = name;
    }

    public void Say(string text)
    {
        AppendLine($"{(dmNameText != null ? dmNameText.text : "DM")}: {text}");
    }

    public void AddPlayerLine(string playerText)
    {
        AppendLine($"You: {playerText}");
    }

    public void ClearTranscript()
    {
        _transcript.Clear();
        if (dmDialogueText != null) dmDialogueText.text = string.Empty;
    }

    private void AppendLine(string line)
    {
        if (dmDialogueText == null) return;
        if (_transcript.Length > 0) _transcript.Append("\n\n");
        _transcript.Append(line);
        dmDialogueText.text = _transcript.ToString();

        if (dialogueScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            dialogueScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // NEW: Set both speaker name + portrait
    public void SetSpeaker(string speakerName, Sprite portrait)
    {
        SetDMName(speakerName);

        if (speakerPortraitImage == null)
            return;

        if (portrait != null)
        {
            speakerPortraitImage.sprite = portrait;
            speakerPortraitImage.enabled = true;
        }
        else if (defaultPortrait != null)
        {
            speakerPortraitImage.sprite = defaultPortrait;
            speakerPortraitImage.enabled = true;
        }
        else
        {
            if (hidePortraitWhenMissing)
                speakerPortraitImage.enabled = false;
        }
    }
}
