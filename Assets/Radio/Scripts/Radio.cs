using System.Collections;
using UnityEngine;

public class Radio : MonoBehaviour
{
    public static Radio Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Libraries")]
    [SerializeField] private SoundLibrary musicLibrary;
    [SerializeField] private SoundLibrary sfxLibrary;

    private Coroutine currentFade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // MUSICS METHODS //

    public void PlayMusic(string id, bool fade = false, float fadeDuration = 1f)
    {
        var sound = musicLibrary.GetSound(id);
        if (sound == null || sound.clip == null) return;

        if (fade)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeInMusic(sound, fadeDuration));
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = sound.clip;
            musicSource.loop = sound.loop;
            musicSource.volume = sound.volume;
            musicSource.Play();
        }
    }

    public void StopMusic(bool fade = false, float fadeDuration = 1f)
    {
        if (fade)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeOutMusic(fadeDuration));
        }
        else
        {
            musicSource.Stop();
        }
    }

    
    private IEnumerator FadeInMusic(SoundData sound, float duration)
    {
        musicSource.Stop();
        musicSource.clip = sound.clip;
        musicSource.loop = sound.loop;
        musicSource.volume = 0f;
        musicSource.Play();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = duration > 0f ? timer / duration : 1f;
            musicSource.volume = Mathf.Lerp(0f, sound.volume, progress);
            yield return null;
        }
        musicSource.volume = sound.volume;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = duration > 0f ? timer / duration : 1f;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    // SFX METHODS

    public void PlaySFX(string id)
    {
        var sound = sfxLibrary.GetSound(id);
        if (sound == null || sound.clip == null) return;

        GameObject temp = new GameObject($"SFX_{sound.clip.name}");
        temp.transform.parent = transform;

        AudioSource tempSource = temp.AddComponent<AudioSource>();
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
