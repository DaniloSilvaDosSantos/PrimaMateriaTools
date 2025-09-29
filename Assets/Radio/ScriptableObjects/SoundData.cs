using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Radio/Sound")]
public class SoundData : ScriptableObject
{
    public string soundID;
    public enum SoundType { Music, SFX }
    public SoundType type;
    public List<ClipData> clips;
}

[System.Serializable]
public class ClipData
{
    public AudioClip clip;
    public float weight = 1f;
    public bool loop;

    [Header("Loop Points (in seconds)")]
    public bool useLoopPoints = false;
    public float loopStart = 0f;
    public float loopEnd = 0f;

    [Header("Volume & Pitch Variation")]
    [Range(0f, 1f)] public float minVolume = 1f;
    [Range(0f, 1f)] public float maxVolume = 1f;
    [Range(0.5f, 2f)] public float minPitch = 1f;
    [Range(0.5f, 2f)] public float maxPitch = 1f;
}

