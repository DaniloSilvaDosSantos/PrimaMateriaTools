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