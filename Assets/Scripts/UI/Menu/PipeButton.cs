using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PipeButton : MonoBehaviour {

    public Color selectedColor = Color.white, deselectedColor = Color.gray;
    public bool leftAnchored;

    private Color disabledColor;
    private Button button;
    private Image image;
    private RectTransform rect;
    private Vector2 anchor, adjustedAnchor;

    public void Start() {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        image = GetComponentInChildren<Image>();
        anchor = leftAnchored ? rect.anchorMax : rect.anchorMin;
        adjustedAnchor = anchor + Vector2.right * (leftAnchored ? -0.1f : 0.1f);
        disabledColor = new(deselectedColor.r, deselectedColor.g, deselectedColor.b, deselectedColor.a/2f);
    }

    public void Update() {
        if (!button.interactable) {
            SetAnchor(adjustedAnchor);
            image.color = disabledColor;
            return;
        }
        if (EventSystem.current.currentSelectedGameObject == gameObject) {
            SetAnchor(anchor);
            image.color = selectedColor;
        } else {
            SetAnchor(adjustedAnchor);
            image.color = deselectedColor;
        }
    }

    private void SetAnchor(Vector2 value) {
        if (leftAnchored)
            DOTween.To(() => rect.anchorMax, v => rect.anchorMin = v, value, 0.2f);
        else
            DOTween.To(() => rect.anchorMin, v => rect.anchorMin = v, value, 0.2f);
    }
}
