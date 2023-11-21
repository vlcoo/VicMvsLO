// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbody2DViewEditor.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a custom editor for the PhotonRigidbody2DView component.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using UnityEditor;
using UnityEngine;

namespace Photon.Pun
{
    [CustomEditor(typeof(PhotonRigidbody2DView))]
    public class PhotonRigidbody2DViewEditor : MonoBehaviourPunEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Editing is disabled in play mode.", MessageType.Info);
                return;
            }

            var view = (PhotonRigidbody2DView)target;

            view.m_TeleportEnabled =
                PhotonGUI.ContainerHeaderToggle("Enable teleport for large distances", view.m_TeleportEnabled);

            if (view.m_TeleportEnabled)
            {
                var rect = PhotonGUI.ContainerBody(20.0f);
                view.m_TeleportIfDistanceGreaterThan = EditorGUI.FloatField(rect, "Teleport if distance greater than",
                    view.m_TeleportIfDistanceGreaterThan);
            }

            view.m_SynchronizeVelocity =
                PhotonGUI.ContainerHeaderToggle("Synchronize Velocity", view.m_SynchronizeVelocity);
            view.m_SynchronizeAngularVelocity =
                PhotonGUI.ContainerHeaderToggle("Synchronize Angular Velocity", view.m_SynchronizeAngularVelocity);

            if (GUI.changed) EditorUtility.SetDirty(view);
        }
    }
}