using Quantum;
using System;
using UnityEngine;

public class MenuSongPlayer : MonoBehaviour
{
    private int currentWorldId;
    [SerializeField] private Songinator menuPlayer, worldsPlayer;

    public void Start()
    {
        QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
    }

    private void OnDisable() {
        currentWorldId = 0;
    }

    public unsafe void OnRulesChanged(EventRulesChanged e) {
        if (!e.MapChanged) return;
        Frame f = e.Game.Frames.Predicted;
        if (QuantumUnityDB.TryGetGlobalAsset(f.Global->Rules.Stage, out Map map)
            && QuantumUnityDB.TryGetGlobalAsset(map.UserAsset, out VersusStageData stage)) {
            OnLevelSelected(stage.WorldIndex);
        }
    }
    
    public void OnGameDestroyed(CallbackGameDestroyed e)
    {
        OnLevelSelected(0);
    }

    public void OnLevelSelected(int worldId)
    {
        if (worldsPlayer is null) return;

        if (worldId == 0) {
            if (worldsPlayer.state != Songinator.PlaybackState.STOPPED)
                worldsPlayer.SetPlaybackState(Songinator.PlaybackState.STOPPED, 0.5f);
            if (menuPlayer.state == Songinator.PlaybackState.PAUSED)
                menuPlayer.SetPlaybackState(Songinator.PlaybackState.PLAYING, 0.5f);
        }

        if (worldId > 0)
        {
            if (currentWorldId != worldId)
                worldsPlayer.SwitchToSong(worldId - 1, true, 0.5f);
            if (menuPlayer.state == Songinator.PlaybackState.PLAYING)
                menuPlayer.SetPlaybackState(Songinator.PlaybackState.PAUSED, 0.5f);
        }
        currentWorldId = worldId;
    }

    public void Stop()
    {
        menuPlayer.SetPlaybackState(Songinator.PlaybackState.STOPPED);
        worldsPlayer.SetPlaybackState(Songinator.PlaybackState.STOPPED);
    }
}