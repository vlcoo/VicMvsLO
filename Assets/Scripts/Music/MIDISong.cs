using System.Collections.Generic;
using FluidMidi;
using UnityEngine;

[CreateAssetMenu(fileName = "MIDISong", menuName = "ScriptableObjects/MIDISong")]
public class MIDISong : ScriptableObject
{
    [SerializeField] public StreamingAsset song;
    [SerializeField] public bool autoSoundfont = true;
    [SerializeField] public StreamingAsset soundfont;
    [SerializeField] public int startTicks;
    [SerializeField] public int startLoopTicks;
    [SerializeField] public int endTicks;
    [SerializeField] [BitField] public int mutedChannelsNormal;
    [SerializeField] [BitField] public int mutedChannelsSpectating;
    [SerializeField] [Range(0.1f, 2.0f)] public float playbackSpeedNormal = 1.0f;
}
