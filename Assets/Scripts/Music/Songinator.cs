using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HGS.Tone;
using KaimiraGames;
using UnityEngine;

public class Songinator : MonoBehaviour
{
    private const float MAX_GAIN = 0.5f;
    
    [NonSerialized] public ToneSequencer player;
    [NonSerialized] public AudioSource source;

    [SerializeField] public bool autoStart = true;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;

    private readonly WeightedList<MIDISong> weightedList = new();

    [NonSerialized] public MIDISong currentSong;
    private int rememberedChannels;

    public void Start()
    {
        player = GetComponent<ToneSequencer>();
        source = GetComponent<AudioSource>();

        if (songs.Count == 0 || chances.Count != songs.Count) return;

        if (songs.Count > 1)
        {
            for (var i = 0; i < songs.Count; i++) weightedList.Add(songs[i], chances[i]);

            currentSong = weightedList.Next();
        }
        else
        {
            currentSong = songs[0];
        }

        player.CreateSynth(currentSong.soundfont.Bytes);
        player.Init();
        // player.StartTicks = currentSong.startLoopTicks;
        // player.EndTicks = currentSong.endTicks;
        player.Sequencer.Speed = currentSong.playbackSpeedNormal;
        player.Sequencer.StartLoopTicks = currentSong.startLoopTicks;
        // player.Gain = MAX_GAIN;
        rememberedChannels = currentSong.mutedChannelsNormal;

        if (autoStart) StartPlayback();
    }

    public void StartPlayback(bool fromBeginning = true)
    {
        player.Play(currentSong.song.Bytes);
        if (currentSong.startTicks > 0 && fromBeginning)
        {
            player.Sequencer.Seek(currentSong.startTicks);
        }
        player.Synthesizer.SetChannelsMuted(rememberedChannels);
    }

    public void SwitchToSong(MIDISong newSong, bool startPlayback = false)
    {
        player.Stop();
        // Destroy(player.synthesizer);
        // Destroy(player);
        // player = gameObject.AddComponent<SongPlayer>();
        // player.playOnStart = false;
        // player.synthesizer = gameObject.AddComponent<Synthesizer>();
        
        currentSong = newSong;
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
        rememberedChannels = currentSong.mutedChannelsNormal;

        if (startPlayback) StartPlayback();
    }

    public void StopPlayback()
    {
        // player.Pause();
        // player.Seek(currentSong.startTicks);
    }

    public void SetSpectating(bool how)
    {
        rememberedChannels = how ? currentSong.mutedChannelsSpectating : currentSong.mutedChannelsNormal;
        player.Synthesizer.SetChannelsMuted(rememberedChannels);
    }
}