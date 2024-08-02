using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingWaitingOn : MonoBehaviour
{
    public GameObject marioLoadingScene;
    public GameObject koopaLoadingScene;
    public Songinator MusicSynth;
    public Songinator MusicSynthIdle;

    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text playerList, highPingAlert, waitingTimer;

    [SerializeField] private string emptyText = "Loading...",
        iveLoadedText = "Wait...",
        readyToStartText = "OK!",
        spectatorText = "Joining as Spectator...";

    [SerializeField] private int waitingTime, waitingLastTime;
    public Coroutine waitingCoroutine;
    private float waitingLastTimer = -1;
    private bool timedOut = false;

    public void Start()
    {
        if (PhotonNetwork.GetPing() < 180)
            highPingAlert.fontSize = 0;

        var isBowsers = Utils.GetCharacterData().isBowsers;
        marioLoadingScene.SetActive(!isBowsers);
        koopaLoadingScene.SetActive(isBowsers);

        waitingCoroutine = StartCoroutine(WaitForEveryone());
    }

    public void Update()
    {
        if (timedOut)
        {
            infoText.text = "Timed out!";
            return;
        }

        if (waitingLastTimer > 0)
        {
            Utils.TickTimer(ref waitingLastTimer, 0, Time.deltaTime);
            waitingTimer.text = (int)waitingLastTimer + "s...";
        }

        if (!GameManager.Instance)
            return;

        if (GlobalController.Instance.joinedAsSpectator)
        {
            infoText.text = spectatorText;
            return;
        }

        if (GameManager.Instance.loaded)
        {
            infoText.text = readyToStartText;
            playerList.text = "";
            return;
        }

        if (GameManager.Instance.loadedPlayers.Count == 0)
        {
            infoText.text = emptyText;
            return;
        }

        infoText.text = iveLoadedText;

        HashSet<Player> waitingFor = new(GameManager.Instance.nonSpectatingPlayers);
        waitingFor.ExceptWith(GameManager.Instance.loadedPlayers);
        playerList.text = waitingFor.Count == 0
            ? ""
            : "<font=\"NSMBStrongFont\">- Waiting for -</font>\n<size=8> </size>\n" + string.Join("\n", waitingFor.Select(pl => pl.GetUniqueNickname()));
    }

    private IEnumerator WaitForEveryone()
    {
        yield return new WaitForSeconds(waitingTime);
        yield return MusicSynth.SetPlaybackState(Songinator.PlaybackState.STOPPED, secondsFading: 1f);
        MusicSynthIdle.SetPlaybackState(Songinator.PlaybackState.PLAYING);
        waitingLastTimer = waitingLastTime;
        yield return new WaitForSeconds(waitingLastTime);
        timedOut = true;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, "DUMMY_TIMEOUT", NetworkUtils.EventAll,
                SendOptions.SendReliable);
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void StopLoading(bool spectating)
    {
        waitingLastTime = 0;
        StopCoroutine(waitingCoroutine);
        GetComponent<Animator>().SetTrigger(spectating ? "spectating" : "loaded");
        MusicSynth.SetPlaybackState(Songinator.PlaybackState.STOPPED, 2.0f);
    }
}