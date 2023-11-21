// ----------------------------------------------------------------------------
// <copyright file="RegionHandler.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   The RegionHandler class provides methods to ping a list of regions,
//   to find the one with best ping.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

#if UNITY_WEBGL
#define PING_VIA_COROUTINE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using ExitGames.Client.Photon;
using Debug = UnityEngine.Debug;

namespace Photon.Realtime
{
#if SUPPORTED_UNITY
    using UnityEngine;
    using Debug = Debug;
#endif
#if SUPPORTED_UNITY || NETFX_CORE
    using SupportClass = SupportClass;
#endif

    /// <summary>
    ///     Provides methods to work with Photon's regions (Photon Cloud) and can be use to find the one with best ping.
    /// </summary>
    /// <remarks>
    ///     When a client uses a Name Server to fetch the list of available regions, the LoadBalancingClient will create a
    ///     RegionHandler
    ///     and provide it via the OnRegionListReceived callback.
    ///     Your logic can decide to either connect to one of those regional servers, or it may use PingMinimumOfRegions to
    ///     test
    ///     which region provides the best ping.
    ///     It makes sense to make clients "sticky" to a region when one gets selected.
    ///     This can be achieved by storing the SummaryToCache value, once pinging was done.
    ///     When the client connects again, the previous SummaryToCache helps limiting the number of regions to ping.
    ///     In best case, only the previously selected region gets re-pinged and if the current ping is not much worse, this
    ///     one region is used again.
    /// </remarks>
    public class RegionHandler
    {
        /// <summary>The implementation of PhotonPing to use for region pinging (Best Region detection).</summary>
        /// <remarks>Defaults to null, which means the Type is set automatically.</remarks>
        public static Type PingImplementation;

        protected internal static ushort PortToPingOverride;

        private readonly List<RegionPinger> pingerList = new();

        private string availableRegionCodes;

        private Region bestRegionCache;
        private Action<RegionHandler> onCompleteCall;
        private int previousPing;
        private string previousSummaryProvided;


        public RegionHandler(ushort masterServerPortOverride = 0)
        {
            PortToPingOverride = masterServerPortOverride;
        }

        /// <summary>A list of region names for the Photon Cloud. Set by the result of OpGetRegions().</summary>
        /// <remarks>
        ///     Implement ILoadBalancingCallbacks and register for the callbacks to get OnRegionListReceived(RegionHandler
        ///     regionHandler).
        ///     You can also put a "case OperationCode.GetRegions:" into your OnOperationResponse method to notice when the result
        ///     is available.
        /// </remarks>
        public List<Region> EnabledRegions { get; protected internal set; }

        /// <summary>
        ///     When PingMinimumOfRegions was called and completed, the BestRegion is identified by best ping.
        /// </summary>
        public Region BestRegion
        {
            get
            {
                if (EnabledRegions == null) return null;
                if (bestRegionCache != null) return bestRegionCache;

                EnabledRegions.Sort((a, b) => a.Ping.CompareTo(b.Ping));

                bestRegionCache = EnabledRegions[0];
                return bestRegionCache;
            }
        }

        /// <summary>
        ///     This value summarizes the results of pinging currently available regions (after PingMinimumOfRegions finished).
        /// </summary>
        /// <remarks>
        ///     This value should be stored in the client by the game logic.
        ///     When connecting again, use it as previous summary to speed up pinging regions and to make the best region sticky
        ///     for the client.
        /// </remarks>
        public string SummaryToCache
        {
            get
            {
                if (BestRegion != null) return BestRegion.Code + ";" + BestRegion.Ping + ";" + availableRegionCodes;

                return availableRegionCodes;
            }
        }

        public bool IsPinging { get; private set; }

        public string GetResults()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Region Pinging Result: {0}\n", BestRegion.ToString());
            foreach (var region in pingerList) sb.AppendFormat(region.GetResults() + "\n");
            sb.AppendFormat("Previous summary: {0}", previousSummaryProvided);

            return sb.ToString();
        }

