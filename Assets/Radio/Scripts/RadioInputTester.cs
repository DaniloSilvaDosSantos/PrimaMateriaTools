using UnityEngine;
using UnityEngine.InputSystem;

public class RadioInputTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource debugAudioSource;
    [Space]

    [Header("Debug Inputs")]
    [SerializeField] private KeyCode musicKey = KeyCode.M;
    [SerializeField] private KeyCode sfxKey = KeyCode.N;
    [SerializeField] private KeyCode sfxKeyCustomAudioSorce = KeyCode.B;
    [Space] 
    [SerializeField] private KeyCode musicFadeIn = KeyCode.I;
    [SerializeField] private KeyCode musicFadeOut = KeyCode.O;
    [SerializeField] private KeyCode musicCrossFade = KeyCode.U;

    [SerializeField] private bool isMusicPlaying = false;

    private void Update()
    {
        if (Input.GetKeyDown(musicKey))
        {
            if (isMusicPlaying)
            {
                Radio.Instance.StopMusic();
                isMusicPlaying = false;
            }
            else
            {
                Radio.Instance.PlayMusic("Music/Test01");
                isMusicPlaying = true;
            }
        }

        if (Input.GetKeyDown(musicFadeIn))
        {
            Radio.Instance.PlayMusic("Music/Test01", MusicTransition.Fade, 1f);
            isMusicPlaying = true;
        }

        if (Input.GetKeyDown(musicFadeOut))
        {
            Radio.Instance.StopMusic(true, 1f);
            isMusicPlaying = false;
        }

        if (Input.GetKeyDown(musicCrossFade))
        {
            Radio.Instance.PlayMusic("Music/Test02", MusicTransition.Crossfade, 2f);
        }

        if (Input.GetKeyDown(sfxKey))
        {
            Radio.Instance.PlaySFX("SFX/Test01");
        }

        if(Input.GetKeyDown(sfxKeyCustomAudioSorce))
        {
            Radio.Instance.PlaySFX("SFX/Test01", debugAudioSource);
        }
    }
}

