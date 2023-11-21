// using System.Collections;

using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerListHandler : MonoBehaviour, IInRoomCallbacks
{
    [SerializeField] private GameObject contentPane, template;
    private readonly Dictionary<string, PlayerListEntry> playerListEntries = new();

    //Unity start
    public void Start()
    {
        if (PhotonNetwork.InRoom)
            PopulatePlayerEntries(false);
    }

    //Register callbacks
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (PhotonNetwork.InRoom)
            PopulatePlayerEntries(true);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        RemoveAllPlayerEntries();
    }

    //Room callbacks
    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateAllPlayerEntries();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!newPlayer.IsLocal)
        {
            AddPlayerEntry(newPlayer);
            UpdateAllPlayerEntries();
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!otherPlayer.IsLocal)
        {
            RemovePlayerEntry(otherPlayer);
            UpdateAllPlayerEntries();
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(Enums.NetPlayerProperties.Spectator))
            UpdateAllPlayerEntries();
        else
            UpdatePlayerEntry(targetPlayer);
    }

    public void PopulatePlayerEntries(bool addSelf)
    {
        RemoveAllPlayerEntries();
        var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        if (addSelf)
            players.Add(PhotonNetwork.LocalPlayer);

        players.ForEach(AddPlayerEntry);
    }

    public void AddPlayerEntry(Player player)
    {
        var id = player.UserId;
        if (!playerListEntries.ContainsKey(id))
        {
            var go = Instantiate(template, contentPane.transform);
            go.name = $"{player.NickName} ({player.UserId})";
            go.SetActive(true);
            playerListEntries[id] = go.GetComponent<PlayerListEntry>();
            playerListEntries[id].player = player;
        }

        UpdatePlayerEntry(player);
    }

    public void RemoveAllPlayerEntries()
    {
        playerListEntries.Values.ToList().ForEach(entry => Destroy(entry.gameObject));
        playerListEntries.Clear();
    }

    public void RemovePlayerEntry(Player player)
    {
        var id = player.UserId;
        if (!playerListEntries.ContainsKey(id))
            return;

        Destroy(playerListEntries[id].gameObject);
        playerListEntries.Remove(id);
    }

    public void UpdateAllPlayerEntries()
    {
        PhotonNetwork.CurrentRoom.Players.Values.ToList().ForEach(UpdatePlayerEntry);
    }

    public void UpdatePlayerEntry(Player player)
    {
        var id = player.UserId;
        if (!playerListEntries.ContainsKey(id))
        {
            AddPlayerEntry(player);
            return;
        }

        playerListEntries[id].UpdateText();
        ReorderEntries();
    }

    public void ReorderEntries()
    {
        foreach (var players in PhotonNetwork.PlayerList.Reverse())
        {
            var id = players.UserId;
            if (!playerListEntries.ContainsKey(id))
                continue;

            playerListEntries[id].transform.SetAsFirstSibling();
        }
    }
}