using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;
using NSMB.Extensions;
using NSMB.UI.MainMenu;
using NSMB.Utils;
using NSMB.Game;

public class SessionData : NetworkBehaviour, IStateAuthorityChanged {

    //---Static Variables
    public static SessionData Instance;

#pragma warning disable CS0414
    //---Default values
    private readonly sbyte defaultStarRequirement = 10;
    private readonly byte defaultCoinRequirement = 8;
    private readonly byte defaultLapRequirement = 1;
    private readonly NetworkBool defaultCustomPowerups = true;
    private readonly HashSet<Rule> defaultRules = new(new MatchConditioner.RuleEqualityComparer());
#pragma warning restore CS0414

    //---Networked Variables
    [Networked] public byte MaxPlayers { get; set; }
    [Networked] public NetworkBool PrivateRoom { get; set; }
    [Networked] public TickTimer GameStartTimer { get; set; }
    [Networked] public NetworkBool GameStarted { get; set; }
    [Networked] public byte Level { get; set; }
    [Networked(Default = nameof(defaultStarRequirement))] public sbyte StarRequirement { get; set; }
    [Networked(Default = nameof(defaultCoinRequirement))] public byte CoinRequirement { get; set; }
    [Networked(Default = nameof(defaultLapRequirement))] public byte LapRequirement { get; set; }
    // TODO vcmi: figure out how to network this. maybe have it always serialized (in a string)?
    /* [Networked(Default = nameof(defaultRules))] */ public HashSet<Rule> ActiveRules { get; set; }
    [Networked] public byte Lives { get; set; }
    [Networked] public byte Timer { get; set; }
    [Networked] public NetworkBool DrawOnTimeUp { get; set; }
    [Networked(Default = nameof(defaultCustomPowerups))] public NetworkBool CustomPowerups { get; set; }
    [Networked] public NetworkBool Teams { get; set; }
    [Networked] public byte AlternatingMusicIndex { get; set; }
    [Networked, Capacity(10)] public NetworkDictionary<PlayerRef, PlayerData> PlayerDatas => default;

    //---Properties
    private MainMenuChat Chat {
        get {
            if (MainMenuManager.Instance) {
                return MainMenuManager.Instance.chat;
            }

            return null;
        }
    }

    //---Public Variables
    public readonly HashSet<Guid> bannedIds = new();

    //---Private Variables
    private readonly Dictionary<Guid, uint> wins = new();
    private Tick lastUpdatedTick;
    private float lastStartCancelTime = -10f;
    private bool playedStartSound;
    private ChangeDetector changeDetector;
    private PropertyReader<byte> levelPropertyReader;

