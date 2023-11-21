// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageToggleIsOnTransition.cs" company="Exit Games GmbH">
// </copyright>
// <summary>
//  Use this on Toggle graphics to have some color transition as well without corrupting toggle's behaviour.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Use this on toggles texts to have some color transition on the text depending on the isOn State.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class GraphicToggleIsOnTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Toggle toggle;

        public Color NormalOnColor = Color.white;
        public Color NormalOffColor = Color.black;
        public Color HoverOnColor = Color.black;
        public Color HoverOffColor = Color.black;

        private Graphic _graphic;

        private bool isHover;

        public void OnEnable()
        {
            _graphic = GetComponent<Graphic>();

            OnValueChanged(toggle.isOn);

            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        public void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHover = true;
            _graphic.color = toggle.isOn ? HoverOnColor : HoverOffColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHover = false;
            _graphic.color = toggle.isOn ? NormalOnColor : NormalOffColor;
        }

        public void OnValueChanged(bool isOn)
        {
            _graphic.color = isOn ? isHover ? HoverOnColor : HoverOnColor : isHover ? NormalOffColor : NormalOffColor;
        }
    }
}