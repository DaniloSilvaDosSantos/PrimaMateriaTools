using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Radio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    public List<SoundData> soundDatas;

    public SoundData GetSoundData(string id)
    {
        return soundDatas.Find(s => s.soundID == id);
    }

    public SoundData GetSoundDataByClip(AudioClip clip)
    {
        foreach (var soundData in soundDatas)
        {
            foreach (var clipData in soundData.clips)
            {
                if (clipData.clip == clip) return soundData;
            }
        }
        return null;
    }
}