// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnClickInstantiate.cs" company="Exit Games GmbH">
// Part of: Photon Unity Utilities
// </copyright>
// <summary>A compact script for prototyping.</summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Instantiates a networked GameObject on click.
    /// </summary>
    /// <remarks>
    ///     Gets OnClick() calls by Unity's IPointerClickHandler. Needs a PhysicsRaycaster on the camera.
    ///     See: https://docs.unity3d.com/ScriptReference/EventSystems.IPointerClickHandler.html
    /// </remarks>
    public class OnClickInstantiate : MonoBehaviour, IPointerClickHandler
    {
        public enum InstantiateOption
        {
            Mine,
            Scene
        }


        public PointerEventData.InputButton Button;
        public KeyCode ModifierKey;

        public GameObject Prefab;

        [SerializeField] private InstantiateOption InstantiateType = InstantiateOption.Mine;


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!PhotonNetwork.InRoom || (ModifierKey != KeyCode.None && !Input.GetKey(ModifierKey)) ||
                eventData.button != Button) return;


            switch (InstantiateType)
            {
                case InstantiateOption.Mine:
                    PhotonNetwork.Instantiate(Prefab.name,
                        eventData.pointerCurrentRaycast.worldPosition + new Vector3(0, 0.5f, 0), Quaternion.identity);
                    break;
                case InstantiateOption.Scene:
                    PhotonNetwork.InstantiateRoomObject(Prefab.name,
                        eventData.pointerCurrentRaycast.worldPosition + new Vector3(0, 0.5f, 0), Quaternion.identity);
                    break;
            }
        }
    }
}