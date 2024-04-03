using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NSMB.UI.MainMenu {
    public class PipeButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

        //---Serialized Variables
        [SerializeField] private RectTransform rect;
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text label;

        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color deselectedColor = Color.gray;
        [SerializeField] private bool leftAnchored;

        //---Private Variables
        private Color disabledColor;
        private Vector2 anchor, adjustedAnchor;
        private bool currentlyPressed;

        public void OnValidate() {
            rect = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            image = GetComponentInChildren<Image>();
            label = GetComponentInChildren<TMP_Text>();
        }

        public void Start() {
            anchor = leftAnchored ? rect.anchorMax : rect.anchorMin;
            adjustedAnchor = anchor + Vector2.right * (leftAnchored ? -0.1f : 0.1f);
            disabledColor = new(deselectedColor.r, deselectedColor.g, deselectedColor.b, deselectedColor.a * 0.5f);
        }

        public void Update() {
            if (!button.interactable) {
                SetAnchor(adjustedAnchor);
                image.color = disabledColor;
                label.color = disabledColor;
                return;
            }

            if (currentlyPressed) return;

            if (EventSystem.current.currentSelectedGameObject == gameObject) {
                SetAnchor(anchor);
                image.color = selectedColor;
            } else {
                SetAnchor(adjustedAnchor);
                image.color = deselectedColor;
            }

            label.color = Color.yellow;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!button.interactable) return;

            currentlyPressed = true;

            SetAnchor(adjustedAnchor, true);
            image.color = selectedColor;
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (!button.interactable) return;

            currentlyPressed = false;
        }

        private void SetAnchor(Vector2 value, bool fastAnim = false) {
            var duration = fastAnim ? 0.07f : 0.15f;

            if (leftAnchored) {
                if (rect.anchorMax != value) DOTween.To(() => rect.anchorMax, v => rect.anchorMax = v, value, duration);
            } else {
                if (rect.anchorMin != value) DOTween.To(() => rect.anchorMin, v => rect.anchorMin = v, value, duration);
            }
        }
    }
}
