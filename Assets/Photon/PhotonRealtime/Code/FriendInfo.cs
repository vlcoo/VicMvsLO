// ----------------------------------------------------------------------------
// <copyright file="FriendInfo.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Collection of values related to a user / friend.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

using System;

namespace Photon.Realtime
{
#if SUPPORTED_UNITY || NETFX_CORE
#endif


    /// <summary>
    ///     Used to store info about a friend's online state and in which room he/she is.
    /// </summary>
    public class FriendInfo
    {
        [Obsolete("Use UserId.")] public string Name => UserId;

        public string UserId { get; protected internal set; }

        public bool IsOnline { get; protected internal set; }
        public string Room { get; protected internal set; }

        public bool IsInRoom => IsOnline && !string.IsNullOrEmpty(Room);

        public override string ToString()
        {
            return string.Format("{0}\t is: {1}", UserId, !IsOnline ? "offline" : IsInRoom ? "playing" : "on master");
        }
    }
}