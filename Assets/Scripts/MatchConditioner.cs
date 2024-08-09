using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class MatchConditioner : MonoBehaviour
{
    public HashSet<MatchRuleDataEntry> ruleList;
    private float timer10Sec = 10;
    private float timer15Sec = 15;
    private float timer30Sec = 30;

    private float timer5Sec = 5;
    private float timer60Sec = 60;

    public int count => ruleList?.Count ?? 0;

    // Start is called before the first frame update
    private void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.MatchRules, out string j);
        ruleList = JsonConvert.DeserializeObject<HashSet<MatchRuleDataEntry>>(j);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (GameManager.Instance.gameover || !GameManager.Instance.started) return;
        var delta = Time.fixedDeltaTime;

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

    public void ConditionActioned(PlayerController byWhom, string condition, bool ignoreMasterCheck = false)
    {
        if (!ignoreMasterCheck && !PhotonNetwork.IsMasterClient) return;
        if (ReferenceEquals(ruleList, null)) return;

        foreach (var rule in ruleList.Where(r => r.Condition.Equals(condition)))
        {
            var actionMethod = GetType().GetMethod(rule.Action);
            if (actionMethod == null) return;
            if (byWhom is null)
                foreach (var player in GameManager.Instance.players)
                    actionMethod.Invoke(this, new[] { player });
            else
                actionMethod.Invoke(this, new[] { byWhom });
        }
    }

    public void ActGiveStar(PlayerController whom)
    {
        // whom.photonView.RPC(nameof(PlayerController.CollectBigStarInstantly), RpcTarget.All, chainableActions);
        whom.CollectBigStarInstantly();
    }

    public void ActGiveCoin(PlayerController whom)
    {
        // whom.photonView.RPC(nameof(PlayerController.CollectCoinInstantly), RpcTarget.All, chainableActions);
        whom.CollectCoinInstantly();
    }

    public void ActRemoveStar(PlayerController whom)
    {
        // whom.photonView.RPC(nameof(PlayerController.RemoveBigStarInstantly), RpcTarget.All, chainableActions);
        whom.RemoveBigStarInstantly();
    }

    public void ActRemoveCoin(PlayerController whom)
    {
        // whom.photonView.RPC(nameof(PlayerController.RemoveCoinInstantly), RpcTarget.All, chainableActions);
        whom.RemoveCoinInstantly();
    }

    public void ActGiveMega(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.TransformToMega), RpcTarget.All, true);
    }

    public void ActGive1Up(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.Give1Up), RpcTarget.All);
    }

    public void ActKillPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.Death), RpcTarget.All, false, false);
    }

    public void ActWinPlayer(PlayerController whom)
    {
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, whom.photonView.Owner, NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void ActDraw(PlayerController whom)
    {
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, null, NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void ActDisqualifyPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.Disqualify), RpcTarget.All);
    }

    public void ActKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.Knockback), RpcTarget.All, whom.facingRight, 1, false, -1);
    }

    public void ActHardKnockbackPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.Knockback), RpcTarget.All, whom.facingRight, 3, false, -1);
    }

    public void ActDoDive(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.DiveForward), RpcTarget.All);
    }

    public void ActLaunchPlayer(PlayerController whom)
    {
        whom.SpinnerInstantly();
    }

    public void ActFreezePlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.FreezeInstantly), RpcTarget.All);
    }

    public void ActHarmPlayer(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.Powerdown), RpcTarget.All, false);
    }

    public void ActSpawnPowerup(PlayerController whom)
    {
        whom.photonView.RPC(nameof(PlayerController.SpawnCoinItemInstantly), RpcTarget.All);
    }

    public void ActSpawnEnemy(PlayerController whom)
    {
        var entity = Utils.GetRandomEnemy();
        PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/" + entity.name,
            whom.transform.position +
            (!whom.facingRight
                ? Vector3.right
                : Vector3.left) + new Vector3(0, 0.2f, 0), Quaternion.identity, 0, new object[] { true });
    }

    public void ActRespawnLevel(PlayerController whom)
    {
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }

    public void ActExplodeLevel(PlayerController whom)
    {
        StartCoroutine(GameManager.Instance.DestroyEnvironment());
    }

    public void ActRandomTeleport(PlayerController whom)
    {
        whom.RandomTeleport();
    }

    public void ActRemoveReserve(PlayerController whom)
    {
        whom.storedPowerup = null;
    }
}