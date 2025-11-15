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
        [TextArea] [SerializeField] public string message;
        [SerializeField] private Vector2 offset = new(16, -16);
        [SerializeField] private float delayBeforeShow = 0.5f;
        
        //---Private Variables
        private Coroutine showCoroutine;
        private Vector2 lastPointerPosition;

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
