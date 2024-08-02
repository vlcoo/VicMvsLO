using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = System.Random;

public class MainMenuManager : MonoBehaviour, ILobbyCallbacks, IInRoomCallbacks, IOnEventCallback, IConnectionCallbacks,
    IMatchmakingCallbacks
{
    public const int NICKNAME_MIN = 2, NICKNAME_MAX = 20;
    public const float PROMPT_ANIM_DURATION = 0.17f;
    private static readonly Color SYSTEM_MESSAGE_COLOR = Color.blue;

    public static MainMenuManager Instance;
    public static string lastRegion;
    private static readonly string roomNameChars = "BCDFGHJKLMNPRQSTVWXYZ";

    private static readonly Random rng = new();
    public AudioSource sfx, music;
    public Songinator MusicSynth;
    public MenuWorldSongPlayer worldSongPlayer;
    public GameObject lobbiesContent, lobbyPrefab;
    public GameObject connecting;

    public GameObject title,
        bg,
        mainMenu,
        optionsMenu,
        lobbyMenu,
        createLobbyPrompt,
        inLobbyMenu,
        creditsMenu,
        controlsMenu,
        privatePrompt,
        updateBox,
        webglWarningBox,
        newRuleS1Prompt,
        newRuleS2Prompt,
        emoteListPrompt,
        RNGRulesBox,
        specialPrompt,
        stagePrompt,
        teamsPrompt,
        powerupsPrompt;

    public GameObject[] levelCameraPositions;

    public GameObject sliderText,
        lobbyText,
        currentMaxPlayers,
        settingsPanel,
        ruleTemplate,
        lblConditions,
        specialTogglesParent;

    public TMP_Dropdown levelDropdown, characterDropdown;
    public RoomIcon selectedRoomIcon, privateJoinRoom;
    public Button joinRoomBtn, createRoomBtn, startGameBtn, exitBtn, backBtn;

    public Toggle ndsResolutionToggle,
        fullscreenToggle,
        livesEnabled,
        powerupsEnabled,
        timeEnabled,
        starcoinsEnabled,
        starsEnabled,
        coinsEnabled,
        drawTimeupToggle,
        fireballToggle,
        rumbleToggle,
        onscreenControlsToggle,
        animsToggle,
        vsyncToggle,
        privateToggle,
        privateToggleRoom,
        aspectToggle,
        spectateToggle,
        scoreboardToggle,
        filterToggle,
        chainableActionsToggle,
        RNGClear,
        teamsToggle,
        friendlyToggle,
        shareToggle,
        nomapToggle,
        coincountToggle;

    public GameObject playersContent, playersPrefab, chatContent, chatPrefab;

    public TMP_InputField nicknameField,
        starsText,
        lapsText,
        coinsText,
        livesField,
        timeField,
        lobbyJoinField,
        chatTextField;

    public Slider musicSlider, sfxSlider, masterSlider, lobbyPlayersSlider, changePlayersSlider, RNGSlider;

    public GameObject mainMenuSelected,
        optionsSelected,
        lobbySelected,
        currentLobbySelected,
        createLobbySelected,
        creditsSelected,
        controlsSelected,
        privateSelected,
        reconnectSelected,
        updateBoxSelected,
        webglWarningBoxSelected,
        newRuleS1Selected,
        newRuleS2Selected,
        emoteListSelected,
        RNGRulesSelected,
        specialSelected,
        stageSelected,
        teamsSelected,
        powerupsSelected;

    public GameObject errorBox, errorButton, rebindPrompt, reconnectBox;

    public TMP_Text errorText,
        errorDetail,
        rebindCountdown,
        rebindText,
        reconnectText,
        updateText,
        RNGSliderText,
        specialCountText,
        teamHintText,
        setSpecialBtn,
        stageText;

    public TMP_Dropdown region;
    public RebindManager rebindManager;
    public string connectThroughSecret = "";
    public string selectedRoom;
    public bool askedToJoin;

    public FadeOutManager fader;

    public Image overallColor, shirtColor;
    public GameObject palette, paletteDisabled;

    public ScrollRect settingsScroll;

    public Selectable[] roomSettings;

    public List<string> maps, debugMaps;

    public ColorChooser colorManager;

    public List<string> POSSIBLE_CONDITIONS = new();
    public List<PowerupChanceListEntry> powerupList = new();

    private readonly List<string> allRegions = new();

    private readonly Dictionary<string, RoomIcon> currentRooms = new();

    private readonly Dictionary<Player, double> lastMessage = new();
    private string aboutToAddAct = "";

    private string aboutToAddCond = "";

    public List<KeyValuePair<string, string>> DISALLOWED_RULES = new()
    {
        new KeyValuePair<string, string>("GotCoin", "ActGiveCoin"),
        new KeyValuePair<string, string>("GotStar", "ActGiveStar"),
        new KeyValuePair<string, string>("Spawned", "ActKillPlayer"),
        new KeyValuePair<string, string>("KnockedBack", "ActKnockbackPlayer"),
        new KeyValuePair<string, string>("Frozen", "ActFreezePlayer"),
        new KeyValuePair<string, string>("Died", "ActFreezePlayer"),
        new KeyValuePair<string, string>("ReachedCoinLimit", "ActGiveCoin")
    };

    private List<string> formattedRegions;

    private bool noUpdateNetRoom;
    private Region[] pingSortedRegions;

    private bool pingsReceived, joinedLate;
    [NonSerialized] public List<string> POSSIBLE_ACTIONS = new();
    private bool quit, validName;
    private bool raceMapSelected;
    private bool warningShown;
    [NonSerialized] public HashSet<MatchRuleListEntry> ruleList = new();
    [NonSerialized] public List<string> specialList = new();

    private Coroutine updatePingCoroutine;

    // Unity Stuff
    public void Start()
    {
        /*
         * dear god this needs a refactor. does every UI element seriously have to have
         * their callbacks into this one fuckin script?
         */

        Instance = this;
        sfx.outputAudioMixerGroup.audioMixer.SetFloat("SFXReverb", 0f);
        // sfx.outputAudioMixerGroup.audioMixer.SetFloat("MasterPitch", -80f);

        //Clear game-specific settings so they don't carry over
        HorizontalCamera.OFFSET_TARGET = 0;
        HorizontalCamera.OFFSET = 0;
        GlobalController.Instance.joinedAsSpectator = false;
        Time.timeScale = 1;

        if (GlobalController.Instance.disconnectCause != null)
        {
            OpenErrorBox(GlobalController.Instance.disconnectCause.Value);
            GlobalController.Instance.disconnectCause = null;
        }

        Camera.main.transform.position =
            levelCameraPositions[UnityEngine.Random.Range(0, maps.Count)].transform.position;
        levelDropdown.AddOptions(maps);
        LoadSettings(!PhotonNetwork.InRoom);

        //Photon stuff.
        if (!PhotonNetwork.IsConnected)
        {
            OpenTitleScreen();
            //PhotonNetwork.NetworkingClient.AppId = "ce540834-2db9-40b5-a311-e58be39e726a";
            PhotonNetwork.NetworkingClient.AppId = "40c2f241-79f7-4721-bdac-3c0366d00f58";

            //version separation
            var match = Regex.Match(Application.version, @"^\w*\.\w*\.\w*");
            PhotonNetwork.NetworkingClient.AppVersion = match.Groups[0].Value;

            var id = PlayerPrefs.GetString("id", null);
            var token = PlayerPrefs.GetString("token", null);

            PhotonNetwork.NetworkingClient.ConnectToNameServer();
        }
        else
        {
            if (PhotonNetwork.InRoom)
            {
                EnterRoom();
                nicknameField.SetTextWithoutNotify(Settings.Instance.nickname);
                UpdateNickname();
            }
            else
            {
                PhotonNetwork.Disconnect();
                nicknameField.text = Settings.Instance.nickname;
            }
        }

        if (PhotonNetwork.NetworkingClient.RegionHandler != null)
        {
            allRegions.AddRange(PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions.Select(r => r.Code));
            allRegions.Sort();

            List<string> newRegions = new();
            pingSortedRegions = PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions.ToArray();
            Array.Sort(pingSortedRegions, NetworkUtils.PingComparer);

            var index = 0;
            for (var i = 0; i < pingSortedRegions.Length; i++)
            {
                var r = pingSortedRegions[i];
                newRegions.Add(
                    $"{NetworkUtils.regionsFullNames.GetValueOrDefault(r.Code, r.Code.ToUpper())} <color=#bbbbbb>({(r.Ping == 4000 ? "?" : r.Ping)} ms)");
                if (r.Code == lastRegion)
                    index = i;
            }

            region.ClearOptions();
            region.AddOptions(newRegions);

            region.value = index;
        }

        lobbyPrefab = lobbiesContent.transform.Find("Template").gameObject;
        nicknameField.characterLimit = NICKNAME_MAX;

        rebindManager.Init();

        foreach (var method in Type.GetType("MatchConditioner").GetMethods())
            if (method.Name.StartsWith("Act"))
                POSSIBLE_ACTIONS.Add(method.Name);

        GlobalController.Instance.DiscordController.UpdateActivity();
        EventSystem.current.SetSelectedGameObject(title);

#if UNITY_WEBGL
        fullscreenToggle.interactable = false;
        exitBtn.interactable = false;
#else
        if (!GlobalController.Instance.checkedForVersion)
        {
            UpdateChecker.IsUpToDate(latestVersion =>
            {
                updateText.text =
                    $"You're running an old\nversion of this mod.\n\nPlease update!\n(Latest: <i>{latestVersion}</i>)";
                OpenPrompt(updateBox, updateBoxSelected);
            });
            GlobalController.Instance.checkedForVersion = true;
        }
#endif

        if (Utils.GetDeviceType() == Utils.DeviceType.MOBILE)
            Application.targetFrameRate = 91;
    }

    private void Update()
    {
        var connected = PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby;
        connecting.SetActive(!connected && lobbyMenu.activeInHierarchy);

        joinRoomBtn.interactable = connected && validName;
        createRoomBtn.interactable = connected && validName;
        region.interactable = connected;

        if (pingsReceived)
        {
            allRegions.Clear();
            allRegions.AddRange(PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions.Select(r => r.Code));
            allRegions.Sort();

            pingsReceived = false;

            region.ClearOptions();
            region.AddOptions(formattedRegions);
            region.value = 0;

            PhotonNetwork.Disconnect();
        }
    }

    // CALLBACK REGISTERING
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // CONNECTION CALLBACKS
    public void OnConnected()
    {
        Debug.Log("[PHOTON] Connected to Photon.");
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("[PHOTON] Disconnected: " + cause);
        if (cause is not (DisconnectCause.None or DisconnectCause.DisconnectByClientLogic
            or DisconnectCause.CustomAuthenticationFailed))
            OpenErrorBox(cause);

        selectedRoom = null;
        selectedRoomIcon = null;
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            foreach (var (key, value) in currentRooms.ToArray())
            {
                Destroy(value);
                currentRooms.Remove(key);
            }

            AuthenticationHandler.Authenticate(PlayerPrefs.GetString("id", null), PlayerPrefs.GetString("token", null),
                lastRegion);

            for (var i = 0; i < pingSortedRegions.Length; i++)
            {
                var r = pingSortedRegions[i];
                if (r.Code == lastRegion)
                {
                    region.value = i;
                    break;
                }
            }
        }
    }

    public void OnRegionListReceived(RegionHandler handler)
    {
        handler.PingMinimumOfRegions(handler =>
        {
            formattedRegions = new List<string>();
            pingSortedRegions = handler.EnabledRegions.ToArray();
            Array.Sort(pingSortedRegions, NetworkUtils.PingComparer);

            foreach (var r in pingSortedRegions)
                formattedRegions.Add(
                    $"{NetworkUtils.regionsFullNames.GetValueOrDefault(r.Code, r.Code.ToUpper())} <color=#bbbbbb>({(r.Ping == 4000 ? "?" : r.Ping)}ms)");

            lastRegion = pingSortedRegions[0].Code;
            pingsReceived = true;
        }, "");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> response)
    {
        Debug.Log("[PHOTON] Auth Successful!");
        PlayerPrefs.SetString("id", PhotonNetwork.AuthValues.UserId);
        if (response.ContainsKey("Token"))
            PlayerPrefs.SetString("token", (string)response["Token"]);
        PlayerPrefs.Save();
    }

    public void OnCustomAuthenticationFailed(string failure)
    {
        Debug.Log("[PHOTON] Auth Failure: " + failure);
        OpenErrorBox(failure);
    }

    public void OnConnectedToMaster()
    {
        JoinMainLobby();
    }

    // ROOM CALLBACKS
    public void OnPlayerPropertiesUpdate(Player player, Hashtable playerProperties)
    {
        // increase or remove when toadette or another character is added
        Utils.GetCustomProperty(Enums.NetRoomProperties.Debug, out bool debug);
        if (PhotonNetwork.IsMasterClient && Utils.GetCharacterIndex(player) > 9 && !debug)
            SwapCharacterExplicit(0);
        UpdateSettingEnableStates();
    }

    public void OnMasterClientSwitched(Player newMaster)
    {
        if (newMaster.IsLocal)
        {
            //i am de captain now
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                [Enums.NetRoomProperties.HostName] = newMaster.GetUniqueNickname()
            });
            LocalChatMessage("You have become the lobby's host.", SYSTEM_MESSAGE_COLOR);
        }
        else
        {
            LocalChatMessage($"<i>{newMaster.GetUniqueNickname()}</i> has become the lobby's host.",
                SYSTEM_MESSAGE_COLOR);
        }

        UpdateSettingEnableStates();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Bans, out object[] bans);
        var banList = bans.Cast<NameIdPair>().ToList();
        if (newPlayer.NickName.Length < NICKNAME_MIN ||
            newPlayer.NickName.Length > NICKNAME_MAX ||
            banList.Any(nip =>
                nip.userId == newPlayer.UserId || newPlayer.GetAuthorityLevel() < Enums.AuthorityLevel.NORMAL))
        {
            if (PhotonNetwork.IsMasterClient)
                StartCoroutine(KickPlayer(newPlayer));
            return;
        }

        LocalChatMessage($"<i>{newPlayer.GetUniqueNickname()}</i> just joined.", SYSTEM_MESSAGE_COLOR);
        sfx.PlayOneShot(Enums.Sounds.UI_PlayerConnect.GetClip());
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Bans, out object[] bans);
        var banList = bans.Cast<NameIdPair>().ToList();
        if (banList.Any(nip => nip.userId == otherPlayer.UserId)) return;
        LocalChatMessage($"<i>{otherPlayer.GetUniqueNickname()}</i> just left.", SYSTEM_MESSAGE_COLOR);
        sfx.PlayOneShot(Enums.Sounds.UI_PlayerDisconnect.GetClip());
    }

    public void OnRoomPropertiesUpdate(Hashtable updatedProperties)
    {
        if (updatedProperties == null)
            return;

        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.Debug, ChangeDebugState);
        AttemptToUpdateProperty<int>(updatedProperties, Enums.NetRoomProperties.Level, ChangeLevel);
        AttemptToUpdateProperty<int>(updatedProperties, Enums.NetRoomProperties.StarRequirement, ChangeStarRequirement);
        AttemptToUpdateProperty<int>(updatedProperties, Enums.NetRoomProperties.LapRequirement, ChangeLapRequirement);
        // AttemptToUpdateProperty<Dictionary<string, string>>(updatedProperties, Enums.NetRoomProperties.MatchRules, DictToMatchRules);
        AttemptToUpdateProperty<string>(updatedProperties, Enums.NetRoomProperties.MatchRules, JsonToMatchRules);
        AttemptToUpdateProperty<Dictionary<string, bool>>(updatedProperties, Enums.NetRoomProperties.SpecialRules,
            DictToSpecialRules);
        AttemptToUpdateProperty<Dictionary<string, int>>(updatedProperties, Enums.NetRoomProperties.PowerupChances,
            DictToPowerupChances);
        AttemptToUpdateProperty<int>(updatedProperties, Enums.NetRoomProperties.CoinRequirement, ChangeCoinRequirement);
        AttemptToUpdateProperty<int>(updatedProperties, Enums.NetRoomProperties.Lives, ChangeLives);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.NewPowerups, ChangeNewPowerups);
        AttemptToUpdateProperty<int>(updatedProperties, Enums.NetRoomProperties.Time, ChangeTime);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.DrawTime, ChangeDrawTime);
        AttemptToUpdateProperty<string>(updatedProperties, Enums.NetRoomProperties.HostName, ChangeLobbyHeader);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.ChainableRules, ChangeChainableRules);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.Starcoins, ChangeStarcoins);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.Teams, ChangeTeams);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.FriendlyFire, ChangeFriendly);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.ShareStars, ChangeShare);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.NoMap, ChangeNoMap);
        AttemptToUpdateProperty<bool>(updatedProperties, Enums.NetRoomProperties.ShowCoinCount, ChangeCoinCount);
    }

    // LOBBY CALLBACKS
    public void OnJoinedLobby()
    {
        Hashtable prop = new()
        {
            { Enums.NetPlayerProperties.Character, Settings.Instance.character },
            { Enums.NetPlayerProperties.Ping, PhotonNetwork.GetPing() },
            { Enums.NetPlayerProperties.PlayerColor, Settings.Instance.skin },
            { Enums.NetPlayerProperties.Spectator, false },
            { Enums.NetPlayerProperties.DeviceType, Utils.GetDeviceType() }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(prop);

        if (connectThroughSecret != "")
        {
            PhotonNetwork.JoinRoom(connectThroughSecret);
            connectThroughSecret = "";
        }

        if (updatePingCoroutine == null)
            updatePingCoroutine = StartCoroutine(UpdatePing());
    }

    public void OnLeftLobby()
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbies)
    {
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        List<string> invalidRooms = new();

        foreach (var room in roomList)
        {
            Utils.GetCustomProperty(Enums.NetRoomProperties.Lives, out int lives, room.CustomProperties);
            Utils.GetCustomProperty(Enums.NetRoomProperties.StarRequirement, out int stars, room.CustomProperties);
            Utils.GetCustomProperty(Enums.NetRoomProperties.CoinRequirement, out int coins, room.CustomProperties);

            var valid = true;
            valid &= room.IsVisible && room.IsOpen;
            valid &= !room.RemovedFromList;
            valid &= room.MaxPlayers >= 2 && room.MaxPlayers <= 10;
            valid &= lives <= 99;
            valid &= stars <= 99;
            valid &= coins <= 99;
            //valid &= host.IsValidUsername();

            if (!valid)
            {
                invalidRooms.Add(room.Name);
                continue;
            }

            RoomIcon roomIcon;
            if (currentRooms.ContainsKey(room.Name))
            {
                roomIcon = currentRooms[room.Name];
            }
            else
            {
                var newLobby = Instantiate(lobbyPrefab, Vector3.zero, Quaternion.identity);
                newLobby.name = room.Name;
                newLobby.SetActive(true);
                newLobby.transform.SetParent(lobbiesContent.transform, false);

                currentRooms[room.Name] = roomIcon = newLobby.GetComponent<RoomIcon>();
                roomIcon.room = room;
            }

            if (room.Name == selectedRoom) selectedRoomIcon = roomIcon;

            roomIcon.UpdateUI(room);
        }

        foreach (var key in invalidRooms)
        {
            if (!currentRooms.ContainsKey(key))
                continue;

            Destroy(currentRooms[key].gameObject);
            currentRooms.Remove(key);
        }

        if (askedToJoin && selectedRoomIcon != null)
        {
            JoinSelectedRoom();
            askedToJoin = false;
            selectedRoom = null;
            selectedRoomIcon = null;
        }

        privateJoinRoom.transform.SetAsFirstSibling();
        privateJoinRoom.gameObject.SetActive(currentRooms.Count <= 0);
    }

    public void OnJoinedRoom()
    {
        Debug.Log($"[PHOTON] Joined Room ({PhotonNetwork.CurrentRoom.Name})");
        LocalChatMessage($"<i>{PhotonNetwork.LocalPlayer.GetUniqueNickname()}</i> just joined.", SYSTEM_MESSAGE_COLOR);
        EnterRoom();
    }

    // MATCHMAKING CALLBACKS
    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnLeftRoom()
    {
        OpenLobbyMenu();
        ClearChat();
        GlobalController.Instance.DiscordController.UpdateActivity();
    }

    public void OnJoinRandomFailed(short reasonId, string reasonMessage)
    {
        OnJoinRoomFailed(reasonId, reasonMessage);
    }

    public void OnJoinRoomFailed(short reasonId, string reasonMessage)
    {
        Debug.LogError($"[PHOTON] Join room failed ({reasonId}, {reasonMessage})");
        OpenErrorBox(reasonMessage);
        JoinMainLobby();
    }

    public void OnCreateRoomFailed(short reasonId, string reasonMessage)
    {
        Debug.LogError($"[PHOTON] Create room failed ({reasonId}, {reasonMessage})");
        OpenErrorBox(reasonMessage);

        OnConnectedToMaster();
    }

    public void OnCreatedRoom()
    {
        Debug.Log($"[PHOTON] Created Room ({PhotonNetwork.CurrentRoom.Name})");
    }

    // CUSTOM EVENT CALLBACKS
    public void OnEvent(EventData e)
    {
        Player sender = null;

        if (PhotonNetwork.CurrentRoom != null)
            sender = PhotonNetwork.CurrentRoom.GetPlayer(e.Sender);

        switch (e.Code)
        {
            case (byte)Enums.NetEventIds.StartGame:
            {
                if (!(sender?.IsMasterClient ?? false) && e.SenderKey != 255)
                    return;

                PlayerPrefs.SetString("in-room", PhotonNetwork.CurrentRoom.Name);
                PlayerPrefs.Save();
                Utils.GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool spectate,
                    PhotonNetwork.LocalPlayer.CustomProperties);
                GlobalController.Instance.joinedAsSpectator = spectate || joinedLate;
                Utils.GetCustomProperty(Enums.NetRoomProperties.Level, out int level);
                PhotonNetwork.IsMessageQueueRunning = false;
                GlobalController.Instance.fastLoad = false;
                SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
                SceneManager.LoadSceneAsync(level + 2, LoadSceneMode.Additive);
                GlobalController.Instance.rumbler.RumbleForSeconds(0.1f, 0.3f, 0.3f);
                break;
            }
            case (byte)Enums.NetEventIds.PlayerChatMessage:
            {
                var message = e.CustomData as string;

                if (string.IsNullOrWhiteSpace(message))
                    return;

                if (sender == null)
                    return;

                var time = lastMessage.GetValueOrDefault(sender);
                if (PhotonNetwork.Time - time < 0.75f)
                    return;

                lastMessage[sender] = PhotonNetwork.Time;

                if (!sender.IsMasterClient)
                {
                    Utils.GetCustomProperty(Enums.NetRoomProperties.Mutes, out object[] mutes);
                    if (mutes.Contains(sender.UserId))
                        return;
                }

                message = message.Substring(0, Mathf.Min(128, message.Length));
                message = message. /*Replace("<", "«").Replace(">", "»").*/Replace("\n", " ").Trim();
                message = Utils.RawMessageToEmoji(message);
                message = "<size=10><i>" + sender.GetUniqueNickname() + "</size></i>\n" + message.Filter();

                LocalChatMessage(message, Color.black, false);
                break;
            }
            case (byte)Enums.NetEventIds.ChangeMaxPlayers:
            {
                ChangeMaxPlayers((byte)e.CustomData);
                break;
            }
            case (byte)Enums.NetEventIds.ChangePrivate:
            {
                ChangePrivate();
                break;
            }
        }
    }

    private IEnumerator KickPlayer(Player player)
    {
        if (player.IsMasterClient)
            yield break;

        while (PhotonNetwork.CurrentRoom.Players.Values.Contains(player))
        {
            PhotonNetwork.CloseConnection(player);
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void ChangeDebugState(bool enabled)
    {
        var index = levelDropdown.value;
        levelDropdown.SetValueWithoutNotify(0);
        levelDropdown.ClearOptions();
        levelDropdown.AddOptions(maps);
        levelDropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, maps.Count - 1));

        if (enabled)
        {
            levelDropdown.AddOptions(debugMaps);
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            Utils.GetCustomProperty(Enums.NetRoomProperties.Level, out int level);
            if (level >= maps.Count)
            {
                Hashtable props = new()
                {
                    [Enums.NetRoomProperties.Level] = maps.Count - 1
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }

        UpdateSettingEnableStates();
    }

    private void AttemptToUpdateProperty<T>(Hashtable updatedProperties, string key, Action<T> updateAction)
    {
        if (updatedProperties[key] == null)
            return;

        updateAction((T)updatedProperties[key]);
    }

    private void JoinMainLobby()
    {
        //Match match = Regex.Match(Application.version, "^\\w*\\.\\w*\\.\\w*");
        //PhotonNetwork.JoinLobby(new TypedLobby(match.Groups[0].Value, LobbyType.Default));

        PhotonNetwork.JoinLobby();
    }

    private void LoadSettings(bool nickname)
    {
        if (nickname)
            nicknameField.text = Settings.Instance.nickname;
        else
            nicknameField.SetTextWithoutNotify(Settings.Instance.nickname);

        musicSlider.value = Settings.Instance.VolumeMusic;
        sfxSlider.value = Settings.Instance.VolumeSFX;
        masterSlider.value = Settings.Instance.VolumeMaster;

        aspectToggle.interactable = ndsResolutionToggle.isOn = Settings.Instance.ndsResolution;
        aspectToggle.isOn = Settings.Instance.fourByThreeRatio;
#if !UNITY_ANDROID
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
#else
        fullscreenToggle.interactable = false;
        onscreenControlsToggle.interactable = false;
        fireballToggle.interactable = false;
#endif
        fireballToggle.isOn = Settings.Instance.fireballFromSprint;
        rumbleToggle.isOn = Settings.Instance.rumbleController;
        onscreenControlsToggle.isOn = Settings.Instance.onScreenControlsAlways;
        vsyncToggle.isOn = Settings.Instance.vsync;
        scoreboardToggle.isOn = Settings.Instance.scoreboardAlways;
        animsToggle.isOn = Settings.Instance.reduceUIAnims;
        filterToggle.isOn = Settings.Instance.filter;
        QualitySettings.vSyncCount = Settings.Instance.vsync ? 1 : 0;
    }

    private IEnumerator UpdatePing()
    {
        // push our ping into our player properties every N seconds. 2 seems good.
        while (true)
        {
            yield return new WaitForSecondsRealtime(1);
            if (PhotonNetwork.InRoom)
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                {
                    { Enums.NetPlayerProperties.Ping, PhotonNetwork.GetPing() }
                });
        }
    }

    public void onSetSpecialRule(GameObject element)
    {
        var name = element.name;
        var thereWereDuplicates = false;
        var how = element.transform.GetChild(2).GetComponent<Toggle>().isOn;

        if (how)
        {
            if (!specialList.Contains(name)) specialList.Add(name);
            else thereWereDuplicates = true;
        }
        else
        {
            specialList.Remove(name);
        }

        specialCountText.text = "Specials: " + specialList.Count;

        if (noUpdateNetRoom || thereWereDuplicates) return;
        Hashtable table = new()
        {
            [Enums.NetRoomProperties.SpecialRules] = SpecialRulesToDict()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void saveMatchRules()
    {
#if UNITY_ANDROID || UNITY_WEBGL
        sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());
        return;
#endif
        var path = Utils.SaveFileBrowser("Ruleset files (JSON)|*.json", "vcmiRuleset.json");
        if (path is null or "") return;

        File.WriteAllText(path, MatchRulesToJson());
    }

    public void loadMatchRules()
    {
#if UNITY_ANDROID || UNITY_WEBGL
        sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());
        return;
#endif
        var path = Utils.OpenFileBrowser("Ruleset files (JSON)|*.json");
        if (path is null or "") return;

        JsonToMatchRules(File.ReadAllText(path));
        Hashtable table = new()
        {
            [Enums.NetRoomProperties.MatchRules] = MatchRulesToJson()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void onAddMatchRuleExplicit(string cond, string act, bool updateNetRoom, bool updateUIList = true)
    {
        if (cond is null || act is null || !POSSIBLE_CONDITIONS.Contains(cond) ||
            DISALLOWED_RULES.Contains(new KeyValuePair<string, string>(cond, act)))
        {
            sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());
            return;
        }

        if (!cond.Equals("") && !act.Equals(""))
        {
            var newEntry = Instantiate(ruleTemplate);
            var newEntryScript = newEntry.GetComponent<MatchRuleListEntry>();
            newEntryScript.setRules(cond, act);
            if (updateUIList)
            {
                newEntry.transform.SetParent(settingsPanel.transform, false);
                newEntry.transform.SetSiblingIndex(lblConditions.transform.GetSiblingIndex() - 1);
                newEntry.SetActive(true);
            }

            ruleList.Add(newEntryScript);

            if (updateNetRoom)
            {
                Hashtable table = new()
                {
                    [Enums.NetRoomProperties.MatchRules] = MatchRulesToJson()
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);
            }
        }
    }

    public void onAddMatchRule()
    {
        if (!POSSIBLE_CONDITIONS.Contains(aboutToAddCond) || !POSSIBLE_ACTIONS.Contains(aboutToAddAct))
            return;

        onAddMatchRuleExplicit(aboutToAddCond, aboutToAddAct, true, false);
        aboutToAddCond = "";
        aboutToAddAct = "";
    }

    public void onRemoveMatchRule(MatchRuleListEntry which)
    {
        which.onRemoveButtonPressed();
        ruleList.Remove(which);
        Destroy(which.gameObject);

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.MatchRules] = MatchRulesToJson()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void onUpPowerupChance(string powerup)
    {
        powerupList.Find(entry => entry.powerup.Equals(powerup)).Chance += 1;
        Hashtable table = new()
        {
            [Enums.NetRoomProperties.PowerupChances] = PowerupChancesToDict()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void onDownPowerupChance(string powerup)
    {
        powerupList.Find(entry => entry.powerup.Equals(powerup)).Chance -= 1;
        Hashtable table = new()
        {
            [Enums.NetRoomProperties.PowerupChances] = PowerupChancesToDict()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void EnterRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        PlayerPrefs.SetString("in-room", null);
        PlayerPrefs.Save();

        Utils.GetCustomProperty(Enums.NetRoomProperties.GameStarted, out bool started);
        if (started)
        {
            //start as spectator
            joinedLate = true;
            OnEvent(new EventData { Code = (byte)Enums.NetEventIds.StartGame, SenderKey = 255 });
            return;
        }

        OpenInLobbyMenu();
        characterDropdown.SetValueWithoutNotify(Utils.GetCharacterIndex());

        Utils.GetCustomProperty(Enums.NetPlayerProperties.PlayerColor, out int value,
            PhotonNetwork.LocalPlayer.CustomProperties);
        SetPlayerColor(value);

        OnRoomPropertiesUpdate(room.CustomProperties);
        ChangeMaxPlayers(room.MaxPlayers);
        ChangePrivate();

        StartCoroutine(SetScroll());

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            [Enums.NetPlayerProperties.GameState] = null
        });
        updatePingCoroutine ??= StartCoroutine(UpdatePing());
        GlobalController.Instance.DiscordController.UpdateActivity();

        Utils.GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool spectating,
            PhotonNetwork.LocalPlayer.CustomProperties);
        spectateToggle.isOn = spectating;
        chatTextField.SetTextWithoutNotify("");
        noUpdateNetRoom = false;
    }

    private IEnumerator SetScroll()
    {
        settingsScroll.verticalNormalizedPosition = 1;
        yield return null;
        settingsScroll.verticalNormalizedPosition = 1;
    }

    public void OpenTitleScreen()
    {
        title.SetActive(true);
        bg.SetActive(false);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);

        EventSystem.current.SetSelectedGameObject(mainMenuSelected);
    }

    public void OpenMainMenu()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);
        ClosePrompt(updateBox);

        EventSystem.current.SetSelectedGameObject(mainMenuSelected);

        if (!warningShown && Utils.GetDeviceType() == Utils.DeviceType.BROWSER)
        {
            OpenPrompt(webglWarningBox, webglWarningBoxSelected);
            warningShown = true;
        }
    }

    public void ConnectOffline()
    {
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.CreateRoom("Single", new RoomOptions
        {
            CustomRoomProperties = NetworkUtils.DefaultRoomProperties
        });

        PhotonNetwork.NickName = nicknameField.text;
        PhotonNetwork.JoinRoom("Single");
        OpenInLobbyMenu();
    }

    public void OpenLobbyMenu()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(true);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);

        foreach (var room in currentRooms.Values)
            room.UpdateUI(room.room);

        EventSystem.current.SetSelectedGameObject(lobbySelected);
    }

    public void OpenCreateLobby()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(true);
        OpenPrompt(createLobbyPrompt, createLobbySelected);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);

        privateToggle.isOn = false;
    }

    public void OpenEmoteList()
    {
        OpenPrompt(emoteListPrompt, emoteListSelected);
    }

    public void GenRandomRules()
    {
        var howMany = (int)RNGSlider.value;
        var clearFirst = RNGClear.isOn;

        if (clearFirst)
        {
            foreach (var rule in ruleList)
                Destroy(rule.gameObject);
            ruleList.Clear();
        }

        for (var i = 0; i < howMany; i++)
            onAddMatchRuleExplicit(POSSIBLE_CONDITIONS[rng.Next(POSSIBLE_CONDITIONS.Count)],
                POSSIBLE_ACTIONS[rng.Next(POSSIBLE_ACTIONS.Count)], false);

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.MatchRules] = MatchRulesToJson()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        ClosePrompt(RNGRulesBox);
    }

    public void OpenNewRuleS1()
    {
        OpenPrompt(newRuleS1Prompt, newRuleS1Selected);
    }

    public void OpenNewRuleS2(string condition)
    {
        ClosePrompt(newRuleS1Prompt);

        aboutToAddCond = condition;
        newRuleS2Prompt.transform.Find("Image/LblExplain").GetComponent<TMP_Text>().text =
            $"What will happen when \"{condition}\" gets triggered?";
        OpenPrompt(newRuleS2Prompt, newRuleS2Selected);
    }

    public void OpenSpecialRule()
    {
        OpenPrompt(specialPrompt, specialSelected);
    }

    public void OpenMapSelector()
    {
        OpenPrompt(stagePrompt, stageSelected);
    }

    public void OpenTeams()
    {
        OpenPrompt(teamsPrompt, teamsSelected);
    }

    public void OpenPowerups()
    {
        foreach (var powerupChanceListEntry in powerupList)
            powerupChanceListEntry.Chance = powerupChanceListEntry.Chance;
        OpenPrompt(powerupsPrompt, powerupsSelected);
    }

    public void CloseNewRuleS2(string action)
    {
        ClosePrompt(newRuleS2Prompt);
        aboutToAddAct = action;
        onAddMatchRule();
    }

    public void OpenRNGRules()
    {
        OpenPrompt(RNGRulesBox, RNGRulesSelected);
    }

    public void OpenOptions()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);

        EventSystem.current.SetSelectedGameObject(optionsSelected);
    }

    public void OpenControls()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(true);
        lobbyMenu.SetActive(false);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);

        EventSystem.current.SetSelectedGameObject(controlsSelected);
    }

    public void OpenCredits()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(false);
        creditsMenu.SetActive(true);
        ClosePrompt(privatePrompt);

        EventSystem.current.SetSelectedGameObject(creditsSelected);
    }

    public void OpenInLobbyMenu()
    {
        title.SetActive(false);
        bg.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        ClosePrompt(createLobbyPrompt);
        inLobbyMenu.SetActive(true);
        creditsMenu.SetActive(false);
        ClosePrompt(privatePrompt);

        EventSystem.current.SetSelectedGameObject(currentLobbySelected);
    }

    public void OpenPrivatePrompt()
    {
        lobbyJoinField.text = "";

        OpenPrompt(privatePrompt, privateSelected);
    }

    private void OpenErrorBox(DisconnectCause cause)
    {
        if (!errorBox.activeSelf)
            sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());

        errorText.text = NetworkUtils.disconnectMessages.GetValueOrDefault(cause, "Unknown cause");
        errorDetail.text = cause.ToString();

        OpenPrompt(errorBox, errorButton);
    }

    public void OpenErrorBox(string text)
    {
        if (!errorBox.activeSelf)
            sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());
        errorText.text = text;

        OpenPrompt(errorBox, errorButton);
    }

    public static void OpenPrompt(GameObject which, GameObject selected = null)
    {
        which.SetActive(true);
        which.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        var boxChild = which.transform.GetChild(0).transform;
        boxChild.localScale = new Vector3(0, 0, 1);
        DOTween.To(() => boxChild.localScale, s => boxChild.localScale = s, new Vector3(1, 1, 1), PROMPT_ANIM_DURATION)
            .SetEase(Ease.OutCubic);
        if (selected != null) EventSystem.current.SetSelectedGameObject(selected);
    }

    public void ClosePrompt(GameObject which)
    {
        StartCoroutine(ClosePromptCoroutine(which));
    }

    public static IEnumerator ClosePromptCoroutine(GameObject which)
    {
        which.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        var boxChild = which.transform.GetChild(0).transform;
        boxChild.localScale = new Vector3(1, 1, 1);
        yield return DOTween
            .To(() => boxChild.localScale, s => boxChild.localScale = s, new Vector3(0, 0, 1), PROMPT_ANIM_DURATION)
            .SetEase(Ease.InCubic).WaitForCompletion();
        which.SetActive(false);
    }

    public void BackSound()
    {
        sfx.PlayOneShot(Enums.Sounds.UI_Back.GetClip());
    }

    public void ConfirmSound()
    {
        ConfirmSound(false);
    }

    public void ConfirmSound(bool alternate = false)
    {
        sfx.PlayOneShot(alternate ? Enums.Sounds.UI_Cursor.GetClip() : Enums.Sounds.UI_Decide.GetClip());
    }

    public void ConnectToDropdownRegion()
    {
        var targetRegion = pingSortedRegions[region.value];
        if (lastRegion == targetRegion.Code)
            return;

        for (var i = 0; i < lobbiesContent.transform.childCount; i++)
        {
            var roomObj = lobbiesContent.transform.GetChild(i).gameObject;
            if (roomObj.GetComponent<RoomIcon>().joinPrivate || !roomObj.activeSelf)
                continue;

            Destroy(roomObj);
        }

        selectedRoomIcon = null;
        selectedRoom = null;
        lastRegion = targetRegion.Code;

        PhotonNetwork.Disconnect();
    }

    public void QuitRoom()
    {
        noUpdateNetRoom = true;
        PhotonNetwork.LeaveRoom();

        worldSongPlayer.OnLevelSelected(0);
        if (MusicSynth.state == Songinator.PlaybackState.PAUSED)
        {
            MusicSynth.SetPlaybackState(Songinator.PlaybackState.PLAYING, 0.5f);
        }
    }

    public void StartGame()
    {
        backBtn.interactable = false;
        sfx.PlayOneShot(Enums.Sounds.UI_Match_Starting.GetClip());
        MusicSynth.SetPlaybackState(Songinator.PlaybackState.STOPPED);
        worldSongPlayer.Stop();
        fader.SetInvisible(GlobalController.Instance.settings.reduceUIAnims);
        fader.anim.speed = 1.5f;
        fader.anim.SetTrigger("out");
        StartCoroutine(WaitForMusicFadeStartGame());
    }

    private IEnumerator WaitForMusicFadeStartGame()
    {
        yield return new WaitForSeconds(0.8f);
        //set started game
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { [Enums.NetRoomProperties.GameStarted] = true });

        //start game with all players
        RaiseEventOptions options = new() { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.StartGame, null, options, SendOptions.SendReliable);
    }

    public void ChangeNewPowerups(bool value)
    {
        powerupsEnabled.SetIsOnWithoutNotify(value);
    }

    public void ChangeLives(int lives)
    {
        livesEnabled.SetIsOnWithoutNotify(lives != -1);
        UpdateSettingEnableStates();
        if (lives == -1)
            return;

        livesField.SetTextWithoutNotify(lives.ToString());
    }

    public void SetLives(TMP_InputField input)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int.TryParse(input.text, out var newValue);
        if (newValue == -1)
            return;

        if (newValue < 1)
            newValue = 5;
        ChangeLives(newValue);
        if (newValue == (int)PhotonNetwork.CurrentRoom.CustomProperties[Enums.NetRoomProperties.Lives])
            return;

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.Lives] = newValue
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void SetNewPowerups(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.NewPowerups] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void EnableLives(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.Lives] = toggle.isOn ? int.Parse(livesField.text) : -1
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void ChangeLevel(int index)
    {
        levelDropdown.SetValueWithoutNotify(index);
        stageText.text = "Map: " + levelDropdown.options[index].text;
        raceMapSelected = levelDropdown.options[index].text.Contains("hudnumber_laps");
        UpdateSettingEnableStates();
        Camera.main.transform.position = levelCameraPositions[index].transform.position;
    }

    public void SetLevelIndex(int newLevelIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (newLevelIndex == (int)PhotonNetwork.CurrentRoom.CustomProperties[Enums.NetRoomProperties.Level])
            return;

        //ChangeLevel(newLevelIndex);

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.Level] = newLevelIndex
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);

        var worldId = worldSongPlayer.levelWorldIds[newLevelIndex];
        worldSongPlayer.OnLevelSelected(newLevelIndex);
        if (worldId == 0 && MusicSynth.state == Songinator.PlaybackState.PAUSED)
        {
            MusicSynth.SetPlaybackState(Songinator.PlaybackState.PLAYING, 0.5f);
            return;
        }

        if (worldId > 0)
        {
            if (MusicSynth.state == Songinator.PlaybackState.PLAYING)
                MusicSynth.SetPlaybackState(Songinator.PlaybackState.PAUSED, 0.5f);
        }
    }

    public void SelectRoom(GameObject room)
    {
        if (selectedRoomIcon)
            selectedRoomIcon.Unselect();

        selectedRoomIcon = room.GetComponent<RoomIcon>();
        selectedRoomIcon.Select();
        selectedRoom = selectedRoomIcon.room?.Name ?? null;

        joinRoomBtn.interactable = room != null && nicknameField.text.Length >= NICKNAME_MIN;
    }

    public void JoinSelectedRoom()
    {
        if (selectedRoomIcon?.joinPrivate ?? false)
        {
            OpenPrivatePrompt();
            return;
        }

        if (selectedRoom == null)
            return;

        PhotonNetwork.NickName = nicknameField.text;
        PhotonNetwork.JoinRoom(selectedRoomIcon.room.Name);
    }

    public void JoinSpecificRoom()
    {
        var id = lobbyJoinField.text.ToUpper();
        if (id.Length == 0) return;
        var index = roomNameChars.IndexOf(id[0]);
        if (id.Length < 8 || index < 0 || index >= allRegions.Count)
        {
            OpenErrorBox("Room doesn't exist.");
            return;
        }

        var region = allRegions[index];
        if (PhotonNetwork.NetworkingClient.CloudRegion.Split("/")[0] != region)
        {
            lastRegion = region;
            connectThroughSecret = id;
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.JoinRoom(id);
        }

        ClosePrompt(privatePrompt);
    }

    public void OnPrivatePromptTextEdited(Button confirmButton)
    {
        confirmButton.interactable = lobbyJoinField.text.Length == 8;
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.LocalPlayer?.GetAuthorityLevel() < Enums.AuthorityLevel.SOFT_BANNED)
        {
            OpenErrorBox("You've been banned.");
            return;
        }

        var players = (byte)lobbyPlayersSlider.value;
        var roomName = "";
        PhotonNetwork.NickName = nicknameField.text;

        roomName += roomNameChars[allRegions.IndexOf(PhotonNetwork.NetworkingClient.CloudRegion.Split("/")[0])];
        for (var i = 0; i < 7; i++)
            roomName += roomNameChars[UnityEngine.Random.Range(0, roomNameChars.Length)];

        var properties = NetworkUtils.DefaultRoomProperties;
        properties[Enums.NetRoomProperties.HostName] = PhotonNetwork.NickName;

        RoomOptions options = new()
        {
            MaxPlayers = players,
            IsVisible = !privateToggle.isOn,
            PublishUserId = true,
            CustomRoomProperties = properties,
            CustomRoomPropertiesForLobby = NetworkUtils.LobbyVisibleRoomProperties
        };
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);

        ClosePrompt(createLobbyPrompt);
        ChangeMaxPlayers(players);
    }

    public void ClearChat()
    {
        for (var i = 0; i < chatContent.transform.childCount; i++)
        {
            var chatMsg = chatContent.transform.GetChild(i).gameObject;
            if (!chatMsg.activeSelf)
                continue;
            Destroy(chatMsg);
        }
    }

    public void UpdateSettingEnableStates()
    {
        foreach (var s in roomSettings)
            s.interactable = PhotonNetwork.IsMasterClient;
        if (ruleList != null)
            foreach (var s in ruleList)
                s.removeButton.interactable = PhotonNetwork.IsMasterClient;

        livesField.interactable = PhotonNetwork.IsMasterClient && livesEnabled.isOn;
        timeField.interactable = PhotonNetwork.IsMasterClient && timeEnabled.isOn;
        starsText.interactable = PhotonNetwork.IsMasterClient && starsEnabled.isOn;
        coinsText.interactable = PhotonNetwork.IsMasterClient && coinsEnabled.isOn;
        drawTimeupToggle.interactable = PhotonNetwork.IsMasterClient && timeEnabled.isOn;
        chainableActionsToggle.interactable = PhotonNetwork.IsMasterClient;
        // setSpecialBtn.text = PhotonNetwork.IsMasterClient ? "Set" : "See";
        // starcoinsEnabled.transform.parent.gameObject.SetActive(raceMapSelected);
        starcoinsEnabled.interactable = PhotonNetwork.IsMasterClient && raceMapSelected;
        // lapsText.transform.parent.gameObject.SetActive(raceMapSelected);
        lapsText.interactable = PhotonNetwork.IsMasterClient && raceMapSelected;
        shareToggle.interactable = PhotonNetwork.IsMasterClient && teamsToggle.isOn;
        friendlyToggle.interactable = PhotonNetwork.IsMasterClient && teamsToggle.isOn;

        Utils.GetCustomProperty(Enums.NetRoomProperties.Debug, out bool debug);
        privateToggleRoom.interactable = PhotonNetwork.IsMasterClient && !debug;

        var playingPlayers = PhotonNetwork.CurrentRoom.Players.Where(pl =>
        {
            Utils.GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool spectating,
                pl.Value.CustomProperties);
            return !spectating;
        }).Count();

        startGameBtn.interactable = PhotonNetwork.IsMasterClient && playingPlayers >= 1;
    }

    public void PlayerChatMessage(string message)
    {
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.PlayerChatMessage, message, NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void LocalChatMessage(string message, Color? color = null, bool filter = true)
    {
        float y = 0;
        for (var i = 0; i < chatContent.transform.childCount; i++)
        {
            var child = chatContent.transform.GetChild(i).gameObject;
            if (!child.activeSelf)
                continue;

            y -= child.GetComponent<RectTransform>().rect.height + 20;
        }

        var chat = Instantiate(chatPrefab, Vector3.zero, Quaternion.identity, chatContent.transform);
        chat.SetActive(true);

        if (color != null)
        {
            var fColor = (Color)color;
            message = $"<color=#{(byte)(fColor.r * 255):X2}{(byte)(fColor.g * 255):X2}{(byte)(fColor.b * 255):X2}>" +
                      message;
        }

        var txtObject = chat.transform.Find("Text").gameObject;
        SetText(txtObject, message, filter);
        Canvas.ForceUpdateCanvases();
        sfx.PlayOneShot(Enums.Sounds.UI_Message.GetClip());

        //RectTransform tf = txtObject.GetComponent<RectTransform>();
        //Bounds bounds = txtObject.GetComponent<TextMeshProUGUI>().textBounds;
        //tf.sizeDelta = new Vector2(tf.sizeDelta.x, bounds.max.y - bounds.min.y - 15f);
    }

    public void SendChat()
    {
        var time = lastMessage.GetValueOrDefault(PhotonNetwork.LocalPlayer);
        if (PhotonNetwork.Time - time < 0.75f)
            return;

        var text = chatTextField.text.Replace("<", "«").Replace(">", "»").Trim();
        text = Utils.RawMessageToEmoji(text);
        if (string.IsNullOrEmpty(text))
            return;
        StartCoroutine(SelectNextFrame(chatTextField));

        if (text.StartsWith("/"))
        {
            RunCommand(text[1..].Split(" "));
            return;
        }

        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.PlayerChatMessage, text, NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void Kick(Player target)
    {
        PhotonNetwork.CloseConnection(target);
        LocalChatMessage($"<i>{target.GetUniqueNickname()}</i> has been kicked.", SYSTEM_MESSAGE_COLOR);
    }

    public void Promote(Player target)
    {
        if (target.IsLocal)
        {
            LocalChatMessage("You are already the host.", SYSTEM_MESSAGE_COLOR);
            return;
        }

        PhotonNetwork.SetMasterClient(target);
    }

    public void CopyPlayerID(Player target)
    {
        if (!target.IsLocal && PhotonNetwork.LocalPlayer.GetAuthorityLevel() < target.GetAuthorityLevel())
        {
            LocalChatMessage($"Unknown player {target.NickName}.", SYSTEM_MESSAGE_COLOR);
        }
        else
        {
            GUIUtility.systemCopyBuffer = target.UserId;
            LocalChatMessage($"Copied {target.NickName}'s ID: {target.UserId}", SYSTEM_MESSAGE_COLOR);
        }
    }

    public void Mute(Player target)
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Mutes, out object[] mutes);
        List<object> mutesList = new(mutes);
        if (mutes.Contains(target.UserId))
        {
            LocalChatMessage($"<i>{target.GetUniqueNickname()}</i> has been unmuted.", SYSTEM_MESSAGE_COLOR);
            mutesList.Remove(target.UserId);
        }
        else
        {
            LocalChatMessage($"<i>{target.GetUniqueNickname()}</i> has been muted.", SYSTEM_MESSAGE_COLOR);
            mutesList.Add(target.UserId);
        }

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.Mutes] = mutesList.ToArray()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void BanOrUnban(string playername)
    {
        var onlineTarget =
            PhotonNetwork.CurrentRoom.Players.Values.FirstOrDefault(
                pl => pl.GetUniqueNickname().ToLower() == playername);
        if (onlineTarget != null)
        {
            //player is in room, ban them
            Ban(onlineTarget);
            return;
        }

        Utils.GetCustomProperty(Enums.NetRoomProperties.Bans, out object[] bans);
        var pairs = bans.Cast<NameIdPair>().ToList();

        playername = playername.ToLower();

        var targetPair = pairs.FirstOrDefault(nip => nip.name.ToLower() == playername);
        if (targetPair != null)
        {
            //player is banned, unban them
            Unban(targetPair);
            return;
        }

        LocalChatMessage($"Unknown player {playername}.", SYSTEM_MESSAGE_COLOR);
    }

    public void Ban(Player target)
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Bans, out object[] bans);
        var pairs = bans.Cast<NameIdPair>().ToList();

        NameIdPair newPair = new()
        {
            name = target.NickName,
            userId = target.UserId
        };

        pairs.Add(newPair);

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.Bans] = pairs.ToArray()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table, null, NetworkUtils.forward);
        PhotonNetwork.CloseConnection(target);
        LocalChatMessage($"<i>{target.GetUniqueNickname()}</i> has been banned from this lobby.", SYSTEM_MESSAGE_COLOR);
    }

    private void Unban(NameIdPair targetPair)
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Bans, out object[] bans);
        var pairs = bans.Cast<NameIdPair>().ToList();

        pairs.Remove(targetPair);

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.Bans] = pairs.ToArray()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table, null, NetworkUtils.forward);
        LocalChatMessage($"{targetPair.name} has been unbanned from this lobby.", SYSTEM_MESSAGE_COLOR);
    }

    private void RunCommand(string[] args)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            LocalChatMessage("You cannot use room commands if you aren't the host!", SYSTEM_MESSAGE_COLOR);
            return;
        }

        var command = args.Length > 0 ? args[0].ToLower() : "";
        switch (command)
        {
            case "kick":
            {
                if (args.Length < 2)
                {
                    LocalChatMessage("Usage: /kick <player name>", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                var strTarget = args[1].ToLower();
                var target =
                    PhotonNetwork.CurrentRoom.Players.Values.FirstOrDefault(pl =>
                        pl.GetUniqueNickname().ToLower() == strTarget);
                if (target == null)
                {
                    LocalChatMessage($"Unknown player {args[1]}.", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                Kick(target);
                return;
            }
            case "host":
            {
                if (args.Length < 2)
                {
                    LocalChatMessage("Usage: /host <player name>", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                var strTarget = args[1].ToLower();
                var target =
                    PhotonNetwork.CurrentRoom.Players.Values.FirstOrDefault(pl =>
                        pl.GetUniqueNickname().ToLower() == strTarget);
                if (target == null)
                {
                    LocalChatMessage($"Unknown player {args[1]}.", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                Promote(target);
                return;
            }
            case "help":
            {
                var sub = args.Length > 1 ? args[1] : "";
                var msg = sub switch
                {
                    "kick" => "/kick <player name> - Remove a player from this lobby.",
                    "ban" => "/ban <player name> - Prevent a player from rejoining this lobby.",
                    "host" => "/host <player name> - Make a player this lobby's host.",
                    "mute" => "/mute <playername> - Prevent a player from talking in chat and using emotes.",
                    //"debug" => "/debug - Enables debug & in-development features",
                    _ => "Available commands: /kick, /host, /mute, /ban"
                };
                LocalChatMessage(msg, SYSTEM_MESSAGE_COLOR);
                return;
            }
            case "mute":
            {
                if (args.Length < 2)
                {
                    LocalChatMessage("Usage: /mute <player name>", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                var strTarget = args[1].ToLower();
                var target =
                    PhotonNetwork.CurrentRoom.Players.Values.FirstOrDefault(pl => pl.NickName.ToLower() == strTarget);
                if (target == null)
                {
                    LocalChatMessage($"Unknown player {args[1]}.", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                Mute(target);
                return;
            }
            case "ban":
            {
                if (args.Length < 2)
                {
                    LocalChatMessage("Usage: /ban <player name>", SYSTEM_MESSAGE_COLOR);
                    return;
                }

                BanOrUnban(args[1]);
                return;
            }
        }

        LocalChatMessage("Unknown command; send /help to learn more.", SYSTEM_MESSAGE_COLOR);
    }

    private IEnumerator SelectNextFrame(TMP_InputField input)
    {
        yield return new WaitForEndOfFrame();
        input.text = "";
        input.ActivateInputField();
    }

    public void PlayDialogSFX()
    {
        sfx.PlayOneShot(Enums.Sounds.UI_WindowOpen.GetClip());
    }

    public void SwapCharacterExplicit(int id)
    {
        Hashtable prop = new()
        {
            { Enums.NetPlayerProperties.Character, id }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(prop);
        Settings.Instance.character = id;
        Settings.Instance.SaveSettingsToPreferences();

        // if (id > 1) return;
        var data = GlobalController.Instance.characters[id];
        sfx.PlayOneShot(Enums.Sounds.Player_Voice_Selected.GetClip(data));
        colorManager.ChangeCharacter(data);

        Utils.GetCustomProperty(Enums.NetPlayerProperties.PlayerColor, out int index,
            PhotonNetwork.LocalPlayer.CustomProperties);
        if (index > Utils.GetColorCountForPlayer(data))
        {
            SetPlayerColor(0);
            colorManager.selected = 0;
            return;
        }

        var colors = index == 0 ? new PlayerColors() : GlobalController.Instance.skins[index].GetPlayerColors(data);
        paletteDisabled.SetActive(index == 0);
        overallColor.color = colors.hatColor;
        shirtColor.color = colors.overallsColor;
    }

    public void SwapCharacter(TMP_Dropdown dropdown)
    {
        SwapCharacterExplicit(dropdown.value);
    }

    public void SetPlayerColor(int index)
    {
        Hashtable prop = new()
        {
            { Enums.NetPlayerProperties.PlayerColor, index }
        };
        var colors = index == 0
            ? new PlayerColors()
            : GlobalController.Instance.skins[index].GetPlayerColors(Utils.GetCharacterData());
        paletteDisabled.SetActive(index == 0);
        overallColor.color = colors.hatColor;
        shirtColor.color = colors.overallsColor;
        PhotonNetwork.LocalPlayer.SetCustomProperties(prop);

        Settings.Instance.skin = index;
        Settings.Instance.SaveSettingsToPreferences();
    }

    private void UpdateNickname()
    {
        validName = PhotonNetwork.NickName.IsValidUsername();
        if (!validName)
        {
            var colors = nicknameField.colors;
            colors.normalColor = new Color(1, 0.7f, 0.7f, 1);
            colors.highlightedColor = new Color(1, 0.55f, 0.55f, 1);
            nicknameField.colors = colors;
        }
        else
        {
            var colors = nicknameField.colors;
            colors.normalColor = Color.white;
            nicknameField.colors = colors;
        }
    }

    public void SetUsername(TMP_InputField field)
    {
        PhotonNetwork.NickName = field.text;
        UpdateNickname();

        Settings.Instance.nickname = field.text;
        Settings.Instance.SaveSettingsToPreferences();
    }

    private void SetText(GameObject obj, string txt, bool filter)
    {
        var textComp = obj.GetComponent<TextMeshProUGUI>();
        textComp.text = filter ? txt.Filter() : txt;
    }

    private void SetText(GameObject obj, string txt, Color color)
    {
        var textComp = obj.GetComponent<TextMeshProUGUI>();
        textComp.text = txt.Filter();
        textComp.color = color;
    }

    public void OpenLinks()
    {
        Application.OpenURL("https://github.com/vlcoo/VicMvsLO/blob/master/LINKS.md");
    }

    public void Quit()
    {
        if (quit)
            return;

        StartCoroutine(FinishQuitting());
    }

    private IEnumerator FinishQuitting()
    {
        var clip = Enums.Sounds.UI_Quit.GetClip();
        sfx.PlayOneShot(clip);
        quit = true;

        yield return new WaitForSeconds(clip.length);
        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    public void JsonToMatchRules(string j)
    {
        foreach (var rule in ruleList) Destroy(rule.gameObject);
        ruleList.Clear();
        List<MatchRuleDataEntry> dataList;

        try
        {
            dataList = JsonConvert.DeserializeObject<List<MatchRuleDataEntry>>(j);
        }
        catch (JsonReaderException)
        {
            dataList = new List<MatchRuleDataEntry>();
        }

        if (dataList is not List<MatchRuleDataEntry>) dataList = new List<MatchRuleDataEntry>();

        foreach (var data in dataList)
        {
            if (data is not MatchRuleDataEntry) return;
            onAddMatchRuleExplicit(data.Condition, data.Action, false);
        }
    }

    public string MatchRulesToJson()
    {
        return JsonConvert.SerializeObject(ruleList.Select(rule => rule.Serialize()).ToList());
    }

    // public void DictToMatchRules(Dictionary<string, string> dict)
    // {
    //     foreach (var rule in ruleList)
    //         Destroy(rule.gameObject);
    //     ruleList.Clear();
    //     
    //     foreach(KeyValuePair<string, string> entry in dict)
    //         onAddMatchRuleExplicit(entry.Key, entry.Value, false, true);
    // }
    //
    // public Dictionary<string, string> MatchRulesToDict()
    // {
    //     Dictionary<string, string> dict = new Dictionary<string, string>();
    //     foreach (var rule in ruleList)
    //     {
    //         if (dict.ContainsKey(rule.Condition)) continue;
    //         dict.Add(rule.Condition, rule.Action);
    //     }
    //
    //     return dict;
    // }

    public void DictToSpecialRules(Dictionary<string, bool> dict)
    {
        specialList = dict.Keys.ToList();
        // whatever lol
        foreach (Transform toggle in specialTogglesParent.transform.GetChild(0).transform)
            toggle.transform.GetChild(2).GetComponent<Toggle>().isOn = specialList.Contains(toggle.name);
        foreach (Transform toggle in specialTogglesParent.transform.GetChild(1).transform)
            toggle.transform.GetChild(2).GetComponent<Toggle>().isOn = specialList.Contains(toggle.name);
        specialCountText.text = "Specials: " + specialList.Count;
    }

    public Dictionary<string, bool> SpecialRulesToDict()
    {
        specialList = specialList.Distinct().ToList();
        return specialList.ToDictionary(x => x, x => true);
    }

    public void DictToPowerupChances(Dictionary<string, int> dict)
    {
        if (dict.Count == 0) return;

        foreach (var entry in powerupList) entry.Chance = dict[entry.powerup];
    }

    public Dictionary<string, int> PowerupChancesToDict()
    {
        Dictionary<string, int> dict = new();
        foreach (var entry in powerupList) dict[entry.powerup] = entry.Chance;

        return dict;
    }

    public void ChangeStarRequirement(int stars)
    {
        starsEnabled.SetIsOnWithoutNotify(stars != -1);
        UpdateSettingEnableStates();
        if (stars == -1)
            return;

        starsText.SetTextWithoutNotify(stars.ToString());
    }

    public void SetStarRequirement(TMP_InputField input)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int.TryParse(input.text, out var newValue);
        if (newValue == -1)
            return;

        if (newValue < 1)
            newValue = 5;
        ChangeStarRequirement(newValue);
        if (newValue == (int)PhotonNetwork.CurrentRoom.CustomProperties[Enums.NetRoomProperties.StarRequirement])
            return;

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.StarRequirement] = newValue
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        //ChangeStarRequirement(newValue);
    }

    public void ChangeLapRequirement(int laps)
    {
        lapsText.SetTextWithoutNotify(laps.ToString());
    }

    public void SetLapRequirement(TMP_InputField input)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int.TryParse(input.text, out var newValue);

        newValue = Math.Clamp(newValue, 1, 99);
        ChangeLapRequirement(newValue);

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.LapRequirement] = newValue
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void ChangeChainableRules(bool how)
    {
        chainableActionsToggle.SetIsOnWithoutNotify(how);
    }

    public void ChangeTeams(bool how)
    {
        teamsToggle.SetIsOnWithoutNotify(how);
        UpdateSettingEnableStates();
        teamHintText.text = "Teams: " + (how ? "ON" : "OFF");
    }

    public void ChangeFriendly(bool how)
    {
        friendlyToggle.SetIsOnWithoutNotify(how);
    }

    public void ChangeShare(bool how)
    {
        shareToggle.SetIsOnWithoutNotify(how);
    }


    public void ChangeStarcoins(bool how)
    {
        starcoinsEnabled.SetIsOnWithoutNotify(how);
    }

    public void ChangeNoMap(bool how)
    {
        nomapToggle.SetIsOnWithoutNotify(how);
    }

    public void ChangeCoinCount(bool how)
    {
        coincountToggle.SetIsOnWithoutNotify(how);
    }

    public void ChangeCoinRequirement(int coins)
    {
        coinsEnabled.SetIsOnWithoutNotify(coins != -1);
        UpdateSettingEnableStates();
        if (coins == -1)
            return;

        coinsText.SetTextWithoutNotify(coins.ToString());
    }

    public void SetCoinRequirement(TMP_InputField input)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int.TryParse(input.text, out var newValue);
        if (newValue == -1)
            return;

        if (newValue < 1 || newValue > 99)
            newValue = 8;
        ChangeCoinRequirement(newValue);
        if (newValue == (int)PhotonNetwork.CurrentRoom.CustomProperties[Enums.NetRoomProperties.CoinRequirement])
            return;

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.CoinRequirement] = newValue
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        //ChangeCoinRequirement(newValue);
    }

    public void CopyRoomCode()
    {
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        LocalChatMessage($"Copied the lobby's ID: {PhotonNetwork.CurrentRoom.Name}", SYSTEM_MESSAGE_COLOR);
    }

    public void OpenDownloadsPage()
    {
        Application.OpenURL("https://github.com/vlcoo/VicMvsLO/releases/latest");
        OpenMainMenu();
    }

    public void ChangePrivate()
    {
        privateToggleRoom.SetIsOnWithoutNotify(!PhotonNetwork.CurrentRoom.IsVisible);
    }

    public void SetPrivate(Toggle toggle)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.CurrentRoom.IsVisible = !toggle.isOn;
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.ChangePrivate, null, NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void ChangeMaxPlayers(byte value)
    {
        changePlayersSlider.SetValueWithoutNotify(value);
        currentMaxPlayers.GetComponent<TextMeshProUGUI>().text = "" + value;
    }

    public void SetMaxPlayers(Slider slider)
    {
        if (!PhotonNetwork.InRoom)
        {
            sliderText.GetComponent<TMP_Text>().text = slider.value.ToString();
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
            return;

        var players = PhotonNetwork.CurrentRoom.PlayerCount;
        if (slider.value < players)
            slider.SetValueWithoutNotify(players);

        if (slider.value == PhotonNetwork.CurrentRoom.MaxPlayers)
            return;

        PhotonNetwork.CurrentRoom.MaxPlayers = (byte)slider.value;
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.ChangeMaxPlayers, (byte)slider.value, NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void SetNoRNGRules(Slider slider)
    {
        RNGSliderText.GetComponent<TMP_Text>().text = slider.value.ToString();
    }

    public void ChangeTime(int time)
    {
        timeEnabled.SetIsOnWithoutNotify(time != -1);
        UpdateSettingEnableStates();
        if (time == -1)
            return;

        var minutes = time / 60;
        var seconds = time % 60;

        timeField.SetTextWithoutNotify($"{minutes}:{seconds:D2}");
    }

    public void SetTime(TMP_InputField input)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        var seconds = ParseTimeToSeconds(input.text);

        if (seconds == -1)
            return;

        if (seconds < 1)
            seconds = 300;

        ChangeTime(seconds);

        if (seconds == (int)PhotonNetwork.CurrentRoom.CustomProperties[Enums.NetRoomProperties.Time])
            return;

        Hashtable table = new()
        {
            [Enums.NetRoomProperties.Time] = seconds
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void EnableChainableActions(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.ChainableRules] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void EnableStars(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.StarRequirement] = toggle.isOn ? int.Parse(starsText.text) : -1
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void EnableCoins(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.CoinRequirement] = toggle.isOn ? int.Parse(coinsText.text) : -1
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void EnableSpectator(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetPlayerProperties.Spectator] = toggle.isOn
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    public void EnableTeams(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.Teams] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        teamHintText.text = "Teams: " + (toggle.isOn ? "ON" : "OFF");
    }

    public void EnableFriendly(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.FriendlyFire] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void EnableShare(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.ShareStars] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void EnableTime(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.Time] = toggle.isOn ? ParseTimeToSeconds(timeField.text) : -1
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void ChangeDrawTime(bool value)
    {
        drawTimeupToggle.SetIsOnWithoutNotify(value);
    }

    public void SetDrawTime(Toggle toggle)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.DrawTime] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void SetStarcoins(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.Starcoins] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void SetCoinCount(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.ShowCoinCount] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void SetNoMap(Toggle toggle)
    {
        Hashtable properties = new()
        {
            [Enums.NetRoomProperties.NoMap] = toggle.isOn
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public int ParseTimeToSeconds(string time)
    {
        int minutes;
        int seconds;

        if (time.Contains(":"))
        {
            var split = time.Split(":");
            int.TryParse(split[0], out minutes);
            int.TryParse(split[1], out seconds);
        }
        else
        {
            minutes = 0;
            int.TryParse(time, out seconds);
        }

        if (seconds >= 60)
        {
            minutes += seconds / 60;
            seconds %= 60;
        }

        seconds = minutes * 60 + seconds;

        return seconds;
    }

    public void ChangeLobbyHeader(string name)
    {
        SetText(lobbyText, $"{name.ToValidUsername()}'s Lobby", true);
    }
}