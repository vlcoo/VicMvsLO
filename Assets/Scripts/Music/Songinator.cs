using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using HGS.Tone;
using KaimiraGames;
using MeltySynth;
using UnityEngine;
using UnityEngine.Serialization;

public class Songinator : MonoBehaviour
{
    public enum PlaybackState
    {
        STOPPED = -2,
        PAUSED = -1,
        PLAYING = 1
    }

    [NonSerialized] public Synthesizer Synth;
    [NonSerialized] public MidiFileSequencer Sequencer;
    [NonSerialized] public AudioSource Source;

    [SerializeField] public bool autoStart = true;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;
    private readonly WeightedList<MIDISong> weightedList = new();

    [NonSerialized] public MIDISong CurrentSong;
    [SerializeField] public PlaybackState state = PlaybackState.STOPPED;
    private TimeSpan timeAtPause = TimeSpan.Zero;
    private int currentlyMutedChannels;

    public Synthesizer.OnMidiMessage OnMidiMessage;
    public delegate void OnFadingComplete();

    private void Start()
    {
        // Load in the current song from the list of candidates.
        if (songs.Count > 1)
        {
            for (var i = 0; i < songs.Count; i++) weightedList.Add(songs[i], chances[i]);
            CurrentSong = weightedList.Next();
        }
        else CurrentSong = songs[0];

        InitializeMeltySynth();

        // All good to go.
        if (autoStart) SetPlaybackState(PlaybackState.PLAYING);
    }

    private void InitializeMeltySynth()
    {
        // Load in the soundfont and create the synth and sequencer objects.
        var sf = new SoundFont(new MemoryStream(CurrentSong.soundfont.Bytes));
        var settings = new SynthesizerSettings(AudioSettings.outputSampleRate);
        settings.EnableReverbAndChorus = false;
        settings.BlockSize = 64;
        Synth = new Synthesizer(sf, settings)
        {
            onMidiMessage = OnMidiMessage
        };
        Sequencer = new MidiFileSequencer(Synth);

        var driver = gameObject.GetComponent<ToneAudioDriver>();
        if (!driver) driver = gameObject.AddComponent<ToneAudioDriver>();
        driver.SetRenderer(Sequencer);
        Source = GetComponent<AudioSource>();

        // Assign to the synth and sequencer the song properties.
        Sequencer.Speed = CurrentSong.playbackSpeedNormal;
        Sequencer.StartLoopTicks = CurrentSong.startLoopTicks;
        Sequencer.EndLoopTicks = CurrentSong.endTicks;
        Source.pitch += CurrentSong.pitchDeltaNormal;
        currentlyMutedChannels = CurrentSong.mutedChannelsNormal;
    }

    public YieldInstruction SetPlaybackState(PlaybackState newState, float secondsFading = 0f)
    {
        if (secondsFading > 0f)
        {
            // Ugly! If fading to STOPPED or PAUSED, change the volume first and then actually stop.
            // If fading from PLAYING, actually start playing at volume 0 then tween it in.
            // This is a recursive function, so the "SetPlaybackState" calls in this if block
            // must not have a fade so we don't fall into an infinite loop.
            if ((int)newState > 0)
            {
                Source.volume = 0.0f;
                SetPlaybackState(newState);
            }
            return FadeVolume((int)newState, secondsFading, () =>
            {
                if ((int)newState < 0) SetPlaybackState(newState);
            });
        }

        state = newState;
        switch (state)
        {
            case PlaybackState.STOPPED:
                timeAtPause = TimeSpan.Zero;
                Sequencer.Stop();
                break;
            case PlaybackState.PAUSED:
                timeAtPause = Sequencer.Pos();
                Sequencer.Stop();
                break;
            case PlaybackState.PLAYING:
                Sequencer.Play(new MidiFile(new MemoryStream(CurrentSong.song.Bytes)), true);
                Synth.SetChannelsMuted(currentlyMutedChannels);
                if (timeAtPause != TimeSpan.Zero) Sequencer.Seek(timeAtPause);
                else if (CurrentSong.startTicks > 0) Sequencer.Seek(CurrentSong.startTicks);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public YieldInstruction FadeVolume(int direction, float secondsDuration, OnFadingComplete onComplete = null)
    {
        var t = DOTween.To(
            () => Source.volume,
            v => Source.volume = v,
            direction > 0 ? 1.0f : 0.0f,
            secondsDuration
        ).SetEase(Ease.Linear);
        t.onComplete = () => onComplete?.Invoke();
        return t.WaitForCompletion();
    }

    public void SwitchToSong(int index, bool startPlayback = false, float secondsFading = 0f)
    {
        if (index < 0 || index >= songs.Count)
        {
            Debug.LogWarning("Invalid song index (out of bounds).");
            return;
        }

        SetPlaybackState(PlaybackState.STOPPED, secondsFading);
        CurrentSong = songs[index];
        InitializeMeltySynth();
        autoStart = startPlayback;
        if (autoStart) SetPlaybackState(PlaybackState.PLAYING, secondsFading);
    }

    public void SetSpectating(bool how)
    {
        currentlyMutedChannels = how ? CurrentSong.mutedChannelsSpectating : CurrentSong.mutedChannelsNormal;
        Synth.SetChannelsMuted(currentlyMutedChannels);
    }
}