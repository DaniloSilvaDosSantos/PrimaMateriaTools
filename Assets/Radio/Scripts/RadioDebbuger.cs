using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadioDebugger : MonoBehaviour
{
    [Header("Radio References")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Transform sfxParent;

    [Header("UI")]
    [SerializeField] private TMP_Text debugText;

    private void Update()
    {
        List<string> lines = new List<string>();

        if (musicSource != null)
        {
            string clipName = musicSource.clip != null ? musicSource.clip.name : "None";
            string loop = musicSource.clip != null ? (musicSource.loop ? "Yes" : "No") : "—";
            string time = musicSource.clip != null 
                ? $"{FormatTime(musicSource.time)} / {FormatTime(musicSource.clip.length)}" 
                : "--:--:---";

            lines.Add($"[MUSIC] {musicSource.gameObject.name} | Clip: {clipName} | Vol: {musicSource.volume:F2} | Loop: {loop} | Time: {time}");
        }

        if (sfxParent != null)
        {
            foreach (Transform child in sfxParent)
            {
                AudioSource src = child.GetComponent<AudioSource>();
                if (src == null || src.clip == null) continue;

                if (musicSource != null && src == musicSource) continue;

                string loop = src.loop ? "Yes" : "No";
                string time = $"{FormatTime(src.time)} / {FormatTime(src.clip.length)}";

                lines.Add($"[SFX] {src.gameObject.name} | Clip: {src.clip.name} | Vol: {src.volume:F2} | Loop: {loop} | Time: {time}");
            }
        }

        debugText.text = lines.Count > 0 ? string.Join("\n", lines) : "[Radio] ---.";
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int milliseconds = Mathf.FloorToInt(t * 1000f % 1000f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
}
