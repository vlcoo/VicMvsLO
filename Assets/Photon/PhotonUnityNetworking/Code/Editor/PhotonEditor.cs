// ----------------------------------------------------------------------------
// <copyright file="PhotonEditor.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   MenuItems and in-Editor scripts for PhotonNetwork.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Photon.Pun
{
    public class PunWizardText
    {
        public string AlreadyRegisteredInfo =
            "The email is registered so we can't fetch your AppId (without password).\n\nPlease login online to get your AppId and paste it above.";

        public string AppliedToSettingsInfo = "Your AppId is now applied to this project.";
        public string CancelButton = "Cancel";
        public string CloseWindowButton = "Close";
        public string ComparisonPageButton = "Cloud versus OnPremise";
        public string ConnectionInfo = "Connecting to the account service...";
        public string ConnectionTitle = "Connecting";
        public string DocumentationLabel = "Documentation:";
        public string EmailOrAppIdLabel = "AppId or Email";
        public string ErrorTextTitle = "Error";

        public string FullRPCListLabel =
            "Your project's RPC-list is too long for PUN.\n\nYou can change PUN's source to use short-typed RPC index. Look for comments 'LIMITS RPC COUNT'\n\nAlternatively, remove some RPC methods (use more parameters per RPC maybe).\n\nAfter a RPC-list refresh, make sure you change the game version where you use PhotonNetwork.ConnectUsingSettings().";

        public string FullRPCListTitle = "Warning: RPC-list is full!";

        public string IncorrectRPCListLabel =
            "Your project's RPC-list is full, so we can't add some RPCs just compiled.\n\nBy removing outdated RPCs, the list will be long enough but incompatible with older client builds!\n\nMake sure you change the game version where you use PhotonNetwork.ConnectUsingSettings().";

        public string IncorrectRPCListTitle = "Warning: RPC-list becoming incompatible!";
        public string LocateSettingsButton = "Locate PhotonServerSettings";
        public string MainMenuButton = "Main Menu";
        public string OkButton = "Ok";
        public string OpenCloudDashboardText = "Cloud Dashboard Login";
        public string OpenCloudDashboardTooltip = "Review Cloud App information and statistics.";
        public string OpenDevNetText = "Doc Pages / Manual";
        public string OpenDevNetTooltip = "Online documentation for Photon.";
        public string OpenForumText = "Open Forum";
        public string OpenForumTooltip = "Online support for Photon.";
        public string OpenPDFText = "Reference PDF";
        public string OpenPDFTooltip = "Opens the local documentation pdf.";
        public string OwnHostCloudCompareLabel = "How 'my own host' compares to 'cloud'.";

        public string PUNNameReplaceLabel =
            "PUN replaces RPC names with numbers by using the RPC-list. All clients must use the same list for that.\n\nClearing it most likely makes your client incompatible with previous versions! Change your game version or make sure the RPC-list matches other clients.";

        public string PUNNameReplaceTitle = "Warning: RPC-list Compatibility";
        public string PUNWizardLabel = "PUN Wizard";

        public string RegisteredNewAccountInfo =
            "We created a (free) account and fetched you an AppId.\nWelcome. Your PUN project is setup.";

        public string RemoveOutdatedRPCsLabel = "Remove outdated RPCs";
        public string RPCListCleared = "Clear RPC-list";

        public string ServerSettingsCleanedWarning =
            "Cleared the PhotonServerSettings.RpcList, which breaks compatibility with older builds. You should update the \"App Version\" in the PhotonServerSettings to avoid issues.";

        public string SettingsButton = "Settings:";
        public string SettingsHighlightLabel = "Highlights the used photon settings file in the project.";
        public string SetupButton = "Setup Project";

        public string SetupCompleteInfo =
            "<b>Done!</b>\nAll connection settings can be edited in the <b>PhotonServerSettings</b> now.\nHave a look.";

        public string SetupServerCloudLabel = "Setup wizard for setting up your own server or the cloud.";

        public string SetupWizardInfo =
            "Thanks for importing Photon Unity Networking.\nThis window should set you up.\n\n<b>-</b> To use an existing Photon Cloud App, enter your AppId.\n<b>-</b> To register an account or access an existing one, enter the account's mail address.\n<b>-</b> To use Photon OnPremise, skip this step.";

        public string SetupWizardTitle = "PUN Setup";

        public string SetupWizardWarningMessage =
            "You have not yet run the Photon setup wizard! Your game won't be able to connect. See Windows -> Photon Unity Networking.";

        public string SetupWizardWarningTitle = "Warning";
        public string SkipButton = "Skip";

        public string SkipRegistrationInfo =
            "Skipping? No problem:\nEdit your server settings in the PhotonServerSettings file.";

        public string SkipRPCListUpdateLabel = "Skip RPC-list update";
        public string StartButton = "Start";
        public string WarningPhotonDisconnect = "Disconnecting PUN due to recompile. Exit PlayMode.";
        public string WindowTitle = "PUN Wizard";

        public string WizardMainWindowInfo =
            "This window should help you find important settings for PUN, as well as documentation.";
    }


    public class PhotonEditor : EditorWindow
    {
        public const string UrlDevNet = "https://doc.photonengine.com/en-us/pun/v2";

        public const string UrlCloudDashboard = "https://dashboard.photonengine.com/en-US/account/signin?email=";

        public const string
            UrlPunSettings =
                "https://doc.photonengine.com/en-us/pun/v2/getting-started/initial-setup"; // the SeverSettings class has this url directly in it's HelpURL attribute.

        protected static Type WindowType = typeof(PhotonEditor);

        private static Texture2D BackgroundImage;

        public static PunWizardText CurrentLang = new();

        /// <summary>
        ///     third parties custom token
        /// </summary>
        public static string CustomToken = null;

        /// <summary>
        ///     third parties custom context
        /// </summary>
        public static string CustomContext = null;

        protected static string DocumentationLocation = "Assets/Photon/PhotonNetworking-Documentation.pdf";

        protected static string UrlFreeLicense = "https://dashboard.photonengine.com/en-US/SelfHosted";

        protected static string UrlForum = "https://forum.photonengine.com";

        protected static string UrlCompare =
            "https://doc.photonengine.com/en-us/realtime/current/getting-started/onpremise-or-saas";

        protected static string UrlHowToSetup =
            "https://doc.photonengine.com/en-us/onpremise/current/getting-started/photon-server-in-5min";

        protected static string UrlAppIDExplained =
            "https://doc.photonengine.com/en-us/realtime/current/getting-started/obtain-your-app-id";


        private static double lastWarning;
        private static bool postInspectorUpdate;

        private readonly Vector2 preferredSize = new(350, 400);
        private bool close;
        private bool highlightedSettings;

        private bool isSetupWizard;
        private string mailOrAppId = string.Empty;


        private bool minimumInput;

        private PhotonSetupStates photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;

        protected Vector2 scrollPos = Vector2.zero;


        private AccountService serviceClient;
        private bool useAppId;
        private bool useMail;
        private bool useSkip;


        [MenuItem("Window/Photon Unity Networking/PUN Wizard &p", false, 0)]
        protected static void MenuItemOpenWizard()
        {
            var win = GetWindow<PhotonEditor>(false, CurrentLang.WindowTitle, true);
            if (win == null) return;
            win.photonSetupState = PhotonSetupStates.MainUi;
            win.isSetupWizard = false;
        }

        [MenuItem("Window/Photon Unity Networking/Highlight Server Settings %#&p", false, 1)]
        protected static void MenuItemHighlightSettings()
        {
            HighlightSettings();
        }


        [InitializeOnLoadMethod]
        public static void InitializeOnLoadMethod()
        {
            //Debug.Log("InitializeOnLoadMethod()");
            EditorApplication.delayCall += OnDelayCall;
        }


        // used to register for various events (post-load)
        private static void OnDelayCall()
        {
            //Debug.Log("OnDelayCall()");

            postInspectorUpdate = true;

            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;

#if UNITY_2021_1_OR_NEWER
            CompilationPipeline.compilationStarted -= OnCompileStarted21;
            CompilationPipeline.compilationStarted += OnCompileStarted21;
#else
            CompilationPipeline.assemblyCompilationStarted -= OnCompileStarted;
            CompilationPipeline.assemblyCompilationStarted += OnCompileStarted;
#endif

#if (UNITY_2018 || UNITY_2018_1_OR_NEWER)
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;
#else
            EditorApplication.projectWindowChanged -= OnProjectChanged;
            EditorApplication.projectWindowChanged += OnProjectChanged;
#endif


            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                OnProjectChanged(); // call this initially from here, as the project change events happened earlier (on start of the Editor)
                UpdateRpcList();
            }
        }


        // called in editor, opens wizard for initial setup, keeps scene PhotonViews up to date and closes connections when compiling (to avoid issues)
        private static void OnProjectChanged()
        {
            PhotonEditorUtils.ProjectChangedWasCalled = true;


            // Prevent issues with Unity Cloud Builds where ServerSettings are not found.
            // Also, within the context of a Unity Cloud Build, ServerSettings is already present anyway.
#if UNITY_CLOUD_BUILD
            return;
#endif

            if (PhotonNetwork.PhotonServerSettings == null || PhotonNetwork.PhotonServerSettings.AppSettings == null ||
                string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
                PhotonNetwork.LoadOrCreateSettings(true);

            if (PhotonNetwork.PhotonServerSettings == null)
                // the PhotonServerSettings are loaded or created. If both fails, the Editor should probably not run (anymore).
                return;

            PunSceneSettings.SanitizeSceneSettings();


            // serverSetting is null when the file gets deleted. otherwise, the wizard should only run once and only if hosting option is not (yet) set
            if (!PhotonNetwork.PhotonServerSettings.DisableAutoOpenWizard)
            {
                ShowRegistrationWizard();
                PhotonNetwork.PhotonServerSettings.DisableAutoOpenWizard = true;
                SaveSettings();
            }
        }


#if UNITY_2021_1_OR_NEWER
        private static void OnCompileStarted21(object obj)
        {
            OnCompileStarted(obj as string);
        }
#endif

        private static void OnCompileStarted(string obj)
        {
            if (PhotonNetwork.IsConnected)
            {
                // log warning, unless there was one recently
                if (EditorApplication.timeSinceStartup - lastWarning > 3)
                {
                    Debug.LogWarning(CurrentLang.WarningPhotonDisconnect);
                    lastWarning = EditorApplication.timeSinceStartup;
                }

                PhotonNetwork.Disconnect();
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
#if UNITY_2019_4_OR_NEWER && UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
            }
        }


        [DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            //Debug.Log("OnDidReloadScripts() postInspectorUpdate: "+postInspectorUpdate + " isPlayingOrWillChangePlaymode: "+EditorApplication.isPlayingOrWillChangePlaymode);
            if (postInspectorUpdate &&
                !EditorApplication
                    .isPlayingOrWillChangePlaymode)
                UpdateRpcList(); // could be called when compilation finished (instead of when reload / compile starts)
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            //Debug.Log("PlayModeStateChanged");
            if (EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime) &&
                !PhotonNetwork.PhotonServerSettings.AppSettings.IsMasterServerAddress)
                EditorUtility.DisplayDialog(CurrentLang.SetupWizardWarningTitle, CurrentLang.SetupWizardWarningMessage,
                    CurrentLang.OkButton);
        }

        protected virtual void RegisterWithEmail(string email)
        {
            var types = new List<ServiceTypes>();
            types.Add(ServiceTypes.Pun);
            if (PhotonEditorUtils.HasChat) types.Add(ServiceTypes.Chat);
            if (PhotonEditorUtils.HasVoice) types.Add(ServiceTypes.Voice);


            if (serviceClient == null)
            {
                serviceClient = new AccountService();
                serviceClient.CustomToken = CustomToken;
                serviceClient.CustomContext = CustomContext;
            }
            else
            {
                // while RegisterByEmail will check RequestPendingResult below, it would also display an error message. no needed in this case
                if (serviceClient.RequestPendingResult)
                {
                    Debug.LogWarning("Registration request is pending a response. Please wait.");
                    return;
                }
            }

            emailSentToAccount = email;
            emailSentToAccountIsRegistered = false;

            if (serviceClient.RegisterByEmail(email, types, RegisterWithEmailSuccessCallback,
                    RegisterWithEmailErrorCallback, "PUN" + PhotonNetwork.PunVersion))
            {
                photonSetupState = PhotonSetupStates.EmailRegistrationPending;
                EditorUtility.DisplayProgressBar(CurrentLang.ConnectionTitle, CurrentLang.ConnectionInfo, 0.5f);
            }
            else
            {
                DisplayErrorMessage(
                    "Email registration request could not be sent. Retry again or check error logs and contact support.");
            }
        }

        private void RegisterWithEmailSuccessCallback(AccountServiceResponse res)
        {
            EditorUtility.ClearProgressBar();
            emailSentToAccountIsRegistered = true; // email is either registered now, or was already

            if (res.ReturnCode == AccountServiceReturnCodes.Success)
            {
                var key = ((int)ServiceTypes.Pun).ToString();
                string appId;
                if (res.ApplicationIds.TryGetValue(key, out appId))
                {
                    mailOrAppId = appId;
                    PhotonNetwork.PhotonServerSettings.UseCloud(mailOrAppId, null);
                    key = ((int)ServiceTypes.Chat).ToString();
                    if (res.ApplicationIds.TryGetValue(key, out appId))
                        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = appId;
                    else if (PhotonEditorUtils.HasChat)
                        Debug.LogWarning("Registration successful but no Chat AppId returned");
                    key = ((int)ServiceTypes.Voice).ToString();
                    if (res.ApplicationIds.TryGetValue(key, out appId))
                        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = appId;
                    else if (PhotonEditorUtils.HasVoice)
                        Debug.LogWarning("Registration successful but no Voice AppId returned");
                    SaveSettings();
                    photonSetupState = PhotonSetupStates.GoEditPhotonServerSettings;
                }
                else
                {
                    DisplayErrorMessage("Registration successful but no PUN AppId returned");
                }
            }
            else
            {
                SaveSettings();

                if (res.ReturnCode == AccountServiceReturnCodes.EmailAlreadyRegistered)
                    photonSetupState = PhotonSetupStates.EmailAlreadyRegistered;
                else
                    DisplayErrorMessage(res.Message);
            }
        }

        private void RegisterWithEmailErrorCallback(string error)
        {
            EditorUtility.ClearProgressBar();
            DisplayErrorMessage(error);
        }

        private void DisplayErrorMessage(string error)
        {
            EditorUtility.DisplayDialog(CurrentLang.ErrorTextTitle, error, CurrentLang.OkButton);
            photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
        }

        // Pings PhotonServerSettings and makes it selected (show in Inspector)
        private static void HighlightSettings()
        {
            var serverSettings =
                (ServerSettings)Resources.Load(PhotonNetwork.ServerSettingsFileName, typeof(ServerSettings));
            Selection.objects = new Object[] { serverSettings };
            EditorGUIUtility.PingObject(serverSettings);
        }

        // Marks settings object as dirty, so it gets saved.
        // unity 5.3 changes the usecase for SetDirty(). but here we don't modify a scene object! so it's ok to use
        private static void SaveSettings()
        {
            EditorUtility.SetDirty(PhotonNetwork.PhotonServerSettings);
        }

        private enum PhotonSetupStates
        {
            MainUi,

            RegisterForPhotonCloud,

            EmailAlreadyRegistered,

            GoEditPhotonServerSettings,

            EmailRegistrationPending
        }


        #region GUI and Wizard

        // setup per window
        public PhotonEditor()
        {
            minSize = preferredSize;
        }

        protected void Awake()
        {
            // check if some appid is set. if so, we can avoid registration calls.
            if (PhotonNetwork.PhotonServerSettings != null && PhotonNetwork.PhotonServerSettings.AppSettings != null &&
                !string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
                mailOrAppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        }

        /// <summary>Creates an Editor window, showing the cloud-registration wizard for Photon (entry point to setup PUN).</summary>
        protected static void ShowRegistrationWizard()
        {
            var win = GetWindow(WindowType, false, CurrentLang.WindowTitle, true) as PhotonEditor;
            if (win == null) return;

            win.photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
            win.isSetupWizard = true;
        }

        // Window Update() callback. On-demand, when Window is open
        protected void Update()
        {
            if (close) Close();
        }

        protected virtual void OnGUI()
        {
            if (BackgroundImage == null)
            {
                var paths = AssetDatabase.FindAssets("PunGradient t:Texture2D");
                if (paths != null && paths.Length > 0)
                    BackgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(paths[0]));
            }

            var oldGuiState =
                photonSetupState; // used to fix an annoying Editor input field issue: wont refresh until focus is changed.

            GUI.SetNextControlName(string.Empty);
            scrollPos = GUILayout.BeginScrollView(scrollPos);


            if (photonSetupState == PhotonSetupStates.MainUi)
            {
                UiMainWizard();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(photonSetupState == PhotonSetupStates.EmailRegistrationPending);
                UiSetupApp();
                EditorGUI.EndDisabledGroup();
            }


            GUILayout.EndScrollView();

            if (oldGuiState != photonSetupState) GUI.FocusControl(string.Empty);
        }

        private string emailSentToAccount;
        private bool emailSentToAccountIsRegistered;


        protected virtual void UiSetupApp()
        {
            GUI.skin.label.wordWrap = true;
            if (!isSetupWizard)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(CurrentLang.MainMenuButton, GUILayout.ExpandWidth(false)))
                    photonSetupState = PhotonSetupStates.MainUi;

                GUILayout.EndHorizontal();
            }


            // setup header
            UiTitleBox(CurrentLang.SetupWizardTitle, BackgroundImage);

            // setup info text
            GUI.skin.label.richText = true;
            GUILayout.Label(CurrentLang.SetupWizardInfo);

            // input of appid or mail
            EditorGUILayout.Separator();
            GUILayout.Label(CurrentLang.EmailOrAppIdLabel);
            minimumInput = false;
            useMail = false;
            useAppId = false;
            mailOrAppId = EditorGUILayout.TextField(mailOrAppId);
            if (!string.IsNullOrEmpty(mailOrAppId))
            {
                mailOrAppId = mailOrAppId.Trim(); // note: we trim all input
                if (AccountService.IsValidEmail(mailOrAppId))
                {
                    // input should be a mail address
                    useMail = true;

                    // check if the current input equals earlier input, which is known to be registered already
                    minimumInput = !mailOrAppId.Equals(emailSentToAccount) || !emailSentToAccountIsRegistered;
                }
                else if (ServerSettings.IsAppId(mailOrAppId))
                {
                    // this should be an appId
                    minimumInput = true;
                    useAppId = true;
                }
            }

            // button to skip setup
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(CurrentLang.SkipButton, GUILayout.Width(100)))
            {
                photonSetupState = PhotonSetupStates.GoEditPhotonServerSettings;
                useSkip = true;
                useMail = false;
                useAppId = false;
            }

            // SETUP button
            EditorGUI.BeginDisabledGroup(!minimumInput);
            if (GUILayout.Button(CurrentLang.SetupButton, GUILayout.Width(100)))
            {
                useSkip = false;
                GUIUtility.keyboardControl = 0;
                if (useMail)
                {
                    RegisterWithEmail(mailOrAppId); // sets state
                }
                else if (useAppId)
                {
                    photonSetupState = PhotonSetupStates.GoEditPhotonServerSettings;
                    Undo.RecordObject(PhotonNetwork.PhotonServerSettings, "Update PhotonServerSettings for PUN");
                    PhotonNetwork.PhotonServerSettings.UseCloud(mailOrAppId);
                    SaveSettings();
                }
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            // existing account needs to fetch AppId online
            if (photonSetupState == PhotonSetupStates.EmailAlreadyRegistered)
            {
                // button to open dashboard and get the AppId
                GUILayout.Space(15);
                GUILayout.Label(CurrentLang.AlreadyRegisteredInfo);


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(
                        new GUIContent(CurrentLang.OpenCloudDashboardText, CurrentLang.OpenCloudDashboardTooltip),
                        GUILayout.Width(205)))
                {
                    Application.OpenURL(string.Concat(UrlCloudDashboard, Uri.EscapeUriString(mailOrAppId)));
                    mailOrAppId = string.Empty;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }


            else if (photonSetupState == PhotonSetupStates.GoEditPhotonServerSettings)
            {
                if (!highlightedSettings)
                {
                    highlightedSettings = true;
                    HighlightSettings();
                }

                GUILayout.Space(15);
                if (useSkip)
                    GUILayout.Label(CurrentLang.SkipRegistrationInfo);
                else if (useMail)
                    GUILayout.Label(CurrentLang.RegisteredNewAccountInfo);
                else if (useAppId) GUILayout.Label(CurrentLang.AppliedToSettingsInfo);


                // setup-complete info
                GUILayout.Space(15);
                GUILayout.Label(CurrentLang.SetupCompleteInfo);


                // close window (done)
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(CurrentLang.CloseWindowButton, GUILayout.Width(205))) close = true;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUI.skin.label.richText = false;
        }

        private void UiTitleBox(string title, Texture2D bgIcon)
        {
            var bgStyle = EditorGUIUtility.isProSkin
                ? new GUIStyle(GUI.skin.GetStyle("Label"))
                : new GUIStyle(GUI.skin.GetStyle("WhiteLabel"));
            bgStyle.padding = new RectOffset(10, 10, 10, 10);
            bgStyle.fontSize = 22;
            bgStyle.fontStyle = FontStyle.Bold;
            if (bgIcon != null) bgStyle.normal.background = bgIcon;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            var scale = GUILayoutUtility.GetLastRect();
            scale.height = 44;

            GUI.Label(scale, title, bgStyle);
            GUILayout.Space(scale.height + 5);
        }

        protected virtual void UiMainWizard()
        {
            GUILayout.Space(15);

            // title
            UiTitleBox(CurrentLang.PUNWizardLabel, BackgroundImage);

            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(10, 10, 10, 10) });

            // wizard info text
            GUILayout.Label(CurrentLang.WizardMainWindowInfo, new GUIStyle("Label") { wordWrap = true });
            GUILayout.Space(15);


            // settings button
            GUILayout.Label(CurrentLang.SettingsButton, EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent(CurrentLang.LocateSettingsButton, CurrentLang.SettingsHighlightLabel)))
                HighlightSettings();
            if (GUILayout.Button(new GUIContent(CurrentLang.OpenCloudDashboardText,
                    CurrentLang.OpenCloudDashboardTooltip)))
                Application.OpenURL(UrlCloudDashboard + Uri.EscapeUriString(mailOrAppId));
            if (GUILayout.Button(new GUIContent(CurrentLang.SetupButton, CurrentLang.SetupServerCloudLabel)))
                photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;

            GUILayout.Space(15);


            // documentation
            GUILayout.Label(CurrentLang.DocumentationLabel, EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent(CurrentLang.OpenPDFText, CurrentLang.OpenPDFTooltip)))
                EditorUtility.OpenWithDefaultApp(DocumentationLocation);

            if (GUILayout.Button(new GUIContent(CurrentLang.OpenDevNetText, CurrentLang.OpenDevNetTooltip)))
                Application.OpenURL(UrlDevNet);

            GUI.skin.label.wordWrap = true;
            GUILayout.Label(CurrentLang.OwnHostCloudCompareLabel);
            if (GUILayout.Button(CurrentLang.ComparisonPageButton)) Application.OpenURL(UrlCompare);


            if (GUILayout.Button(new GUIContent(CurrentLang.OpenForumText, CurrentLang.OpenForumTooltip)))
                Application.OpenURL(UrlForum);

            GUILayout.EndVertical();
        }

        #endregion

        #region RPC List Handling

        public static void UpdateRpcList()
        {
            //Debug.Log("UpdateRpcList()");

            if (PhotonNetwork.PhotonServerSettings == null)
            {
                Debug.LogWarning(
                    "UpdateRpcList() wasn not able to access the PhotonServerSettings. Not updating the RPCs.");
                return;
            }


            // check all "script assemblies" for methods with PunRPC attribute
            var additionalRpcs = new List<string>(); // not yet listed rpc-method names go here
            var allRpcs = new List<string>();


#if UNITY_2019_2_OR_NEWER

            // we can make use of the new TypeCache to find methods with PunRPC attribute
            var extractedMethods = TypeCache.GetMethodsWithAttribute<PunRPC>();
            foreach (var methodInfo in extractedMethods)
            {
                allRpcs.Add(methodInfo.Name);
                if (!PhotonNetwork.PhotonServerSettings.RpcList.Contains(methodInfo.Name) &&
                    !additionalRpcs.Contains(methodInfo.Name)) additionalRpcs.Add(methodInfo.Name);
            }

#else
            System.Reflection.Assembly[] assemblies =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  System.AppDomain.CurrentDomain.GetAssemblies().Where(a => !(a.ManifestModule is System.Reflection.Emit.ModuleBuilder)).ToArray();

            foreach (var assembly in assemblies)
            {
                if (!assembly.Location.Contains("ScriptAssemblies") || assembly.FullName.StartsWith("Assembly-CSharp-Editor"))
                {
                    continue;
                }

                var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MonoBehaviour)));
                var methodInfos =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           types.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                var methodNames =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        methodInfos.Where(m => m.IsDefined(typeof(PunRPC), false)).Select(mi => mi.Name).ToArray();
                var additional =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     methodNames.Where(n => !PhotonNetwork.PhotonServerSettings.RpcList.Contains(n) && !additionalRpcs.Contains(n));

                allRpcs.AddRange(methodNames);
                additionalRpcs.AddRange(additional);
            }

