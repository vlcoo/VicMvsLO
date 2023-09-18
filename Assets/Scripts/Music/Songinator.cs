using System;
using System.Collections;
using System.Collections.Generic;
using FluidMidi;
using KaimiraGames;
using UnityEngine;

public class Songinator : MonoBehaviour
{
    [SerializeField] public SongPlayer player;
    [SerializeField] public List<MIDISong> songs;
    [SerializeField] public List<int> chances;
    private MIDISong currentSong;
    private readonly WeightedList<MIDISong> weightedList = new();

    public void Start()
    {
        for (int i = 0; i < songs.Count; i++)
        {
            weightedList.Add(songs[i], chances[i]);
        }

        currentSong = weightedList.Next();
        player.synthesizer.soundFont.SetFullPath(currentSong.autoSoundfont
            ? currentSong.song.GetFullPath().Replace(".mid", ".sf2")
            : currentSong.soundfont.GetFullPath());
        player.synthesizer.Init();
        
        player.song = currentSong.song;
        player.StartTicks = currentSong.startTicks;
        player.EndTicks = currentSong.endTicks;
        player.Tempo = currentSong.playbackSpeedNormal;
        player.Init();

        StartCoroutine(StartSkip());
    }
    
    IEnumerator StartSkip()
    {
        player.Channels = 0;
        player.Play();
        yield return new WaitForSeconds(0.2f);  // wowie...
        player.Seek(currentSong.startTicks);
        player.Channels = ~currentSong.mutedChannelsNormal;
    }
}
