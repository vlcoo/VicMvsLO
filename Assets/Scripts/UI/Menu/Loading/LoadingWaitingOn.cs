using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using TMPro;

using Photon.Realtime;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LoadingWaitingOn : MonoBehaviour
{
    public GameObject marioLoadingScene;
    public GameObject koopaLoadingScene;
    public Songinator MusicSynth;
    public Songinator MusicSynthIdle;
    public Coroutine waitingCoroutine;

    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text playerList, highPingAlert, waitingTimer;
    [SerializeField] private string emptyText = "Loading...", iveLoadedText = "Wait...", readyToStartText = "OK!", spectatorText = "Joining as Spectator...";
    [SerializeField] private int waitingTime, waitingLastTime;
    private float waitingLastTimer = 0;

    public void Start() {
        if (PhotonNetwork.GetPing() < 180)
            highPingAlert.fontSize = 0;

        bool isBowsers = Utils.GetCharacterData().isBowsers;
        marioLoadingScene.SetActive(!isBowsers);
        koopaLoadingScene.SetActive(isBowsers);

        waitingCoroutine = StartCoroutine(WaitForEveryone());
    }

    public void Update() {
        if (waitingLastTimer > 0)
        {
            Utils.TickTimer(ref waitingLastTimer, 0, Time.deltaTime);
            waitingTimer.text = (int)waitingLastTimer + "s...";
        }
        
        if (!GameManager.Instance)
            return;
        
        if (GlobalController.Instance.joinedAsSpectator) {
            infoText.text = spectatorText;
            return;
        }

        if (GameManager.Instance.loaded) {
            infoText.text = readyToStartText;
            playerList.text = "";
            return;
        }

        if (GameManager.Instance.loadedPlayers.Count == 0) {
            infoText.text = emptyText;
            return;
        }

        infoText.text = iveLoadedText;

        HashSet<Player> waitingFor = new(GameManager.Instance.nonSpectatingPlayers);
        waitingFor.ExceptWith(GameManager.Instance.loadedPlayers);
        playerList.text = (waitingFor.Count) == 0 ? "" : "- Waiting for -\n" + string.Join("\n", waitingFor.Select(pl => pl.GetUniqueNickname()));
    }

    IEnumerator WaitForEveryone()
    {
        yield return new WaitForSeconds(waitingTime);
        yield return DOTween.To(() => MusicSynth.player.Gain, v => MusicSynth.player.Gain = v, 0, 1f).WaitForCompletion();
        MusicSynthIdle.StartPlayback();
        waitingLastTimer = waitingLastTime;
        yield return new WaitForSeconds(waitingLastTime);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new() { [Enums.NetRoomProperties.GameStarted] = false });
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.DestroyAll();
        SceneManager.LoadScene("MainMenu");
    }

    public void StopLoading(bool spectating)
    {
        StopCoroutine(waitingCoroutine);
        GetComponent<Animator>().SetTrigger(spectating ? "spectating" : "loaded");
        DOTween.To(() => MusicSynth.player.Gain, v => MusicSynth.player.Gain = v, 0f,  2f).SetEase(Ease.Linear);
    }
}
