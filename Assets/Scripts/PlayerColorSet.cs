using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerColor", menuName = "ScriptableObjects/PlayerColorSet")]
public class PlayerColorSet : ScriptableObject
{
    public PlayerColors[] colors = { new() };

    public PlayerColors GetPlayerColors(PlayerData player)
    {
        return colors.FirstOrDefault(skin => skin.player.Equals(player)) ?? colors[0];
    }
}

[Serializable]
public class PlayerColors
{
    public PlayerData player;
    public Color hatColor = Color.white, overallsColor = Color.white;
}