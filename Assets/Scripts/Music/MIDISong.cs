using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MIDISong", menuName = "ScriptableObjects/MIDISong")]
public class MIDISong : ScriptableObject
{
    [SerializeField] public MidAssetData song;
    [SerializeField] public bool autoSoundfont = true;
    [SerializeField] public SoundfontAssetData soundfont;

    [SerializeField] public int startTicks;
    [SerializeField] public int startLoopTicks;
    [SerializeField] public int endTicks;

    [SerializeField] [ChannelField] public int mutedChannelsNormal;
    [SerializeField] [ChannelField] public int mutedChannelsSpectating;
    [SerializeField] [Range(-1, 15)] public int bahChannel = -1;

    [SerializeField] [Range(0.1f, 2.0f)] public float playbackSpeedNormal = 1.0f;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoSoundfont)
        {
            soundfont = AssetDatabase.LoadAssetAtPath<SoundfontAssetData>(AssetDatabase.GetAssetPath(song).Replace(".mid", ".sf2"));
        }
    }
    #endif
}