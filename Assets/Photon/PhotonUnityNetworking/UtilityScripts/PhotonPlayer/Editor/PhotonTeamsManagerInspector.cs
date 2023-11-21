// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonTeamsManagerEditor.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities, 
// </copyright>
// <summary>
//  Custom inspector for PhotonTeamsManager
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
    [CustomEditor(typeof(PhotonTeamsManager))]
    public class PhotonTeamsManagerEditor : Editor
    {
        private const string proSkinString =
            "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAECAYAAACzzX7wAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAACJJREFUeNpi/P//PwM+wHL06FG8KpgYCABGZWVlvCYABBgA7/sHvGw+cz8AAAAASUVORK5CYII=";

        private const string lightSkinString =
            "iVBORw0KGgoAAAANSUhEUgAAAAgAAAACCAIAAADq9gq6AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAABVJREFUeNpiVFZWZsAGmBhwAIAAAwAURgBt4C03ZwAAAABJRU5ErkJggg==";

        private const string removeTextureName = "removeButton_generated";
        private readonly Dictionary<byte, bool> foldouts = new();

        private bool isOpen;
        private SerializedProperty listFoldIsOpenSp;
        private PhotonTeamsManager photonTeams;
        private Texture removeTexture;
        private SerializedProperty teamsListSp;

        private void OnEnable()
        {
            photonTeams = target as PhotonTeamsManager;
            teamsListSp = serializedObject.FindProperty("teamsList");
            listFoldIsOpenSp = serializedObject.FindProperty("listFoldIsOpen");
            isOpen = listFoldIsOpenSp.boolValue;
            removeTexture = LoadTexture(removeTextureName, proSkinString, lightSkinString);
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        /// <summary>
        ///     Read width and height if PNG file in pixels.
        /// </summary>
        /// <param name="imageData">PNG image data.</param>
        /// <param name="width">Width of image in pixels.</param>
        /// <param name="height">Height of image in pixels.</param>
        private static void GetImageSize(byte[] imageData, out int width, out int height)
        {
            width = ReadInt(imageData, 3 + 15);
            height = ReadInt(imageData, 3 + 15 + 2 + 2);
        }

        private static int ReadInt(byte[] imageData, int offset)
        {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }

        private Texture LoadTexture(string textureName, string proSkin, string lightSkin)
        {
            var skin = EditorGUIUtility.isProSkin ? proSkin : lightSkin;
            // Get image data (PNG) from base64 encoded strings.
            var imageData = Convert.FromBase64String(skin);
            // Gather image size from image data.
            int texWidth, texHeight;
            GetImageSize(imageData, out texWidth, out texHeight);
            // Generate texture asset.
            var tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false, true);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.name = textureName;
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(imageData);
            return tex;
        }

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                DrawTeamsList();
                return;
            }

            var availableTeams = photonTeams.GetAvailableTeams();
            if (availableTeams != null)
            {
                EditorGUI.indentLevel++;
                foreach (var availableTeam in availableTeams)
                {
                    if (!foldouts.ContainsKey(availableTeam.Code)) foldouts[availableTeam.Code] = true;
                    Player[] teamMembers;
                    if (photonTeams.TryGetTeamMembers(availableTeam, out teamMembers) && teamMembers != null)
                        foldouts[availableTeam.Code] = EditorGUILayout.Foldout(foldouts[availableTeam.Code],
                            string.Format("{0} ({1})", availableTeam.Name, teamMembers.Length));
                    else
                        foldouts[availableTeam.Code] = EditorGUILayout.Foldout(foldouts[availableTeam.Code],
                            string.Format("{0} (0)", availableTeam.Name));
                    if (foldouts[availableTeam.Code] && teamMembers != null)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var player in teamMembers)
                            EditorGUILayout.LabelField(string.Empty,
                                string.Format("{0} {1}", player, player.IsLocal ? " - You -" : string.Empty));
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawTeamsList()
        {
            GUILayout.Space(5);
            var codes = new HashSet<byte>();
            var names = new HashSet<string>();
            for (var i = 0; i < teamsListSp.arraySize; i++)
            {
                var e = teamsListSp.GetArrayElementAtIndex(i);
                var name = e.FindPropertyRelative("Name").stringValue;
                var code = (byte)e.FindPropertyRelative("Code").intValue;
                codes.Add(code);
                names.Add(name);
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            isOpen = PhotonGUI.ContainerHeaderFoldout(string.Format("Teams List ({0})", teamsListSp.arraySize), isOpen);
            if (EditorGUI.EndChangeCheck()) listFoldIsOpenSp.boolValue = isOpen;
            if (isOpen)
            {
                const float containerElementHeight = 22;
                const float propertyHeight = 16;
                const float paddingRight = 29;
                const float paddingLeft = 5;
                const float spacingY = 3;
                var containerHeight = (teamsListSp.arraySize + 1) * containerElementHeight;
                var containerRect = PhotonGUI.ContainerBody(containerHeight);
                var propertyWidth = containerRect.width - paddingRight;
                var codePropertyWidth = propertyWidth / 5;
                var namePropertyWidth = 4 * propertyWidth / 5;
                var elementRect = new Rect(containerRect.xMin, containerRect.yMin,
                    containerRect.width, containerElementHeight);
                var propertyPosition = new Rect(elementRect.xMin + paddingLeft, elementRect.yMin + spacingY,
                    codePropertyWidth, propertyHeight);
                EditorGUI.LabelField(propertyPosition, "Code");
                var secondPropertyPosition = new Rect(elementRect.xMin + paddingLeft + codePropertyWidth,
                    elementRect.yMin + spacingY,
                    namePropertyWidth, propertyHeight);
                EditorGUI.LabelField(secondPropertyPosition, "Name");
                for (var i = 0; i < teamsListSp.arraySize; ++i)
                {
                    elementRect = new Rect(containerRect.xMin, containerRect.yMin + containerElementHeight * (i + 1),
                        containerRect.width, containerElementHeight);
                    propertyPosition = new Rect(elementRect.xMin + paddingLeft, elementRect.yMin + spacingY,
                        codePropertyWidth, propertyHeight);
                    var teamElementSp = teamsListSp.GetArrayElementAtIndex(i);
                    var teamNameSp = teamElementSp.FindPropertyRelative("Name");
                    var teamCodeSp = teamElementSp.FindPropertyRelative("Code");
                    var oldName = teamNameSp.stringValue;
                    var oldCode = (byte)teamCodeSp.intValue;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(propertyPosition, teamCodeSp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var newCode = (byte)teamCodeSp.intValue;
                        if (codes.Contains(newCode))
                        {
                            Debug.LogWarningFormat("Team with the same code {0} already exists", newCode);
                            teamCodeSp.intValue = oldCode;
                        }
                    }

                    secondPropertyPosition = new Rect(elementRect.xMin + paddingLeft + codePropertyWidth,
                        elementRect.yMin + spacingY,
                        namePropertyWidth, propertyHeight);
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(secondPropertyPosition, teamNameSp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var newName = teamNameSp.stringValue;
                        if (string.IsNullOrEmpty(newName))
                        {
                            Debug.LogWarning("Team name cannot be null or empty");
                            teamNameSp.stringValue = oldName;
                        }
                        else if (names.Contains(newName))
                        {
                            Debug.LogWarningFormat("Team with the same name \"{0}\" already exists", newName);
                            teamNameSp.stringValue = oldName;
                        }
                    }

                    var removeButtonRect = new Rect(
                        elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                        elementRect.yMin + 2,
                        PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                        PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);
                    if (GUI.Button(removeButtonRect, new GUIContent(removeTexture), PhotonGUI.DefaultRemoveButtonStyle))
                        teamsListSp.DeleteArrayElementAtIndex(i);
                    if (i < teamsListSp.arraySize - 1)
                    {
                        var texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4,
                            1);
                        PhotonGUI.DrawSplitter(texturePosition);
                    }
                }
            }

            if (PhotonGUI.AddButton())
            {
                byte c = 0;
                while (codes.Contains(c) && c < byte.MaxValue) c++;
                teamsListSp.arraySize++;
                var teamElementSp = teamsListSp.GetArrayElementAtIndex(teamsListSp.arraySize - 1);
                var teamNameSp = teamElementSp.FindPropertyRelative("Name");
                var teamCodeSp = teamElementSp.FindPropertyRelative("Code");
                teamCodeSp.intValue = c;
                var n = "New Team";
                var o = 1;
                while (names.Contains(n))
                {
                    n = string.Format("New Team {0}", o);
                    o++;
                }

                teamNameSp.stringValue = n;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}