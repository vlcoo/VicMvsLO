using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

using Photon.Realtime;
using NSMB.Utils;
using Photon.Pun;

public class LoadingWaitingOn : MonoBehaviour
{
    public GameObject marioLoadingScene;
    public GameObject koopaLoadingScene;

    [SerializeField] private TMP_Text playerList, highPingAlert;
    [SerializeField] private string emptyText = "Loading...", iveLoadedText = "Wait...", readyToStartText = "OK!", spectatorText = "Joining as Spectator...";

    private TMP_Text text;

    public void Start() {
        text = GetComponent<TMP_Text>();
        if (PhotonNetwork.GetPing() < 180)
            highPingAlert.fontSize = 0;

        bool isBowsers = Utils.GetCharacterData().isBowsers;
        marioLoadingScene.SetActive(!isBowsers);
        koopaLoadingScene.SetActive(isBowsers);
        GetComponent<LoopingMusic>().FastMusic = Random.value >= 0.6;
    }

    public void Update() {
        if (!GameManager.Instance)
            return;
        
        if (GlobalController.Instance.joinedAsSpectator) {
            text.text = spectatorText;
            return;
        }

        if (GameManager.Instance.loaded) {
            text.text = readyToStartText;
            playerList.text = "";
            return;
        }

        if (GameManager.Instance.loadedPlayers.Count == 0) {
            text.text = emptyText;
            return;
        }

        text.text = iveLoadedText;

        HashSet<Player> waitingFor = new(GameManager.Instance.nonSpectatingPlayers);
        waitingFor.ExceptWith(GameManager.Instance.loadedPlayers);
        playerList.text = (waitingFor.Count) == 0 ? "" : "<font=\"MarioFont\"><size=22>- Waiting for -</font></size>\n" + string.Join("\n", waitingFor.Select(pl => pl.GetUniqueNickname()));
    }
}
