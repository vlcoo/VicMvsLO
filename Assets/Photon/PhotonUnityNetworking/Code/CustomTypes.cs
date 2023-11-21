// ----------------------------------------------------------------------------
// <copyright file="CustomTypes.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Sets up support for Unity-specific types. Can be a blueprint how to register your own Custom Types for sending.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun
{
    /// <summary>
    ///     Internally used class, containing de/serialization method for PUN specific classes.
    /// </summary>
    internal static class CustomTypes
    {
        /// <summary>
        ///     Register de/serializer methods for PUN specific types. Makes the type usable in RaiseEvent, RPC and sync
        ///     updates of PhotonViews.
        /// </summary>
        internal static void Register()
        {
            PhotonPeer.RegisterType(typeof(Player), (byte)'P', SerializePhotonPlayer, DeserializePhotonPlayer);
        }


        #region Custom De/Serializer Methods

        public static readonly byte[] memPlayer = new byte[4];

        private static short SerializePhotonPlayer(StreamBuffer outStream, object customobject)
        {
            var ID = ((Player)customobject).ActorNumber;

            lock (memPlayer)
            {
                var bytes = memPlayer;
                var off = 0;
                Protocol.Serialize(ID, bytes, ref off);
                outStream.Write(bytes, 0, 4);
                return 4;
            }
        }

        private static object DeserializePhotonPlayer(StreamBuffer inStream, short length)
        {
            if (length != 4) return null;

            int ID;
            lock (memPlayer)
            {
                inStream.Read(memPlayer, 0, length);
                var off = 0;
                Protocol.Deserialize(out ID, memPlayer, ref off);
            }

            if (PhotonNetwork.CurrentRoom != null)
            {
                var player = PhotonNetwork.CurrentRoom.GetPlayer(ID);
                return player;
            }

            return null;
        }

        #endregion
    }
}