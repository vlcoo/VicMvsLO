﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Realtime;
using NSMB.Utils;
using ExitGames.Client.Photon;

public class RoomIcon : MonoBehaviour {

    [SerializeField] private Color defaultColor, highlightColor, selectedColor;
    [SerializeField] private TMP_Text playersText, nameText, inProgressText, symbolsText;

    public RoomInfo room;
    public bool joinPrivate;

    private Image icon;

    public void Start() {
        icon = GetComponent<Image>();
    }

    public void UpdateUI(RoomInfo newRoom) {
        if (joinPrivate)
            return;

        room = newRoom;
        Hashtable prop = room.CustomProperties;

        nameText.text = $"{((string) prop[Enums.NetRoomProperties.HostName]).ToValidUsername()}'s Lobby";
        playersText.text = $"{room.PlayerCount}/{room.MaxPlayers} players";
        inProgressText.text = (bool) prop[Enums.NetRoomProperties.GameStarted] ? "In Progress" : "Not Started";

        string symbols = "";
        Utils.GetCustomProperty(Enums.NetRoomProperties.StarRequirement, out int stars, newRoom.CustomProperties);
        Utils.GetCustomProperty(Enums.NetRoomProperties.CoinRequirement, out int coins, newRoom.CustomProperties);
        Utils.GetCustomProperty(Enums.NetRoomProperties.Lives, out int lives, newRoom.CustomProperties);
        bool powerups = (bool) prop[Enums.NetRoomProperties.NewPowerups];
        bool time = ((int) prop[Enums.NetRoomProperties.Time]) >= 1;
        //bool password = ((string) prop[Enums.NetRoomProperties.Password]) != "";

        if (powerups)
            symbols += "<sprite=8>";
        if (time)
            symbols += "<sprite=6>";
        if (lives >= 1)
            symbols += "<sprite=9>" + Utils.GetSymbolString(lives.ToString(), Utils.smallSymbols);
        if (stars >= 1)
            symbols += "<sprite=38>" + Utils.GetSymbolString(stars.ToString(), Utils.smallSymbols);
        if (coins >= 1)
            symbols += "<sprite=37>" + Utils.GetSymbolString(coins.ToString(), Utils.smallSymbols);
        //if (password)
        //    symbols += "<sprite=7>";

        symbolsText.text = symbols;
    }

     public void Select() {
    }
    
    public void Unselect() {
    }
    
    public void Hover() {
    }
    
    public void Unhover() {
        
    }
}
