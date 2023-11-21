using System;
using System.Collections;
using System.Collections.Generic;
using FluidMidi;
using FluidSynth;
using KaimiraGames;
using UnityEditor;
using UnityEngine;

public class Songinator : MonoBehaviour
{
    [SerializeField] public bool autoStart = true;
    [SerializeField] public SongPlayer player;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;
    
    [NonSerialized] public MIDISong currentSong;
    
    private readonly WeightedList<MIDISong> weightedList = new();
    private int rememberedChannels;

    public void Start()
    {
        if (songs.Count == 0 || chances.Count != songs.Count) return;
        
        if (songs.Count > 1)
        {
            for (int i = 0; i < songs.Count; i++)
            {
                weightedList.Add(songs[i], chances[i]);
            }

            currentSong = weightedList.Next();
        }
        else currentSong = songs[0];

        player.synthesizer.soundFont.SetFullPath(currentSong.autoSoundfont
            ? currentSong.song.GetFullPath().Replace(".mid", ".sf2")
            : currentSong.soundfont.GetFullPath());
        player.synthesizer.Init();
        
        player.song = currentSong.song;
        player.StartTicks = currentSong.startLoopTicks;
        player.EndTicks = currentSong.endTicks;
        player.Tempo = currentSong.playbackSpeedNormal;
        player.Init();
        rememberedChannels = ~currentSong.mutedChannelsNormal;

        if (autoStart) StartPlayback();
        
        // player.SetTickEvent(tick => OnTick(tick));
    }

    private IEnumerator StartSkip()
    {
        player.Channels = 0;
        player.Play();
        yield return new WaitForSeconds(0.2f);  // wowie...
        player.Seek(currentSong.startTicks);
        player.Channels = rememberedChannels;
    }

    public void StartPlayback(bool fromBeginning = true)
    {
        if (currentSong.startTicks > 0 && fromBeginning) StartCoroutine(StartSkip());
        else
        {
            if (fromBeginning) player.Seek(0);
            player.Channels = rememberedChannels;
            player.Play();
        }
    }

    public void StopPlayback()
    {
        player.Pause();
        player.Seek(currentSong.startTicks);
    }

    public void SetSpectating(bool how)
    {
        rememberedChannels = how ? ~currentSong.mutedChannelsSpectating : ~currentSong.mutedChannelsNormal;
        player.Channels = rememberedChannels;
    }
}
