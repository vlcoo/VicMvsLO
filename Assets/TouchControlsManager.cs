using NSMB;
using UnityEngine;
using UnityEngine.UI;

public class TouchControlsManager : MonoBehaviour
{
    public static TouchControlsManager Instance { get; private set; }

    [Header("Touch Controls")]
    public CanvasGroup tC;

    [Header("Scalable Buttons")]
    public RectTransform[] scalableButtons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ApplyOpacity(Settings.Instance.mobileTCOpacity);
        ApplyButtonScale(Settings.Instance.mobileTCSize);
    }

    public void ApplyOpacity(float value)
    {
        tC.alpha = value;
    }

    public void ApplyButtonScale(float scale)
    {
        foreach (var button in scalableButtons)
        {
            if (button != null)
                button.localScale = Vector3.one * scale;
        }
    }
}
