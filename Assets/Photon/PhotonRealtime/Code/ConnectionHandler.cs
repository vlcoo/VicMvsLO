// ----------------------------------------------------------------------------
// <copyright file="ConnectionHandler.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   If the game logic does not call Service() for whatever reason, this keeps the connection.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

using System;
using System.Diagnostics;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
#if SUPPORTED_UNITY
    using UnityEngine;
#endif


#if SUPPORTED_UNITY
    public class ConnectionHandler : MonoBehaviour
#else
    public class ConnectionHandler
#endif
    {
        /// <summary>
        ///     Photon client to log information and statistics from.
        /// </summary>
        public LoadBalancingClient Client { get; set; }

        /// <summary>Option to let the fallback thread call Disconnect after the KeepAliveInBackground time. Default: false.</summary>
        /// <remarks>
        ///     If set to true, the thread will disconnect the client regularly, should the client not call SendOutgoingCommands /
        ///     Service.
        ///     This may happen due to an app being in background (and not getting a lot of CPU time) or when loading assets.
        ///     If false, a regular timeout time will have to pass (on top) to time out the client.
        /// </remarks>
        public bool DisconnectAfterKeepAlive;

        /// <summary>Defines for how long the Fallback Thread should keep the connection, before it may time out as usual.</summary>
        /// <remarks>
        ///     We want to the Client to keep it's connection when an app is in the background (and doesn't call Update /
        ///     Service Clients should not keep their connection indefinitely in the background, so after some milliseconds, the
        ///     Fallback Thread should stop keeping it up.
        /// </remarks>
        public int KeepAliveInBackground = 60000;

        /// <summary>
        ///     Counts how often the Fallback Thread called SendAcksOnly, which is purely of interest to monitor if the game
        ///     logic called SendOutgoingCommands as intended.
        /// </summary>
        public int CountSendAcksOnly { get; private set; }

        /// <summary>True if a fallback thread is running. Will call the client's SendAcksOnly() method to keep the connection up.</summary>
        public bool FallbackThreadRunning => fallbackThreadId < 255;

        /// <summary>Keeps the ConnectionHandler, even if a new scene gets loaded.</summary>
        public bool ApplyDontDestroyOnLoad = true;

        /// <summary>Indicates that the app is closing. Set in OnApplicationQuit().</summary>
        [NonSerialized] public static bool AppQuits;


        private byte fallbackThreadId = 255;
        private bool didSendAcks;
        private readonly Stopwatch backgroundStopwatch = new();


#if SUPPORTED_UNITY

#if UNITY_2019_4_OR_NEWER

        /// <summary>
        ///     Resets statics for Domain Reload
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StaticReset()
        {
            AppQuits = false;
        }

#endif


        /// <summary>Called by Unity when the application gets closed. The UnityEngine will also call OnDisable, which disconnects.</summary>
        protected void OnApplicationQuit()
        {
            AppQuits = true;
        }


        /// <summary></summary>
        protected virtual void Awake()
        {
            if (ApplyDontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        /// <summary>Called by Unity when the application gets closed. Disconnects if OnApplicationQuit() was called before.</summary>
        protected virtual void OnDisable()
        {
            StopFallbackSendAckThread();

            if (AppQuits)
            {
                if (Client != null && Client.IsConnected)
                {
                    Client.Disconnect();
                    Client.LoadBalancingPeer.StopThread();
                }

                SupportClass.StopAllBackgroundCalls();
            }
        }

#endif


        public void StartFallbackSendAckThread()
        {
#if !UNITY_WEBGL
            if (FallbackThreadRunning) return;

#if UNITY_SWITCH
            this.fallbackThreadId =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         SupportClass.StartBackgroundCalls(this.RealtimeFallbackThread, 50);  // as workaround, we don't name the Thread.
#else
            fallbackThreadId = SupportClass.StartBackgroundCalls(RealtimeFallbackThread, 50, "RealtimeFallbackThread");
#endif
#endif
        }

        public void StopFallbackSendAckThread()
        {
#if !UNITY_WEBGL
            if (!FallbackThreadRunning) return;

            SupportClass.StopBackgroundCalls(fallbackThreadId);
            fallbackThreadId = 255;
#endif
        }


        /// <summary>
        ///     A thread which runs independent from the Update() calls. Keeps connections online while loading or in
        ///     background. See <see cref="KeepAliveInBackground" />.
        /// </summary>
        public bool RealtimeFallbackThread()
        {
            if (Client != null)
            {
                if (!Client.IsConnected)
                {
                    didSendAcks = false;
                    return true;
                }

                if (Client.LoadBalancingPeer.ConnectionTime - Client.LoadBalancingPeer.LastSendOutgoingTime > 100)
                {
                    if (!didSendAcks)
                    {
                        backgroundStopwatch.Reset();
                        backgroundStopwatch.Start();
                    }

                    // check if the client should disconnect after some seconds in background
                    if (backgroundStopwatch.ElapsedMilliseconds > KeepAliveInBackground)
                    {
                        if (DisconnectAfterKeepAlive) Client.Disconnect();
                        return true;
                    }


                    didSendAcks = true;
                    CountSendAcksOnly++;
                    Client.LoadBalancingPeer.SendAcksOnly();
                }
                else
                {
                    didSendAcks = false;
                }
            }

            return true;
        }
    }
}