using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LeftRightButton : Selectable
{
    public GameObject leftArrow;
    public GameObject rightArrow;
    public float cutoff = 0.6f, offset = 10;
    private float leftOffset, rightOffset;

    private bool leftSelected, rightSelected;
    private RectTransform leftTransform, rightTransform;

    protected override void Start()
    {
        base.Start();
        leftTransform = leftArrow.GetComponent<RectTransform>();
        rightTransform = rightArrow.GetComponent<RectTransform>();
        leftOffset = leftTransform.anchoredPosition.x;
        rightOffset = rightTransform.anchoredPosition.x;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InputSystem.controls.Player.Movement.performed += OnNavigation;
        InputSystem.controls.Player.Movement.canceled += OnNavigation;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InputSystem.controls.Player.Movement.performed -= OnNavigation;
        InputSystem.controls.Player.Movement.canceled -= OnNavigation;
    }

    private void OnNavigation(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.paused)
        {
            leftSelected = false;
            rightSelected = false;
            SetOffset(false, false);
            SetOffset(true, false);
            return;
        }

        var direction = context.ReadValue<Vector2>();
        if (Vector2.Dot(direction, Vector2.left) > cutoff)
        {
            //select left
            if (!leftSelected)
            {
                GameManager.Instance.SpectationManager.SpectatePreviousPlayer();
                SetOffset(false, true);
            }

            leftSelected = true;
        }
        else if (leftSelected)
        {
            SetOffset(false, false);
            leftSelected = false;
        }

        if (Vector2.Dot(direction, Vector2.right) > cutoff)
        {
            //select left
            if (!rightSelected)
            {
                GameManager.Instance.SpectationManager.SpectateNextPlayer();
                SetOffset(true, true);
            }

            rightSelected = true;
        }
        else if (rightSelected)
        {
            SetOffset(true, false);
            rightSelected = false;
        }
    }

    private void SetOffset(bool right, bool selected)
    {
        var transform = right ? rightTransform : leftTransform;
        var offset = right ? rightOffset : leftOffset;
        var selOffset = selected ? offset : 0;
        transform.anchoredPosition = new Vector2(offset + selOffset, transform.anchoredPosition.y);
    }
}