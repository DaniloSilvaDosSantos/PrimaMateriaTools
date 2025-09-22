using UnityEngine;

public class RadioInputTester : MonoBehaviour
{
    [Header("Teclas de Debug")]
    [SerializeField] private KeyCode musicKey = KeyCode.M;
    [SerializeField] private KeyCode sfxKey = KeyCode.N;
    [SerializeField] private KeyCode musicFadeIn = KeyCode.I;
    [SerializeField] private KeyCode musicFadeOut = KeyCode.O;

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
            Radio.Instance.PlayMusic("Music/Test01", true, 1f);
            isMusicPlaying = true;
        }

        if (Input.GetKeyDown(musicFadeOut))
        {
            Radio.Instance.StopMusic(true, 1f);
            isMusicPlaying = false;
        }

        if (Input.GetKeyDown(sfxKey))
        {
            Radio.Instance.PlaySFX("SFX/Test01");
        }
    }
}

