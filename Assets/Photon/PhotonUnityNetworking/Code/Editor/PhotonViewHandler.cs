// ----------------------------------------------------------------------------
// <copyright file="PhotonViewHandler.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a Editor script to initialize PhotonView components.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System.Collections.Generic;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun
{
    [InitializeOnLoad]
    public class PhotonViewHandler : EditorWindow
    {
        static PhotonViewHandler()
        {
            // called once per change (per key-press in inspectors) and once after play-mode ends.
#if (UNITY_2018 || UNITY_2018_1_OR_NEWER)
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
#else
			EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
#endif
        }


        internal static void OnHierarchyChanged()
        {
            // set prefabs to viewID 0 if needed
            // organize resource PVs in a list per viewID

            // process the lists: if more than one photonView is in a list, we have to resolve the clash
            // check if only one view had the viewId earlier
            // apply a new viewID to the others

            // update the cached list of instances and their viewID


            //Debug.LogWarning("OnHierarchyChanged(). isPlaying: " + Application.isPlaying);
            if (Application.isPlaying) return;


            var photonViewResources = Resources.FindObjectsOfTypeAll<PhotonView>();
            var photonViewInstances = new List<PhotonView>();
            var viewInstancesPerViewId = new Dictionary<int, List<PhotonView>>();
            var photonViewsToReassign = new List<PhotonView>();

            foreach (var view in photonViewResources)
            {
                if (PhotonEditorUtils.IsPrefab(view.gameObject))
                {
                    // prefabs should use 0 as ViewID and sceneViewId
                    if (view.ViewID != 0 || view.sceneViewId != 0)
                    {
                        view.ViewID = 0;
                        view.sceneViewId = 0;
                        EditorUtility.SetDirty(view);
                    }

                    continue; // skip prefabs in further processing
                }

                photonViewInstances.Add(view);


                // assign a new viewID if the viewId is lower than the minimum for this scene
                if (!IsViewIdOkForScene(view))
                {
                    photonViewsToReassign.Add(view);
                    continue; // this view definitely gets cleaned up, so it does not count versus duplicates, checked below
                }


                // organize the viewInstances into lists per viewID, so we know duplicate usage
                if (!viewInstancesPerViewId.ContainsKey(view.sceneViewId))
                    viewInstancesPerViewId[view.sceneViewId] = new List<PhotonView>();
                viewInstancesPerViewId[view.sceneViewId].Add(view);
            }

            //Debug.Log("PreviousAssignments: "+PunSceneViews.Instance.Views.Count);

            foreach (var list in viewInstancesPerViewId.Values)
            {
                if (list.Count <= 1) continue; // skip lists with just one entry (the viewID is unique)


                PhotonView previousAssignment = null;
                var wasAssigned = PunSceneViews.Instance.Views.TryGetValue(list[0].sceneViewId, out previousAssignment);

                foreach (var view in list)
                {
                    if (wasAssigned && view.Equals(previousAssignment))
                        // previously, we cached the used viewID as assigned to the current view. we don't change this.
                        continue;

                    //Debug.LogWarning("View to reassign due to viewID: "+view, view.gameObject);
                    photonViewsToReassign.Add(view);
                }
            }

            int i;
            foreach (var view in photonViewsToReassign)
            {
                i = MinSceneViewId(view);
                while (viewInstancesPerViewId.ContainsKey(i)) i++;
                view.sceneViewId = i;
                viewInstancesPerViewId.Add(i,
                    null); // we don't need the lists anymore but we care about getting the viewIDs listed
                EditorUtility.SetDirty(view);
            }


            // update the "semi persistent" list of viewIDs and their PhotonViews
            PunSceneViews.Instance.Views.Clear();
            foreach (var view in photonViewInstances)
            {
                if (PunSceneViews.Instance.Views.ContainsKey(view.sceneViewId))
                {
                    Debug.LogError("ViewIDs should no longer have duplicates! " + view.sceneViewId, view);
                    continue;
                }

                PunSceneViews.Instance.Views[view.sceneViewId] = view;
            }

            //Debug.Log("photonViewsToReassign.Count: "+photonViewsToReassign.Count + " count of viewIDs in use: "+viewInstancesPerViewId.Values.Count);
            //Debug.Log("PreviousAssignments now counts: "+PunSceneViews.Instance.Views.Count);
        }


        private static int MinSceneViewId(PhotonView view)
        {
            var result = PunSceneSettings.MinViewIdForScene(view.gameObject.scene.name);
            return result;
        }

        private static bool IsViewIdOkForScene(PhotonView view)
        {
            return view.sceneViewId >= MinSceneViewId(view);
        }
    }

    /// <summary>
    ///     Stores a PhotonView instances per viewId (key). Instance is used as cache storage in-Editor.
    /// </summary>
    public class PunSceneViews : ScriptableObject
    {
        private static PunSceneViews instanceField;

        [SerializeField] public Dictionary<int, PhotonView> Views = new();

        public static PunSceneViews Instance
        {
            get
            {
                if (instanceField != null) return instanceField;

                instanceField = FindObjectOfType<PunSceneViews>();
                if (instanceField == null) instanceField = CreateInstance<PunSceneViews>();

                return instanceField;
            }
        }
    }
}