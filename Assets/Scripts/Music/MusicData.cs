using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "ScriptableObjects/MusicData")]
public class MusicData : ScriptableObject {

    public AudioClip clip, fastClip;
    public float loopStartSample, loopEndSample;

    public float[] bahTimestamps;
    public bool hasBahs => bahTimestamps != null && bahTimestamps.Length != 0;

    public MusicData(AudioClip audio)
    {
        clip = audio;
    }
}