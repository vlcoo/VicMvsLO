using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour, IOnEventCallback, IInRoomCallbacks, IConnectionCallbacks,
    IMatchmakingCallbacks
{
    private static GameManager _instance;

    public Songinator MusicSynth, MusicSynthMega, MusicSynthStarman;

    public int levelMinTileX, levelMinTileY, levelWidthTile, levelHeightTile;
    public float cameraMinY, cameraHeightY, cameraMinX = -1000, cameraMaxX = 1000;
    public bool loopingLevel = true;
    public bool raceLevel, reverberedSFX;
    public Vector3 spawnpoint;
    public Vector3 checkpoint;
    public Tilemap tilemap;
    [ColorUsage(false)] public Color levelUIColor = new(24, 178, 170);
    public bool spawnBigPowerups = true, spawnVerticalPowerups = true;
    public string levelDesigner = "", richPresenceId = "", levelName = "Unknown";
    public TileBase breakableTileReplacement;
    public TileBase[] nonReplaceableTiles;
    public int startServerTime, endServerTime = -1;
    public long startRealTime = -1, endRealTime = -1;

    public Canvas nametagCanvas;
    public GameObject nametagPrefab;
    public TMP_ColorGradient gradientMarioText, gradientLuigiText, gradientNegativeAltText;

    //Audio
    public AudioSource sfx;

    public GameObject localPlayer;
    public bool paused, loaded, started;
    public GameObject pauseUI, pausePanel, pauseButton, onScreenControls;
    public TMP_Text quitButtonLbl, rulesLbl, speedrunTimer;
    public GameObject resetHardButton;
    public Animator pausePanel1Animator;
    public bool gameover, musicEnabled;
    public int starRequirement, timedGameDuration = -1, coinRequirement, lapRequirement;
    public bool hurryup;
    public bool tenSecondCountdown;

    public int playerCount = 1;
    public List<PlayerController> players = new();
    public EnemySpawnpoint[] enemySpawnpoints;
    public FadeOutManager fader;


    public float size = 1.39f, ySize = 0.8f;

    [Range(1, 10)] public int playersToVisualize = 10;
    private readonly List<BahableEntity> bahableEntities = new();

    public readonly HashSet<Player> loadedPlayers = new();
    private readonly List<GameObject> remainingSpawns = new();

    private ParticleSystem brickBreak;

    private GameObject[] coins;
    private GoalFlagpole goal;

    //lazy mofo
    private float? middleX, minX, minY, maxX, maxY;
    public Enums.MusicState? musicState = Enums.MusicState.Normal;
    [NonSerialized] public bool needsStarcoins, showCoinCount, hideMap;

    public HashSet<Player> nonSpectatingPlayers;
    private BoundsInt origin;
    private TileBase[] originalTiles;
    private long speedrunTimerStartTimestamp;

    private bool speedup;
    private GameObject[] starSpawns;
    [NonSerialized] public bool teamsMatch;

    public static GameManager Instance
    {
        get
        {
            if (_instance)
                return _instance;

            if (SceneManager.GetActiveScene().buildIndex >= 2)
                _instance = FindObjectOfType<GameManager>();

            return _instance;
        }
        private set => _instance = value;
    }

    public MatchConditioner MatchConditioner { get; private set; }
    public Togglerizer Togglerizer { get; private set; }
    public TeamGrouper TeamGrouper { get; private set; }
    public SpectationManager SpectationManager { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        SpectationManager = GetComponent<SpectationManager>();
        MatchConditioner = GetComponent<MatchConditioner>();
        Togglerizer = GetComponent<Togglerizer>();
        TeamGrouper = GetComponent<TeamGrouper>();
        levelUIColor.a = .7f;
        coins = GameObject.FindGameObjectsWithTag("coin");
        if (Togglerizer.currentEffects.Contains("ReverseLoop") && loopingLevel) loopingLevel = !loopingLevel;
        if (loopingLevel)
        {
            cameraMinX = -1000;
            cameraMaxX = 1000;
        }

        if (reverberedSFX) sfx.outputAudioMixerGroup.audioMixer.SetFloat("SFXReverb", 0.35f);

        onScreenControls.SetActive(Utils.GetDeviceType() == Utils.DeviceType.MOBILE ||
                                   Settings.Instance.onScreenControlsAlways);
        foreach (var onScreenButton in onScreenControls.transform.GetComponentsInChildren<Image>())
            if (onScreenButton.transform.name != "Item") 
                onScreenButton.color = new Color(levelUIColor.r, levelUIColor.g, levelUIColor.b, .4f);

        InputSystem.controls.LoadBindingOverridesFromJson(GlobalController.Instance.controlsJson);

        //Spawning in editor??
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom("Debug", new RoomOptions
            {
                CustomRoomProperties = NetworkUtils.DefaultRoomProperties
            });
        }

        nonSpectatingPlayers = PhotonNetwork.CurrentRoom.Players.Values.Where(pl => !pl.IsSpectator()).ToHashSet();

        //Respawning Tilemaps
        origin = new BoundsInt(levelMinTileX, levelMinTileY, 0, levelWidthTile, levelHeightTile, 1);
        if (Togglerizer.currentEffects.Contains("AllBricks"))
            // funny...
            for (var x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
            for (var y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            for (var z = tilemap.cellBounds.min.z; z < tilemap.cellBounds.max.z; z++)
                if (!nonReplaceableTiles.Contains(tilemap.GetTile(new Vector3Int(x, y, z))))
                    tilemap.SetTile(new Vector3Int(x, y, z), breakableTileReplacement);
        originalTiles = tilemap.GetTilesBlock(origin);

        //Star spawning
        starSpawns = GameObject.FindGameObjectsWithTag("StarSpawn");
        Utils.GetCustomProperty(Enums.NetRoomProperties.StarRequirement, out starRequirement);
        Utils.GetCustomProperty(Enums.NetRoomProperties.CoinRequirement, out coinRequirement);
        Utils.GetCustomProperty(Enums.NetRoomProperties.LapRequirement, out lapRequirement);

        SceneManager.SetActiveScene(gameObject.scene);

        PhotonNetwork.IsMessageQueueRunning = true;

        if (!GlobalController.Instance.joinedAsSpectator)
        {
            localPlayer = PhotonNetwork.Instantiate("Prefabs/Players/" + Utils.GetCharacterData().prefab, spawnpoint,
                Quaternion.identity);
            localPlayer.GetComponent<Rigidbody2D>().isKinematic = true;

            RaiseEventOptions options = new()
                { Receivers = ReceiverGroup.Others, CachingOption = EventCaching.AddToRoomCache };
            SendAndExecuteEvent(Enums.NetEventIds.PlayerFinishedLoading, null, SendOptions.SendReliable, options);
        }
        else
        {
            SpectationManager.Spectating = true;
        }

        if (Togglerizer.currentEffects.Contains("HeckaSpeed"))
        {
            sfx.outputAudioMixerGroup.audioMixer.SetFloat("MasterPitch", 0f);
            Time.timeScale = 1.5f;
        }

        Utils.GetCustomProperty(Enums.NetRoomProperties.Starcoins, out needsStarcoins);
        if (raceLevel)
        {
            goal = FindObjectOfType<GoalFlagpole>();
            goal.SetUnlocked(!needsStarcoins);
            if (!needsStarcoins)
                foreach (var coin in FindObjectsOfType<Starcoin>())
                    coin.SetDisabled();
        }

        Utils.GetCustomProperty(Enums.NetRoomProperties.NoMap, out hideMap);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShowCoinCount, out showCoinCount);
        showCoinCount = showCoinCount && coinRequirement > 0;

        if (PhotonNetwork.IsMasterClient) quitButtonLbl.text = "End Match";
        if (MatchConditioner.ruleList is not null && MatchConditioner.ruleList.Count > 0)
        {
            rulesLbl.text = "";
            foreach (var entry in MatchConditioner.ruleList)
            {
                var sanitizedCond = Regex.Replace(entry.Condition, "(\\B[A-Z0-9])", " $1");
                var sanitizedAct = Regex.Replace(entry.Action, "(\\B[A-Z0-9])", " $1").Replace("Act ", "");
                rulesLbl.text += sanitizedCond + " .. " + sanitizedAct +
                                 (MatchConditioner.ruleList.Last().Equals(entry) ? "" : "\n");
            }
        }

        rulesLbl.text += "\n& " + Togglerizer.currentEffects.Count + " special effects";

        brickBreak = ((GameObject)Instantiate(Resources.Load("Prefabs/Particle/BrickBreak")))
            .GetComponent<ParticleSystem>();
        resetHardButton.SetActive(raceLevel && nonSpectatingPlayers.Count == 1 && !SpectationManager.Spectating);
    }

    public void Update()
    {
        if (gameover)
            return;

        if (endServerTime != -1)
        {
            var timeRemaining = (endServerTime - PhotonNetwork.ServerTimestamp) / 1000f;

            if (timeRemaining > 0 && !gameover)
            {
                timeRemaining -= Time.deltaTime;
                //play hurry sound if time < 60 (will play instantly when a match starts with a time limit less than 60s)
                if (!hurryup && timeRemaining <= 60)
                {
                    hurryup = true;
                    sfx.PlayOneShot(Enums.Sounds.UI_HurryUp.GetClip());
                    MatchConditioner.ConditionActioned(null, "1MinRemaining");
                }

                if (!tenSecondCountdown && timeRemaining <= 10)
                {
                    sfx.PlayOneShot(Enums.Sounds.UI_Countdown_Long.GetClip());
                    tenSecondCountdown = true;
                }

                if (timeRemaining - Time.deltaTime <= 0) CheckForWinner();
            }
        }

        if (started && musicEnabled)
        {
            var allNull = true;
            foreach (var controller in players)
                if (controller)
                {
                    allNull = false;
                    break;
                }

            if (SpectationManager.Spectating && allNull)
            {
                StartCoroutine(EndGame(null));
                return;
            }
        }

        if (musicEnabled)
            HandleMusic();
    }


    //Register callbacks & controls
    public void OnEnable()
    {
        Instance = this;
        PhotonNetwork.AddCallbackTarget(this);
        InputSystem.controls.UI.Pause.performed += OnPause;
    }

    public void OnDisable()
    {
        foreach (var gm in FindObjectsOfType<GameManager>())
            if (gm != this)
            {
                Instance = gm;
                return;
            }

        PhotonNetwork.RemoveCallbackTarget(this);
        InputSystem.controls.UI.Pause.performed -= OnPause;
    }

    public void OnDrawGizmos()
    {
        if (!tilemap)
            return;

        for (var i = 0; i < playersToVisualize; i++)
        {
            Gizmos.color = new Color((float)i / playersToVisualize, 0, 0, 0.75f);
            Gizmos.DrawCube(GetSpawnpoint(i, playersToVisualize) + Vector3.down / 4f, Vector2.one / 2f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(checkpoint, Vector2.one / 2f);

        Vector3 size = new(levelWidthTile / 2f, levelHeightTile / 2f);
        Vector3 origin = new(GetLevelMinX() + levelWidthTile / 4f, GetLevelMinY() + levelHeightTile / 4f, 1);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(origin, size);

        size = new Vector3(levelWidthTile / 2f, cameraHeightY);
        origin = new Vector3(GetLevelMinX() + levelWidthTile / 4f, cameraMinY + cameraHeightY / 2f, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin, size);


        if (!tilemap)
            return;
        for (var x = 0; x < levelWidthTile; x++)
        for (var y = 0; y < levelHeightTile; y++)
        {
            Vector3Int loc = new(x + levelMinTileX, y + levelMinTileY, 0);
            var tile = tilemap.GetTile(loc);
            if (tile is CoinTile)
                Gizmos.DrawIcon(Utils.TilemapToWorldPosition(loc, this) + Vector3.one * 0.25f, "coin");
            if (tile is PowerupTile)
                Gizmos.DrawIcon(Utils.TilemapToWorldPosition(loc, this) + Vector3.one * 0.25f, "powerup");
        }

        Gizmos.color = new Color(1, 0.9f, 0.2f, 0.2f);
        foreach (var starSpawn in GameObject.FindGameObjectsWithTag("StarSpawn"))
        {
            Gizmos.DrawCube(starSpawn.transform.position, Vector3.one);
            Gizmos.DrawIcon(starSpawn.transform.position, "star", true, new Color(1, 1, 1, 0.5f));
        }
    }

    // CONNECTION CALLBACKS
    public void OnConnected()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        GlobalController.Instance.disconnectCause = cause;
        SceneManager.LoadScene(0);
    }

    public void OnRegionListReceived(RegionHandler handler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> response)
    {
    }

    public void OnCustomAuthenticationFailed(string failure)
    {
    }

    public void OnConnectedToMaster()
    {
    }

    public void OnPlayerPropertiesUpdate(Player player, Hashtable playerProperties)
    {
    }

    public void OnRoomPropertiesUpdate(Hashtable properties)
    {
    }

    public void OnMasterClientSwitched(Player newMaster)
    {
        //TODO: chat message

        if (newMaster.IsLocal)
            //i am de captain now
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                [Enums.NetRoomProperties.HostName] = newMaster.NickName
            });
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Spectator joined. Sync the room state.
        //TODO: chat message

        if (PhotonNetwork.IsMasterClient)
        {
            Utils.GetCustomProperty(Enums.NetRoomProperties.Bans, out object[] bans);
            var banList = bans.Cast<NameIdPair>().ToList();
            if (banList.Any(nip => nip.userId == newPlayer.UserId))
            {
                PhotonNetwork.CloseConnection(newPlayer);
                return;
            }
        }

        //SYNCHRONIZE PLAYER STATE
        if (localPlayer)
            localPlayer.GetComponent<PlayerController>().UpdateGameState();

        //SYNCHRONIZE TILEMAPS
        if (PhotonNetwork.IsMasterClient)
        {
            var changes = Utils.GetTilemapChanges(originalTiles, origin, tilemap);
            RaiseEventOptions options = new()
                { CachingOption = EventCaching.DoNotCache, TargetActors = new[] { newPlayer.ActorNumber } };
            PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.SyncTilemap, changes, options, SendOptions.SendReliable);

            foreach (var coin in coins)
                if (!coin.GetComponent<SpriteRenderer>().enabled)
                    PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.SetCoinState,
                        new object[] { coin.GetPhotonView().ViewID, false }, options, SendOptions.SendReliable);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        //TODO: player disconnect message

        nonSpectatingPlayers = PhotonNetwork.CurrentRoom.Players.Values.Where(pl => !pl.IsSpectator()).ToHashSet();
        CheckIfAllLoaded();

        if (musicEnabled && FindObjectsOfType<PlayerController>().Length <= 0)
            //all players left.
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, null, NetworkUtils.EventAll,
                    SendOptions.SendReliable);
    }

    // MATCHMAKING CALLBACKS

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
    }

    public void OnLeftRoom()
    {
        OnDisconnected(DisconnectCause.DisconnectByServerLogic);
    }

    // ROOM CALLBACKS
    public void OnJoinedRoom()
    {
    }

    public void OnEvent(EventData e)
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        HandleEvent(e.Code, e.CustomData, players.ContainsKey(e.Sender) ? players[e.Sender] : null, e.Parameters);
    }

    // EVENT CALLBACK
    public void SendAndExecuteEvent(Enums.NetEventIds eventId, object parameters, SendOptions sendOption,
        RaiseEventOptions eventOptions = null)
    {
        if (eventOptions == null)
            eventOptions = NetworkUtils.EventOthers;

        HandleEvent((byte)eventId, parameters, PhotonNetwork.LocalPlayer, null);
        PhotonNetwork.RaiseEvent((byte)eventId, parameters, eventOptions, sendOption);
    }

    public void HandleEvent(byte eventId, object customData, Player sender, ParameterDictionary parameters)
    {
        var data = customData as object[];

        //Debug.Log($"id:{eventId} sender:{sender} master:{sender?.IsMasterClient ?? false}");
        switch (eventId)
        {
            // object spawning anti-cheat
            case PunEvent.Instantiation:
            {
                var table = (Hashtable)parameters.paramDict[245];
                var prefab = (string)table[0];
                var viewId = (int)table[7];

                //Debug.Log((sender.IsMasterClient ? "[H] " : "") + sender.NickName + " (" + sender.UserId + ") - Instantiating " + prefab);

                //even the host can't be trusted...
                if ((sender?.IsMasterClient ?? false) && (prefab.Contains("Static") || prefab.Contains("1-Up") ||
                                                          (musicEnabled && prefab.Contains("Player"))))
                {
                    //abandon ship
                    PhotonNetwork.Disconnect();
                    return;
                }

                //server room instantiation
                if (sender is not { IsMasterClient: not true })
                    return;

                var controller = players.FirstOrDefault(pl => sender == pl.photonView.Owner);
                var invalidProjectile =
                    controller.state != Enums.PowerupState.FireFlower && prefab.Contains("Fireball");
                invalidProjectile |= controller.state != Enums.PowerupState.IceFlower && prefab.Contains("Iceball");

                if (prefab.Contains("Enemy") || prefab.Contains("Powerup") || prefab.Contains("Static") ||
                    prefab.Contains("Bump") || prefab.Contains("BigStar") || prefab.Contains("Coin") ||
                    ((!nonSpectatingPlayers.Contains(sender) || musicEnabled) && prefab.Contains("Player")))
                {
                    PhotonNetwork.CloseConnection(sender);
                    PhotonNetwork.DestroyPlayerObjects(sender);
                }

                break;
            }
            case (byte)Enums.NetEventIds.AllFinishedLoading:
            {
                if (!(sender?.IsMasterClient ?? true))
                    return;

                if (loaded)
                    break;

                StartCoroutine(LoadingComplete((int)customData));
                break;
            }
            case (byte)Enums.NetEventIds.EndGame:
            {
                if (!(sender?.IsMasterClient ?? false) || gameover)
                    return;

                var winner = customData is string ? null : (Player)customData;
                StartCoroutine(EndGame(winner, customData as string));
                break;
            }
            case (byte)Enums.NetEventIds.SetTile:
            {
                var x = (int)data[0];
                var y = (int)data[1];
                var tilename = (string)data[2];
                Vector3Int loc = new(x, y, 0);

                var tile = Utils.GetTileFromCache(tilename);
                tilemap.SetTile(loc, tile);
                //Debug.Log($"SetTile by {sender?.NickName} ({sender?.UserId}): {tilename}");
                break;
            }
            case (byte)Enums.NetEventIds.SetTileBatch:
            {
                var x = (int)data[0];
                var y = (int)data[1];
                var width = (int)data[2];
                var height = (int)data[3];
                var tiles = (string[])data[4];
                var tileObjects = new TileBase[tiles.Length];
                for (var i = 0; i < tiles.Length; i++)
                {
                    var tile = tiles[i];
                    if (tile == "")
                        continue;

                    tileObjects[i] = (TileBase)Resources.Load("Tilemaps/Tiles/" + tile);
                }

                tilemap.SetTilesBlock(new BoundsInt(x, y, 0, width, height, 1), tileObjects);
                //Debug.Log($"SetTileBatch by {sender?.NickName} ({sender?.UserId}): {tileObjects[0]}");
                break;
            }
            case (byte)Enums.NetEventIds.ResetTiles:
            {
                if (!(sender?.IsMasterClient ?? false))
                    return;

                tilemap.SetTilesBlock(origin, originalTiles);

                foreach (var coin in coins)
                {
                    //dont use setactive cause it breaks animation cycles being synced
                    coin.GetComponent<SpriteRenderer>().enabled = true;
                    coin.GetComponent<BoxCollider2D>().enabled = true;
                }

                StartCoroutine(BigStarRespawn());

                if (!PhotonNetwork.IsMasterClient)
                    return;
                if (!Togglerizer.currentEffects.Contains("NoEnemies"))
                    foreach (var point in enemySpawnpoints)
                        point.AttemptSpawning();

                break;
            }
            case (byte)Enums.NetEventIds.SyncTilemap:
            {
                if (!(sender?.IsMasterClient ?? false))
                    return;

                var changes = (Hashtable)customData;
                //Debug.Log($"SyncTilemap by {sender?.NickName} ({sender?.UserId}): {changes}");
                Utils.ApplyTilemapChanges(originalTiles, origin, tilemap, changes);
                break;
            }
            case (byte)Enums.NetEventIds.PlayerFinishedLoading:
            {
                if (sender == null || !nonSpectatingPlayers.Contains(sender))
                    return;

                loadedPlayers.Add(sender);
                CheckIfAllLoaded();
                break;
            }
            case (byte)Enums.NetEventIds.BumpTile:
            {
                var x = (int)data[0];
                var y = (int)data[1];

                var downwards = (bool)data[2];
                var newTile = (string)data[3];
                var spawnResult = (string)data[4];
                var spawnOffset = data.Length > 5 ? (Vector2)data[5] : Vector2.zero;

                Vector3Int loc = new(x, y, 0);

                if (tilemap.GetTile(loc) == null)
                    return;

                var bump = (GameObject)Instantiate(Resources.Load("Prefabs/Bump/BlockBump"),
                    Utils.TilemapToWorldPosition(loc) + Vector3.one * 0.25f, Quaternion.identity);
                var bb = bump.GetComponentInChildren<BlockBump>();

                bb.fromAbove = downwards;
                bb.resultTile = newTile;
                bb.sprite = tilemap.GetSprite(loc);
                bb.resultPrefab = spawnResult;
                bb.spawnOffset = spawnOffset;

                tilemap.SetTile(loc, null);
                break;
            }
            case (byte)Enums.NetEventIds.SetThenBumpTile:
            {
                var x = (int)data[0];
                var y = (int)data[1];

                var downwards = (bool)data[2];
                var newTile = (string)data[3];
                var spawnResult = (string)data[4];

                Vector3Int loc = new(x, y, 0);

                tilemap.SetTile(loc, Utils.GetTileFromCache(newTile));
                //Debug.Log($"SetThenBumpTile by {sender?.NickName} ({sender?.UserId}): {newTile}");
                tilemap.RefreshTile(loc);

                var bump = (GameObject)Instantiate(Resources.Load("Prefabs/Bump/BlockBump"),
                    Utils.TilemapToWorldPosition(loc) + Vector3.one * 0.25f, Quaternion.identity);
                var bb = bump.GetComponentInChildren<BlockBump>();

                bb.fromAbove = downwards;
                bb.resultTile = newTile;
                bb.sprite = tilemap.GetSprite(loc);
                bb.resultPrefab = spawnResult;

                tilemap.SetTile(loc, null);
                break;
            }
            case (byte)Enums.NetEventIds.SetCoinState:
            {
                if (!(sender?.IsMasterClient ?? false))
                    return;

                var view = (int)data[0];
                var visible = (bool)data[1];
                var coin = PhotonView.Find(view).gameObject;
                coin.GetComponent<SpriteRenderer>().enabled = visible;
                coin.GetComponent<BoxCollider2D>().enabled = visible;
                break;
            }
            case (byte)Enums.NetEventIds.SpawnParticle:
            {
                var x = (int)data[0];
                var y = (int)data[1];
                var particleName = (string)data[2];
                var color = data.Length > 3 ? (Vector3)data[3] : new Vector3(1, 1, 1);
                var worldPos = Utils.TilemapToWorldPosition(new Vector3Int(x, y)) + new Vector3(0.25f, 0.25f);

                GameObject particle;
                if (particleName == "BrickBreak")
                {
                    brickBreak.transform.position = worldPos;
                    brickBreak.Emit(
                        new ParticleSystem.EmitParams { startColor = new Color(color.x, color.y, color.z, 1) }, 4);
                }
                else
                {
                    particle = (GameObject)Instantiate(Resources.Load("Prefabs/Particle/" + particleName), worldPos,
                        Quaternion.identity);

                    var system = particle.GetComponent<ParticleSystem>();
                    var main = system.main;
                    main.startColor = new Color(color.x, color.y, color.z, 1);
                }

                break;
            }
            case (byte)Enums.NetEventIds.SpawnResizableParticle:
            {
                var pos = (Vector2)data[0];
                var right = (bool)data[1];
                var upsideDown = (bool)data[2];
                var size = (Vector2)data[3];
                var prefab = (string)data[4];
                var particle = (GameObject)Instantiate(Resources.Load("Prefabs/Particle/" + prefab), pos,
                    Quaternion.Euler(0, 0, upsideDown ? 180 : 0));

                var sr = particle.GetComponent<SpriteRenderer>();
                sr.size = size;

                var body = particle.GetComponent<Rigidbody2D>();
                body.velocity = new Vector2(right ? 7 : -7, 6);
                body.angularVelocity = right ^ upsideDown ? -300 : 300;

                particle.transform.position += new Vector3(sr.size.x / 4f, size.y / 4f * (upsideDown ? -1 : 1));
                break;
            }
        }
    }

    private void CheckIfAllLoaded()
    {
        if (loaded || !PhotonNetwork.IsMasterClient || loadedPlayers.Count < nonSpectatingPlayers.Count)
            return;

        RaiseEventOptions options = new()
            { CachingOption = EventCaching.AddToRoomCacheGlobal, Receivers = ReceiverGroup.All };
        SendAndExecuteEvent(Enums.NetEventIds.AllFinishedLoading,
            PhotonNetwork.ServerTimestamp + (nonSpectatingPlayers.Count - 1) * 250 + 1000, SendOptions.SendReliable,
            options);
        loaded = true;
    }

    private PlayerController GetController(Player player)
    {
        foreach (var pl in players)
            if (pl.photonView.Owner == player)
                return pl;
        return null;
    }

    private IEnumerator LoadingComplete(int startTimestamp)
    {
        GlobalController.Instance.DiscordController.UpdateActivity();

        loaded = true;
        loadedPlayers.Clear();
        enemySpawnpoints = FindObjectsOfType<EnemySpawnpoint>();
        var spectating = GlobalController.Instance.joinedAsSpectator;
        var gameStarting = startTimestamp - PhotonNetwork.ServerTimestamp > 0;

        StartCoroutine(BigStarRespawn(false));

        if (PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
        {
            //clear buffered loading complete events.
            RaiseEventOptions options = new()
                { Receivers = ReceiverGroup.All, CachingOption = EventCaching.RemoveFromRoomCache };
            PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.PlayerFinishedLoading, null, options,
                SendOptions.SendReliable);
        }

        yield return new WaitForSeconds(Mathf.Max(1f, (startTimestamp - PhotonNetwork.ServerTimestamp) / 1000f));

        var loadingScript = GameObject.FindGameObjectWithTag("LoadingCanvas")?.GetComponent<LoadingWaitingOn>();
        if (loadingScript) loadingScript.StopLoading(spectating);

        started = true;

        playerCount = players.Count;
        foreach (var controllers in players.Where(controllers => controllers))
        {
            if (spectating && controllers.sfx)
            {
                controllers.sfxBrick.enabled = true;
                controllers.sfx.enabled = true;
            }

            controllers.gameObject.SetActive(spectating);

            if (TeamGrouper.teams.Count != 0) TeamGrouper.teams[controllers.character.prefab].Add(controllers);
        }

        try
        {
            ScoreboardUpdater.instance.Populate(players);
            if (Settings.Instance.scoreboardAlways)
                ScoreboardUpdater.instance.SetEnabled();
        }
        catch
        {
        }

        if (Togglerizer.currentEffects.Contains("HideSeek"))
            tilemap.transform.parent.position = new Vector3(0, 0, -5);

        if (gameStarting)
        {
            if (!GlobalController.Instance.fastLoad)
            {
                yield return new WaitForSeconds(3.5f);
                fader.FadeOut();
            }

            if (PhotonNetwork.IsMasterClient && !Togglerizer.currentEffects.Contains("NoEnemies"))
                foreach (var point in FindObjectsOfType<EnemySpawnpoint>())
                {
                    point.AttemptSpawning();
                    if (point.currentEntity == null) continue;
                    var entity = point.currentEntity.GetComponent<BahableEntity>();
                    if (entity == null) continue;
                    bahableEntities.Add(entity);
                }

            if (Togglerizer.currentEffects.Contains("NoEnemies"))
            {
                foreach (var launcher in FindObjectsOfType<BulletBillLauncher>())
                    launcher.enabled = false;
                foreach (var plant in FindObjectsOfType<PiranhaPlantController>())
                    plant.enabled = false;
            }

            if (localPlayer)
                localPlayer.GetComponent<PlayerController>().OnGameStart();
        }

        teamsMatch = TeamGrouper.isTeamsMatch;
        // else {
        //     if (MusicSynth.currentSong.hasBahs) MusicSynth.player.SetTickEvent(tick =>
        // {
        //     if (MusicSynth.nextBahTick == tick) BahAllEnemies();
        //     MusicSynth.AdvanceBah();
        // });}

        startServerTime = startTimestamp + 3500;
        foreach (var wfgs in FindObjectsOfType<WaitForGameStart>())
            wfgs.AttemptExecute();

        yield return new WaitForSeconds(1f);

        musicEnabled = true;
        MusicSynth.SetPlaybackState(Songinator.PlaybackState.PLAYING);
        Utils.GetCustomProperty(Enums.NetRoomProperties.Time, out timedGameDuration);

        startRealTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (timedGameDuration > 0)
        {
            endServerTime = startTimestamp + 4500 + timedGameDuration * 1000;
            endRealTime = startRealTime + 4500 + timedGameDuration * 1000;
        }

        GlobalController.Instance.DiscordController.UpdateActivity();

        if (loadingScript)
            SceneManager.UnloadSceneAsync("Loading");

        if (SpectationManager.Spectating) fader.SetInvisible(true);
    }

    private IEnumerator CountdownSound(float t, float times)
    {
        //The match countdown sound system. t is the tempo, and times is the # of times the sound will play (variable if match is started at 10s or less)
        for (var i = 0; i < times; i++)
        {
            if (gameover) //This is to ensure that if a win or draw occurs in the last 10 seconds, the countdown sound doesn't play past the match's length.
                yield break;

            if (i >= times * 0.7f)
            {
                //Countdown sound will speed up and play twice per second as a match's end draws near.
                sfx.PlayOneShot(Enums.Sounds.UI_Countdown_0.GetClip());
                yield return new WaitForSeconds(t / 2);
                sfx.PlayOneShot(Enums.Sounds.UI_Countdown_0.GetClip());
                yield return new WaitForSeconds(t / 2);
            }
            else
            {
                //Or it'll just play normally.
                sfx.PlayOneShot(Enums.Sounds.UI_Countdown_0.GetClip());
                yield return new WaitForSeconds(t);
            }
        }
    }

    public void FadeMusic(bool how)
    {
        if (!how)
        {
            MusicSynth.SetPlaybackState(Songinator.PlaybackState.STOPPED);
            // MusicSynthMega.player.Pause();
            // MusicSynthStarman.player.Pause();
        }
        else
        {
            if (gameover) return;
            switch (musicState)
            {
                case Enums.MusicState.Normal:
                    MusicSynth.SetPlaybackState(Songinator.PlaybackState.PLAYING, secondsFading: 0.5f);
                    break;
                case Enums.MusicState.Starman:
                    MusicSynthStarman.SetPlaybackState(Songinator.PlaybackState.PLAYING, secondsFading: 0.5f);
                    break;
                case Enums.MusicState.MegaMushroom:
                    MusicSynthMega.SetPlaybackState(Songinator.PlaybackState.PLAYING, secondsFading: 0.5f);
                    break;
            }
        }
    }

    public void SetSpectateMusic(bool how)
    {
        MusicSynth.SetSpectating(how);
        MusicSynthMega.SetSpectating(how);
        MusicSynthStarman.SetSpectating(how);
        if (musicState == Enums.MusicState.Normal) MusicSynth.SetPlaybackState(Songinator.PlaybackState.PLAYING);;
    }

    public void SetStartSpeedrunTimer(PlayerController byWhom)
    {
        if (!byWhom.photonView.IsMine || speedrunTimerStartTimestamp > 0)
            return;

        ResetStartSpeedrunTimer(false);
    }

    public void ResetStartSpeedrunTimer(bool fullReset)
    {
        if (fullReset)
        {
            var p = players[0];
            p.gotCheckpoint = false;
            p.Death(false, false);
            SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
            speedrunTimerStartTimestamp = 0;
        }
        else
        {
            speedrunTimerStartTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }

    public void ResetHardSpeedrunTimer()
    {
        GlobalController.Instance.fastLoad = true;
        Utils.GetCustomProperty(Enums.NetRoomProperties.Level, out int level);
        SceneManager.LoadSceneAsync(level + 2, LoadSceneMode.Single);
    }

    public void PlayerEnteredPipe(PlayerController whom, PipeManager pipe)
    {
        MatchConditioner.ConditionActioned(whom, "EnteredPipe");
        if (pipe.fadeOutMusic)
            MusicSynth.FadeVolume(-1, 0.5f);
    }
    
    public void PlayerEnteredDoor(PlayerController whom, DoorManager door)
    {
        MatchConditioner.ConditionActioned(whom, "EnteredDoor");
        if (door.fadeOutMusic)
            MusicSynth.FadeVolume(-1, 0.5f);
    }

    private IEnumerator EndGame(Player winner, string causeString = "")
    {
        gameover = true;
        var cancelled = causeString is "DUMMY_HOST_END";
        sfx.outputAudioMixerGroup.audioMixer.SetFloat("SFXReverb", 0f);

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { [Enums.NetRoomProperties.GameStarted] = false });
        FadeMusic(false);

        if (causeString is "DUMMY_TIMEOUT")
        {
            sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.DestroyAll();

            yield return new WaitForSecondsRealtime(1f);
            SceneManager.LoadScene("MainMenu");
            yield return null;
        }

        var text = GameObject.FindWithTag("wintext");
        var winnerCharacterIndex = -1;
        var uniqueName = "";
        if (winner != null)
        {
            winnerCharacterIndex = (int)winner.CustomProperties[Enums.NetPlayerProperties.Character];
            uniqueName = teamsMatch
                ? "The " + GlobalController.Instance.characters[winnerCharacterIndex].legalName +
                  "\n" +
                  GlobalController.Instance.characters[winnerCharacterIndex].uistring + "Team"
                : winner.GetUniqueNickname();
        }

        text.GetComponent<TMP_Text>().text = cancelled
            ? "No contest"
            : winner == null
                ? "It's a tie..."
                : $"{uniqueName} Wins!";

        if (!cancelled) yield return new WaitForSecondsRealtime(0.2f);

        var teams = winner != null && localPlayer != null && TeamGrouper.IsPlayerTeammate(
            localPlayer.GetComponent<PlayerController>(),
            GlobalController.Instance.characters[winnerCharacterIndex].prefab);
        var win = winner != null && (winner.IsLocal || teams) && !cancelled;
        var draw = winner == null && !cancelled;
        var secondsUntilMenu = cancelled ? 1.7f : 4.5f;

        if (draw)
        {
            sfx.PlayOneShot(Enums.Sounds.UI_Match_Draw.GetClip());
            text.GetComponent<Animator>().SetTrigger("startNegative");
        }
        else if (win)
        {
            sfx.PlayOneShot(Enums.Sounds.UI_Match_Win.GetClip());
            if (raceLevel)
            {
                speedrunTimer.gameObject.SetActive(true);
                var timeTotal =
                    TimeSpan.FromMilliseconds(DateTimeOffset.Now.ToUnixTimeMilliseconds() -
                                              speedrunTimerStartTimestamp);
                speedrunTimer.text = string.Format("{0:D2}:{1:D2}<size=22>.{2:D3}", (int)timeTotal.TotalMinutes,
                    timeTotal.Seconds, timeTotal.Milliseconds);
            }

            text.GetComponent<Animator>().SetTrigger("start");
            if (winnerCharacterIndex % 2 == 0)
                text.GetComponent<TMP_Text>().colorGradientPreset = gradientMarioText;
            else
                text.GetComponent<TMP_Text>().colorGradientPreset = gradientLuigiText;
        }
        else if (cancelled)
        {
            text.GetComponent<TMP_Text>().colorGradientPreset = gradientNegativeAltText;
            sfx.PlayOneShot(Enums.Sounds.UI_Match_Cancelled.GetClip());
            text.GetComponent<Animator>().SetTrigger("startNegative");
        }
        else
        {
            if (GlobalController.Instance.joinedAsSpectator)
            {
                text.GetComponent<TMP_Text>().colorGradientPreset = gradientNegativeAltText;
                sfx.PlayOneShot(Enums.Sounds.UI_Match_Concluded.GetClip());
                text.GetComponent<Animator>().SetTrigger("start");
                secondsUntilMenu += 0.5f;
            }
            else
            {
                sfx.PlayOneShot(Enums.Sounds.UI_Match_Lose.GetClip());
                text.GetComponent<Animator>().SetTrigger("startNegative");
            }
        }

        //TODO: make a results screen?

        yield return new WaitForSecondsRealtime(secondsUntilMenu);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.DestroyAll();
        SceneManager.LoadScene("MainMenu");
    }

    public IEnumerator DestroyEnvironment()
    {
        var bomb = PhotonNetwork
            .Instantiate("Prefabs/Enemy/Bobomb", spawnpoint + new Vector3(10, 0, 0), Quaternion.identity)
            .GetComponent<BobombWalk>();
        bomb.hasBigExplosion = true;
        bomb.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 1);
        yield return new WaitForSeconds(0.1f);
        bomb.photonView.RPC("Light", RpcTarget.All);
    }

    private IEnumerator BigStarRespawn(bool wait = true)
    {
        if (starRequirement < 0)
            yield break;

        if (wait)
            yield return new WaitForSeconds(10.4f - playerCount / 5f);

        if (!PhotonNetwork.IsMasterClient || gameover)
            yield break;

        bigwhile:
        while (starSpawns.Length > 0)
        {
            if (remainingSpawns.Count <= 0)
                remainingSpawns.AddRange(starSpawns);

            var index = Random.Range(0, remainingSpawns.Count);
            var spawnPos = remainingSpawns[index].transform.position;
            //Check for people camping spawn
            foreach (var hit in Physics2D.OverlapCircleAll(spawnPos, 4))
                if (hit.gameObject.CompareTag("Player") || hit.gameObject.CompareTag("bigstar"))
                {
                    //cant spawn here
                    remainingSpawns.RemoveAt(index);
                    yield return new WaitForSeconds(0.2f);
                    goto bigwhile;
                }

            PhotonNetwork.InstantiateRoomObject("Prefabs/BigStar", spawnPos, Quaternion.identity);
            remainingSpawns.RemoveAt(index);
            break;
        }
    }

    public void CreateNametag(PlayerController controller)
    {
        var nametag = Instantiate(nametagPrefab, nametagPrefab.transform.parent);
        nametag.GetComponent<UserNametag>().parent = controller;
        nametag.SetActive(!hideMap);
    }

    public void AllStarcoinsCollected()
    {
        if (!raceLevel || !needsStarcoins || !goal) return;
        goal.SetUnlocked(true);
    }

    public void WinByGoal(PlayerController whom)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        foreach (var player in players.Where(player => player != null && player != whom))
            player.photonView.RPC("Disqualify", RpcTarget.All);
        FadeMusic(false);
    }

    public void CheckForWinner()
    {
        if (gameover || !PhotonNetwork.IsMasterClient)
            return;

        var starGame = starRequirement != -1;
        var timeUp = endServerTime != -1 && endServerTime - Time.deltaTime - PhotonNetwork.ServerTimestamp < 0;
        var winningStars = -1;
        var winningLives = -1;
        List<PlayerController> winningPlayers = new();
        List<PlayerController> alivePlayers = new();
        foreach (var player in players)
        {
            if (player == null || player.lives == 0)
                continue;

            alivePlayers.Add(player);

            if ((starGame && player.stars >= starRequirement) || (starGame && timeUp))
            {
                //we're in a state where this player would win.
                //check if someone has more stars
                if (player.stars > winningStars)
                {
                    winningPlayers.Clear();
                    winningStars = player.stars;
                    winningPlayers.Add(player);
                }
                else if (player.stars == winningStars)
                {
                    winningPlayers.Add(player);
                }
            }

            if (!starGame && timeUp)
            {
                if (player.lives >= 1)
                    break;

                if (player.lives > winningLives)
                {
                    winningPlayers.Clear();
                    winningLives = player.lives;
                    winningPlayers.Add(player);
                }
                else if (player.lives == winningLives)
                {
                    winningPlayers.Add(player);
                }
            }
        }

        //LIVES CHECKS
        if (alivePlayers.Count == 0)
        {
            //everyone's dead...? ok then, draw?
            PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, null, NetworkUtils.EventAll,
                SendOptions.SendReliable);
            return;
        }

        if ((alivePlayers.Count == 1 || (teamsMatch && alivePlayers.Count != 0 &&
                                         alivePlayers.All(controller =>
                                             TeamGrouper.IsPlayerTeammate(alivePlayers[0], controller, true)))) &&
            playerCount >= 2)
        {
            //one player left alive (and not in a solo game). winner!
            PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, alivePlayers[0].photonView.Owner,
                NetworkUtils.EventAll, SendOptions.SendReliable);
            return;
        }

        //TIMED CHECKS
        if (timeUp)
        {
            Utils.GetCustomProperty(Enums.NetRoomProperties.DrawTime, out bool draw);
            //time up! check who has most stars, if a tie keep playing, if draw is on end game in a draw
            if (draw)
                // it's a draw! Thanks for playing the demo!
                PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, null, NetworkUtils.EventAll,
                    SendOptions.SendReliable);
            else if (winningPlayers.Count == 1 || (teamsMatch && winningPlayers.Count != 0 &&
                                                   winningPlayers.All(controller =>
                                                       TeamGrouper.IsPlayerTeammate(winningPlayers[0], controller,
                                                           true))))
                PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, winningPlayers[0].photonView.Owner,
                    NetworkUtils.EventAll, SendOptions.SendReliable);

            return;
        }

        if (starGame && winningStars >= starRequirement)
            if (winningPlayers.Count == 1)
                PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, winningPlayers[0].photonView.Owner,
                    NetworkUtils.EventAll, SendOptions.SendReliable);
    }

    public void BahAllEnemies()
    {
        foreach (var enemy in bahableEntities) enemy.bah();
        MatchConditioner.ConditionActioned(null, "Bah‘d");
    }

    private void PlaySong(Enums.MusicState state)
    {
        if (musicState == state)
            return;

        MusicSynth.SetPlaybackState(Songinator.PlaybackState.STOPPED);
        MusicSynthMega.SetPlaybackState(Songinator.PlaybackState.STOPPED);
        MusicSynthStarman.SetPlaybackState(Songinator.PlaybackState.STOPPED);

        musicState = state;
        if (localPlayer != null && !localPlayer.GetComponent<PlayerController>().spawned) return;

        var songPlayer = state switch
        {
            Enums.MusicState.Normal => MusicSynth,
            Enums.MusicState.MegaMushroom => MusicSynthMega,
            Enums.MusicState.Starman => MusicSynthStarman,
            _ => null
        };
        // if (songPlayer != null) songPlayer.SetPlaybackState(Songinator.PlaybackState.PLAYING);
    }

    private void HandleMusic()
    {
        var invincible = false;
        var mega = false;

        foreach (var player in players)
        {
            if (!player)
                continue;

            if (player.state == Enums.PowerupState.MegaMushroom && player.giantTimer != 15)
                mega = true;
            if (player.invincible > 0)
                invincible = true;
            if ((player.stars + 1f) / starRequirement >= 0.95f || hurryup)
                speedup = true;
            if (player.lives == 1 && players.Count <= 2)
                speedup = true;
        }

        speedup |= players.All(pl => !pl || pl.lives == 1 || pl.lives == 0);

        if (mega)
            PlaySong(Enums.MusicState.MegaMushroom);
        else if (invincible)
            PlaySong(Enums.MusicState.Starman);
        else
            PlaySong(Enums.MusicState.Normal);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        Pause();
    }

    public void Pause()
    {
        if (gameover || !musicEnabled)
            return;

        paused = !paused;
        sfx.PlayOneShot(Enums.Sounds.UI_Pause.GetClip());
        if (paused) MainMenuManager.OpenPrompt(pauseUI, pauseButton);
        else StartCoroutine(MainMenuManager.ClosePromptCoroutine(pauseUI));
    }

    public void AttemptQuit()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            sfx.PlayOneShot(Enums.Sounds.UI_Decide.GetClip());
            HostEndMatch();
            return;
        }

        Quit();
    }

    public void GiveUp()
    {
        Pause();
        var controller = localPlayer.GetComponent<PlayerController>();
        if (SpectationManager.Spectating)
        {
            sfx.PlayOneShot(Enums.Sounds.UI_Error.GetClip());
            return;
        }

        sfx.PlayOneShot(Enums.Sounds.UI_Decide.GetClip());
        controller.photonView.RPC(nameof(PlayerController.Disqualify), RpcTarget.All);
    }

    public void HostEndMatch()
    {
        Pause();
        sfx.PlayOneShot(Enums.Sounds.UI_Decide.GetClip());
        PhotonNetwork.RaiseEvent((byte)Enums.NetEventIds.EndGame, "DUMMY_HOST_END", NetworkUtils.EventAll,
            SendOptions.SendReliable);
    }

    public void Quit()
    {
        sfx.PlayOneShot(Enums.Sounds.UI_Decide.GetClip());
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }

    public float GetLevelMiddleX()
    {
        if (middleX == null)
            middleX = (GetLevelMaxX() + GetLevelMinX()) / 2;
        return (float)middleX;
    }

    public float GetLevelMinX()
    {
        if (minX == null)
            minX = levelMinTileX * tilemap.transform.localScale.x + tilemap.transform.position.x;
        return (float)minX;
    }

    public float GetLevelMinY()
    {
        if (minY == null)
            minY = levelMinTileY * tilemap.transform.localScale.y + tilemap.transform.position.y;
        return (float)minY;
    }

    public float GetLevelMaxX()
    {
        if (maxX == null)
            maxX = (levelMinTileX + levelWidthTile) * tilemap.transform.localScale.x + tilemap.transform.position.x;
        return (float)maxX;
    }

    public float GetLevelMaxY()
    {
        if (maxY == null)
            maxY = (levelMinTileY + levelHeightTile) * tilemap.transform.localScale.y + tilemap.transform.position.y;
        return (float)maxY;
    }

    public Vector3 GetSpawnpoint(int playerIndex, int players = -1)
    {
        if (players <= -1)
            players = playerCount;
        if (players == 0)
            players = 1;

        var comp = (float)playerIndex / players * 2 * Mathf.PI + Mathf.PI / 2f + Mathf.PI / (2 * players);
        var scale = (2 - (players + 1f) / players) * size;
        var spawn = spawnpoint +
                    new Vector3(Mathf.Sin(comp) * scale, Mathf.Cos(comp) * (players > 2 ? scale * ySize : 0), 0);
        if (spawn.x < GetLevelMinX())
            spawn += new Vector3(levelWidthTile / 2f, 0);
        if (spawn.x > GetLevelMaxX())
            spawn -= new Vector3(levelWidthTile / 2f, 0);
        return spawn;
    }
}