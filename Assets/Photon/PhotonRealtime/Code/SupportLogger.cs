// ----------------------------------------------------------------------------
// <copyright file="SupportLogger.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Implements callbacks of the Realtime API to logs selected information
//   for support cases.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif


using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ExitGames.Client.Photon;


namespace Photon.Realtime
{
#if SUPPORTED_UNITY
    using UnityEngine;
#endif

#if SUPPORTED_UNITY || NETFX_CORE
    using Hashtable = Hashtable;
#endif

    /// <summary>
    ///     Helper class to debug log basic information about Photon client and vital traffic statistics.
    /// </summary>
    /// <remarks>
    ///     Set SupportLogger.Client for this to work.
    /// </remarks>
#if SUPPORTED_UNITY
    [DisallowMultipleComponent]
#if PUN_2_OR_NEWER || FUSION_UNITY
    [AddComponentMenu("")] // hide from Unity Menus and searches
#endif
    public class SupportLogger : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks,
        ILobbyCallbacks, IErrorInfoCallback
#else
	public class SupportLogger : IConnectionCallbacks, IInRoomCallbacks, IMatchmakingCallbacks , ILobbyCallbacks
#endif
    {
        /// <summary>
        ///     Toggle to enable or disable traffic statistics logging.
        /// </summary>
        public bool LogTrafficStats = true;

        private bool loggedStillOfflineMessage;

        private LoadBalancingClient client;

        private Stopwatch startStopwatch;

        /// helps skip the initial OnApplicationPause call, which is not really of interest on start
        private bool initialOnApplicationPauseSkipped;

        private int pingMax;
        private int pingMin;

        /// <summary>
        ///     Photon client to log information and statistics from.
        /// </summary>
        public LoadBalancingClient Client
        {
            get => client;
            set
            {
                if (client != value)
                {
                    if (client != null) client.RemoveCallbackTarget(this);
                    client = value;
                    if (client != null) client.AddCallbackTarget(this);
                }
            }
        }


#if SUPPORTED_UNITY
        protected void Start()
        {
            LogBasics();

            if (startStopwatch == null)
            {
                startStopwatch = new Stopwatch();
                startStopwatch.Start();
            }
        }

        protected void OnDestroy()
        {
            Client = null; // will remove this SupportLogger as callback target
        }

        protected void OnApplicationPause(bool pause)
        {
            if (!initialOnApplicationPauseSkipped)
            {
                initialOnApplicationPauseSkipped = true;
                return;
            }

            Debug.Log(string.Format("{0} SupportLogger OnApplicationPause({1}). Client: {2}.", GetFormattedTimestamp(),
                pause, client == null ? "null" : client.State.ToString()));
        }

        protected void OnApplicationQuit()
        {
            CancelInvoke();
        }
#endif

        public void StartLogStats()
        {
#if SUPPORTED_UNITY
            InvokeRepeating("LogStats", 10, 10);
#else
            Debug.Log("Not implemented for non-Unity projects.");
#endif
        }

        public void StopLogStats()
        {
#if SUPPORTED_UNITY
            CancelInvoke("LogStats");
#else
            Debug.Log("Not implemented for non-Unity projects.");
#endif
        }

        private void StartTrackValues()
        {
#if SUPPORTED_UNITY
            InvokeRepeating("TrackValues", 0.5f, 0.5f);
#else
            Debug.Log("Not implemented for non-Unity projects.");
#endif
        }

        private void StopTrackValues()
        {
#if SUPPORTED_UNITY
            CancelInvoke("TrackValues");
#else
            Debug.Log("Not implemented for non-Unity projects.");
#endif
        }

        private string GetFormattedTimestamp()
        {
            if (startStopwatch == null)
            {
                startStopwatch = new Stopwatch();
                startStopwatch.Start();
            }

            var span = startStopwatch.Elapsed;
            if (span.Minutes > 0) return string.Format("[{0}:{1}.{1}]", span.Minutes, span.Seconds, span.Milliseconds);

            return string.Format("[{0}.{1}]", span.Seconds, span.Milliseconds);
        }


        // called via InvokeRepeatedly
        private void TrackValues()
        {
            if (client != null)
            {
                var currentRtt = client.LoadBalancingPeer.RoundTripTime;
                if (currentRtt > pingMax) pingMax = currentRtt;
                if (currentRtt < pingMin) pingMin = currentRtt;
            }
        }


        /// <summary>
        ///     Debug logs vital traffic statistics about the attached Photon Client.
        /// </summary>
        public void LogStats()
        {
            if (client == null || client.State == ClientState.PeerCreated) return;

            if (LogTrafficStats)
                Debug.Log(string.Format("{0} SupportLogger {1} Ping min/max: {2}/{3}", GetFormattedTimestamp(),
                    client.LoadBalancingPeer.VitalStatsToString(false), pingMin, pingMax));
        }

        /// <summary>
        ///     Debug logs basic information (AppId, AppVersion, PeerID, Server address, Region) about the attached Photon Client.
        /// </summary>
        private void LogBasics()
        {
            if (client != null)
            {
                var buildProperties = new List<string>(10);
#if SUPPORTED_UNITY
                buildProperties.Add(Application.unityVersion);
                buildProperties.Add(Application.platform.ToString());
#endif
#if ENABLE_IL2CPP
                buildProperties.Add("ENABLE_IL2CPP");
#endif
#if ENABLE_MONO
                buildProperties.Add("ENABLE_MONO");
#endif
#if DEBUG
                buildProperties.Add("DEBUG");
#endif
#if MASTER
                buildProperties.Add("MASTER");
#endif
#if NET_4_6
                buildProperties.Add("NET_4_6");
#endif
#if NET_STANDARD_2_0
                buildProperties.Add("NET_STANDARD_2_0");
#endif
#if NETFX_CORE
                buildProperties.Add("NETFX_CORE");
#endif
#if NET_LEGACY
                buildProperties.Add("NET_LEGACY");
#endif
#if UNITY_64
                buildProperties.Add("UNITY_64");
#endif
#if UNITY_FUSION
                buildProperties.Add("UNITY_FUSION");
#endif


                var sb = new StringBuilder();

                var appIdShort = string.IsNullOrEmpty(client.AppId) || client.AppId.Length < 8
                    ? client.AppId
                    : string.Concat(client.AppId.Substring(0, 8), "***");

                sb.AppendFormat("{0} SupportLogger Info: ", GetFormattedTimestamp());
                sb.AppendFormat("AppID: \"{0}\" AppVersion: \"{1}\" Client: v{2} ({4}) Build: {3} ", appIdShort,
                    client.AppVersion, PhotonPeer.Version, string.Join(", ", buildProperties.ToArray()),
                    client.LoadBalancingPeer.TargetFramework);
                if (client != null && client.LoadBalancingPeer != null &&
                    client.LoadBalancingPeer.SocketImplementation != null)
                    sb.AppendFormat("Socket: {0} ", client.LoadBalancingPeer.SocketImplementation.Name);

                sb.AppendFormat("UserId: \"{0}\" AuthType: {1} AuthMode: {2} {3} ", client.UserId,
                    client.AuthValues != null ? client.AuthValues.AuthType.ToString() : "N/A", client.AuthMode,
                    client.EncryptionMode);

                sb.AppendFormat("State: {0} ", client.State);
                sb.AppendFormat("PeerID: {0} ", client.LoadBalancingPeer.PeerID);
                sb.AppendFormat("NameServer: {0} Current Server: {1} IP: {2} Region: {3} ", client.NameServerHost,
                    client.CurrentServerAddress, client.LoadBalancingPeer.ServerIpAddress, client.CloudRegion);

                Debug.LogWarning(sb.ToString());
            }
        }


        public void OnConnected()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnConnected().");
            pingMax = 0;
            pingMin = client.LoadBalancingPeer.RoundTripTime;
            LogBasics();

            if (LogTrafficStats)
            {
                client.LoadBalancingPeer.TrafficStatsEnabled = false;
                client.LoadBalancingPeer.TrafficStatsEnabled = true;
                StartLogStats();
            }

            StartTrackValues();
        }