        public void SetRegions(OperationResponse opGetRegions)
        {
            if (opGetRegions.OperationCode != OperationCode.GetRegions) return;
            if (opGetRegions.ReturnCode != ErrorCode.Ok) return;

            var regions = opGetRegions[ParameterCode.Region] as string[];
            var servers = opGetRegions[ParameterCode.Address] as string[];
            if (regions == null || servers == null || regions.Length != servers.Length)
                //TODO: log error
                //Debug.LogError("The region arrays from Name Server are not ok. Must be non-null and same length. " + (regions == null) + " " + (servers == null) + "\n" + opGetRegions.ToStringFull());
                return;

            bestRegionCache = null;
            EnabledRegions = new List<Region>(regions.Length);

            for (var i = 0; i < regions.Length; i++)
            {
                var server = servers[i];
                if (PortToPingOverride != 0)
                    server = LoadBalancingClient.ReplacePortWithAlternative(servers[i], PortToPingOverride);

                var tmp = new Region(regions[i], server);
                if (string.IsNullOrEmpty(tmp.Code)) continue;

                EnabledRegions.Add(tmp);
            }

            Array.Sort(regions);
            availableRegionCodes = string.Join(",", regions);
        }


        public bool PingMinimumOfRegions(Action<RegionHandler> onCompleteCallback, string previousSummary)
        {
            if (EnabledRegions == null || EnabledRegions.Count == 0)
                //TODO: log error
                //Debug.LogError("No regions available. Maybe all got filtered out or the AppId is not correctly configured.");
                return false;

            if (IsPinging)
                //TODO: log warning
                //Debug.LogWarning("PingMinimumOfRegions() skipped, because this RegionHandler is already pinging some regions.");
                return false;

            IsPinging = true;
            onCompleteCall = onCompleteCallback;
            previousSummaryProvided = previousSummary;

            if (string.IsNullOrEmpty(previousSummary)) return PingEnabledRegions();

            var values = previousSummary.Split(';');
            if (values.Length < 3) return PingEnabledRegions();

            int prevBestRegionPing;
            var secondValueIsInt = int.TryParse(values[1], out prevBestRegionPing);
            if (!secondValueIsInt) return PingEnabledRegions();

            var prevBestRegionCode = values[0];
            var prevAvailableRegionCodes = values[2];


            if (string.IsNullOrEmpty(prevBestRegionCode)) return PingEnabledRegions();
            if (string.IsNullOrEmpty(prevAvailableRegionCodes)) return PingEnabledRegions();
            if (!availableRegionCodes.Equals(prevAvailableRegionCodes) ||
                !availableRegionCodes.Contains(prevBestRegionCode)) return PingEnabledRegions();
            if (prevBestRegionPing >= RegionPinger.PingWhenFailed) return PingEnabledRegions();

            // let's check only the preferred region to detect if it's still "good enough"
            previousPing = prevBestRegionPing;


            var preferred = EnabledRegions.Find(r => r.Code.Equals(prevBestRegionCode));
            var singlePinger = new RegionPinger(preferred, OnPreferredRegionPinged);

            lock (pingerList)
            {
                pingerList.Add(singlePinger);
            }

            singlePinger.Start();
            return true;
        }

        private void OnPreferredRegionPinged(Region preferredRegion)
        {
            if (preferredRegion.Ping > previousPing * 1.50f)
            {
                PingEnabledRegions();
            }
            else
            {
                IsPinging = false;
                onCompleteCall(this);
#if PING_VIA_COROUTINE
                MonoBehaviourEmpty.SelfDestroy();
#endif
            }
        }


        private bool PingEnabledRegions()
        {
            if (EnabledRegions == null || EnabledRegions.Count == 0)
                //TODO: log
                //Debug.LogError("No regions available. Maybe all got filtered out or the AppId is not correctly configured.");
                return false;

            lock (pingerList)
            {
                pingerList.Clear();

                foreach (var region in EnabledRegions)
                {
                    var rp = new RegionPinger(region, OnRegionDone);
                    pingerList.Add(rp);
                    rp.Start(); // TODO: check return value
                }
            }

            return true;
        }

