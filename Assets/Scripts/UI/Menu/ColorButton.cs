using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorButton : MonoBehaviour, ISelectHandler, IDeselectHandler {

    [SerializeField] private Sprite overlayUnpressed, overlayPressed;
    [SerializeField] private Image shirt, overalls, overlay;

    public PlayerColorSet palette;

    public void Instantiate(PlayerColors col) {
        if (col == null) {
            shirt.enabled = false;
            overalls.enabled = false;
            return;
        }

        shirt.color = col.overallsColor;
        overalls.color = col.hatColor;
        overlay.enabled = false;
    }

    public void OnSelect(BaseEventData eventData) {
        overlay.enabled = true;
        overlay.sprite = overlayUnpressed;
    }

    public void OnDeselect(BaseEventData eventData) {
        overlay.enabled = false;
        overlay.sprite = overlayUnpressed;
    }

    public void OnPress() {
        overlay.sprite = overlayPressed;
    }
}