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
        if (byWhomsID < 0) return;
        PlayerController player = PhotonView.Find(byWhomsID).GetComponent<PlayerController>();
        ConditionActioned(player, condition);
    }

    public void ConditionActioned(PlayerController byWhom, string condition)
    {
        if (ReferenceEquals(currentMapping, null) || !currentMapping.ContainsKey(condition)) return;
        MethodInfo actionMethod = GetType().GetMethod(currentMapping[condition]);
        if (actionMethod == null) return;
        actionMethod.Invoke(this, new []{byWhom});
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

    public void ActRespawnLevel(PlayerController whom)
    {
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }

    public void ActKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Knockback), RpcTarget.All, whom.facingRight, 1, false, -1);
    }

    public void ActHardKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Knockback), RpcTarget.All, false, 3, false, -1);
    }

    public void ActHarmPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.Powerdown), RpcTarget.All, true);
    }

    public void ActGiveStar(PlayerController whom)
    {
        whom.photonView.RPC(nameof(whom.CollectBigStarInstantly), RpcTarget.AllViaServer);
    }
}
