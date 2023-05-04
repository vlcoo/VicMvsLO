using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MatchConditioner : MonoBehaviour
{
    public Dictionary<string, string> currentMapping = new();
    public bool chainableActions = false;

    private float timer5Sec = 5;
    private float timer10Sec = 10;
    private float timer15Sec = 15;
    private float timer30Sec = 30;
    private float timer60Sec = 60;

    // Start is called before the first frame update
    void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.MatchRules, out currentMapping);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ChainableRules, out chainableActions);
        chainableActions = !chainableActions;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.gameover || !GameManager.Instance.started) return;
        float delta = Time.fixedDeltaTime;
        
        if (timer5Sec == 0)
        {
            timer5Sec = 5;
            ConditionActioned(null, "Every5Sec");
        }
        if (timer10Sec == 0)
        {
            timer10Sec = 10;
            ConditionActioned(null, "Every10Sec");
        }
        if (timer15Sec == 0)
        {
            timer15Sec = 15;
            ConditionActioned(null, "Every15Sec");
        }
        if (timer30Sec == 0)
        {
            timer30Sec = 30;
            ConditionActioned(null, "Every30Sec");
        }
        if (timer60Sec == 0)
        {
            timer60Sec = 60;
            ConditionActioned(null, "Every60Sec");
        }
        
        Utils.TickTimer(ref timer5Sec, 0, delta);
        Utils.TickTimer(ref timer10Sec, 0, delta);
        Utils.TickTimer(ref timer15Sec, 0, delta);
        Utils.TickTimer(ref timer30Sec, 0, delta);
        Utils.TickTimer(ref timer60Sec, 0, delta);
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

    public void ActLaunchPlayer(PlayerController whom)
    {
        whom.SpinnerInstantly();
    }
    
    public void ActFreezePlayer(PlayerController whom)
    {
        if (!whom.photonView.IsMine) return;
        whom.FreezeInstantly(matchConditioned:chainableActions);
    }

    public void ActHarmPlayer(PlayerController whom)
    {
        whom.Powerdown(false, matchConditioned:chainableActions);
    }

    public void ActSpawnPowerup(PlayerController whom)
    {
        whom.SpawnCoinItemInstantly();
    }

    public void ActSpawnEnemy(PlayerController whom)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        GameObject entity = Utils.GetRandomEnemy();
        PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/" + entity.name,
            whom.transform.position +
            (!whom.facingRight
                ? Vector3.right
                : Vector3.left) + new Vector3(0, 0.2f, 0), Quaternion.identity, 0, new object[] {true});
    }

    public void ActRespawnLevel(PlayerController whom)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }

    public void ActExplodeLevel(PlayerController whom)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(GameManager.Instance.DestroyEnvironment());
    }

    public void ActRandomTeleport(PlayerController whom)
    {
        whom.RandomTeleport();
    }
}
