using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TeamGrouper : MonoBehaviour
{
    public Dictionary<string, List<PlayerController>> teams = new();
    public bool isTeamsMatch;
    public bool friendlyFire;
    public bool shareStars;

    public void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Teams, out isTeamsMatch);
        Utils.GetCustomProperty(Enums.NetRoomProperties.FriendlyFire, out friendlyFire);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareStars, out shareStars);
        
        foreach (PlayerData character in GlobalController.Instance.characters)
        {
            teams.Add(character.prefab, new List<PlayerController>());
        }
    }

    public void PlayerGotStar(PlayerController whom)
    {
        if (!PhotonNetwork.IsMasterClient || !isTeamsMatch) return;
        if (!shareStars) return;
        
        foreach (PlayerController teammate in teams[whom.character.prefab].Where(controller => !controller.Equals(whom)))
        {
            teammate.CollectBigStarInstantly(-2);
        }
    }
    
    public void PlayerLostStar(PlayerController whom)
    {
        if (!PhotonNetwork.IsMasterClient || !isTeamsMatch) return;
        if (!shareStars) return;
        
        foreach (PlayerController teammate in teams[whom.character.prefab].Where(controller => !controller.Equals(whom)))
        {
            teammate.RemoveBigStarInstantly(-2);
        }
    }

    public bool IsPlayerTeammate(PlayerController whom, PlayerController opponent, bool unconditionalCheck)
    {
        if (!isTeamsMatch || (!unconditionalCheck && !friendlyFire)) return false;
        return whom.character.prefab.Equals(opponent.character.prefab);
    }

    public bool IsPlayerTeammate(PlayerController whom, int opponentID, bool unconditionalCheck)
    {
        if (opponentID < 0) return false;
        if (!isTeamsMatch || (!unconditionalCheck && !friendlyFire)) return false;
        return teams[whom.character.prefab].Any(controller => controller.photonView.ViewID == opponentID);
    }

    public bool IsPlayerTeammate(PlayerController whom, string opponentPrefab)
    {
        if (!isTeamsMatch) return false;
        return whom.character.prefab.Equals(opponentPrefab);
    }
}