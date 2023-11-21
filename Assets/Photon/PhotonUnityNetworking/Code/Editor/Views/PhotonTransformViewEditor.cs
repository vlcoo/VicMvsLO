// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformViewEditor.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a custom editor for the TransformView component.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using UnityEditor;
using UnityEngine;

namespace Photon.Pun
{
    [CustomEditor(typeof(PhotonTransformView))]
    public class PhotonTransformViewEditor : Editor
    {
        private bool helpToggle;

        private SerializedProperty pos, rot, scl, lcl;

        public void OnEnable()
        {
            pos = serializedObject.FindProperty("m_SynchronizePosition");
            rot = serializedObject.FindProperty("m_SynchronizeRotation");
            scl = serializedObject.FindProperty("m_SynchronizeScale");
            lcl = serializedObject.FindProperty("m_UseLocal");
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Editing is disabled in play mode.", MessageType.Info);
                return;
            }

            var view = (PhotonTransformView)target;


            EditorGUILayout.LabelField("Synchronize Options");


            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical("HelpBox");
                {
                    EditorGUILayout.PropertyField(pos, new GUIContent("Position", pos.tooltip));
                    EditorGUILayout.PropertyField(rot, new GUIContent("Rotation", rot.tooltip));
                    EditorGUILayout.PropertyField(scl, new GUIContent("Scale", scl.tooltip));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(lcl, new GUIContent("Use Local", lcl.tooltip));
            }

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

            helpToggle = EditorGUILayout.Foldout(helpToggle, "Info");
            if (helpToggle)
                EditorGUILayout.HelpBox(
                    "The Photon Transform View of PUN 2 is simple by design.\nReplace it with the Photon Transform View Classic if you want the old options.\nThe best solution is a custom IPunObservable implementation.",
                    MessageType.Info, true);
        }
    }
}