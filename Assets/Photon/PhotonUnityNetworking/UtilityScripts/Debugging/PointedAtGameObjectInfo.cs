// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PointedAtGameObjectInfo.cs" company="Exit Games GmbH">
// </copyright>
// <summary>
//  Display ViewId, OwnerActorNr, IsCeneView and IsMine when clicked using the old UI system
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Display ViewId, OwnerActorNr, IsCeneView and IsMine when clicked.
    /// </summary>
    public class PointedAtGameObjectInfo : MonoBehaviour
    {
        public static PointedAtGameObjectInfo Instance;

        public Text text;

        private Transform focus;

        private void Start()
        {
            if (Instance != null)
            {
                Debug.LogWarning("PointedAtGameObjectInfo is already featured in the scene, gameobject is destroyed");
                Destroy(gameObject);
            }

            Instance = this;
        }

        private void LateUpdate()
        {
            if (focus != null) transform.position = Camera.main.WorldToScreenPoint(focus.position);
        }

        public void SetFocus(PhotonView pv)
        {
            focus = pv != null ? pv.transform : null;

            if (pv != null)
                text.text = string.Format("id {0} own: {1} {2}{3}", pv.ViewID, pv.OwnerActorNr,
                    pv.IsRoomView ? "scn" : "", pv.IsMine ? " mine" : "");
            //GUI.Label (new Rect (Input.mousePosition.x + 5, Screen.height - Input.mousePosition.y - 15, 300, 30), );
            else
                text.text = string.Empty;
        }

        public void RemoveFocus(PhotonView pv)
        {
            if (pv == null)
            {
                text.text = string.Empty;
                return;
            }

            if (pv.transform == focus) text.text = string.Empty;
        }
    }
}