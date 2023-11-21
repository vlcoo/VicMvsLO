// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PunTeamsInspector.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities, 
// </copyright>
// <summary>
//  Custom inspector for PunTeams
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections.Generic;
using UnityEditor;

namespace Photon.Pun.UtilityScripts
{
#pragma warning disable 0618
    [CustomEditor(typeof(PunTeams))]
    public class PunTeamsInspector : Editor
    {
        private Dictionary<PunTeams.Team, bool> _Foldouts;

        public override void OnInspectorGUI()
        {
            if (_Foldouts == null) _Foldouts = new Dictionary<PunTeams.Team, bool>();

            if (PunTeams.PlayersPerTeam != null)
                foreach (var _pair in PunTeams.PlayersPerTeam)
                {
#pragma warning restore 0618
                    if (!_Foldouts.ContainsKey(_pair.Key)) _Foldouts[_pair.Key] = true;

                    _Foldouts[_pair.Key] = EditorGUILayout.Foldout(_Foldouts[_pair.Key],
                        "Team " + _pair.Key + " (" + _pair.Value.Count + ")");

                    if (_Foldouts[_pair.Key])
                    {
                        EditorGUI.indentLevel++;
                        foreach (var _player in _pair.Value)
                            EditorGUILayout.LabelField("",
                                _player + (PhotonNetwork.LocalPlayer == _player ? " - You -" : ""));
                        EditorGUI.indentLevel--;
                    }
                }
        }
    }
}