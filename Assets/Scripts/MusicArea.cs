using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicArea : MonoBehaviour
{
    public Songinator musicSynth;
    public MIDISong switchToSong;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (musicSynth.CurrentSong == switchToSong) return;
        var playerController = other.GetComponent<PlayerController>();
        if (!playerController.photonView.IsMineOrLocal()) return;
        
        musicSynth.SwitchToSong(switchToSong,
            playerController.spawned && !playerController.dead && !playerController.pipeEntering &&
            GameManager.Instance.musicState == Enums.MusicState.Normal);
    }
}
