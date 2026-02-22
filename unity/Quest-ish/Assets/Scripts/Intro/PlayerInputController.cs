using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputController : MonoBehaviour
{
    [Header("Wired by Builder")]
    public TMP_InputField inputField;
    public Button sendButton;

    public event Action<string> OnSubmit;

    private void Awake()
    {
        AutoWireIfMissing();

        if (sendButton != null)
            sendButton.onClick.AddListener(Submit);

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(_ => Submit());
            inputField.onEndEdit.AddListener(_ => Submit());
        }
    }

    private void AutoWireIfMissing()
    {
        if (inputField == null)
            inputField = FindFirstObjectByType<TMP_InputField>();

        if (sendButton == null)
            sendButton = FindFirstObjectByType<Button>();

        if (inputField != null && inputField.textViewport == null)
        {
            var viewport = inputField.transform.Find("TextArea") as RectTransform;
            if (viewport != null)
                inputField.textViewport = viewport;
        }
    }

    public void SetInteractable(bool enabled)
    {
        if (inputField != null) inputField.interactable = enabled;
        if (sendButton != null) sendButton.interactable = enabled;
    }

    private void Submit()
    {
        if (inputField == null) return;

        var text = (inputField.text ?? "").Trim();
        if (string.IsNullOrEmpty(text)) return;

        inputField.text = "";
        inputField.ActivateInputField();

        OnSubmit?.Invoke(text);
    }
}
