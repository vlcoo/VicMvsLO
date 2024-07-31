using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HGS.Tone;
using KaimiraGames;
using UnityEngine;

public class Songinator : MonoBehaviour
{
    [NonSerialized] public ToneSequencer Player;
    [NonSerialized] public AudioSource Source;

    [SerializeField] public bool autoStart = true;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;

    private readonly WeightedList<MIDISong> weightedList = new();

    [NonSerialized] public MIDISong CurrentSong;
    private int rememberedChannels;

    public void Start()
    {
        Player = GetComponent<ToneSequencer>();
        Source = GetComponent<AudioSource>();

        if (songs.Count == 0 || chances.Count != songs.Count) return;

        if (songs.Count > 1)
        {
            for (var i = 0; i < songs.Count; i++) weightedList.Add(songs[i], chances[i]);

            CurrentSong = weightedList.Next();
        }
        else
        {
            CurrentSong = songs[0];
        }

        Player.CreateSynth(CurrentSong.soundfont.Bytes);
        Player.Init();
        Player.Sequencer.Speed = CurrentSong.playbackSpeedNormal;
        Player.Sequencer.StartLoopTicks = CurrentSong.startLoopTicks;
        Player.Sequencer.EndLoopTicks = CurrentSong.endTicks;
        Source.pitch += CurrentSong.pitchDeltaNormal;
        rememberedChannels = CurrentSong.mutedChannelsNormal;

        if (autoStart) StartPlayback();
    }

    public void StartPlayback(bool fromBeginning = true)
    {
        Player.Play(CurrentSong.song.Bytes);
        if (CurrentSong.startTicks > 0 && fromBeginning)
        {
            Player.Sequencer.Seek(CurrentSong.startTicks);
        }
        Player.Synthesizer.SetChannelsMuted(rememberedChannels);
    }

    public void SwitchToSong(MIDISong newSong, bool startPlayback = false)
    {
        Player.Stop();
        // Destroy(player.synthesizer);
        // Destroy(player);
        // player = gameObject.AddComponent<SongPlayer>();
        // player.playOnStart = false;
        // player.synthesizer = gameObject.AddComponent<Synthesizer>();
        
        CurrentSong = newSong;
        // player.synthesizer.soundFont.SetFullPath(currentSong.autoSoundfont
        //     ? currentSong.songStreaming.GetFullPath().Replace(".mid", ".sf2")
        //     : currentSong.soundfontStreaming.GetFullPath());
        // player.synthesizer.Init();

        // player.song = currentSong.songStreaming;
        // player.StartTicks = currentSong.startLoopTicks;
        // player.EndTicks = currentSong.endTicks;
        // player.Tempo = currentSong.playbackSpeedNormal;
        // player.Gain = MAX_GAIN;
        // player.Init();
        rememberedChannels = CurrentSong.mutedChannelsNormal;

        if (startPlayback) StartPlayback();
    }

    public void StopPlayback()
    {
        // player.Pause();
        // player.Seek(currentSong.startTicks);
    }

    public void SetSpectating(bool how)
    {
        rememberedChannels = how ? CurrentSong.mutedChannelsSpectating : CurrentSong.mutedChannelsNormal;
        Player.Synthesizer.SetChannelsMuted(rememberedChannels);
    }
}