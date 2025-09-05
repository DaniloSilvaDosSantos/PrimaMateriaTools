using UnityEngine;

public class Radio : MonoBehaviour
{
    public static Radio Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Libraries")]
    [SerializeField] private SoundLibrary musicLibrary;
    [SerializeField] private SoundLibrary sfxLibrary;

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

    public void PlayMusic(string id)
    {
        var sound = musicLibrary.GetSound(id);
        if (sound == null || sound.clip == null) return;

        musicSource.Stop();
        musicSource.clip = sound.clip;
        musicSource.loop = sound.loop;
        musicSource.volume = sound.volume;
        musicSource.Play();
    }

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

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
