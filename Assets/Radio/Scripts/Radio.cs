using System.Collections;
using System.Collections.Generic;
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

    private Dictionary<(SoundData.SoundType, string), int> lastPlayedIndex = new Dictionary<(SoundData.SoundType, string), int>();

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

    private void Update()
    {
        HandleMusicLoop(activeMusicSource);
    }

    // MUSICS METHODS //

    public void PlayMusic(string id, MusicTransition transition = MusicTransition.None, float duration = 1f)
    {
        SoundData soundData = musicLibrary.GetSoundData(id);

        int soundClipIndex = ChooseSound(soundData);

        if (soundData == null || soundData.clips[soundClipIndex] == null) return;

        if (currentTransition != null) StopFade(currentTransition);

        //Debug.Log($"Transition raw: {transition} ({(int)transition})");

        switch (transition)
        {
            case MusicTransition.None:
                PlayDirect(soundData, soundClipIndex);
                break;

            case MusicTransition.Fade:
                currentTransition = StartCoroutine(FadeOutInMusic(soundData, soundClipIndex, duration));
                break;

            case MusicTransition.Crossfade:
                currentTransition = StartCoroutine(Crossfade(soundData, soundClipIndex, duration));
                break;
        }
    }

    private void PlayDirect(SoundData sound, int soundClipIndex)
    {
        activeMusicSource.Stop();
        activeMusicSource.clip = sound.clips[soundClipIndex].clip;

        if (!sound.clips[soundClipIndex].useLoopPoints)
        {
            activeMusicSource.loop = sound.clips[soundClipIndex].loop;
        }
        else
        {
            activeMusicSource.loop = false;
        }

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

        if (!sound.clips[soundClipIndex].useLoopPoints)
        {
            activeMusicSource.loop = sound.clips[soundClipIndex].loop;
        }
        else
        {
            activeMusicSource.loop = false;
        }

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

        if (!newSound.clips[soundClipIndex].useLoopPoints)
        {
            activeMusicSource.loop = newSound.clips[soundClipIndex].loop;
        }
        else
        {
            activeMusicSource.loop = false;
        }

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

        (inactiveMusicSource, activeMusicSource) = (activeMusicSource, inactiveMusicSource);
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

    public void PlaySFX(string id, AudioSource customSource = null)
    {
        SoundData soundData = sfxLibrary.GetSoundData(id);
        int soundClipIndex = ChooseSound(soundData);

        if (soundData == null || soundData.clips[soundClipIndex] == null) return;

        AudioSource targetSource = customSource;

        if(targetSource == null)
        {
            GameObject temp = new GameObject($"SFX_{soundData.clips[soundClipIndex].clip.name}");
            temp.transform.parent = transform;
            targetSource = temp.AddComponent<AudioSource>();
        }

        targetSource.pitch = ChoosePitch(soundData, soundClipIndex);
        targetSource.clip = soundData.clips[soundClipIndex].clip;
        targetSource.volume = ChooseVolume(soundData, soundClipIndex);
        targetSource.loop = soundData.clips[soundClipIndex].loop;

        targetSource.Play();

        if (customSource == null && !targetSource.loop)
        {
            Destroy(targetSource, soundData.clips[soundClipIndex].clip.length + 0.1f);
        }
    }

    // UTILITIES METHODS

    private int ChooseSound(SoundData soundData)
    {
        if (soundData == null || soundData.clips == null || soundData.clips.Count == 0) return -1;

        var key = (soundData.type, soundData.soundID);

        if (soundData.clips.Count == 1)
        {
            lastPlayedIndex[key] = 0;
            return 0;
        }

        int lastIndex;
        lastPlayedIndex.TryGetValue(key, out lastIndex);

        int soundIndex = WeightedPick();

        if (soundIndex == lastIndex) soundIndex = WeightedPick();

        lastPlayedIndex[key] = soundIndex;
        return soundIndex;

        int WeightedPick()
        {
            float totalWeight = 0f;
            foreach (ClipData w in soundData.clips) totalWeight += Mathf.Max(0f, w.weight);

            float randomSoundPick = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            for (int i = 0; i < soundData.clips.Count; i++)
            {
                cumulativeWeight += Mathf.Max(0f, soundData.clips[i].weight);

                if (randomSoundPick <= cumulativeWeight) return i;
            }

            return soundData.clips.Count - 1;
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

    private void HandleMusicLoop(AudioSource audioSource)
    {
        if (audioSource == null || !audioSource.isPlaying || audioSource.clip == null) return;

        SoundData soundData = musicLibrary.GetSoundDataByClip(audioSource.clip);
        if (soundData == null) return;

        var key = (soundData.type, soundData.soundID);

        int lastIndex;
        if (!lastPlayedIndex.TryGetValue(key, out lastIndex)) return;

        ClipData clipData = soundData.clips[lastIndex];
        if (!clipData.useLoopPoints) return;

        int loopStartSample = Mathf.FloorToInt(clipData.loopStart * audioSource.clip.frequency);
        int loopEndSample = Mathf.FloorToInt(clipData.loopEnd * audioSource.clip.frequency);

        if (audioSource.timeSamples >= loopEndSample)
        {
            audioSource.timeSamples = loopStartSample;
        }
    }
}
