using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "ScriptableObjects/MusicData")]
public class MusicData : ScriptableObject
{
    public AudioClip clip, fastClip;
    public float loopStartSample, loopEndSample;

    public float[] bahTimestamps;

    public MusicData(AudioClip audio, float start, float end)
    {
        clip = audio;
        loopStartSample = start;
        loopEndSample = end;
    }

    public bool hasBahs => bahTimestamps != null && bahTimestamps.Length != 0;
}