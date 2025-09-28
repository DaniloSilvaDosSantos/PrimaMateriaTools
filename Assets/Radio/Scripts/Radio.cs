using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Radio : MonoBehaviour
{
    public static Radio Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    [Header("Sound Libraries")]
    [SerializeField] private SoundLibrary musicLibrary;
    [SerializeField] private SoundLibrary sfxLibrary;

    private Coroutine currentTransition;

    private Dictionary<string, int> lastPlayedIndex = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;
    }

    // MUSICS METHODS //

    public void PlayMusic(string id, MusicTransition transition = MusicTransition.None, float duration = 1f)
    {
        var sound = musicLibrary.GetSoundData(id);

        int soundClipIndex = ChooseSound(sound);

        if (sound == null || sound.clips[soundClipIndex] == null) return;

        if (currentTransition != null) StopFade(currentTransition);

        //Debug.Log($"Transition raw: {transition} ({(int)transition})");

        switch (transition)
        {
            case MusicTransition.None:
                PlayDirect(sound, soundClipIndex);
                break;

            case MusicTransition.Fade:
                currentTransition = StartCoroutine(FadeOutInMusic(sound, soundClipIndex, duration));
                break;

            case MusicTransition.Crossfade:
                currentTransition = StartCoroutine(Crossfade(sound, soundClipIndex, duration));
                break;
        }
    }

    private void PlayDirect(SoundData sound, int soundClipIndex)
    {
        activeMusicSource.Stop();
        activeMusicSource.clip = sound.clips[soundClipIndex].clip;
        activeMusicSource.loop = sound.clips[soundClipIndex].loop;
        activeMusicSource.volume = ChooseVolume(sound, soundClipIndex);
        activeMusicSource.Play();
    }

    public void StopMusic(bool fade = false, float fadeDuration = 1f)
    {
        if (fade)
        {
            if (currentTransition != null) StopFade(currentTransition);
            currentTransition = StartCoroutine(FadeOutMusic(fadeDuration));
        }
        else
        {
            activeMusicSource.Stop();
        }
    }

    // MUSICS TRANSITIONS //

    private IEnumerator FadeOutInMusic(SoundData newSound, int soundClipIndex, float duration)
    {
        // Fade Out
        yield return StartCoroutine(FadeOutMusic(duration * 0.5f));

        // Fade In
        yield return StartCoroutine(FadeInMusic(newSound, soundClipIndex, duration * 0.5f));
    }


    private IEnumerator FadeOutMusic(float duration, bool resetVolumeToStartVolume = false)
    {
        float startVolume = activeMusicSource.volume;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = duration > 0f ? timer / duration : 1f;
            activeMusicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        activeMusicSource.volume = 0f;
        activeMusicSource.Stop();

        if (resetVolumeToStartVolume) activeMusicSource.volume = startVolume;
    }

    private IEnumerator FadeInMusic(SoundData sound, int soundClipIndex, float duration)
    {
        activeMusicSource.Stop();
        activeMusicSource.clip = sound.clips[soundClipIndex].clip;
        activeMusicSource.loop = sound.clips[soundClipIndex].clip;
        activeMusicSource.volume = 0f;
        activeMusicSource.Play();

        float timer = 0f;

        float sortedVolume = ChooseVolume(sound, soundClipIndex);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = duration > 0f ? timer / duration : 1f;
            activeMusicSource.volume = Mathf.Lerp(0f, sortedVolume, progress);
            yield return null;
        }

        activeMusicSource.volume = sortedVolume;
    }

    private IEnumerator Crossfade(SoundData newSound, int soundClipIndex, float duration)
    {

        inactiveMusicSource.clip = newSound.clips[soundClipIndex].clip;
        inactiveMusicSource.loop = newSound.clips[soundClipIndex].clip;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.Play();

        float timer = 0f;
        float startVolume = activeMusicSource.volume;

        float sortedVolume = ChooseVolume(newSound, soundClipIndex);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            activeMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            inactiveMusicSource.volume = Mathf.Lerp(0f, sortedVolume, t);

            yield return null;
        }

        activeMusicSource.Stop();
        inactiveMusicSource.volume = sortedVolume;

        var temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;
    }

    public void StopFade(Coroutine fadeRoutine, bool stopAndMute = false)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);

            if (stopAndMute)
            {
                activeMusicSource.Stop();
                activeMusicSource.volume = 0f;
            }
        }
    }

    // SFX METHODS

    public void PlaySFX(string id)
    {
        var sound = sfxLibrary.GetSoundData(id);

        int soundClipIndex = ChooseSound(sound);

        if (sound == null || sound.clips[soundClipIndex] == null) return;

        GameObject temp = new GameObject($"SFX_{sound.clips[soundClipIndex].clip.name}");
        temp.transform.parent = transform;

        AudioSource tempSource = temp.AddComponent<AudioSource>();

        tempSource.pitch = ChoosePitch(sound, soundClipIndex);
        tempSource.clip = sound.clips[soundClipIndex].clip;
        tempSource.volume = ChooseVolume(sound, soundClipIndex);
        tempSource.loop = sound.clips[soundClipIndex].loop;

        tempSource.Play();

        if (!tempSource.loop)
        {
            Destroy(temp, sound.clips[soundClipIndex].clip.length + 0.1f);
        }
    }

    // UTILITIES METHODS

    private int ChooseSound(SoundData sound)
    {
        if (sound == null || sound.clips == null || sound.clips.Count == 0) return -1;

        if (sound.clips.Count == 1)
        {
            lastPlayedIndex[sound.soundID] = 0;
            return 0;
        }

        int lastIndex;
        lastPlayedIndex.TryGetValue(sound.soundID, out lastIndex);

        int soundIndex = WeightedPick();

        if (soundIndex == lastIndex) soundIndex = WeightedPick();

        lastPlayedIndex[sound.soundID] = soundIndex;
        return soundIndex;

        int WeightedPick()
        {
            float totalWeight = 0f;
            foreach (var w in sound.clips) totalWeight += Mathf.Max(0f, w.weight);

            float randSoundPick = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            for (int i = 0; i < sound.clips.Count; i++)
            {
                cumulativeWeight += Mathf.Max(0f, sound.clips[i].weight);

                if (randSoundPick <= cumulativeWeight) return i;
            }

            return sound.clips.Count - 1;
        }
    }

    private float ChooseVolume(SoundData sound, int soundClipIndex)
    {
        if (sound.clips[soundClipIndex].minVolume < sound.clips[soundClipIndex].maxVolume)
        {
            return Random.Range(sound.clips[soundClipIndex].minVolume, sound.clips[soundClipIndex].maxVolume);
        }
        else
        {
            return sound.clips[soundClipIndex].maxVolume;
        }
    }

    private float ChoosePitch(SoundData sound, int soundClipIndex)
    {
        if (sound.clips[soundClipIndex].minPitch < sound.clips[soundClipIndex].maxPitch)
        {
            return Random.Range(sound.clips[soundClipIndex].minPitch, sound.clips[soundClipIndex].maxPitch);
        }
        else
        {
            return sound.clips[soundClipIndex].maxPitch;
        }
    }
}
