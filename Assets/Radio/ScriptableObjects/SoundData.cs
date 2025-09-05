using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Radio/Sound")]
public class SoundData : ScriptableObject
{
    public string soundID;
    public AudioClip clip;
    public bool loop;
    [Range(0f, 1f)] public float volume = 1f;
}
