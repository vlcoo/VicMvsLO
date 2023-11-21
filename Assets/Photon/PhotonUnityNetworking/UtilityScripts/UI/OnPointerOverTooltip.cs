// <copyright file="OnPointerOverTooltip.cs" company="Exit Games GmbH">
// </copyright>
// <summary>
// Set focus to a given photonView when pointed is over
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Set focus to a given photonView when pointed is over
    /// </summary>
    public class OnPointerOverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private void OnDestroy()
        {
            PointedAtGameObjectInfo.Instance.RemoveFocus(GetComponent<PhotonView>());
        }

        #region IPointerEnterHandler implementation

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            PointedAtGameObjectInfo.Instance.SetFocus(GetComponent<PhotonView>());
        }

        #endregion

        #region IPointerExitHandler implementation

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            PointedAtGameObjectInfo.Instance.RemoveFocus(GetComponent<PhotonView>());
        }

        #endregion
    }
}