using UnityEngine;
using UnityEngine.UI;

public class SimpleSettings : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider musicSlider;   // 0..1
    public Slider sfxSlider;     // 0..1

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    const string MUSIC_KEY = "MusicVol";
    const string SFX_KEY = "SfxVol";

    void Start()
    {
        // Load saved values (defaults)
        float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 0.3f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 0.6f);

        // Set slider UI
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        // Apply immediately
        ApplyMusic(musicVol);
        ApplySfx(sfxVol);

        // Listen for changes
        musicSlider.onValueChanged.AddListener(v =>
        {
            ApplyMusic(v);
            PlayerPrefs.SetFloat(MUSIC_KEY, v);
            PlayerPrefs.Save();
        });

        sfxSlider.onValueChanged.AddListener(v =>
        {
            ApplySfx(v);
            PlayerPrefs.SetFloat(SFX_KEY, v);
            PlayerPrefs.Save();
        });
    }

    void ApplyMusic(float v)
    {
        if (musicSource != null) musicSource.volume = v;
    }

    void ApplySfx(float v)
    {
        if (sfxSource != null) sfxSource.volume = v;
    }
}
