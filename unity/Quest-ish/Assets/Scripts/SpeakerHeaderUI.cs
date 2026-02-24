using System;
using UnityEngine;
using UnityEngine.UI;

public class SpeakerHeaderUI : MonoBehaviour
{
    [Header("Wired by builder (or assign manually)")]
    public Image portraitImage;
    public Text nameLabel;

    [Serializable]
    public class SpeakerPortrait
    {
        public string speakerId;   // e.g. "DM", "Goblin", "Shopkeeper"
        public Sprite portrait;
        public string displayName; // optional override (leave empty to use speakerId)
    }

    [Header("Assign portraits here")]
    public SpeakerPortrait[] portraits;

    [Header("Fallback")]
    public Sprite defaultPortrait;
    public string defaultName = "DM";

    public void SetSpeaker(string speakerId)
    {
        if (string.IsNullOrWhiteSpace(speakerId))
            speakerId = defaultName;

        Sprite spriteToUse = defaultPortrait;
        string nameToUse = speakerId;

        if (portraits != null)
        {
            for (int i = 0; i < portraits.Length; i++)
            {
                if (portraits[i] != null && string.Equals(portraits[i].speakerId, speakerId, StringComparison.OrdinalIgnoreCase))
                {
                    if (portraits[i].portrait != null) spriteToUse = portraits[i].portrait;
                    if (!string.IsNullOrWhiteSpace(portraits[i].displayName)) nameToUse = portraits[i].displayName;
                    break;
                }
            }
        }

        if (portraitImage != null) portraitImage.sprite = spriteToUse;
        if (nameLabel != null) nameLabel.text = nameToUse;
    }
}
