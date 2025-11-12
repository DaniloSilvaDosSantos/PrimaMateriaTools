using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadioDebugger : MonoBehaviour
{
    [Header("Radio References")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private Transform parentTransform;

    [Header("UI")]
    [SerializeField] private TMP_Text debugText;

    private void Update()
    {
        List<string> lines = new List<string>();

        // --- MUSIC SOURCES ---
        LogAudioSource(musicSourceA, "[MUSIC]", lines);
        LogAudioSource(musicSourceB, "[MUSIC]", lines);

        // --- SFX SOURCES ---
        if (parentTransform != null)
        {
            foreach (Transform child in parentTransform)
            {
                AudioSource src = child.GetComponent<AudioSource>();
                if (src == null || src.clip == null) continue;

                if (src == musicSourceA || src == musicSourceB) continue;

                LogAudioSource(src, "[SFX]", lines);
            }
        }

        debugText.text = lines.Count > 0 ? string.Join("\n", lines) : "[Radio] ---.";
    }

    private void LogAudioSource(AudioSource src, string tag, List<string> lines)
    {
        if (src == null) return;

        string clipName = src.clip != null ? src.clip.name : "None";
        string loop = src.clip != null ? (src.loop ? "Yes" : "No") : "—";
        string time = src.clip != null ? $"{FormatTime(src.time)} / {FormatTime(src.clip.length)}" : "--:--:---";

        lines.Add($"{tag} {src.gameObject.name} | Clip: {clipName} | Vol: {src.volume:F2} | Pitch: {src.pitch:F2} | Loop: {loop} | Time: {time}");
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int milliseconds = Mathf.FloorToInt(t * 1000f % 1000f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
}
