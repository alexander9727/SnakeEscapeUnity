using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public const string DiceRollClip = "DiceRoll";
    public const string BananaSlipClip = "BananaSlip";
    public const string ButtonClickClip = "ButtonClick";
    public const string CardSoundClip = "CardSound";
    public const string DamageClip = "Damage";
    public const string HeadChompClip = "HeadChomp";
    public const string OuchClip = "Ouch";
    public const string HoppingClip = "Hopping";
    public const string BoardBGMClip = "BoardBGM";
    public const string BattleBGMClip = "BattleBGM";

    private static AudioManager instance;
    static Dictionary<string, AudioSourceContainer> audioSources;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }



        instance = this;
        audioSources = new Dictionary<string, AudioSourceContainer>();
        for (int i = 0; i < transform.childCount; i++)
        {
            AudioSource source = transform.GetChild(i).GetComponent<AudioSource>();
            if (source != null)
            {
                audioSources.Add(source.name, new AudioSourceContainer(source));
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    public static void PlayClip(string clipName)
    {
        if (instance == null)
        {
            Debug.LogWarning($"No audio manager in scene");
            return;
        }
        if (audioSources.ContainsKey(clipName))
        {
            audioSources[clipName].Play();
        }
        else
        {
            Debug.LogWarning($"Audio clip {clipName} couldn't be found");
        }
    }

    public static void CrossFadeClip(string fromClip, string toClip, float duration)
    {
        if (instance == null)
        {
            Debug.LogWarning($"No audio manager in scene");
            return;
        }

        instance.StartCoroutine(CrossFade(fromClip, toClip, duration));
    }

    public static void FadeOutClip(string clip, float duration)
    {
        if (instance == null)
        {
            Debug.LogWarning($"No audio manager in scene");
            return;
        }

        instance.StartCoroutine(FadeOut(clip, duration));
    }

    public static void FadeInClip(string clip, float duration)
    {
        if (instance == null)
        {
            Debug.LogWarning($"No audio manager in scene");
            return;
        }

        instance.StartCoroutine(FadeIn(clip, duration));
    }

    static IEnumerator CrossFade(string fromClip, string toClip, float duration)
    {
        if (!audioSources.ContainsKey(fromClip)) yield break;
        if (!audioSources.ContainsKey(toClip)) yield break;
        float t = 0;

        var from = audioSources[fromClip];
        var to = audioSources[toClip];
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            from.Volume = Mathf.Lerp(from.DefaultVolume, 0, t);
            to.Volume = Mathf.Lerp(0, to.DefaultVolume, t);
            yield return null;
        }

        t = 1;
        from.Volume = Mathf.Lerp(from.DefaultVolume, 0, t);
        to.Volume = Mathf.Lerp(0, to.DefaultVolume, t);
    }
    static IEnumerator FadeIn(string clip, float duration)
    {
        if (!audioSources.ContainsKey(clip)) yield break;
        Debug.Log($"Fading in {clip}");
        var c = audioSources[clip];
        float t = 0;
        c.Play();
        while (t < 1)
        {
            Debug.Log($"Fading in {clip} {c.Volume}");
            t += Time.deltaTime / duration;
            c.Volume = Mathf.Lerp(0, c.DefaultVolume, t);
            yield return null;
        }

        t = 1;
        c.Volume = Mathf.Lerp(0, c.DefaultVolume, t);
        Debug.Log($"Fading in {clip} {c.Volume}");
    }

    static IEnumerator FadeOut(string clip, float duration)
    {
        if (!audioSources.ContainsKey(clip)) yield break;

        float t = 0;
        var c = audioSources[clip];

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            c.Volume = Mathf.Lerp(c.DefaultVolume, 0, t);
            yield return null;
        }

        t = 1;
        c.Volume = Mathf.Lerp(c.DefaultVolume, 0, t);
        c.Source.Stop();
    }
}

public class AudioSourceContainer
{
    public AudioSource Source { get; }
    public float DefaultVolume { get; }

    public float Volume
    {
        get => Source.volume;
        set => Source.volume = value;
    }

    public AudioSourceContainer(AudioSource source)
    {
        Source = source;
        DefaultVolume = source.volume;
    }

    internal void Play()
    {
        Source.Play();
    }

    internal void PlayOneShot()
    {
        Source.PlayOneShot(Source.clip);
    }
}