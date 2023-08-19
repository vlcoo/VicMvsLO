using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class TeamGrouper : MonoBehaviour
{
    public Dictionary<string, List<PlayerController>> teams = new();

    public void Start()
    {
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