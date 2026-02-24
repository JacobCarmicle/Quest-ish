using UnityEngine;

public class AutoMusicPlayer : MonoBehaviour
{
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 0.25f;
    public bool loop = true;

    private AudioSource _src;

    private void Start()
    {
        // Make sure audio isn't globally paused/muted
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        _src = GetComponent<AudioSource>();
        if (_src == null) _src = gameObject.AddComponent<AudioSource>();

        _src.playOnAwake = false;
        _src.spatialBlend = 0f; // 2D
        _src.loop = loop;
        _src.volume = volume;

        if (musicClip == null)
        {
            Debug.LogError("AutoMusicPlayer: musicClip is NULL. Drag an AudioClip into the Music Clip field.");
            return;
        }

        _src.clip = musicClip;
        _src.Stop();
        _src.Play();

        Debug.Log($"AutoMusicPlayer: PLAY called. Clip={musicClip.name} length={musicClip.length}s volume={_src.volume}");
    }
}
