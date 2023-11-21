// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PunTeams.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities, 
// </copyright>
// <summary>
// Implements teams in a room/game with help of player properties. Access them by Player.GetTeam extension.
// </summary>
// <remarks>
// Teams are defined by enum Team. Change this to get more / different teams.
// There are no rules when / if you can join a team. You could add this in JoinTeam or something.
// </remarks>                                                                                           
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Implements teams in a room/game with help of player properties. Access them by Player.GetTeam extension.
    /// </summary>
    /// <remarks>
    ///     Teams are defined by enum Team. Change this to get more / different teams.
    ///     There are no rules when / if you can join a team. You could add this in JoinTeam or something.
    /// </remarks>
    [Obsolete("do not use this or add it to the scene. use PhotonTeamsManager instead")]
    public class PunTeams : MonoBehaviourPunCallbacks
    {
        /// <summary>
        ///     Enum defining the teams available. First team should be neutral (it's the default value any field of this enum
        ///     gets).
        /// </summary>
        [Obsolete("use custom PhotonTeam instead")]
        public enum Team : byte
        {
            none,
            red,
            blue
        }

        /// <summary>Defines the player custom property name to use for team affinity of "this" player.</summary>
        [Obsolete("do not use this. PhotonTeamsManager.TeamPlayerProp is used internally instead.")]
        public const string TeamPlayerProp = "team";

        /// <summary>The main list of teams with their player-lists. Automatically kept up to date.</summary>
        /// <remarks>Note that this is static. Can be accessed by PunTeam.PlayersPerTeam. You should not modify this.</remarks>
        [Obsolete("use PhotonTeamsManager.Instance.TryGetTeamMembers instead")]
        public static Dictionary<Team, List<Player>> PlayersPerTeam;

        [Obsolete("do not call this.")]
        public void UpdateTeams()
        {
            var enumVals = Enum.GetValues(typeof(Team));
            foreach (var enumVal in enumVals) PlayersPerTeam[(Team)enumVal].Clear();

            for (var i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var player = PhotonNetwork.PlayerList[i];
                var playerTeam = player.GetTeam();
                PlayersPerTeam[playerTeam].Add(player);
            }
        }


        #region Events by Unity and Photon

        public void Start()
        {
            PlayersPerTeam = new Dictionary<Team, List<Player>>();
            var enumVals = Enum.GetValues(typeof(Team));
            foreach (var enumVal in enumVals) PlayersPerTeam[(Team)enumVal] = new List<Player>();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Start();
        }

        /// <summary>Needed to update the team lists when joining a room.</summary>
        /// <remarks>Called by PUN. See enum MonoBehaviourPunCallbacks for an explanation.</remarks>
        public override void OnJoinedRoom()
        {
            UpdateTeams();
        }

        public override void OnLeftRoom()
        {
            Start();
        }

        /// <summary>Refreshes the team lists. It could be a non-team related property change, too.</summary>
        /// <remarks>Called by PUN. See enum MonoBehaviourPunCallbacks for an explanation.</remarks>
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            UpdateTeams();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateTeams();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateTeams();
        }

        #endregion
    }

    /// <summary>Extension used for PunTeams and Player class. Wraps access to the player's custom property.</summary>
    public static class TeamExtensions
    {
        /// <summary>Extension for Player class to wrap up access to the player's custom property.</summary>
        /// <returns>PunTeam.Team.none if no team was found (yet).</returns>
        [Obsolete("Use player.GetPhotonTeam")]
        public static PunTeams.Team GetTeam(this Player player)
        {
            object teamId;
            if (player.CustomProperties.TryGetValue(PunTeams.TeamPlayerProp, out teamId)) return (PunTeams.Team)teamId;

            return PunTeams.Team.none;
        }

        /// <summary>Switch that player's team to the one you assign.</summary>
        /// <remarks>Internally checks if this player is in that team already or not. Only team switches are actually sent.</remarks>
        /// <param name="player"></param>
        /// <param name="team"></param>
        [Obsolete("Use player.JoinTeam")]
        public static void SetTeam(this Player player, PunTeams.Team team)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogWarning("JoinTeam was called in state: " + PhotonNetwork.NetworkClientState +
                                 ". Not IsConnectedAndReady.");
                return;
            }

            var currentTeam = player.GetTeam();
            if (currentTeam != team)
                player.SetCustomProperties(new Hashtable { { PunTeams.TeamPlayerProp, (byte)team } });
        }
    }
}