using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using HGS.Tone;
using KaimiraGames;
using MeltySynth;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class Songinator : MonoBehaviour
{
    public delegate void OnFadingComplete();

    public enum PlaybackState
    {
        STOPPED = -2,
        PAUSED = -1,
        PLAYING = 1
    }

    private const float OriginalPitch = 0.985f;

    [SerializeField] public bool autoStart = true;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;
    [SerializeField] public PlaybackState state = PlaybackState.STOPPED;
    private readonly WeightedList<MIDISong> weightedList = new();

    private MidiFile _currentMidiFile;
    private int currentlyMutedChannels;
    private bool initted = false;

    [NonSerialized] public MIDISong CurrentSong;
    [NonSerialized] private MidiFileSequencer Sequencer;
    [NonSerialized] private AudioSource Source;
    [NonSerialized] private ToneAudioDriver Driver;
    private Coroutine switchToSongCoroutine;

    [NonSerialized] private Synthesizer Synth;
    private TimeSpan timeAtPause = TimeSpan.Zero;

    private void Start()
    {
        // CalculateCurrentSong();
    }

    private void OnEnable() 
    {
        CalculateCurrentSong();
    }

    private void OnDisable() {
        Driver.BufferingStopped = true;
    }

    private void CalculateCurrentSong(bool discardInitializedSynth = false)
    {
        SetPlaybackState(PlaybackState.STOPPED);
        if (discardInitializedSynth && initted) {
            StopCoroutine(switchToSongCoroutine);
            Sequencer.Stop();
            Sequencer = null;
            Synth = null;
            initted = false;
        }
        
        // Load in the current song from the list of candidates.
        if (songs.Count > 1)
        {
            weightedList.Clear();
            for (var i = 0; i < songs.Count; i++) weightedList.Add(songs[i], chances[i]);
            CurrentSong = weightedList.Next();
        }
        else
        {
            CurrentSong = songs[0];
        }

        InitializeMeltySynth();

        // All good to go.
        if (autoStart) {
            Driver.SetVolume(1.0f);
            SetPlaybackState(PlaybackState.PLAYING);
        }
    }

    private void InitializeMeltySynth()
    {
        
        // Load in the soundfont and create the synth and sequencer objects.
        Synth = new Synthesizer(new SoundFont(Decompress(CurrentSong.soundfont.Bytes)),
            new SynthesizerSettings(AudioSettings.outputSampleRate)
            {
                EnableReverbAndChorus = false,
                BlockSize = 64,
                MaximumPolyphony = 128,
            }
        );
        Sequencer = new MidiFileSequencer(Synth);

        Driver ??= gameObject.GetComponent<ToneAudioDriver>();
        Driver.SetRenderer(Sequencer);
        Source ??= GetComponent<AudioSource>();

        _currentMidiFile = new MidiFile(new MemoryStream(CurrentSong.song.Bytes));

        // Assign to the synth and sequencer the song properties.
        Sequencer.Speed = CurrentSong.playbackSpeedNormal;
        Sequencer.StartLoopTicks = CurrentSong.startLoopTicks;
        Sequencer.EndLoopTicks = CurrentSong.endTicks;
        Source.pitch = OriginalPitch + CurrentSong.pitchDeltaNormal;
        currentlyMutedChannels = CurrentSong.mutedChannelsNormal;
        
        initted = true;
    }

    public YieldInstruction SetPlaybackState(PlaybackState newState, float secondsFading = 0f)
    {
        if (Sequencer is null || Synth is null || _currentMidiFile is null)
        {
            CurrentSong = songs[0];
            InitializeMeltySynth();
        }

        if (newState == state) return null;

        if (secondsFading > 0f) {
            // Ugly! If fading to STOPPED or PAUSED, change the volume first and then actually stop.
            // If fading from PLAYING, actually start playing at volume 0 then tween it in.
            // This is a recursive function, so the "SetPlaybackState" calls in this if block
            // must not have a fade so we don't fall into an infinite loop.
            if ((int) newState > 0) {
                Driver.SetVolume(0.0f);
                SetPlaybackState(newState);
            }

            return FadeVolume((int) newState, secondsFading, () => {
                if ((int) newState < 0) SetPlaybackState(newState);
            });
        }

        if (secondsFading == 0 && newState == PlaybackState.PLAYING) Driver.SetVolume(1.0f);

        state = newState;
        switch (state)
        {
            case PlaybackState.STOPPED:
                timeAtPause = TimeSpan.Zero;
                Sequencer.Stop();
                Source.enabled = false;
                Driver.BufferingStopped = true;
                break;
            case PlaybackState.PAUSED:
                timeAtPause = Sequencer.Pos();
                Sequencer.Stop();
                Source.enabled = false;
                Driver.BufferingStopped = true;
                break;
            case PlaybackState.PLAYING:
                Sequencer.Play(_currentMidiFile, true);
                Synth.SetChannelsMuted(currentlyMutedChannels);
                if (timeAtPause != TimeSpan.Zero) Sequencer.Seek(timeAtPause);
                else if (CurrentSong.startTicks > 0) Sequencer.Seek(CurrentSong.startTicks);
                Source.enabled = true;
                Driver.BufferingStopped = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public YieldInstruction FadeVolume(int direction, float secondsDuration, OnFadingComplete onComplete = null)
    {
        var t = DOTween.To(
            () => Driver.GetVolume(),
            v => Driver.SetVolume(v),
            direction > 0 ? 1.0f : 0.0f,
            secondsDuration
        ).SetEase(Ease.Linear);
        t.onComplete = () => onComplete?.Invoke();
        return t.WaitForCompletion();
    }

    public void SwitchToSong(int index, bool startPlayback = false, float secondsFading = 0f)
    {
        if (switchToSongCoroutine != null)
        {
            StopCoroutine(switchToSongCoroutine);
            switchToSongCoroutine = null;
            SetPlaybackState(PlaybackState.STOPPED);
        }

        switchToSongCoroutine = StartCoroutine(SwitchToSongCoroutine(index, startPlayback, secondsFading));
    }

    private IEnumerator SwitchToSongCoroutine(int index, bool startPlayback = false, float secondsFading = 0f)
    {
        if (index < -1 || index >= songs.Count)
        {
            Debug.LogWarning("Invalid song index (out of bounds).");
            yield return null;
        }
        CurrentSong = index == -1 ? weightedList.Next() : songs[index];

        timeAtPause = TimeSpan.Zero;
        yield return SetPlaybackState(PlaybackState.STOPPED, secondsFading);
        InitializeMeltySynth();
        // autoStart = startPlayback;
        if (startPlayback) yield return SetPlaybackState(PlaybackState.PLAYING, secondsFading);
        switchToSongCoroutine = null;
    }

    public void SetSpectating(bool how)
    {
        var newMutedChannels = how ? CurrentSong.mutedChannelsSpectating : CurrentSong.mutedChannelsNormal;
        if (newMutedChannels == currentlyMutedChannels) return;
        currentlyMutedChannels = newMutedChannels;
        Synth.SetChannelsMuted(currentlyMutedChannels);
    }

    public void SetOnMidiMessage(Synthesizer.OnMidiMessage func)
    {
        Synth.onMidiMessage += func;
    }

    public static MemoryStream Decompress(byte[] data) {
        using var stream = new MemoryStream(data);
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        var output = new MemoryStream();
        gzip.CopyTo(output);
        output.Position = 0;
        return output;
    }
}