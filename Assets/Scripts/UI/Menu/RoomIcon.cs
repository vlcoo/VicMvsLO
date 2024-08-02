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
        playersText.text = $"{room.PlayerCount}/{room.MaxPlayers} - " + ((bool)prop[Enums.NetRoomProperties.GameStarted] ? "Ongoing..." : "Not Started");
        // inProgressText.text = (bool)prop[Enums.NetRoomProperties.GameStarted] ? "In Progress" : "Not Started";

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
            symbols += "<sprite name=\"room_rules\">" +
                       Utils.GetNumberString(matchRules.Split("},{").Length.ToString(), "room_smallnumber");
        if (teams) symbols += "<sprite name=\"room_teams\">";
        if (powerups) symbols += "<sprite name=\"room_powerups\">";
        if (time) symbols += "<sprite name=\"room_timer\">";
        if (lives >= 1) symbols += "<sprite name=\"room_lives\">" + Utils.GetNumberString(lives.ToString(), "room_smallnumber");
        if (stars >= 1) symbols += "<sprite name=\"room_stars\">" + Utils.GetNumberString(stars.ToString(), "room_smallnumber");
        if (coins >= 1) symbols += "<sprite name=\"room_coins\">" + Utils.GetNumberString(coins.ToString(), "room_smallnumber");
        //if (password)
        //    symbols += "<sprite name=\"room_privae\">";

        symbolsText.text = symbols;
    }

    public void Select()
    {
        icon.color = selectedColor;
        playersText.color = Color.black;
    }

    public void Unselect()
    {
        icon.color = defaultColor;
        playersText.color = new Color(0.75f, 0.75f, 0.75f);
    }

    public void Hover()
    {
        icon.color = highlightColor;
        playersText.color = Color.black;
    }

    public void Unhover()
    {
        if (MainMenuManager.Instance.selectedRoomIcon == this)
            Select();
        else
            Unselect();
    }
}