// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnClickInstantiate.cs" company="Exit Games GmbH">
// Part of: Photon Unity Utilities
// </copyright>
// <summary>A compact script for prototyping.</summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     This component will instantiate a network GameObject when in a room and the user click on that component's
    ///     GameObject.
    ///     Uses PhysicsRaycaster for positioning.
    /// </summary>
    public class OnClickRpc : MonoBehaviourPun, IPointerClickHandler
    {
        public PointerEventData.InputButton Button;
        public KeyCode ModifierKey;

        public RpcTarget Target;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!PhotonNetwork.InRoom || (ModifierKey != KeyCode.None && !Input.GetKey(ModifierKey)) ||
                eventData.button != Button) return;

            photonView.RPC("ClickRpc", Target);
        }


        #region RPC Implementation

        private Material originalMaterial;
        private Color originalColor;
        private bool isFlashing;

        [PunRPC]
        public void ClickRpc()
        {
            //Debug.Log("ClickRpc Called");
            StartCoroutine(ClickFlash());
        }

        public IEnumerator ClickFlash()
        {
            if (isFlashing) yield break;
            isFlashing = true;

            originalMaterial = GetComponent<Renderer>().material;
            if (!originalMaterial.HasProperty("_EmissionColor"))
            {
                Debug.LogWarning("Doesn't have emission, can't flash " + gameObject);
                yield break;
            }

            var wasEmissive = originalMaterial.IsKeywordEnabled("_EMISSION");
            originalMaterial.EnableKeyword("_EMISSION");

            originalColor = originalMaterial.GetColor("_EmissionColor");
            originalMaterial.SetColor("_EmissionColor", Color.white);

            for (var f = 0.0f; f <= 1.0f; f += 0.08f)
            {
                var lerped = Color.Lerp(Color.white, originalColor, f);
                originalMaterial.SetColor("_EmissionColor", lerped);
                yield return null;
            }

            originalMaterial.SetColor("_EmissionColor", originalColor);
            if (!wasEmissive) originalMaterial.DisableKeyword("_EMISSION");
            isFlashing = false;
        }

        #endregion
    }
}