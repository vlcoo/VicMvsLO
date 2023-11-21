// ----------------------------------------------------------------------------
// <copyright file="ServerSettingsInspector.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a custom editor for the ServerSettings scriptable object.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using System.Reflection;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun
{
    [CustomEditor(typeof(ServerSettings))]
    public class ServerSettingsInspector : Editor
    {
        private const string notAvailableLabel = "n/a";

        private string prefLabel;

        private string[] regionsPrefsList;

        private string rpcCrc;
        private bool showRpcs;
        private string versionPhoton;

        private GUIStyle vertboxStyle;

        public void Awake()
        {
            versionPhoton = Assembly.GetAssembly(typeof(PhotonPeer)).GetName().Version.ToString();
        }


        public override void OnInspectorGUI()
        {
            if (vertboxStyle == null)
                vertboxStyle = new GUIStyle("HelpBox") { padding = new RectOffset(6, 6, 6, 6) };

            var sObj = new SerializedObject(target);
            var settings = target as ServerSettings;


            EditorGUI.BeginChangeCheck();

            #region Version Vertical Box

            EditorGUILayout.BeginVertical( /*vertboxStyle*/);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Version:", "Version of PUN and Photon3Unity3d.dll."));
            GUILayout.FlexibleSpace();
            var helpicorect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(16));
            EditorGUIUtility.AddCursorRect(helpicorect, MouseCursor.Link);
            if (GUI.Button(helpicorect, PhotonGUI.HelpIcon, GUIStyle.none))
                Application.OpenURL(PhotonEditor.UrlPunSettings);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Pun: " + PhotonNetwork.PunVersion + " Photon lib: " + versionPhoton);
            EditorGUILayout.EndVertical();

            #endregion Version Vertical Box

            EditorGUI.indentLevel--;
            var showSettingsProp = serializedObject.FindProperty("ShowSettings");
            var showSettings =
                showSettingsProp.Foldout(new GUIContent("Server/Cloud Settings", "Core Photon Server/Cloud settings."));
            EditorGUI.indentLevel++;


            if (showSettings != settings.ShowSettings) showSettingsProp.boolValue = showSettings;

            if (showSettingsProp.boolValue)
            {
                var settingsSp = serializedObject.FindProperty("AppSettings");

                EditorGUI.indentLevel++;

                //Realtime APP ID
                BuildAppIdField(settingsSp.FindPropertyRelative("AppIdRealtime"), "App Id PUN");

                if (PhotonEditorUtils.HasChat) BuildAppIdField(settingsSp.FindPropertyRelative("AppIdChat"));
                if (PhotonEditorUtils.HasVoice) BuildAppIdField(settingsSp.FindPropertyRelative("AppIdVoice"));

                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("AppVersion"));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("UseNameServer"),
                    new GUIContent("Use Name Server",
                        "Photon Cloud requires this checked.\nUncheck for Photon Server SDK (OnPremise)."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("FixedRegion"),
                    new GUIContent("Fixed Region",
                        "Photon Cloud setting, needs a Name Server.\nDefine one region to always connect to.\nLeave empty to use the best region from a server-side region list."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("Server"),
                    new GUIContent("Server",
                        "Typically empty for Photon Cloud.\nFor Photon OnPremise, enter your host name or IP. Also uncheck \"Use Name Server\" for older Photon OnPremise servers."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("Port"),
                    new GUIContent("Port",
                        "Leave 0 to use default Photon Cloud ports for the Name Server.\nOnPremise defaults to 5055 for UDP and 4530 for TCP."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("ProxyServer"),
                    new GUIContent("Proxy Server",
                        "HTTP Proxy Server for WebSocket connection. See LoadBalancingClient.ProxyServerAddress for options."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("Protocol"),
                    new GUIContent("Protocol",
                        "Use UDP where possible.\nWSS works on WebGL and Xbox exports.\nDefine WEBSOCKET for use on other platforms."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("EnableProtocolFallback"),
                    new GUIContent("Protocol Fallback",
                        "Automatically try another network protocol, if initial connect fails.\nWill use default Name Server ports."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("EnableLobbyStatistics"),
                    new GUIContent("Lobby Statistics",
                        "When using multiple room lists (lobbies), the server can send info about their usage."));
                EditorGUILayout.PropertyField(settingsSp.FindPropertyRelative("NetworkLogging"),
                    new GUIContent("Network Logging", "Log level for the Photon libraries."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("PunLogging"),
                new GUIContent("PUN Logging", "Log level for the PUN layer."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableSupportLogger"),
                new GUIContent("Support Logger",
                    "Logs additional info for debugging.\nUse this when you submit bugs to the Photon Team."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RunInBackground"),
                new GUIContent("Run In Background",
                    "Enables apps to keep the connection without focus. Android and iOS ignore this."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StartInOfflineMode"),
                new GUIContent("Start In Offline Mode", "Simulates an online connection.\nPUN can be used as usual."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DevRegion"),
                new GUIContent("Dev Region",
                    "Photon Cloud setting, needs a Name Server.\nDefine region the Editor and Development builds will always connect to - ensuring all users can find common rooms.\nLeave empty to use the Fixed Region or best region from a server-side region list. This value will be ignored for non-Development builds."));

            #region Best Region Box

            EditorGUILayout.BeginVertical(vertboxStyle);

            if (!string.IsNullOrEmpty(PhotonNetwork.BestRegionSummaryInPreferences))
            {
                regionsPrefsList =
                    PhotonNetwork.BestRegionSummaryInPreferences.Split(new[] { ';' },
                        StringSplitOptions.RemoveEmptyEntries);
                if (regionsPrefsList.Length < 2)
                    prefLabel = notAvailableLabel;
                else
                    prefLabel = string.Format("'{0}' ping:{1}ms ", regionsPrefsList[0], regionsPrefsList[1]);
            }
            else
            {
                prefLabel = notAvailableLabel;
            }

            EditorGUILayout.LabelField(new GUIContent("Best Region Preference: " + prefLabel,
                "Best region is used if Fixed Region is empty."));

            EditorGUILayout.BeginHorizontal();

            var resetrect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));
            var editrect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));
            if (GUI.Button(resetrect, "Reset", EditorStyles.miniButton))
                ServerSettings.ResetBestRegionCodeInPreferences();

            if (GUI.Button(editrect, "Edit WhiteList", EditorStyles.miniButton))
                Application.OpenURL("https://dashboard.photonengine.com/en-US/App/RegionsWhitelistEdit/" +
                                    PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            #endregion Best Region Box


            //this.showRpcs = EditorGUILayout.Foldout(this.showRpcs, new GUIContent("RPCs", "RPC shortcut list."));
            EditorGUI.indentLevel--;
            showRpcs = showRpcs.Foldout(new GUIContent("RPCs", "RPC shortcut list."));
            EditorGUI.indentLevel++;

            if (showRpcs)
            {
                // first time check to get the rpc has proper
                if (string.IsNullOrEmpty(rpcCrc)) rpcCrc = RpcListHashCode().ToString("X");

                #region Begin Vertical Box CRC

                EditorGUILayout.BeginVertical(vertboxStyle);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("List CRC");

                EditorGUI.indentLevel--;
                var copyrect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(16));
                EditorGUILayout.GetControlRect(GUILayout.MaxWidth(12));
                var hashrect =
                    EditorGUILayout.GetControlRect(
                        GUILayout.MinWidth(16)); // new Rect(copyrect) { xMin = copyrect.xMin + 32 };

                EditorGUIUtility.AddCursorRect(copyrect, MouseCursor.Link);
                EditorGUI.LabelField(copyrect, new GUIContent("", "Copy Hashcode to Clipboard"));
                if (GUI.Button(copyrect, PhotonGUI.CopyIcon, GUIStyle.none))
                {
                    Debug.Log("RPC-List HashCode copied into your ClipBoard: " + rpcCrc +
                              ". Make sure clients that send each other RPCs have the same RPC-List.");
                    EditorGUIUtility.systemCopyBuffer = rpcCrc;
                }

                EditorGUI.SelectableLabel(hashrect, rpcCrc);

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();

                var refreshrect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));
                var clearrect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));

                if (GUI.Button(refreshrect, "Refresh RPCs", EditorStyles.miniButton))
                {
                    PhotonEditor.UpdateRpcList();
                    Repaint();
                }

                if (GUI.Button(clearrect, "Clear RPCs", EditorStyles.miniButton)) PhotonEditor.ClearRpcList();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                #endregion End Vertical Box CRC

                EditorGUI.indentLevel++;

                var sRpcs = sObj.FindProperty("RpcList");
                EditorGUILayout.PropertyField(sRpcs, true);

                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                sObj.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();

                // cache the rpc hash
                rpcCrc = RpcListHashCode().ToString("X");
            }

            #region Simple Settings

            /// Conditional Simple Sync Settings DrawGUI - Uses reflection to avoid having to hard connect the libraries
            var SettingsScriptableObjectBaseType = GetType("Photon.Utilities.SettingsScriptableObjectBase");
            if (SettingsScriptableObjectBaseType != null)
            {
                EditorGUILayout.GetControlRect(false, 3);

                EditorGUILayout.LabelField("Simple Extension Settings", (GUIStyle)"BoldLabel");

                var drawAllMethod = SettingsScriptableObjectBaseType.GetMethod("DrawAllSettings");

                if (drawAllMethod != null && this != null)
                {
                    var initializeAsOpen = false;
                    drawAllMethod.Invoke(null, new object[2] { this, initializeAsOpen });
                }
            }

            #endregion
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private int RpcListHashCode()
        {
            // this is a hashcode generated to (more) easily compare this Editor's RPC List with some other
            var hashCode = PhotonNetwork.PhotonServerSettings.RpcList.Count + 1;
            foreach (var s in PhotonNetwork.PhotonServerSettings.RpcList)
            {
                var h1 = s.GetHashCode();
                hashCode = ((h1 << 5) + h1) ^ hashCode;
            }

            return hashCode;
        }

        private void BuildAppIdField(SerializedProperty property, string label = null)
        {
            EditorGUILayout.BeginHorizontal();

            if (label != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label), GUILayout.MinWidth(32));
            else
                EditorGUILayout.PropertyField(property, GUILayout.MinWidth(32));

            property.stringValue = property.stringValue.Trim();
            var appId = property.stringValue;

            var url = "https://dashboard.photonengine.com/en-US/PublicCloud";

            if (!string.IsNullOrEmpty(appId))
                url = string.Format("https://dashboard.photonengine.com/en-US/App/Manage/{0}", appId);
            if (GUILayout.Button("Dashboard", EditorStyles.miniButton, GUILayout.MinWidth(78), GUILayout.MaxWidth(78)))
                Application.OpenURL(url);
            EditorGUILayout.EndHorizontal();
        }
    }
}