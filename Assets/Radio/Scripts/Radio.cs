using System.Collections;
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
        var sound = musicLibrary.GetSound(id);
        if (sound == null || sound.clip == null) return;

        if (currentTransition != null) StopFade(currentTransition);

        Debug.Log($"Transition raw: {transition} ({(int)transition})");

        switch (transition)
        {
            case MusicTransition.None:
                PlayDirect(sound);
                break;

            case MusicTransition.Fade:
                currentTransition = StartCoroutine(FadeOutInMusic(sound, duration));
                break;

            case MusicTransition.Crossfade:
                currentTransition = StartCoroutine(Crossfade(sound, duration));
                break;
        }
    }

    private void PlayDirect(SoundData sound)
    {
        activeMusicSource.Stop();
        activeMusicSource.clip = sound.clip;
        activeMusicSource.loop = sound.loop;
        activeMusicSource.volume = sound.volume;
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

    private IEnumerator FadeOutInMusic(SoundData newSound, float duration)
    {
        // Fade Out
        yield return StartCoroutine(FadeOutMusic(duration * 0.5f));

        // Fade In
        yield return StartCoroutine(FadeInMusic(newSound, duration * 0.5f));
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
    
    private IEnumerator FadeInMusic(SoundData sound, float duration)
    {
        activeMusicSource.Stop();
        activeMusicSource.clip = sound.clip;
        activeMusicSource.loop = sound.loop;
        activeMusicSource.volume = 0f;
        activeMusicSource.Play();

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = duration > 0f ? timer / duration : 1f;
            activeMusicSource.volume = Mathf.Lerp(0f, sound.volume, progress);
            yield return null;
        }

        activeMusicSource.volume = sound.volume;
    }

    private IEnumerator Crossfade(SoundData newSound, float duration)
    {

        inactiveMusicSource.clip = newSound.clip;
        inactiveMusicSource.loop = newSound.loop;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.Play();

        float timer = 0f;
        float startVolume = activeMusicSource.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            activeMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            inactiveMusicSource.volume = Mathf.Lerp(0f, newSound.volume, t);

            yield return null;
        }

        activeMusicSource.Stop();
        inactiveMusicSource.volume = newSound.volume;

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
        var sound = sfxLibrary.GetSound(id);
        if (sound == null || sound.clip == null) return;

        GameObject temp = new GameObject($"SFX_{sound.clip.name}");
        temp.transform.parent = transform;

        AudioSource tempSource = temp.AddComponent<AudioSource>();

        tempSource.pitch = Random.Range(sound.minPitch, sound.maxPitch);
        tempSource.clip = sound.clip;
        tempSource.volume = sound.volume;
        tempSource.loop = sound.loop;

        tempSource.Play();

        if (!tempSource.loop)
        {
            Destroy(temp, sound.clip.length + 0.1f);
        }
    }
}
