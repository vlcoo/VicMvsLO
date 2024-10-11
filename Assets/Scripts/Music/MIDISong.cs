using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MIDISong", menuName = "ScriptableObjects/MIDISong")]
public class MIDISong : ScriptableObject
{
    [Header("Song info")] [Tooltip("The imported .mid file to play.")] [SerializeField]
    public MidAssetData song;

    [Tooltip("If enabled, the soundfont will be automatically chosedn based on the song's filename.")] [SerializeField]
    public bool autoSoundfont = true;

    [Tooltip("The imported .sf2 file to use as instrument bank for the song.")] [SerializeField]
    public SoundfontAssetData soundfont;

    [Header("Playback settings")]
    [Tooltip("The time, in MIDI ticks, at which the song starts playing.")]
    [SerializeField]
    public int startTicks;

    [Tooltip("The time, in MIDI ticks, to continue the song at when looping.")] [SerializeField]
    public int startLoopTicks;

    [Tooltip("The time, in MIDI ticks, to end the song at.")] [SerializeField]
    public int endTicks;

    [Header("Channel settings")]
    [Tooltip("Which channels to mute when playing the song under normal circumstances.")]
    [SerializeField]
    [ChannelField]
    public int mutedChannelsNormal;

    [Tooltip("Which channels to mute when the player is in a match and spectating.")] [SerializeField] [ChannelField]
    public int mutedChannelsSpectating;

    [Tooltip(
        "Which channel (0-indexed, i.e. a value of 0 means the first channel) corresponds to the 'BAH' voice samples in the song. If none are present, set to -1.")]
    [SerializeField]
    [Range(-1, 15)]
    public int bahChannel = -1;

    [Header("Remixing")]
    [Tooltip(
        "The speed at which the song should be played under normal circumstances. A value of 1.0 corresponds to the original speed.")]
    [SerializeField]
    [Range(0.1f, 2.0f)]
    public float playbackSpeedNormal = 1.0f;

    [Tooltip(
        "The relative pitch that will be added to the audio source that plays the song under normal circumstances. A value of 0.0 corresponds to the original pitch.")]
    [SerializeField]
    [Range(-0.5f, 0.5f)]
    public float pitchDeltaNormal;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoSoundfont)
            soundfont = AssetDatabase.LoadAssetAtPath<SoundfontAssetData>(AssetDatabase.GetAssetPath(song)
                .Replace(".mid", ".sf2"));
    }
#endif
}