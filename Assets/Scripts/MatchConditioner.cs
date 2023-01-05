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
    public Dictionary<string, string> currentMapping = new();
    public bool chainableActions = true;

    // Start is called before the first frame update
    void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.MatchRules, out currentMapping);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ChainableRules, out chainableActions);
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
        whom.CollectBigStarInstantly(matchConditioned:chainableActions);
    }

    public void ActGiveCoin(PlayerController whom)
    {
        whom.CollectCoinInstantly(matchConditioned:chainableActions);
    }
    
    public void ActRemoveStar(PlayerController whom)
    {
        whom.RemoveBigStarInstantly(matchConditioned:chainableActions);
    }

    public void ActRemoveCoin(PlayerController whom)
    {
        whom.RemoveCoinInstantly(matchConditioned:chainableActions);
    }

    public void ActGiveMega(PlayerController whom)
    {
        whom.TransformToMega(true, matchConditioned:chainableActions);
    }

    public void ActGive1Up(PlayerController whom)
    {
        whom.Give1Up();
    }

    public void ActKillPlayer(PlayerController whom)
    {
        //whom.photonView.RPC(nameof(whom.Death), RpcTarget.All, false, false);
        whom.Death(false, false, matchConditioned:chainableActions);
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
        whom.Disqualify(matchConditioned:chainableActions);
    }
    
    public void ActKnockbackPlayer(PlayerController whom)
    {
        whom.Knockback(whom.facingRight, 1, false, -1, matchConditioned:chainableActions);
    }

    public void ActHardKnockbackPlayer(PlayerController whom)
    {
        whom.Knockback(whom.facingRight, 3, false, -1, matchConditioned:chainableActions);
    }

    public void ActDoDive(PlayerController whom)
    {
        whom.DiveForward();
    }
    
    public void ActFreezePlayer(PlayerController whom) 
    {
        whom.FreezeInstantly(matchConditioned:chainableActions);
    }

    public void ActHarmPlayer(PlayerController whom)
    {
        whom.Powerdown(true, matchConditioned:chainableActions);
    }

    public void ActSpawnPowerup(PlayerController whom)
    {
        whom.SpawnCoinItemInstantly();
    }

    public void ActSpawnEnemy(PlayerController whom)
    {
        GameObject entity = Utils.GetRandomEnemy();
        PhotonNetwork.Instantiate("Prefabs/Enemy/" + entity.name,
            whom.transform.position +
            (!whom.facingRight
                ? Vector3.right
                : Vector3.left) + new Vector3(0, 0.2f, 0), Quaternion.identity);
    }

    public void ActRespawnLevel(PlayerController whom)
    {
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }
}