        private void OnRegionDone(Region region)
        {
            lock (pingerList)
            {
                if (IsPinging == false) return;

                bestRegionCache = null;
                foreach (var pinger in pingerList)
                    if (!pinger.Done)
                        return;

                IsPinging = false;
            }

            onCompleteCall(this);
#if PING_VIA_COROUTINE
            MonoBehaviourEmpty.SelfDestroy();
#endif
        }
    }

    public class RegionPinger
    {
        public static int Attempts = 5;
        public static bool IgnoreInitialAttempt = true;

        public static int
            MaxMilliseconsPerPing = 800; // enter a value you're sure some server can beat (have a lower rtt)

        public static int PingWhenFailed = Attempts * MaxMilliseconsPerPing;
        private readonly Action<Region> onDoneCall;

        private readonly Region region;
        public int CurrentAttempt;

        private PhotonPing ping;
        private string regionAddress;

        private List<int> rttResults;

        public RegionPinger(Region region, Action<Region> onDoneCallback)
        {
            this.region = region;
            this.region.Ping = PingWhenFailed;
            Done = false;
            onDoneCall = onDoneCallback;
        }

        public bool Done { get; private set; }

        /// <summary>Selects the best fitting ping implementation or uses the one set in RegionHandler.PingImplementation.</summary>
        /// <returns>PhotonPing instance to use.</returns>
        private PhotonPing GetPingImplementation()
        {
            PhotonPing ping = null;

            // using each type explicitly in the conditional code, makes sure Unity doesn't strip the class / constructor.

#if !UNITY_EDITOR && NETFX_CORE
            if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingWindowsStore))
            {
                ping = new PingWindowsStore();
            }
#elif NATIVE_SOCKETS || NO_SOCKET
            if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingNativeDynamic))
            {
                ping = new PingNativeDynamic();
            }
#elif UNITY_WEBGL
            if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingHttp))
            {
                ping = new PingHttp();
            }
#else
            if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingMono))
                ping = new PingMono();
#endif

            if (ping == null)
                if (RegionHandler.PingImplementation != null)
                    ping = (PhotonPing)Activator.CreateInstance(RegionHandler.PingImplementation);

            return ping;
        }


        /// <summary>
        ///     Starts the ping routine for the assigned region.
        /// </summary>
        /// <remarks>
        ///     Pinging runs in a ThreadPool worker item or (if needed) in a Thread.
        ///     WebGL runs pinging on the Main Thread as coroutine.
        /// </remarks>
        /// <returns>Always true.</returns>
        public bool Start()
        {
            // all addresses for Photon region servers will contain a :port ending. this needs to be removed first.
            // PhotonPing.StartPing() requires a plain (IP) address without port or protocol-prefix (on all but Windows 8.1 and WebGL platforms).
            var address = region.HostAndPort;
            var indexOfColon = address.LastIndexOf(':');
            if (indexOfColon > 1) address = address.Substring(0, indexOfColon);
            regionAddress = ResolveHost(address);


            ping = GetPingImplementation();


            Done = false;
            CurrentAttempt = 0;
            rttResults = new List<int>(Attempts);


#if PING_VIA_COROUTINE
            MonoBehaviourEmpty.Instance.StartCoroutine(this.RegionPingCoroutine());
#else
            var queued = false;
#if !NETFX_CORE
            try
            {
                queued = ThreadPool.QueueUserWorkItem(RegionPingPooled);
            }
            catch
            {
                queued = false;
            }
#endif
            if (!queued)
                SupportClass.StartBackgroundCalls(RegionPingThreaded, 0,
                    "RegionPing_" + region.Code + "_" + region.Cluster);
#endif


            return true;
        }

        // wraps RegionPingThreaded() to get the signature compatible with ThreadPool.QueueUserWorkItem
        protected internal void RegionPingPooled(object context)
        {
            RegionPingThreaded();
        }

        protected internal bool RegionPingThreaded()
        {
            region.Ping = PingWhenFailed;

            var rttSum = 0.0f;
            var replyCount = 0;


            var sw = new Stopwatch();
            for (CurrentAttempt = 0; CurrentAttempt < Attempts; CurrentAttempt++)
            {
                var overtime = false;
                sw.Reset();
                sw.Start();

                try
                {
                    ping.StartPing(regionAddress);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "RegionPinger.RegionPingThreaded() catched an exception for ping.StartPing(). Exception: " + e +
                        " Source: " + e.Source + " Message: " + e.Message);
                    break;
                }


                while (!ping.Done())
                {
                    if (sw.ElapsedMilliseconds >= MaxMilliseconsPerPing)
                    {
                        overtime = true;
                        break;
                    }
#if !NETFX_CORE
                    Thread.Sleep(0);
#endif
                }


                sw.Stop();
                var rtt = (int)sw.ElapsedMilliseconds;
                rttResults.Add(rtt);

                if (IgnoreInitialAttempt && CurrentAttempt == 0)
                {
                    // do nothing.
                }
                else if (ping.Successful && !overtime)
                {
                    rttSum += rtt;
                    replyCount++;
                    region.Ping = (int)(rttSum / replyCount);
                }

#if !NETFX_CORE
                Thread.Sleep(10);
#endif
            }

            //Debug.Log("Done: "+ this.region.Code);
            Done = true;
            ping.Dispose();

            onDoneCall(region);

            return false;
        }


