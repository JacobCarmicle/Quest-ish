using System;
using System.Collections.Generic;
using UnityEngine;

public class IntroDirectiveExecutor : MonoBehaviour
{
    [Serializable]
    public class NamedSprite { public string id; public Sprite sprite; }
    [Serializable]
    public class NamedAudio { public string id; public AudioClip clip; }
    [Serializable]
    public class NamedActor { public string id; public GameObject actor; }

    public SpriteRenderer backgroundRenderer;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public List<NamedSprite> backgrounds = new List<NamedSprite>();
    public List<NamedAudio> musicClips = new List<NamedAudio>();
    public List<NamedAudio> sfxClips = new List<NamedAudio>();
    public List<NamedActor> actors = new List<NamedActor>();

    [Serializable] private class BackgroundPayload { public string id; }
    [Serializable] private class AudioPayload { public string id; }
    [Serializable] private class ActorPayload { public string id; }

    public bool Execute(IntroDirective directive)
    {
        if (directive == null || string.IsNullOrWhiteSpace(directive.type)) return false;

        switch (directive.type)
        {
            case "set_background": return SetBackground(directive.dataJson);
            case "play_music": return PlayMusic(directive.dataJson);
            case "play_sfx": return PlaySfx(directive.dataJson);
            case "show_actor": return SetActorActive(directive.dataJson, true);
            case "hide_actor": return SetActorActive(directive.dataJson, false);
            default: return false;
        }
    }

    private bool SetBackground(string json)
    {
        if (backgroundRenderer == null) return false;
        var id = ParseId<BackgroundPayload>(json);
        var sprite = FindById(backgrounds, id, x => x.id, x => x.sprite);
        if (sprite == null) return false;
        backgroundRenderer.sprite = sprite;
        return true;
    }

    private bool PlayMusic(string json)
    {
        if (musicSource == null) return false;
        var id = ParseId<AudioPayload>(json);
        var clip = FindById(musicClips, id, x => x.id, x => x.clip);
        if (clip == null) return false;
        musicSource.clip = clip;
        if (!musicSource.isPlaying) musicSource.Play();
        return true;
    }

    private bool PlaySfx(string json)
    {
        if (sfxSource == null) return false;
        var id = ParseId<AudioPayload>(json);
        var clip = FindById(sfxClips, id, x => x.id, x => x.clip);
        if (clip == null) return false;
        sfxSource.PlayOneShot(clip);
        return true;
    }

    private bool SetActorActive(string json, bool active)
    {
        var id = ParseId<ActorPayload>(json);
        var actor = FindById(actors, id, x => x.id, x => x.actor);
        if (actor == null) return false;
        actor.SetActive(active);
        return true;
    }

    private static string ParseId<T>(string json) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var obj = JsonUtility.FromJson<T>(json);
            var f = typeof(T).GetField("id");
            return (string)f?.GetValue(obj);
        }
        catch { return null; }
    }

    private static TValue FindById<TItem, TValue>(IEnumerable<TItem> list, string id, Func<TItem, string> idSelector, Func<TItem, TValue> valueSelector) where TValue : class
    {
        if (string.IsNullOrWhiteSpace(id) || list == null) return null;
        foreach (var item in list)
            if (string.Equals(idSelector(item), id, StringComparison.OrdinalIgnoreCase))
                return valueSelector(item);
        return null;
    }
}
