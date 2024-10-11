using UnityEngine;

public class MenuWorldSongPlayer : MonoBehaviour
{
    public int[] levelWorldIds;
    private int currentWorldId;

    private Songinator worldsSonginator;

    public void Start()
    {
        worldsSonginator = GetComponent<Songinator>();
    }

    public void OnLevelSelected(int levelId)
    {
        if (worldsSonginator is null) return;

        var worldId = levelWorldIds[levelId];

        if (worldId == 0)
            if (worldsSonginator.state != Songinator.PlaybackState.STOPPED)
                worldsSonginator.SetPlaybackState(Songinator.PlaybackState.STOPPED, 0.5f);

        if (worldId > 0)
        {
            if (currentWorldId != worldId)
                worldsSonginator.SwitchToSong(worldId - 1, true, 0.5f);
            currentWorldId = worldId;
        }
    }

    public void Stop()
    {
        worldsSonginator.SetPlaybackState(Songinator.PlaybackState.STOPPED);
    }
}