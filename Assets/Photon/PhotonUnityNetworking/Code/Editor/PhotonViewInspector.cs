// ----------------------------------------------------------------------------
// <copyright file="PhotonViewInspector.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Custom inspector for the PhotonView component.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun
{
    [CustomEditor(typeof(PhotonView))]
    [CanEditMultipleObjects]
    internal class PhotonViewInspector : Editor
    {
        private static readonly GUIContent ownerTransferGuiContent =
            new("Ownership Transfer", "Determines how ownership changes may be initiated.");

        private static readonly GUIContent syncronizationGuiContent =
            new("Synchronization", "Determines how sync updates are culled and sent.");

        private static readonly GUIContent observableSearchGuiContent = new("Observable Search",
            "When set to Auto, On Awake, Observables on this GameObject (and child GameObjects) will be found and populate the Observables List." +
            "\n\nNested PhotonViews (children with a PhotonView) and their children will not be included in the search.");

        private PhotonView m_Target;

        public void OnEnable()
        {
            m_Target = (PhotonView)target;

            if (!Application.isPlaying)
                m_Target.FindObservables();
        }

        public override void OnInspectorGUI()
        {
            m_Target = (PhotonView)target;
            var isProjectPrefab = PhotonEditorUtils.IsPrefab(m_Target.gameObject);
            var multiSelected = Selection.gameObjects.Length > 1;

            if (m_Target.ObservedComponents == null) m_Target.ObservedComponents = new List<Component>();

            if (m_Target.ObservedComponents.Count == 0) m_Target.ObservedComponents.Add(null);

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical("HelpBox");
            // View ID - Hide if we are multi-selected
            if (!multiSelected)
            {
                if (isProjectPrefab)
                {
                    EditorGUILayout.LabelField("View ID", "<i>Set at runtime</i>",
                        new GUIStyle("Label") { richText = true });
                }
                else if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.LabelField("View ID", m_Target.ViewID.ToString());
                }
                else
                {
                    // this is an object in a scene, modified at edit-time. we can store this as sceneViewId
                    var idValue = EditorGUILayout.IntField("View ID [1.." + (PhotonNetwork.MAX_VIEW_IDS - 1) + "]",
                        m_Target.sceneViewId);
                    if (m_Target.sceneViewId != idValue)
                    {
                        Undo.RecordObject(m_Target, "Change PhotonView viewID");
                        m_Target.sceneViewId = idValue;
                    }
                }
            }

            // Locally Controlled
            if (EditorApplication.isPlaying)
            {
                var masterClientHint = PhotonNetwork.IsMasterClient ? " (master)" : "";
                EditorGUILayout.LabelField("IsMine:", m_Target.IsMine + masterClientHint);
                var room = PhotonNetwork.CurrentRoom;
                var cretrId = m_Target.CreatorActorNr;
                var cretr = room != null ? room.GetPlayer(cretrId) : null;
                var owner = m_Target.Owner;
                var ctrlr = m_Target.Controller;
                EditorGUILayout.LabelField("Controller:",
                    ctrlr != null
                        ? "[" + ctrlr.ActorNumber + "] '" + ctrlr.NickName + "' " +
                          (ctrlr.IsMasterClient ? " (master)" : "")
                        : "[0] <null>");
                EditorGUILayout.LabelField("Owner:",
                    owner != null
                        ? "[" + owner.ActorNumber + "] '" + owner.NickName + "' " +
                          (owner.IsMasterClient ? " (master)" : "")
                        : "[0] <null>");
                EditorGUILayout.LabelField("Creator:",
                    cretr != null
                        ? "[" + cretrId + "] '" + cretr.NickName + "' " + (cretr.IsMasterClient ? " (master)" : "")
                        : "[0] <null>");
            }

            EditorGUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            GUILayout.Space(5);

            // Ownership section

            EditorGUILayout.LabelField("Ownership", (GUIStyle)"BoldLabel");

            var own = (OwnershipOption)EditorGUILayout.EnumPopup(ownerTransferGuiContent,
                m_Target.OwnershipTransfer /*, GUILayout.MaxWidth(68), GUILayout.MinWidth(68)*/);
            if (own != m_Target.OwnershipTransfer)
            {
                // jf: fixed 5 and up prefab not accepting changes if you quit Unity straight after change.
                // not touching the define nor the rest of the code to avoid bringing more problem than solving.
                EditorUtility.SetDirty(m_Target);

                Undo.RecordObject(m_Target, "Change PhotonView Ownership Transfer");
                m_Target.OwnershipTransfer = own;
            }


            GUILayout.Space(5);

            // Observables section

            EditorGUILayout.LabelField("Observables", (GUIStyle)"BoldLabel");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Synchronization"), syncronizationGuiContent);

            if (m_Target.Synchronization == ViewSynchronization.Off)
            {
                // Show warning if there are any observables. The null check is because the list allows nulls.
                var observed = m_Target.ObservedComponents;
                if (observed.Count > 0)
                    for (int i = 0, cnt = observed.Count; i < cnt; ++i)
                        if (observed[i] != null)
                        {
                            EditorGUILayout.HelpBox(
                                "Synchronization is set to Off. Select a Synchronization setting in order to sync the listed Observables.",
                                MessageType.Warning);
                            break;
                        }
            }


            var autoFindObservables =
                (PhotonView.ObservableSearch)EditorGUILayout.EnumPopup(observableSearchGuiContent,
                    m_Target.observableSearch);

            if (m_Target.observableSearch != autoFindObservables)
            {
                Undo.RecordObject(m_Target, "Change Auto Find Observables Toggle");
                m_Target.observableSearch = autoFindObservables;
            }

            m_Target.FindObservables();

            if (!multiSelected)
            {
                var disableList = Application.isPlaying || autoFindObservables != PhotonView.ObservableSearch.Manual;

                if (disableList)
                    EditorGUI.BeginDisabledGroup(true);

                DrawObservedComponentsList(disableList);

                if (disableList)
                    EditorGUI.EndDisabledGroup();
            }

            // Cleanup: save and fix look
            if (GUI.changed) PhotonViewHandler.OnHierarchyChanged(); // TODO: check if needed

            EditorGUI.EndDisabledGroup();
        }


        private int GetObservedComponentsCount()
        {
            var count = 0;

            for (var i = 0; i < m_Target.ObservedComponents.Count; ++i)
                if (m_Target.ObservedComponents[i] != null)
                    count++;

            return count;
        }

        /// <summary>
        ///     Find Observables, and then baking them into the serialized object.
        /// </summary>
        private void EditorFindObservables()
        {
            Undo.RecordObject(serializedObject.targetObject, "Find Observables");
            var property = serializedObject.FindProperty("ObservedComponents");

            // Just doing a Find updates the Observables list, but Unity fails to save that change.
            // Instead we do the find, and then iterate the found objects into the serialize property, then apply that.
            property.ClearArray();
            m_Target.FindObservables(true);
            for (var i = 0; i < m_Target.ObservedComponents.Count; ++i)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = m_Target.ObservedComponents[i];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawObservedComponentsList(bool disabled = false)
        {
            var listProperty = serializedObject.FindProperty("ObservedComponents");

            if (listProperty == null) return;

            float containerElementHeight = 22;
            var containerHeight = listProperty.arraySize * containerElementHeight;

            var foldoutLabel = "Observed Components (" + GetObservedComponentsCount() + ")";
            var isOpen = PhotonGUI.ContainerHeaderFoldout(foldoutLabel,
                serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue, () => EditorFindObservables(),
                "Find");
            serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue = isOpen;

            if (isOpen == false) containerHeight = 0;

            //Texture2D statsIcon = AssetDatabase.LoadAssetAtPath( "Assets/Photon Unity Networking/Editor/PhotonNetwork/PhotonViewStats.png", typeof( Texture2D ) ) as Texture2D;

            var containerRect = PhotonGUI.ContainerBody(containerHeight);


            var wasObservedComponentsEmpty = m_Target.ObservedComponents.FindAll(item => item != null).Count == 0;
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

                        // keep track of old type to catch when a new type is observed
                        var _oldType = listProperty.GetArrayElementAtIndex(i).objectReferenceValue != null
                            ? listProperty.GetArrayElementAtIndex(i).objectReferenceValue.GetType()
                            : null;

                        EditorGUI.PropertyField(propertyPosition, listProperty.GetArrayElementAtIndex(i),
                            new GUIContent());

                        // new type, could be different from old type
                        var _newType = listProperty.GetArrayElementAtIndex(i).objectReferenceValue != null
                            ? listProperty.GetArrayElementAtIndex(i).objectReferenceValue.GetType()
                            : null;

                        // the user dropped a Transform, we must change it by adding a PhotonTransformView and observe that instead
                        if (_oldType != _newType)
                        {
                            if (_newType == typeof(PhotonView))
                            {
                                listProperty.GetArrayElementAtIndex(i).objectReferenceValue = null;
                                Debug.LogError(
                                    "PhotonView Detected you dropped a PhotonView, this is not allowed. \n It's been removed from observed field.");
                            }
                            else if (_newType == typeof(Transform))
                            {
                                // try to get an existing PhotonTransformView ( we don't want any duplicates...)
                                var _ptv = m_Target.gameObject.GetComponent<PhotonTransformView>();
                                if (_ptv == null)
                                    // no ptv yet, we create one and enable position and rotation, no scaling, as it's too rarely needed to take bandwidth for nothing
                                    _ptv = Undo.AddComponent<PhotonTransformView>(m_Target.gameObject);
                                // switch observe from transform to _ptv
                                listProperty.GetArrayElementAtIndex(i).objectReferenceValue = _ptv;
                                Debug.Log(
                                    "PhotonView has detected you dropped a Transform. Instead it's better to observe a PhotonTransformView for better control and performances");
                            }
                            else if (_newType == typeof(Rigidbody))
                            {
                                var _rb = listProperty.GetArrayElementAtIndex(i).objectReferenceValue as Rigidbody;

                                // try to get an existing PhotonRigidbodyView ( we don't want any duplicates...)
                                var _prbv = _rb.gameObject.GetComponent<PhotonRigidbodyView>();
                                if (_prbv == null)
                                    // no _prbv yet, we create one
                                    _prbv = Undo.AddComponent<PhotonRigidbodyView>(_rb.gameObject);
                                // switch observe from transform to _prbv
                                listProperty.GetArrayElementAtIndex(i).objectReferenceValue = _prbv;
                                Debug.Log(
                                    "PhotonView has detected you dropped a RigidBody. Instead it's better to observe a PhotonRigidbodyView for better control and performances");
                            }
                            else if (_newType == typeof(Rigidbody2D))
                            {
                                // try to get an existing PhotonRigidbody2DView ( we don't want any duplicates...)
                                var _prb2dv = m_Target.gameObject.GetComponent<PhotonRigidbody2DView>();
                                if (_prb2dv == null)
                                    // no _prb2dv yet, we create one
                                    _prb2dv = Undo.AddComponent<PhotonRigidbody2DView>(m_Target.gameObject);
                                // switch observe from transform to _prb2dv
                                listProperty.GetArrayElementAtIndex(i).objectReferenceValue = _prb2dv;
                                Debug.Log(
                                    "PhotonView has detected you dropped a Rigidbody2D. Instead it's better to observe a PhotonRigidbody2DView for better control and performances");
                            }
                            else if (_newType == typeof(Animator))
                            {
                                // try to get an existing PhotonAnimatorView ( we don't want any duplicates...)
                                var _pav = m_Target.gameObject.GetComponent<PhotonAnimatorView>();
                                if (_pav == null)
                                    // no _pav yet, we create one
                                    _pav = Undo.AddComponent<PhotonAnimatorView>(m_Target.gameObject);
                                // switch observe from transform to _prb2dv
                                listProperty.GetArrayElementAtIndex(i).objectReferenceValue = _pav;
                                Debug.Log(
                                    "PhotonView has detected you dropped a Animator, so we switched to PhotonAnimatorView so that you can serialized the Animator variables");
                            }
                            else if (!typeof(IPunObservable).IsAssignableFrom(_newType))
                            {
                                var _ignore = false;
#if PLAYMAKER
                                _ignore = _newType == typeof(PlayMakerFSM);
                                // Photon Integration for PlayMaker will swap at runtime to a proxy using iPunObservable.
#endif

                                if (_newType == null || _newType == typeof(Rigidbody) ||
                                    _newType == typeof(Rigidbody2D)) _ignore = true;

                                if (!_ignore)
                                {
                                    listProperty.GetArrayElementAtIndex(i).objectReferenceValue = null;
                                    Debug.LogError(
                                        "PhotonView Detected you dropped a Component missing IPunObservable Interface,\n You dropped a <" +
                                        _newType + "> instead. It's been removed from observed field.");
                                }
                            }
                        }

                        //Debug.Log( listProperty.GetArrayElementAtIndex( i ).objectReferenceValue.GetType() );
                        //Rect statsPosition = new Rect( propertyPosition.xMax + 7, propertyPosition.yMin, statsIcon.width, statsIcon.height );
                        //ReorderableListResources.DrawTexture( statsPosition, statsIcon );
                        var removeButtonRect = new Rect(
                            elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                            elementRect.yMin + 2,
                            PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                            PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);

                        GUI.enabled = !disabled && listProperty.arraySize > 1;
                        if (GUI.Button(removeButtonRect, new GUIContent(ReorderableListResources.texRemoveButton),
                                PhotonGUI.DefaultRemoveButtonStyle)) listProperty.DeleteArrayElementAtIndex(i);
                        GUI.enabled = !disabled;

                        if (i < listProperty.arraySize - 1)
                        {
                            texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4,
                                1);
                            PhotonGUI.DrawSplitter(texturePosition);
                        }
                    }
                }

            if (PhotonGUI.AddButton()) listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));

            serializedObject.ApplyModifiedProperties();

            var isObservedComponentsEmpty = m_Target.ObservedComponents.FindAll(item => item != null).Count == 0;

            if (wasObservedComponentsEmpty && isObservedComponentsEmpty == false &&
                m_Target.Synchronization == ViewSynchronization.Off)
            {
                Undo.RecordObject(m_Target, "Change PhotonView");
                m_Target.Synchronization = ViewSynchronization.UnreliableOnChange;
                serializedObject.Update();
            }

            if (wasObservedComponentsEmpty == false && isObservedComponentsEmpty)
            {
                Undo.RecordObject(m_Target, "Change PhotonView");
                m_Target.Synchronization = ViewSynchronization.Off;
                serializedObject.Update();
            }
        }
    }
}