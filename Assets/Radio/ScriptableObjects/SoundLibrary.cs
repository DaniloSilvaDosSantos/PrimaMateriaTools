using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Radio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    public List<SoundData> sounds;

    public SoundData GetSound(string id)
    {
        return sounds.Find(s => s.soundID == id);
    }
}

[CreateAssetMenu(fileName = "NewSound", menuName = "Radio/Sound")]
public class SoundData : ScriptableObject
{
    public string soundID;
    public AudioClip clip;
    public bool loop;
    [Range(0f, 1f)] public float volume = 1f;
}