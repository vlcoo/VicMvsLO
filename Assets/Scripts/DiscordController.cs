using System;
using Discord;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordController : MonoBehaviour
{
    public ActivityManager activityManager;

    public Discord.Discord discord;

    private PlayerController localController;

    public void Awake()
    {
#if UNITY_WEBGL
        return;
#endif

        discord = new Discord.Discord(1290003629423726684, (ulong)CreateFlags.NoRequireDiscord);
        activityManager = discord.GetActivityManager();
        activityManager.OnActivityJoinRequest += AskToJoin;
        activityManager.OnActivityJoin += TryJoinGame;

//#if UNITY_STANDALONE_WIN
        try
        {
            var filename = AppDomain.CurrentDomain.ToString();
            filename = string.Join(" ", filename.Split(" ")[..^2]);
            var dir = AppDomain.CurrentDomain.BaseDirectory + "\\" + filename;
            activityManager.RegisterCommand(dir);
            Debug.Log($"[DISCORD] Set launch path to \"{dir}\"");
        }
        catch
        {
            Debug.Log($"[DISCORD] Failed to set launch path (on {Application.platform})");
        }
//#endif
    }

    public void Update()
    {
#if UNITY_WEBGL
        return;
#endif
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
        }
    }

    public void OnDisable()
    {
        discord?.Dispose();
    }

    public void TryJoinGame(string secret)
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
            return;

        Debug.Log($"[DISCORD] Attempting to join game with secret \"{secret}\"");
        var split = secret.Split("-");
        var region = split[0];
        var room = split[1];

        MainMenuManager.lastRegion = region;
        MainMenuManager.Instance.connectThroughSecret = room;
        PhotonNetwork.Disconnect();
    }

    //TODO this doesn't work???
    public void AskToJoin(ref User user)
    {
        //activityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, (res) => {
        //    Debug.Log($"[DISCORD] Ask to Join response: {res}");
        //});
    }

    public void UpdateActivity()
    {
#if UNITY_WEBGL
        return;
#endif
        if (discord == null || activityManager == null || !Application.isPlaying)
            return;

        Activity activity = new();

        if (GameManager.Instance)
        {
            //in a level
            var gm = GameManager.Instance;
            var room = PhotonNetwork.CurrentRoom;
            if (localController == null && gm.localPlayer != null)
                localController = gm.localPlayer.GetComponent<PlayerController>();

            // activity.Details = PhotonNetwork.OfflineMode ? "Playing Offline" : "Playing Online";
            activity.Party = new ActivityParty
            {
                Size = new PartySize { CurrentSize = room.PlayerCount, MaxSize = room.MaxPlayers },
                Id = PhotonNetwork.CurrentRoom.Name
            };
            activity.State = room.IsVisible ? "In a Public Match" : "In a Private Match";
            activity.Secrets = new ActivitySecrets { Join = PhotonNetwork.CloudRegion + "-" + room.Name };

            ActivityAssets assets = new();
            /*if (gm.richPresenceId != "")
                assets.LargeImage = $"level-{gm.richPresenceId}";
            else
                assets.LargeImage = "mainmenu";*/ // TODO: add app and images...
            assets.LargeImage = "logo";
            assets.LargeText = "Playing in " + gm.levelName;
            if (localController == null || gm.SpectationManager.Spectating)
            {
                assets.SmallImage = "spectating";
                assets.SmallText = "Spectating";
            }
            else
            {
                assets.SmallImage = localController.character.legalName.ToLower().Replace(" ", "");
                assets.SmallText = "Playing as " + localController.character.legalName;
            }

            activity.Assets = assets;

            if (gm.timedGameDuration == -1)
                activity.Timestamps = new ActivityTimestamps { Start = gm.startRealTime / 1000 };
            else
                activity.Timestamps = new ActivityTimestamps { End = gm.endRealTime / 1000 };
        }
        else if (PhotonNetwork.InRoom)
        {
            //in a room
            var room = PhotonNetwork.CurrentRoom;
            localController = null;

            // activity.Details = PhotonNetwork.OfflineMode ? "Playing Offline" : "Playing Online";
            activity.Party = new ActivityParty
            {
                Size = new PartySize { CurrentSize = room.PlayerCount, MaxSize = room.MaxPlayers },
                Id = PhotonNetwork.CurrentRoom.Name
            };
            activity.State = room.IsVisible ? "In a Public Lobby" : "In a Private Lobby";
            activity.Secrets = new ActivitySecrets { Join = PhotonNetwork.CloudRegion + "-" + room.Name };

            activity.Assets = new ActivityAssets { LargeImage = "logo" };
        }
        else
        {
            //in the main menu, not in a room
            localController = null;

            activity.Details = "In the Main Menu";
            activity.Assets = new ActivityAssets { LargeImage = "logo" };
        }


        activityManager.UpdateActivity(activity, res =>
        {
            //head empty.
            Debug.Log($"[DISCORD] Rich Presence Update: {res}");
        });
    }
}