    public void Awake() {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void Spawned() {
        Instance = this;

        foreach (var data in FindObjectsOfType<PlayerData>()) {
            PlayerDatas.Add(data.Owner, data);
        }

        if (HasStateAuthority) {
            PrivateRoom = !Runner.SessionInfo.IsVisible;
            if (MaxPlayers == 0) {
                NetworkUtils.GetSessionProperty(Runner.SessionInfo, Enums.NetRoomProperties.BoolProperties, out int packedBoolProperties);
                NetworkUtils.IntegerProperties intProperties = (NetworkUtils.IntegerProperties) packedBoolProperties;
                MaxPlayers = (byte) intProperties.maxPlayers;
            }

            NetworkHandler.OnConnectRequest += OnConnectRequest;

            if (Runner.IsResume) {
                GameStartTimer = TickTimer.None;
                GameStarted = false;
            }
        }

        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        levelPropertyReader = GetPropertyReader<byte>(nameof(Level));
        gameObject.name = "SessionData (" + Runner.SessionInfo.Name + ")";

        if (MainMenuManager.Instance) {
            MainMenuManager.Instance.roomSettingsCallbacks.UpdateAllSettings(Instance, false);
        }

        if (GameManager.Instance) {
            GameManager.Instance.SetGameTimestamps();
        }
    }

    public override void Render() {

        if (!GameStarted && GameStartTimer.IsActive(Runner) && MainMenuManager.Instance) {
            int ticksLeft = (GameStartTimer.RemainingTicks(Runner) ?? 0) + 1;
            if (ticksLeft % Runner.TickRate == 0) {
                // Send countdown
                int seconds = ticksLeft / Runner.TickRate;
                MainMenuManager.Instance.OnCountdownTick(seconds);
            }
        }

        foreach (var change in changeDetector.DetectChanges(this, out var previous, out _)) {
            switch (change) {
            case nameof(MaxPlayers):
            case nameof(PrivateRoom):
            case nameof(GameStarted):
            case nameof(Level):
            case nameof(StarRequirement):
            case nameof(CoinRequirement):
            case nameof(Lives):
            case nameof(Timer):
            case nameof(DrawOnTimeUp):
            case nameof(CustomPowerups):
            case nameof(Teams):
                OnSettingChanged(previous);
                break;
            case nameof(GameStartTimer):
                OnGameStartTimerChanged();
                break;
            }
        }
    }

    public override void FixedUpdateNetwork() {
        if (!GameStarted && GameStartTimer.IsRunning && MainMenuManager.Instance) {

            if (!MainMenuManager.Instance.IsRoomConfigurationValid()) {
                GameStartTimer = TickTimer.None;
                return;
            }

            if (GameStartTimer.Expired(Runner)) {
                // Start game
                Rpc_StartGame();
            }
        }
    }

    public void StateAuthorityChanged() {
        if (HasStateAuthority) {
            UpdateProperty(Enums.NetRoomProperties.HostName, Runner.GetLocalPlayerData().RawNickname);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        NetworkHandler.OnConnectRequest -= OnConnectRequest;
    }

    private bool OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
        try {
            ConnectionToken connectionToken = ConnectionToken.Deserialize(token);

            // Connection token not signed by the auth server
            if (!connectionToken.HasValidSignature()) {
                Debug.Log($"[Network] Received an unsigned connection token from an incoming connection: {request.RemoteAddress}");
                return false;
            }

            if (bannedIds.Contains(connectionToken.signedData.UserId)) {
                Debug.Log($"[Network] Rejecting join from a banned userid ({connectionToken.signedData.UserId})");
                return false;
            }

            return true;
        } catch {
            // Malformed connection token.
            Debug.Log($"[Network] Received a malformed connection token from an incoming connection: {request.RemoteAddress}");
            return false;
        }
    }

    public void SaveWins(PlayerData data) {
        wins[data.UserId] = data.Wins;
    }

    public void LoadWins(PlayerData data) {
        if (wins.ContainsKey(data.UserId)) {
            data.Wins = wins[data.UserId];
        }
    }

    public void AddBan(PlayerData data) {
        bannedIds.Add(data.UserId);
    }

    public void SetGameStarted(bool value) {
        GameStarted = value;
        UpdateBooleanProperties();
    }

    public void SetMaxPlayers(byte value) {
        MaxPlayers = value;
        UpdateIntProperties();
    }

    public void SetLevel(byte value) {
        Level = value;
        UpdateIntProperties();
    }

    public void SetStarRequirement(sbyte value) {
        StarRequirement = value;
        UpdateIntProperties();
    }

    public void SetLapRequirement(byte value) {
        LapRequirement = value;
        UpdateIntProperties();
    }

    public void SetCoinRequirement(byte value) {
        CoinRequirement = value;
        UpdateIntProperties();
    }

    public void SetLives(byte value) {
        Lives = value;
        UpdateIntProperties();
    }

    public void SetTimer(byte value) {
        Timer = value;
        UpdateIntProperties();
    }

    public void SetDrawOnTimeUp(bool value) {
        DrawOnTimeUp = value;
        // No session property here.
    }

    public void SetCustomPowerups(bool value) {
        CustomPowerups = value;
        UpdateBooleanProperties();
    }

    public void SetTeams(bool value) {
        Teams = value;
        UpdateBooleanProperties();
    }

    public void SetPrivateRoom(bool value) {
        PrivateRoom = value;
        Runner.SessionInfo.IsVisible = !value;
        // No session property here.
    }

    public bool AddRule(Rule rule) {
        if (MatchConditioner.IsRuleForbidden(rule) || !ActiveRules.Add(rule))
            return false;

        UpdateActiveRulesProperty();
        return true;
    }

    public void RemoveRule(Rule rule) {
        ActiveRules.Remove(rule);
        UpdateActiveRulesProperty();
    }

    public void ClearRules() {
        ActiveRules.Clear();
        UpdateActiveRulesProperty();
    }

    private void UpdateIntProperties() {
        NetworkUtils.IntegerProperties properties = new() {
            level = Level,
            timer = Timer,
            lives = Lives,
            coinRequirement = CoinRequirement,
            starRequirement = StarRequirement,
            maxPlayers = MaxPlayers
        };
        UpdateProperty(Enums.NetRoomProperties.IntProperties, (int) properties);
    }

    private void UpdateBooleanProperties() {
        NetworkUtils.BooleanProperties properties = new() {
            gameStarted = GameStarted,
            customPowerups = CustomPowerups,
            teams = Teams,
        };
        UpdateProperty(Enums.NetRoomProperties.BoolProperties, (int) properties);
    }

    private void UpdateActiveRulesProperty() {
        string rulesJson = "";
        // TODO vcmi: Serialize ActiveRules to JSON
        UpdateProperty(Enums.NetRoomProperties.ActiveRulesProperty, rulesJson);
    }

    public void UpdateProperty(string property, SessionProperty value) {
        Runner.SessionInfo.UpdateCustomProperties(new() {
            [property] = value
        });
    }

    public void Disconnect(PlayerRef target) {
        if (Runner.Topology == Topologies.ClientServer) {
            Runner.Disconnect(target);
        } else {
            Rpc_Disconnect(target);
        }
    }

    //---RPCs
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = false)]
    public void Rpc_Disconnect([RpcTarget] PlayerRef target) {
        Runner.Shutdown(shutdownReason: ShutdownReason.ConnectionRefused);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void Rpc_ChatIncomingMessage(string message, RpcInfo info = default) {
        ChatManager.Instance.IncomingPlayerMessage(message, info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_ChatDisplayMessage(string message, PlayerRef player) {
        ChatManager.Instance.DisplayPlayerMessage(message, player);
    }

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void Rpc_UpdateTypingCounter(RpcInfo info = default) {
        if (Chat) {
            Chat.SetTypingIndicator(info.Source);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void Rpc_StartGame() {

        // Set PlayerIDs and spectator values for players
        sbyte count = 0;
        foreach (PlayerRef player in Runner.ActivePlayers) {
            PlayerData data = player.GetPlayerData();
            if (!data) {
                continue;
            }

            data.IsCurrentlySpectating = data.IsManualSpectator;
            data.IsLoaded = false;
            data.IsReady = false;

            if (data.IsCurrentlySpectating) {
                data.PlayerId = -1;
                data.Team = -1;
                continue;
            }

            data.PlayerId = count;
            if (!Teams) {
                data.Team = count;
            } else if (data.Team == -1) {
                data.Team = 0;
            }

            count++;
        }

        ChatManager.Instance.AddSystemMessage("ui.inroom.chat.server.started", ChatManager.Red);
        SetGameStarted(true);

        // Load the correct scene
        if (Runner.IsSceneAuthority) {
            Runner.LoadScene(MainMenuManager.Instance.GetCurrentSceneRef(), LoadSceneMode.Single);
        }
    }

    //---OnChangeds
    public void OnSettingChanged(NetworkBehaviourBuffer previous) {
        Tick currentTick = Object.Runner.Tick;
        if (currentTick <= lastUpdatedTick) {
            return;
        }

        //no "started" setting to update
        int oldLevel = previous.Read(levelPropertyReader);

        if (MainMenuManager.Instance && MainMenuManager.Instance.roomSettingsCallbacks) {
            MainMenuManager.Instance.roomSettingsCallbacks.UpdateAllSettings(this, oldLevel != Level);
        }

        lastUpdatedTick = currentTick;
    }

    public void OnGameStartTimerChanged() {
        if (!MainMenuManager.Instance) {
            return;
        }

        float time = Runner.SimulationTime;

        if (!GameStartTimer.IsRunning) {
            if (playedStartSound) {
                ChatManager.Instance.AddSystemMessage("ui.inroom.chat.server.startcancelled", ChatManager.Red);
            }
            lastStartCancelTime = Runner.SimulationTime;
            MainMenuManager.Instance.OnCountdownTick(-1);
            playedStartSound = false;
        } else {
            if (lastStartCancelTime + 3f < time || Runner.GetLocalPlayerData().IsRoomOwner) {
                ChatManager.Instance.AddSystemMessage("ui.inroom.chat.server.starting", ChatManager.Red, "countdown", Mathf.CeilToInt((GameStartTimer.RemainingTime(Runner) - 0.01f) ?? 0).ToString());
                MainMenuManager.Instance.sfx.PlayOneShot(Enums.Sounds.UI_FileSelect);
                playedStartSound = true;
            }
            MainMenuManager.Instance.OnCountdownTick(3);
        }
    }
}
