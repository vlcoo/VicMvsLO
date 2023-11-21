// ----------------------------------------------------------------------------
// <copyright file="WebRpc.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This class wraps responses of a Photon WebRPC call, coming from a
//   third party web service.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;


namespace Photon.Realtime
{
#if SUPPORTED_UNITY || NETFX_CORE
    using SupportClass = SupportClass;
#endif


    /// <summary>Reads an operation response of a WebRpc and provides convenient access to most common values.</summary>
    /// <remarks>
    ///     See LoadBalancingClient.OpWebRpc.<br />
    ///     Create a WebRpcResponse to access common result values.<br />
    ///     The operationResponse.OperationCode should be: OperationCode.WebRpc.<br />
    /// </remarks>
    public class WebRpcResponse
    {
        /// <summary>An OperationResponse for a WebRpc is needed to read it's values.</summary>
        public WebRpcResponse(OperationResponse response)
        {
            object value;
            if (response.Parameters.TryGetValue(ParameterCode.UriPath, out value)) Name = value as string;

            ResultCode = -1;
            if (response.Parameters.TryGetValue(ParameterCode.WebRpcReturnCode, out value)) ResultCode = (byte)value;

            if (response.Parameters.TryGetValue(ParameterCode.WebRpcParameters, out value))
                Parameters = value as Dictionary<string, object>;

            if (response.Parameters.TryGetValue(ParameterCode.WebRpcReturnMessage, out value))
                Message = value as string;
        }

        /// <summary>Name of the WebRpc that was called.</summary>
        public string Name { get; }

        /// <summary>ResultCode of the WebService that answered the WebRpc.</summary>
        /// <remarks>
        ///     0 is: "OK" for WebRPCs.<br />
        ///     -1 is: No ResultCode by WebRpc service (check <see cref="OperationResponse.ReturnCode" />).<br />
        ///     Other ResultCode are defined by the individual WebRpc and service.
        /// </remarks>
        public int ResultCode { get; }

        [Obsolete("Use ResultCode instead")] public int ReturnCode => ResultCode;

        /// <summary>Might be empty or null.</summary>
        public string Message { get; }

        [Obsolete("Use Message instead")] public string DebugMessage => Message;


        /// <summary>Other key/values returned by the webservice that answered the WebRpc.</summary>
        public Dictionary<string, object> Parameters { get; }

        /// <summary>Turns the response into an easier to read string.</summary>
        /// <returns>String resembling the result.</returns>
        public string ToStringFull()
        {
            return string.Format("{0}={2}: {1} \"{3}\"", Name, SupportClass.DictionaryToString(Parameters), ResultCode,
                Message);
        }
    }


    /// <summary>
    ///     Optional flags to be used in Photon client SDKs with Op RaiseEvent and Op SetProperties.
    ///     Introduced mainly for webhooks 1.2 to control behavior of forwarded HTTP requests.
    /// </summary>
    public class WebFlags
    {
        public const byte HttpForwardConst = 0x01;
        public const byte SendAuthCookieConst = 0x02;
        public const byte SendSyncConst = 0x04;
        public const byte SendStateConst = 0x08;

        public static readonly WebFlags Default = new(0);
        public byte WebhookFlags;

        public WebFlags(byte webhookFlags)
        {
            WebhookFlags = webhookFlags;
        }

        /// <summary>
        ///     Indicates whether to forward HTTP request to web service or not.
        /// </summary>
        public bool HttpForward
        {
            get => (WebhookFlags & HttpForwardConst) != 0;
            set
            {
                if (value)
                    WebhookFlags |= HttpForwardConst;
                else
                    WebhookFlags = (byte)(WebhookFlags & ~(1 << 0));
            }
        }

        /// <summary>
        ///     Indicates whether to send AuthCookie of actor in the HTTP request to web service or not.
        /// </summary>
        public bool SendAuthCookie
        {
            get => (WebhookFlags & SendAuthCookieConst) != 0;
            set
            {
                if (value)
                    WebhookFlags |= SendAuthCookieConst;
                else
                    WebhookFlags = (byte)(WebhookFlags & ~(1 << 1));
            }
        }

        /// <summary>
        ///     Indicates whether to send HTTP request synchronously or asynchronously to web service.
        /// </summary>
        public bool SendSync
        {
            get => (WebhookFlags & SendSyncConst) != 0;
            set
            {
                if (value)
                    WebhookFlags |= SendSyncConst;
                else
                    WebhookFlags = (byte)(WebhookFlags & ~(1 << 2));
            }
        }

        /// <summary>
        ///     Indicates whether to send serialized game state in HTTP request to web service or not.
        /// </summary>
        public bool SendState
        {
            get => (WebhookFlags & SendStateConst) != 0;
            set
            {
                if (value)
                    WebhookFlags |= SendStateConst;
                else
                    WebhookFlags = (byte)(WebhookFlags & ~(1 << 3));
            }
        }
    }
}