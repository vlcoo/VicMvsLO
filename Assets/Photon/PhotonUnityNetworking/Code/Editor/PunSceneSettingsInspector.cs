// ----------------------------------------------------------------------------
// <copyright file="PunSceneSettingsInspector.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2019 Exit Games GmbH
// </copyright>
// <summary>
//   Custom inspector for the PunSceneSettings component.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun
{
    [CustomEditor(typeof(PunSceneSettings))]
    internal class PunSceneSettingsInspector : Editor
    {
        private List<string> _duplicateScenesDefinition;
        private List<int> _duplicateViewIdDefinition;

        private bool _firstTime;
        private SerializedProperty _sceneSettings_i;
        private bool isOpen;

        private SerializedProperty listProperty;
        private PunSceneSettings m_Target;
        private SerializedProperty minViewIdProperty;
        private SerializedProperty sceneAssetProperty;
        private SerializedProperty sceneNameProperty;

        private void OnEnable()
        {
            _firstTime = true;
        }


        public override void OnInspectorGUI()
        {
            m_Target = (PunSceneSettings)target;

            // error checking
            _duplicateScenesDefinition = m_Target.MinViewIdPerScene.GroupBy(x => x.sceneName)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            _duplicateViewIdDefinition = m_Target.MinViewIdPerScene.GroupBy(x => x.minViewId)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            DrawSceneSettingsList();

            foreach (var dup in _duplicateScenesDefinition)
                EditorGUILayout.LabelField("Found duplicates for scene", dup);


            foreach (var sceneSettings in m_Target.MinViewIdPerScene)
            {
                if (_duplicateViewIdDefinition.Contains(sceneSettings.minViewId))
                    GUILayout.Label("Found view Id duplicates '" + sceneSettings.minViewId + "' for scene: " +
                                    sceneSettings.sceneName);

                if (sceneSettings.minViewId > PhotonNetwork.MAX_VIEW_IDS)
                    GUILayout.Label(sceneSettings.sceneName + " view Id can not exceed the max view Id " +
                                    PhotonNetwork.MAX_VIEW_IDS);

                if (sceneSettings.minViewId < 1)
                    GUILayout.Label(sceneSettings.sceneName + " view Id can not be less than 1");

                if (sceneSettings.sceneAsset == null && !string.IsNullOrEmpty(sceneSettings.sceneName))
                    GUILayout.Label("'" + sceneSettings.sceneName + "' scene is missing in the project");
            }

            _firstTime = false;
        }

        private void DrawSceneSettingsList()
        {
            GUILayout.Space(5);

            // check for changes ( from undo for example)
            serializedObject.Update();

            listProperty = serializedObject.FindProperty("MinViewIdPerScene");

            if (listProperty == null) return;

            float containerElementHeight = 44;
            var containerHeight = listProperty.arraySize * containerElementHeight;

            isOpen = PhotonGUI.ContainerHeaderFoldout("Scene Settings (" + listProperty.arraySize + ")",
                serializedObject.FindProperty("SceneSettingsListFoldoutOpen").boolValue);
            serializedObject.FindProperty("SceneSettingsListFoldoutOpen").boolValue = isOpen;

            if (isOpen == false) containerHeight = 0;

            var containerRect = PhotonGUI.ContainerBody(containerHeight);
            if (isOpen)
                for (var i = 0; i < listProperty.arraySize; ++i)
                {
                    var elementRect = new Rect(containerRect.xMin, containerRect.yMin + containerElementHeight * i,
                        containerRect.width, containerElementHeight);
                    {
                        var texturePosition = new Rect(elementRect.xMin + 6,
                            elementRect.yMin + elementRect.height / 2f - 1, 9, 5);
                        ReorderableListResources.DrawTexture(texturePosition, ReorderableListResources.texGrabHandle);

                        var propertyPosition = new Rect(elementRect.xMin + 20, elementRect.yMin + 3,
                            elementRect.width - 45, 16);

                        _sceneSettings_i = listProperty.GetArrayElementAtIndex(i);

                        sceneNameProperty = _sceneSettings_i.FindPropertyRelative("sceneName");
                        sceneAssetProperty = _sceneSettings_i.FindPropertyRelative("sceneAsset");
                        minViewIdProperty = _sceneSettings_i.FindPropertyRelative("minViewId");

                        var _sceneName = sceneNameProperty.stringValue;
                        var _sceneAsset = m_Target.MinViewIdPerScene[i].sceneAsset;

                        // check if we need to find the scene asset based on the scene name. This is for backward compatibility or when the scene asset was deleted
                        if (_firstTime)
                            if (_sceneAsset == null && !string.IsNullOrEmpty(_sceneName))
                            {
                                var guids = AssetDatabase.FindAssets(_sceneName + " t:SceneAsset");

                                foreach (var guid in guids)
                                {
                                    var path = AssetDatabase.GUIDToAssetPath(guid);
                                    if (Path.GetFileNameWithoutExtension(path) == _sceneName)
                                    {
                                        sceneAssetProperty.objectReferenceValue =
                                            AssetDatabase.LoadAssetAtPath<SceneAsset>(
                                                AssetDatabase.GUIDToAssetPath(guid));
                                        break;
                                    }
                                }
                            }

                        var _missingSceneAsset = _sceneAsset == null && !string.IsNullOrEmpty(_sceneName);
                        // if we don't have a scene asset for the serialized scene named, we show an error.
                        if (_missingSceneAsset ||
                            (sceneNameProperty != null && _duplicateScenesDefinition != null &&
                             _duplicateScenesDefinition.Contains(sceneNameProperty.stringValue))
                           )
                            GUI.color = Color.red;

                        EditorGUI.BeginChangeCheck();
                        var _label = _missingSceneAsset
                            ? "Scene Asset: Missing '" + _sceneName + "'"
                            : "Scene Asset";

                        EditorGUI.PropertyField(propertyPosition, sceneAssetProperty, new GUIContent(_label));

                        if (EditorGUI.EndChangeCheck())
                        {
                            _sceneAsset = sceneAssetProperty.objectReferenceValue as SceneAsset;
                            if (_sceneAsset == null && !string.IsNullOrEmpty(sceneNameProperty.stringValue))
                                sceneNameProperty.stringValue = null;
                            else if (sceneNameProperty.stringValue != _sceneAsset.name)
                                sceneNameProperty.stringValue = _sceneAsset.name;
                        }


                        // EditorGUI.PropertyField(propertyPosition,  sceneNameProperty,
                        //    new GUIContent("Scene Name"));
                        GUI.color = Color.white;

                        if (minViewIdProperty.intValue < 1 || minViewIdProperty.intValue > PhotonNetwork.MAX_VIEW_IDS)
                            GUI.color = Color.red;
                        var secondPropertyPosition = new Rect(elementRect.xMin + 20,
                            elementRect.yMin + containerElementHeight / 2,
                            elementRect.width - 45, 16);

                        EditorGUI.PropertyField(secondPropertyPosition,
                            _sceneSettings_i.FindPropertyRelative("minViewId"),
                            new GUIContent("Minimum View ID"));

                        GUI.color = Color.white;


                        //Debug.Log( listProperty.GetArrayElementAtIndex( i ).objectReferenceValue.GetType() );
                        //Rect statsPosition = new Rect( propertyPosition.xMax + 7, propertyPosition.yMin, statsIcon.width, statsIcon.height );
                        //ReorderableListResources.DrawTexture( statsPosition, statsIcon );
                        var removeButtonRect = new Rect(
                            elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                            elementRect.yMin + 2,
                            PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                            PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);


                        if (GUI.Button(removeButtonRect, new GUIContent(ReorderableListResources.texRemoveButton),
                                PhotonGUI.DefaultRemoveButtonStyle))
                        {
                            listProperty.DeleteArrayElementAtIndex(i);

                            Undo.RecordObject(m_Target, "Removed SceneSettings Entry");
                        }


                        if (i < listProperty.arraySize - 1)
                        {
                            texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4,
                                1);
                            PhotonGUI.DrawSplitter(texturePosition);
                        }
                    }
                }

            if (PhotonGUI.AddButton())
            {
                listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));
                _sceneSettings_i = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                sceneNameProperty = _sceneSettings_i.FindPropertyRelative("sceneName");
                sceneAssetProperty = _sceneSettings_i.FindPropertyRelative("sceneAsset");
                minViewIdProperty = _sceneSettings_i.FindPropertyRelative("minViewId");

                sceneAssetProperty.objectReferenceValue = null;
                sceneNameProperty.stringValue = "";
                minViewIdProperty.intValue = 1;

                Undo.RecordObject(m_Target, "Added SceneSettings Entry");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}