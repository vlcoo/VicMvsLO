using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class KeepChildInFocus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scrollAmount = 15;

    private readonly List<ScrollRect> components = new();
    private bool mouseOver;
    private ScrollRect rect;
    private float scrollPos;

    private void Awake()
    {
        rect = GetComponent<ScrollRect>();
    }

    private void Update()
    {
        if (mouseOver || rect.content == null)
            return;

        rect.verticalNormalizedPosition =
            Mathf.Lerp(rect.verticalNormalizedPosition, scrollPos, scrollAmount * Time.deltaTime);

        if (!EventSystem.current.currentSelectedGameObject)
            return;

        var target = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();

        if (IsFirstParent(target) && target.name != "Scrollbar Vertical")
            scrollPos = rect.ScrollToCenter(target, false);
        else
            scrollPos = rect.verticalNormalizedPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }

    private bool IsFirstParent(Transform target)
    {
        do
        {
            if (target.GetComponent<IFocusIgnore>() != null)
                return false;

            target.GetComponents(components);

            if (components.Count >= 1)
                return components.Contains(rect);

            target = target.parent;
        } while (target != null);

        return false;
    }

    public interface IFocusIgnore
    {
    }
}