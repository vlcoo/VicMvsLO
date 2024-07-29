using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluidMidi;
using HGS.Tone;
using KaimiraGames;
using UnityEngine;

public class Songinator : MonoBehaviour
{
    private const float MAX_GAIN = 0.5f;
    
    [NonSerialized] public ToneSequencer player;

    [SerializeField] public bool autoStart = true;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;

    private readonly WeightedList<MIDISong> weightedList = new();

    [NonSerialized] public MIDISong currentSong;
    private int rememberedChannels;

    public void Start()
    {
        player = GetComponent<ToneSequencer>();

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

        // player.soundFont.SetFullPath(currentSong.autoSoundfont
        //     ? currentSong.songStreaming.GetFullPath().Replace(".mid", ".sf2")
        //     : currentSong.soundfontStreaming.GetFullPath());
        // var hgsSoundfont = ScriptableObject.CreateInstance<ToneSoundFont>();
        // hgsSoundfont.Load(currentSong.soundfont.Stream);
        // player.soundFont = hgsSoundfont;
        // player.CreateSynth();
        player.CreateSynth(currentSong.soundfont.Bytes);
        player.Init();
        if (autoStart) player.Play(currentSong.song.Bytes);

        // player.song = currentSong.song;
        // player.StartTicks = currentSong.startLoopTicks;
        // player.EndTicks = currentSong.endTicks;
        // player.Tempo = currentSong.playbackSpeedNormal;
        // player.Gain = MAX_GAIN;
        // player.Init();
        rememberedChannels = ~currentSong.mutedChannelsNormal;

        if (autoStart) StartPlayback();

        // player.SetTickEvent(tick => OnTick(tick));
    }

    private IEnumerator StartSkip()
    {
        // player.Channels = 0;
        // player.Play();
        yield return new WaitForSeconds(0.2f); // wowie...
        // player.Seek(currentSong.startTicks);
        // player.Channels = rememberedChannels;
    }

    public void StartPlayback(bool fromBeginning = true)
    {
        if (currentSong.startTicks > 0 && fromBeginning)
        {
            StartCoroutine(StartSkip());
        }
        else
        {
            // if (fromBeginning) player.Seek(0);
            // player.Channels = rememberedChannels;
            // player.Play();
            player.Play(currentSong.song.Bytes);
        }
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
        rememberedChannels = ~currentSong.mutedChannelsNormal;

        if (startPlayback) StartPlayback();
    }

    public void StopPlayback()
    {
        // player.Pause();
        // player.Seek(currentSong.startTicks);
    }

    public void SetSpectating(bool how)
    {
        rememberedChannels = how ? ~currentSong.mutedChannelsSpectating : ~currentSong.mutedChannelsNormal;
        // player.Channels = rememberedChannels;
    }
}