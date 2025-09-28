using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Radio/Sound")]
public class SoundData : ScriptableObject
{
    public string soundID;
    public List<AudioClip> clips;
    public bool loop;
    [Range(0f, 1f)] public float volume = 1f;

    
    [Header("Pitch Variation")]
    [Range(0.5f, 2f)] public float minPitch = 1f;
    [Range(0.5f, 2f)] public float maxPitch = 1f;
}
