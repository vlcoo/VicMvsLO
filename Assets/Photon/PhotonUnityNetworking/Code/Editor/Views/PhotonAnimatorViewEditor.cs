// ----------------------------------------------------------------------------
// <copyright file="PhotonAnimatorViewEditor.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a custom editor for the AnimatorView component.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Photon.Pun
{
    [CustomEditor(typeof(PhotonAnimatorView))]
    public class PhotonAnimatorViewEditor : MonoBehaviourPunEditor
    {
        private Animator m_Animator;
        private AnimatorController m_Controller;
        private PhotonAnimatorView m_Target;

        private void OnEnable()
        {
            m_Target = (PhotonAnimatorView)target;
            m_Animator = m_Target.GetComponent<Animator>();

            if (m_Animator)
            {
                m_Controller = GetEffectiveController(m_Animator) as AnimatorController;

                CheckIfStoredParametersExist();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_Animator == null)
            {
                EditorGUILayout.HelpBox("GameObject doesn't have an Animator component to synchronize",
                    MessageType.Warning);
                return;
            }

            DrawWeightInspector();

            if (GetLayerCount() == 0)
                EditorGUILayout.HelpBox("Animator doesn't have any layers setup to synchronize", MessageType.Warning);

            DrawParameterInspector();

            if (GetParameterCount() == 0)
                EditorGUILayout.HelpBox("Animator doesn't have any parameters setup to synchronize",
                    MessageType.Warning);

            serializedObject.ApplyModifiedProperties();

            //GUILayout.Label( "m_SynchronizeLayers " + serializedObject.FindProperty( "m_SynchronizeLayers" ).arraySize );
            //GUILayout.Label( "m_SynchronizeParameters " + serializedObject.FindProperty( "m_SynchronizeParameters" ).arraySize );
        }


        private int GetLayerCount()
        {
            return m_Controller == null ? 0 : m_Controller.layers.Length;
        }

        private int GetParameterCount()
        {
            return m_Controller == null ? 0 : m_Controller.parameters.Length;
        }

        private AnimatorControllerParameter GetAnimatorControllerParameter(int i)
        {
            return m_Controller.parameters[i];
        }


        private RuntimeAnimatorController GetEffectiveController(Animator animator)
        {
            var controller = animator.runtimeAnimatorController;

            var overrideController = controller as AnimatorOverrideController;
            while (overrideController != null)
            {
                controller = overrideController.runtimeAnimatorController;
                overrideController = controller as AnimatorOverrideController;
            }

            return controller;
        }

        private void DrawWeightInspector()
        {
            var foldoutProperty = serializedObject.FindProperty("ShowLayerWeightsInspector");
            foldoutProperty.boolValue =
                PhotonGUI.ContainerHeaderFoldout("Synchronize Layer Weights", foldoutProperty.boolValue);

            if (foldoutProperty.boolValue == false) return;

            float lineHeight = 20;
            var containerRect = PhotonGUI.ContainerBody(GetLayerCount() * lineHeight);

            for (var i = 0; i < GetLayerCount(); ++i)
            {
                if (m_Target.DoesLayerSynchronizeTypeExist(i) == false)
                    m_Target.SetLayerSynchronized(i, PhotonAnimatorView.SynchronizeType.Disabled);

                var syncType = m_Target.GetLayerSynchronizeType(i);

                var elementRect = new Rect(containerRect.xMin, containerRect.yMin + i * lineHeight, containerRect.width,
                    lineHeight);

                var labelRect = new Rect(elementRect.xMin + 5, elementRect.yMin + 2, EditorGUIUtility.labelWidth - 5,
                    elementRect.height);
                GUI.Label(labelRect, "Layer " + i);

                var popupRect = new Rect(elementRect.xMin + EditorGUIUtility.labelWidth, elementRect.yMin + 2,
                    elementRect.width - EditorGUIUtility.labelWidth - 5, EditorGUIUtility.singleLineHeight);
                syncType = (PhotonAnimatorView.SynchronizeType)EditorGUI.EnumPopup(popupRect, syncType);

                if (i < GetLayerCount() - 1)
                {
                    var splitterRect = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                    PhotonGUI.DrawSplitter(splitterRect);
                }

                if (syncType != m_Target.GetLayerSynchronizeType(i))
                {
                    Undo.RecordObject(target, "Modify Synchronize Layer Weights");
                    m_Target.SetLayerSynchronized(i, syncType);
                }
            }
        }

        private bool DoesParameterExist(string name)
        {
            for (var i = 0; i < GetParameterCount(); ++i)
                if (GetAnimatorControllerParameter(i).name == name)
                    return true;

            return false;
        }

        private void CheckIfStoredParametersExist()
        {
            var syncedParams = m_Target.GetSynchronizedParameters();
            var paramsToRemove = new List<string>();

            for (var i = 0; i < syncedParams.Count; ++i)
            {
                var parameterName = syncedParams[i].Name;
                if (DoesParameterExist(parameterName) == false)
                {
                    Debug.LogWarning("Parameter '" + m_Target.GetSynchronizedParameters()[i].Name +
                                     "' doesn't exist anymore. Removing it from the list of synchronized parameters");
                    paramsToRemove.Add(parameterName);
                }
            }

            if (paramsToRemove.Count > 0)
                foreach (var param in paramsToRemove)
                    m_Target.GetSynchronizedParameters().RemoveAll(item => item.Name == param);
        }


        private void DrawParameterInspector()
        {
            // flag to expose a note in Interface if one or more trigger(s) are synchronized
            var isUsingTriggers = false;

            var foldoutProperty = serializedObject.FindProperty("ShowParameterInspector");
            foldoutProperty.boolValue =
                PhotonGUI.ContainerHeaderFoldout("Synchronize Parameters", foldoutProperty.boolValue);

            if (foldoutProperty.boolValue == false) return;

            float lineHeight = 20;
            var containerRect = PhotonGUI.ContainerBody(GetParameterCount() * lineHeight);

            for (var i = 0; i < GetParameterCount(); i++)
            {
                AnimatorControllerParameter parameter = null;
                parameter = GetAnimatorControllerParameter(i);

                var defaultValue = "";

                if (parameter.type == AnimatorControllerParameterType.Bool)
                {
                    if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
                        defaultValue += m_Animator.GetBool(parameter.name);
                    else
                        defaultValue += parameter.defaultBool.ToString();
                }
                else if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
                        defaultValue += m_Animator.GetFloat(parameter.name).ToString("0.00");
                    else
                        defaultValue += parameter.defaultFloat.ToString();
                }
                else if (parameter.type == AnimatorControllerParameterType.Int)
                {
                    if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
                        defaultValue += m_Animator.GetInteger(parameter.name);
                    else
                        defaultValue += parameter.defaultInt.ToString();
                }
                else if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
                        defaultValue += m_Animator.GetBool(parameter.name);
                    else
                        defaultValue += parameter.defaultBool.ToString();
                }

                if (m_Target.DoesParameterSynchronizeTypeExist(parameter.name) == false)
                    m_Target.SetParameterSynchronized(parameter.name, (PhotonAnimatorView.ParameterType)parameter.type,
                        PhotonAnimatorView.SynchronizeType.Disabled);

                var value = m_Target.GetParameterSynchronizeType(parameter.name);

                // check if using trigger and actually synchronizing it
                if (value != PhotonAnimatorView.SynchronizeType.Disabled &&
                    parameter.type == AnimatorControllerParameterType.Trigger) isUsingTriggers = true;

                var elementRect = new Rect(containerRect.xMin, containerRect.yMin + i * lineHeight, containerRect.width,
                    lineHeight);

                var labelRect = new Rect(elementRect.xMin + 5, elementRect.yMin + 2, EditorGUIUtility.labelWidth - 5,
                    elementRect.height);
                GUI.Label(labelRect, parameter.name + " (" + defaultValue + ")");

                var popupRect = new Rect(elementRect.xMin + EditorGUIUtility.labelWidth, elementRect.yMin + 2,
                    elementRect.width - EditorGUIUtility.labelWidth - 5, EditorGUIUtility.singleLineHeight);
                value = (PhotonAnimatorView.SynchronizeType)EditorGUI.EnumPopup(popupRect, value);

                if (i < GetParameterCount() - 1)
                {
                    var splitterRect = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                    PhotonGUI.DrawSplitter(splitterRect);
                }

                if (value != m_Target.GetParameterSynchronizeType(parameter.name))
                {
                    Undo.RecordObject(target, "Modify Synchronize Parameter " + parameter.name);
                    m_Target.SetParameterSynchronized(parameter.name, (PhotonAnimatorView.ParameterType)parameter.type,
                        value);
                }
            }

            // display note when synchronized triggers are detected.
            if (isUsingTriggers)
                EditorGUILayout.HelpBox("When using triggers, make sure this component is last in the stack. " +
                                        "If you still experience issues, implement triggers as a regular RPC " +
                                        "or in custom IPunObservable component instead.", MessageType.Warning);
        }
    }
}