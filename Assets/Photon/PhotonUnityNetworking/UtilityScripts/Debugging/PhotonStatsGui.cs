// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonStatsGui.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities,
// </copyright>
// <summary>
// Basic GUI to show traffic and health statistics of the connection to Photon,
// toggled by shift+tab.
// Part of the [Optional GUI](@ref optionalGui).
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Basic GUI to show traffic and health statistics of the connection to Photon,
    ///     toggled by shift+tab.
    /// </summary>
    /// <remarks>
    ///     The shown health values can help identify problems with connection losses or performance.
    ///     Example:
    ///     If the time delta between two consecutive SendOutgoingCommands calls is a second or more,
    ///     chances rise for a disconnect being caused by this (because acknowledgements to the server
    ///     need to be sent in due time).
    /// </remarks>
    /// \ingroup optionalGui
    public class PhotonStatsGui : MonoBehaviour
    {
        /// <summary>Shows or hides GUI (does not affect if stats are collected).</summary>
        public bool statsWindowOn = true;

        /// <summary>Option to turn collecting stats on or off (used in Update()).</summary>
        public bool statsOn = true;

        /// <summary>Shows additional "health" values of connection.</summary>
        public bool healthStatsVisible;

        /// <summary>Shows additional "lower level" traffic stats.</summary>
        public bool trafficStatsOn;

        /// <summary>Show buttons to control stats and reset them.</summary>
        public bool buttonsOn;

        /// <summary>Positioning rect for window.</summary>
        public Rect statsRect = new(0, 100, 200, 50);

        /// <summary>Unity GUI Window ID (must be unique or will cause issues).</summary>
        public int WindowId = 100;


        public void Start()
        {
            if (statsRect.x <= 0) statsRect.x = Screen.width - statsRect.width;
        }

        /// <summary>Checks for shift+tab input combination (to toggle statsOn).</summary>
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
            {
                statsWindowOn = !statsWindowOn;
                statsOn = true; // enable stats when showing the window
            }
        }

        public void OnGUI()
        {
            if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled != statsOn)
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = statsOn;

            if (!statsWindowOn) return;

            statsRect = GUILayout.Window(WindowId, statsRect, TrafficStatsWindow, "Messages (shift+tab)");
        }

        public void TrafficStatsWindow(int windowID)
        {
            var statsToLog = false;
            var gls = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsGameLevel;
            var elapsedMs = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsElapsedMs / 1000;
            if (elapsedMs == 0) elapsedMs = 1;

            GUILayout.BeginHorizontal();
            buttonsOn = GUILayout.Toggle(buttonsOn, "buttons");
            healthStatsVisible = GUILayout.Toggle(healthStatsVisible, "health");
            trafficStatsOn = GUILayout.Toggle(trafficStatsOn, "traffic");
            GUILayout.EndHorizontal();

            var total = string.Format("Out {0,4} | In {1,4} | Sum {2,4}", gls.TotalOutgoingMessageCount,
                gls.TotalIncomingMessageCount, gls.TotalMessageCount);
            var elapsedTime = string.Format("{0}sec average:", elapsedMs);
            var average = string.Format("Out {0,4} | In {1,4} | Sum {2,4}", gls.TotalOutgoingMessageCount / elapsedMs,
                gls.TotalIncomingMessageCount / elapsedMs, gls.TotalMessageCount / elapsedMs);
            GUILayout.Label(total);
            GUILayout.Label(elapsedTime);
            GUILayout.Label(average);

            if (buttonsOn)
            {
                GUILayout.BeginHorizontal();
                statsOn = GUILayout.Toggle(statsOn, "stats on");
                if (GUILayout.Button("Reset"))
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsReset();
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = true;
                }

                statsToLog = GUILayout.Button("To Log");
                GUILayout.EndHorizontal();
            }

            var trafficStatsIn = string.Empty;
            var trafficStatsOut = string.Empty;
            if (trafficStatsOn)
            {
                GUILayout.Box("Traffic Stats");
                trafficStatsIn = "Incoming: \n" + PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsIncoming;
                trafficStatsOut =
                    "Outgoing: \n" + PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsOutgoing;
                GUILayout.Label(trafficStatsIn);
                GUILayout.Label(trafficStatsOut);
            }

            var healthStats = string.Empty;
            if (healthStatsVisible)
            {
                GUILayout.Box("Health Stats");
                healthStats = string.Format(
                    "ping: {6}[+/-{7}]ms resent:{8} \n\nmax ms between\nsend: {0,4} \ndispatch: {1,4} \n\nlongest dispatch for: \nev({3}):{2,3}ms \nop({5}):{4,3}ms",
                    gls.LongestDeltaBetweenSending,
                    gls.LongestDeltaBetweenDispatching,
                    gls.LongestEventCallback,
                    gls.LongestEventCallbackCode,
                    gls.LongestOpResponseCallback,
                    gls.LongestOpResponseCallbackOpCode,
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime,
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTimeVariance,
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.ResentReliableCommands);
                GUILayout.Label(healthStats);
            }

            if (statsToLog)
            {
                var complete = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}", total, elapsedTime, average,
                    trafficStatsIn, trafficStatsOut, healthStats);
                Debug.Log(complete);
            }

            // if anything was clicked, the height of this window is likely changed. reduce it to be layouted again next frame
            if (GUI.changed) statsRect.height = 100;

            GUI.DragWindow();
        }
    }
}