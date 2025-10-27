using NSMB.Utilities.Extensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NSMB.UI.Elements {
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler {

        //---Serialized Variables
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string message;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float delayBeforeShow = 0f;
        private Coroutine showCoroutine;
        private Vector2 lastPointerPosition;

        public void OnValidate() {
            
        }

        public void OnEnable() {
            
        }

        public void OnDisable() {
            
        }

        public void Update() {
            // if (objectToShow && objectToShow.activeInHierarchy && followCursor) {
            //     objectToShow.transform.position = Settings.Controls.UI.Point.ReadValue<Vector2>() + offset;
            // }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (delayBeforeShow <= 0f) {
                panel.SetActive(true);
                panel.transform.position = eventData.position + offset;
                label.text = message;
                return;
            }
            
            showCoroutine = StartCoroutine(ShowTooltipAfterDelay());
            lastPointerPosition = eventData.position;
        }

        public void OnPointerExit(PointerEventData eventData) {
            panel.SetActive(false);
            if (showCoroutine != null) {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        public void OnPointerMove(PointerEventData eventData) {
            if (!panel.activeInHierarchy) {
                if (showCoroutine != null) {
                    StopCoroutine(showCoroutine);
                }
                showCoroutine = StartCoroutine(ShowTooltipAfterDelay());
            }
            lastPointerPosition = eventData.position;
        }
        
        private IEnumerator ShowTooltipAfterDelay() {
            yield return new WaitForSeconds(delayBeforeShow);
            panel.SetActive(true);
            panel.transform.position = lastPointerPosition + offset;
            label.text = message;
        }
    }
}
