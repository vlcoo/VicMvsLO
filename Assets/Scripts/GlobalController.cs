using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

public class GlobalController : Singleton<GlobalController>, IInRoomCallbacks, ILobbyCallbacks
{
    public PlayerColorSet[] skins;
    public TMP_ColorGradient logoGradient;
    public TMP_SpriteAsset emotesAsset;

    public GameObject ndsCanvas, fourByThreeImage, anyAspectImage;

    public RenderTexture ndsTexture;
    public PlayerData[] characters;
    public int[] emoteKeyMapping;
    public Settings settings;
    public string controlsJson;

    public bool joinedAsSpectator, checkedForVersion, fastLoad;
    public List<string> EMOTE_NAMES = new();
    public DisconnectCause? disconnectCause = null;

    public List<SpecialPlayer> SPECIAL_PLAYERS = new();

    private int windowWidth, windowHeight;
    public DiscordController DiscordController { get; private set; }
    public DeviceRumbler rumbler { get; private set; }

    public void Awake()
    {
        if (!InstanceCheck())
            return;

        Instance = this;
        settings = GetComponent<Settings>();
        DiscordController = GetComponent<DiscordController>();
        rumbler = GetComponent<DeviceRumbler>();
        PopulateEmoteNames();
        PopulateSpecialPlayers();

        PhotonNetwork.AddCallbackTarget(this);
    }

    [Obsolete]
    public void Start()
    {
        //Photon settings.
        PhotonPeer.RegisterType(typeof(NameIdPair), 69, NameIdPair.Serialize, NameIdPair.Deserialize);
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.MaxResendsBeforeDisconnect = 15;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try {
            ReplaceWinProc();
        } catch {}
#endif
    }

    public void Update()
    {
        var currentWidth = Screen.width;
        var currentHeight = Screen.height;

        if (settings.ndsResolution && SceneManager.GetActiveScene().buildIndex != 0)
        {
            var aspect = (float)currentWidth / currentHeight;
            var targetHeight = 224;
            var targetWidth = (int)(targetHeight * (settings.fourByThreeRatio ? 4 / 3f : aspect));
            if (ndsTexture == null || ndsTexture.width != targetWidth || ndsTexture.height != targetHeight)
            {
                if (ndsTexture != null)
                    ndsTexture.Release();
                ndsTexture = RenderTexture.GetTemporary(targetWidth, targetHeight);
                ndsTexture.filterMode = FilterMode.Point;
                ndsTexture.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            }

            ndsCanvas.SetActive(true);
        }
        else
        {
            ndsCanvas.SetActive(false);
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        NetworkUtils.nicknameCache.Remove(newPlayer.UserId);
        PopulateSpecialPlayers();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        NetworkUtils.nicknameCache.Remove(otherPlayer.UserId);
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
    }

    public void OnJoinedLobby()
    {
        NetworkUtils.nicknameCache.Clear();
    }

    public void OnLeftLobby()
    {
        NetworkUtils.nicknameCache.Clear();
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void CreateInstance()
    {
        Instantiate(Resources.Load("Prefabs/Static/GlobalController"));
    }

    private async void PopulateSpecialPlayers()
    {
        SPECIAL_PLAYERS.Clear();

        //get http results
        var request = (HttpWebRequest)WebRequest.Create(PhotonExtensions.SPECIALS_URL);
        request.Accept = "application/json";
        request.UserAgent = "vlcoo/VicMvsLO";

        var response = (HttpWebResponse)await request.GetResponseAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            return;

        var json = await new StreamReader(response.GetResponseStream()!).ReadToEndAsync();
        var deserializedJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (deserializedJson == null) return;
        foreach (var player in deserializedJson)
        {
            var sp = new SpecialPlayer(player.Key.Split("|")[1], int.Parse(player.Value));
            SPECIAL_PLAYERS.Add(sp);
        }
    }

    private void PopulateEmoteNames()
    {
        emotesAsset.spriteCharacterTable.ForEach(character => EMOTE_NAMES.Add(character.name));
    }

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private static IntPtr oldWndProcPtr;

    private static void ReplaceWinProc()
    {
        // get window that we're using
        var hwnd = GetActiveWindow();
        // Get pointer to our replacement WndProc.
        WndProcDelegate newWndProc = WndProc;
        var newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
        // Override Unity's WndProc with our custom one, and save the original WndProc (Unity's) so we can use it later for non-focus related messages.
        oldWndProcPtr = SetWindowLongPtr(hwnd, -4, newWndProcPtr); // (GWLP_WNDPROC == -4)
    }

    private const uint WM_INITMENUPOPUP = 0x0117;
    private const uint WM_CLOSE = 0x0010;
    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_INITMENUPOPUP:
            {
                //prevent menu bar (the one that appears when right click top bar, has "move" etc
                //from appearing, to avoid the game pausing when the menu bar is active

                //bit 16 = top menu bar
                if (lParam.ToInt32() >> 16 == 1)
                {
                    //cancel the menu from popping up
                    SendMessage(hWnd, 0x001F, 0, 0);
                    return IntPtr.Zero;
                }

                break;
            }
            case WM_CLOSE:
            {
                //we're closing, pass back to our existing wndproc to avoid crashing
                SetWindowLongPtr(hWnd, -4, oldWndProcPtr);
                break;
            }
        }

        return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
    }
#endif
}