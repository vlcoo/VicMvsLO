using System.Collections;
using System.Collections.Generic;
using NSMB.Utils;
using Photon.Realtime;
using UnityEngine;

public class TeamGrouper : MonoBehaviour
{
    public Dictionary<string, List<PlayerController>> teams = new();
    public bool isTeamsMatch;

    public void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Teams, out isTeamsMatch);
        if (!isTeamsMatch) return;
        
        foreach (PlayerData character in GlobalController.Instance.characters)
        {
            teams.Add(character.prefab, new List<PlayerController>());
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}