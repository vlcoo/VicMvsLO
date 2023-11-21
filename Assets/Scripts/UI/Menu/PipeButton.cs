using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PipeButton : MonoBehaviour
{
    public Color selectedColor = Color.white, deselectedColor = Color.gray;
    public bool leftAnchored;
    private Vector2 anchor, adjustedAnchor;
    private Button button;

    private Color disabledColor;
    private Image image;
    private RectTransform rect;

    public void Start()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        image = GetComponentInChildren<Image>();
        anchor = leftAnchored ? rect.anchorMax : rect.anchorMin;
        adjustedAnchor = anchor + Vector2.right * (leftAnchored ? -0.1f : 0.1f);
        disabledColor = new Color(deselectedColor.r, deselectedColor.g, deselectedColor.b, deselectedColor.a / 2f);
    }

    public void Update()
    {
        if (!button.interactable)
        {
            SetAnchor(adjustedAnchor);
            image.color = disabledColor;
            return;
        }

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            SetAnchor(anchor);
            image.color = selectedColor;
        }
        else
        {
            SetAnchor(adjustedAnchor);
            image.color = deselectedColor;
        }
    }

    private void SetAnchor(Vector2 value)
    {
        if (leftAnchored)
            DOTween.To(() => rect.anchorMax, v => rect.anchorMax = v, value, 0.15f);
        else
            DOTween.To(() => rect.anchorMin, v => rect.anchorMin = v, value, 0.15f);
    }
}