        public void OnConnectedToMaster()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnConnectedToMaster().");
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnFriendListUpdate(friendList).");
        }

        public void OnJoinedLobby()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinedLobby(" + client.CurrentLobby + ").");
        }

        public void OnLeftLobby()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLeftLobby().");
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log(
                GetFormattedTimestamp() + " SupportLogger OnCreateRoomFailed(" + returnCode + "," + message + ").");
        }

        public void OnJoinedRoom()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinedRoom(" + client.CurrentRoom + "). " +
                      client.CurrentLobby + " GameServer:" + client.GameServerAddress);
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinRoomFailed(" + returnCode + "," + message + ").");
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log(
                GetFormattedTimestamp() + " SupportLogger OnJoinRandomFailed(" + returnCode + "," + message + ").");
        }

        public void OnCreatedRoom()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCreatedRoom(" + client.CurrentRoom + "). " +
                      client.CurrentLobby + " GameServer:" + client.GameServerAddress);
        }

        public void OnLeftRoom()
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLeftRoom().");
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            StopLogStats();
            StopTrackValues();

            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnDisconnected(" + cause + ").");
            LogBasics();
            LogStats();
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRegionListReceived(regionHandler).");
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRoomListUpdate(roomList). roomList.Count: " +
                      roomList.Count);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerEnteredRoom(" + newPlayer + ").");
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerLeftRoom(" + otherPlayer + ").");
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRoomPropertiesUpdate(propertiesThatChanged).");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerPropertiesUpdate(targetPlayer,changedProps).");
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnMasterClientSwitched(" + newMasterClient + ").");
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationResponse(" + data.ToStringFull() +
                      ").");
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationFailed(" + debugMessage + ").");
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLobbyStatisticsUpdate(lobbyStatistics).");
        }


#if !SUPPORTED_UNITY
        private static class Debug
        {
            public static void Log(string msg)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
            public static void LogWarning(string msg)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
            public static void LogError(string msg)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
        }
#endif

        public void OnErrorInfo(ErrorInfo errorInfo)
        {
            Debug.LogError(errorInfo.ToString());
        }
    }
}