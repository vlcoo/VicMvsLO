using NSMB.Utils;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomIcon : MonoBehaviour
{
    [SerializeField] private Color defaultColor, highlightColor, selectedColor;
    [SerializeField] private TMP_Text playersText, nameText, inProgressText, symbolsText;
    public bool joinPrivate;

    private Image icon;

    public RoomInfo room;

    public void Start()
    {
        icon = GetComponent<Image>();
        Unselect();
    }

    public void UpdateUI(RoomInfo newRoom)
    {
        if (joinPrivate)
            return;

        room = newRoom;
        var prop = room.CustomProperties;

        nameText.text = $"{((string)prop[Enums.NetRoomProperties.HostName]).ToValidUsername()}'s Lobby";
        playersText.text = $"{room.PlayerCount}/{room.MaxPlayers} players";
        inProgressText.text = (bool)prop[Enums.NetRoomProperties.GameStarted] ? "In Progress" : "Not Started";

        var symbols = "";
        Utils.GetCustomProperty(Enums.NetRoomProperties.StarRequirement, out int stars, newRoom.CustomProperties);
        Utils.GetCustomProperty(Enums.NetRoomProperties.CoinRequirement, out int coins, newRoom.CustomProperties);
        Utils.GetCustomProperty(Enums.NetRoomProperties.Lives, out int lives, newRoom.CustomProperties);
        Utils.GetCustomProperty(Enums.NetRoomProperties.Teams, out bool teams, newRoom.CustomProperties);
        Utils.GetCustomProperty(Enums.NetRoomProperties.MatchRules, out string matchRules, newRoom.CustomProperties);
        var powerups = (bool)prop[Enums.NetRoomProperties.NewPowerups];
        var time = (int)prop[Enums.NetRoomProperties.Time] >= 1;
        //bool password = ((string) prop[Enums.NetRoomProperties.Password]) != "";

        if (!string.IsNullOrEmpty(matchRules.Trim()))
            symbols += "<sprite=56>" +
                       Utils.GetSymbolString(matchRules.Split("},{").Length.ToString(), Utils.smallSymbols);
        if (teams) symbols += "<sprite=76>";
        if (powerups) symbols += "<sprite=8>";
        if (time) symbols += "<sprite=6>";
        if (lives >= 1) symbols += "<sprite=9>" + Utils.GetSymbolString(lives.ToString(), Utils.smallSymbols);
        if (stars >= 1) symbols += "<sprite=38>" + Utils.GetSymbolString(stars.ToString(), Utils.smallSymbols);
        if (coins >= 1) symbols += "<sprite=37>" + Utils.GetSymbolString(coins.ToString(), Utils.smallSymbols);
        //if (password)
        //    symbols += "<sprite=7>";

        symbolsText.text = symbols;
    }

    public void Select()
    {
        icon.color = selectedColor;
    }

    public void Unselect()
    {
        icon.color = defaultColor;
    }

    public void Hover()
    {
        icon.color = highlightColor;
    }

    public void Unhover()
    {
        if (MainMenuManager.Instance.selectedRoomIcon == this)
            Select();
        else
            Unselect();
    }
}