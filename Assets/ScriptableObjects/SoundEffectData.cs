using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "Audio/Sound Effect")]
public class SoundEffectData : ScriptableObject
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;
}