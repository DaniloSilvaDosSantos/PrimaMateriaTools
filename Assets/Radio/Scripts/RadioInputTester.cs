using UnityEngine;

public class RadioInputTester : MonoBehaviour
{
    [Header("Teclas de Debug")]
    [SerializeField] private KeyCode musicKey = KeyCode.M;
    [SerializeField] private KeyCode sfxKey = KeyCode.N;

    private bool isMusicPlaying = false;

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

        if (Input.GetKeyDown(sfxKey))
        {
            Radio.Instance.PlaySFX("SFX/Test01");
        }
    }
}

