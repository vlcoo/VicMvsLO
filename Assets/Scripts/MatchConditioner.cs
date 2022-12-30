using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MatchConditioner : MonoBehaviour
{
    public string[] POSSIBLE_CONDITIONS = new[]
    {
        "Spawned", "GotCoin", "GotPowerup", "LostPowerup", "GotStar", "KnockedBack", "Stomped",
        "Died", "Jumped", "LookedRight", "LookedLeft", "LookedUp", "LookedDown", "Ran"
    };

    public List<string> POSSIBLE_ACTIONS = new List<string>();
    public Dictionary<string, Action<PlayerController>> currentMapping = new Dictionary<string, Action<PlayerController>>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var method in GetType().GetMethods())
            if (method.Name.StartsWith("Action"))
                POSSIBLE_ACTIONS.Add(method.Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConditionActioned(int byWhomsID, string condition)
    {
        if (byWhomsID < 0) return;
        PlayerController player = PhotonView.Find(byWhomsID).GetComponent<PlayerController>();
        ConditionActioned(player, condition);
    }

    public void ConditionActioned(PlayerController byWhom, string condition)
    {
        Action<PlayerController> action = currentMapping.GetValueOrDefault(condition, null);
        if (action == null) return;
        
        action.Invoke(byWhom);
        /*switch (condition)
        {
            case "Spawned":
                break;
            
            case "GotCoin":
                ActionKnockbackPlayer(byWhom);
                break;
            
            case "GotPowerup":
                break;
            
            case "LostPowerup":
                break;
            
            case "GotStar":
                break;

            case "KnockedBack":
                break;

            case "Stomped":
                break;
            
            case "Died":
                ActionDraw();
                break;

            case "Jumped":
                break;

            case "LookedRight":
                break;
            
            case "LookedLeft":
                break;

            case "LookedDown":
                break;

            case "LookedUp":
                break;

            case "Ran":
                break;
        }*/
    }

    public void ActionKillPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Death), RpcTarget.All, false, false);
    }

    public void ActionWinPlayer(PlayerController whom)
    {
        PhotonNetwork.RaiseEvent((byte) Enums.NetEventIds.EndGame, whom.photonView.Owner, NetworkUtils.EventAll, SendOptions.SendReliable);
    }

    public void ActionDraw(PlayerController whom)
    {
        PhotonNetwork.RaiseEvent((byte) Enums.NetEventIds.EndGame, null, NetworkUtils.EventAll, SendOptions.SendReliable);
    }

    public void ActionDisqualifyPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Disqualify), RpcTarget.All);
    }

    public void ActionRespawnLevel(PlayerController whom)
    {
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }

    public void ActionKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Knockback), RpcTarget.All, whom.facingRight, 1, false, -1);
    }

    public void ActionHardKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Knockback), RpcTarget.All, false, 3, false, -1);
    }

    public void ActionHarmPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Powerdown), RpcTarget.All, true);
    }

    public void ActionGiveStar(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.CollectBigStarInstantly), RpcTarget.AllViaServer);
    }
}