#if SUPPORTED_UNITY
        /// <remarks>
        ///     Affected by frame-rate of app, as this Coroutine checks the socket for a result once per frame.
        /// </remarks>
        protected internal IEnumerator RegionPingCoroutine()
        {
            region.Ping = PingWhenFailed;

            var rttSum = 0.0f;
            var replyCount = 0;


            var sw = new Stopwatch();
            for (CurrentAttempt = 0; CurrentAttempt < Attempts; CurrentAttempt++)
            {
                var overtime = false;
                sw.Reset();
                sw.Start();

                try
                {
                    ping.StartPing(regionAddress);
                }
                catch (Exception e)
                {
                    Debug.Log("catched: " + e);
                    break;
                }


                while (!ping.Done())
                {
                    if (sw.ElapsedMilliseconds >= MaxMilliseconsPerPing)
                    {
                        overtime = true;
                        break;
                    }

                    yield return 0; // keep this loop tight, to avoid adding local lag to rtt.
                }


                sw.Stop();
                var rtt = (int)sw.ElapsedMilliseconds;
                rttResults.Add(rtt);


                if (IgnoreInitialAttempt && CurrentAttempt == 0)
                {
                    // do nothing.
                }
                else if (ping.Successful && !overtime)
                {
                    rttSum += rtt;
                    replyCount++;
                    region.Ping = (int)(rttSum / replyCount);
                }

                yield return new WaitForSeconds(0.1f);
            }


            //Debug.Log("Done: "+ this.region.Code);
            Done = true;
            ping.Dispose();
            onDoneCall(region);
            yield return null;
        }
#endif


        public string GetResults()
        {
            return string.Format("{0}: {1} ({2})", region.Code, region.Ping, rttResults.ToStringFull());
        }

        /// <summary>
        ///     Attempts to resolve a hostname into an IP string or returns empty string if that fails.
        /// </summary>
        /// <remarks>
        ///     To be compatible with most platforms, the address family is checked like this:<br />
        ///     if (ipAddress.AddressFamily.ToString().Contains("6")) // ipv6...
        /// </remarks>
        /// <param name="hostName">Hostname to resolve.</param>
        /// <returns>IP string or empty string if resolution fails</returns>
        public static string ResolveHost(string hostName)
        {
            if (hostName.StartsWith("wss://")) hostName = hostName.Substring(6);
            if (hostName.StartsWith("ws://")) hostName = hostName.Substring(5);

            var ipv4Address = string.Empty;

            try
            {
#if UNITY_WSA || NETFX_CORE || UNITY_WEBGL
                return hostName;
#else

                var address = Dns.GetHostAddresses(hostName);
                if (address.Length == 1) return address[0].ToString();

                // if we got more addresses, try to pick a IPv6 one
                // checking ipAddress.ToString() means we don't have to import System.Net.Sockets, which is not available on some platforms (Metro)
                for (var index = 0; index < address.Length; index++)
                {
                    var ipAddress = address[index];
                    if (ipAddress != null)
                    {
                        if (ipAddress.ToString().Contains(":")) return ipAddress.ToString();
                        if (string.IsNullOrEmpty(ipv4Address)) ipv4Address = address.ToString();
                    }
                }
#endif
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(
                    "RegionPinger.ResolveHost() catched an exception for Dns.GetHostAddresses(). Exception: " + e +
                    " Source: " + e.Source + " Message: " + e.Message);
            }

            return ipv4Address;
        }
    }

#if PING_VIA_COROUTINE
    internal class MonoBehaviourEmpty : MonoBehaviour
    {
        private static bool instanceSet; // to avoid instance null check which may be incorrect
        private static MonoBehaviourEmpty instance;

        public static MonoBehaviourEmpty Instance
        {
            get
            {
                if (instanceSet)
                {
                    return instance;
                }
                GameObject go = new GameObject();
                DontDestroyOnLoad(go);
                go.name = "RegionPinger";
                instance = go.AddComponent<MonoBehaviourEmpty>();
                instanceSet = true;
                return instance;
            }
        }

        public static void SelfDestroy()
        {
            if (instanceSet)
            {
                instanceSet = false;
                Destroy(instance.gameObject);
            }
        }
    }
#endif
}