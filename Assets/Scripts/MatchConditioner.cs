using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MatchConditioner : MonoBehaviour
{
    public Dictionary<string, string> currentMapping = new Dictionary<string, string>();
    public bool chainableActions = true;

    // Start is called before the first frame update
    void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.MatchRules, out currentMapping);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConditionActioned(int byWhomsID, string condition)
    {
        var player = PhotonView.Find(byWhomsID);
        if (player is null) return;
        ConditionActioned(player.GetComponent<PlayerController>(), condition);
    }

    public void ConditionActioned(PlayerController byWhom, string condition)
    {
        if (ReferenceEquals(currentMapping, null) || !currentMapping.ContainsKey(condition)) return;
        MethodInfo actionMethod = GetType().GetMethod(currentMapping[condition]);
        if (actionMethod == null) return;
        if (byWhom is null)
            DoToEveryone(actionMethod);
        else
            actionMethod.Invoke(this, new[] { byWhom });
    }

    public void DoToEveryone(MethodInfo actionFunc)
    {
        foreach (var player in GameManager.Instance.players)
            actionFunc.Invoke(this, new[] { player });
    }

    public void ActGiveStar(PlayerController whom)
    {
        whom.CollectBigStarInstantly();
    }

    public void ActGiveCoin(PlayerController whom)
    {
        whom.CollectCoinInstantly();
    }
    
    public void ActRemoveStar(PlayerController whom)
    {
        whom.RemoveBigStarInstantly();
    }

    public void ActRemoveCoin(PlayerController whom)
    {
        whom.RemoveCoinInstantly();
    }

    public void ActGiveMega(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.TransformToMega), RpcTarget.All, true);
    }

    public void ActKillPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Death), RpcTarget.All, false, false);
    }

    public void ActWinPlayer(PlayerController whom)
    {
        PhotonNetwork.RaiseEvent((byte) Enums.NetEventIds.EndGame, whom.photonView.Owner, NetworkUtils.EventAll, SendOptions.SendReliable);
    }

    public void ActDraw(PlayerController whom)
    {
        PhotonNetwork.RaiseEvent((byte) Enums.NetEventIds.EndGame, null, NetworkUtils.EventAll, SendOptions.SendReliable);
    }

    public void ActDisqualifyPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Disqualify), RpcTarget.All);
    }
    
    public void ActKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Knockback), RpcTarget.All, whom.facingRight, 1, false, -1);
    }

    public void ActHardKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Knockback), RpcTarget.All, whom.facingRight, 3, false, -1);
    }

    public void ActDoDive(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.DiveForward), RpcTarget.All);
    }
    
    public void ActFreezePlayer(PlayerController whom) 
    {
        whom.FreezeInstantly();
    }

    public void ActHarmPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Powerdown), RpcTarget.All, true);
    }

    public void ActRespawnLevel(PlayerController whom)
    {
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }
}