#endif


            if (additionalRpcs.Count <= 0)
                //Debug.Log("UpdateRPCs did not found new.");
                return;


            if (additionalRpcs.Count + PhotonNetwork.PhotonServerSettings.RpcList.Count >= byte.MaxValue)
            {
                if (allRpcs.Count <= byte.MaxValue)
                {
                    var clearList = EditorUtility.DisplayDialog(CurrentLang.IncorrectRPCListTitle,
                        CurrentLang.IncorrectRPCListLabel, CurrentLang.RemoveOutdatedRPCsLabel,
                        CurrentLang.CancelButton);
                    if (clearList)
                    {
                        PhotonNetwork.PhotonServerSettings.RpcList.Clear();
                        additionalRpcs = allRpcs.Distinct().ToList(); // we add all unique names
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(CurrentLang.FullRPCListTitle, CurrentLang.FullRPCListLabel,
                        CurrentLang.SkipRPCListUpdateLabel);
                    return;
                }
            }


            additionalRpcs.Sort();
            Undo.RecordObject(PhotonNetwork.PhotonServerSettings, "RPC-list update of PUN.");
            PhotonNetwork.PhotonServerSettings.RpcList.AddRange(additionalRpcs);
            EditorUtility.SetDirty(PhotonNetwork.PhotonServerSettings);

            //Debug.Log("Updated RPCs. Added: "+additionalRpcs.Count);
        }


        public static void ClearRpcList()
        {
            var clearList = EditorUtility.DisplayDialog(CurrentLang.PUNNameReplaceTitle,
                CurrentLang.PUNNameReplaceLabel, CurrentLang.RPCListCleared, CurrentLang.CancelButton);
            if (clearList)
            {
                var serverSettings = PhotonNetwork.PhotonServerSettings;

                Undo.RecordObject(serverSettings, "RPC-list cleared for PUN.");
                serverSettings.RpcList.Clear();
                EditorUtility.SetDirty(serverSettings);

                Debug.LogWarning(CurrentLang.ServerSettingsCleanedWarning);
            }
        }

        #endregion
    }
}