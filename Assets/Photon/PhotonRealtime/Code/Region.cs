// ----------------------------------------------------------------------------
// <copyright file="Region.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Represents regions in the Photon Cloud.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

namespace Photon.Realtime
{
#if SUPPORTED_UNITY || NETFX_CORE
#endif


    public class Region
    {
        public Region(string code, string address)
        {
            SetCodeAndCluster(code);
            HostAndPort = address;
            Ping = int.MaxValue;
        }

        public Region(string code, int ping)
        {
            SetCodeAndCluster(code);
            Ping = ping;
        }

        public string Code { get; private set; }

        /// <summary>Unlike the CloudRegionCode, this may contain cluster information.</summary>
        public string Cluster { get; private set; }

        public string HostAndPort { get; protected internal set; }

        public int Ping { get; set; }

        public bool WasPinged => Ping != int.MaxValue;

        private void SetCodeAndCluster(string codeAsString)
        {
            if (codeAsString == null)
            {
                Code = "";
                Cluster = "";
                return;
            }

            codeAsString = codeAsString.ToLower();
            var slash = codeAsString.IndexOf('/');
            Code = slash <= 0 ? codeAsString : codeAsString.Substring(0, slash);
            Cluster = slash <= 0 ? "" : codeAsString.Substring(slash + 1, codeAsString.Length - slash - 1);
        }

        public override string ToString()
        {
            return ToString();
        }

        public string ToString(bool compact = false)
        {
            var regionCluster = Code;
            if (!string.IsNullOrEmpty(Cluster)) regionCluster += "/" + Cluster;

            if (compact)
                return string.Format("{0}:{1}", regionCluster, Ping);
            return string.Format("{0}[{2}]: {1}ms", regionCluster, Ping, HostAndPort);
        }
    }
}