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
            dmDialogueText.overflowMode = TextOverflowModes.Overflow;
        if (dialogueScrollRect == null)
            dialogueScrollRect = FindFirstObjectByType<ScrollRect>();

        if (dialogueScrollRect == null && dmDialogueText != null)
            EnsureRuntimeScrollView();
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

        var contentGO = new GameObject("Content", typeof(RectTransform));
        var content = contentGO.GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0, 0);
        content.anchorMax = new Vector2(1, 1);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        textRT.SetParent(content, false);
        textRT.anchorMin = new Vector2(0, 1);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.pivot = new Vector2(0.5f, 1f);
        textRT.offsetMin = new Vector2(8, -1200);
        textRT.offsetMax = new Vector2(-8, 0);

        dialogueScrollRect = viewportGO.GetComponent<ScrollRect>();
        dialogueScrollRect.viewport = viewport;
        dialogueScrollRect.content = content;
        dialogueScrollRect.horizontal = false;
        dialogueScrollRect.vertical = true;
        dialogueScrollRect.movementType = ScrollRect.MovementType.Clamped;
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
            dialogueScrollRect.verticalNormalizedPosition = 0f;
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
