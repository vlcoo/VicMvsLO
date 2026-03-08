using NSMB;
using NSMB.UI.Game;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class TouchControlsManager : MonoBehaviour
{
    public enum TouchControlsRole
    {
        Active,
        Preview
    }

    [Header("Role")]
    public TouchControlsRole role;

    [Header("Touch Controls")]
    public CanvasGroup tC;
    public GameObject filledIn, outlined;
    public GameObject spectatorControls;

    [Header("Scalable Buttons")]
    public RectTransform[] scalableButtons;

    private void Awake()
    {
        if (role == TouchControlsRole.Preview)
        {
            DisableInteraction();
        }
    }

    private void Start()
        {
            ApplyOpacity(Settings.Instance.mobileTCOpacity);
            ApplyButtonScale(Settings.Instance.mobileTCSize);
            SetSpectatorControlsVisible(Settings.Instance.mobileSpectatorTC);
        }

    public void SetSpectatorControlsVisible(bool visible)
    {
       
    bool anySpectating = false;

        foreach (var playerElement in PlayerElements.AllPlayerElements) {
            if (playerElement.IsSpectating) {
                anySpectating = true;
                break;
            }
        }
        QuantumRunner runner = QuantumRunner.Default;
        if (spectatorControls != null)
        if (anySpectating || runner.Session.IsReplay) {
                spectatorControls.SetActive(visible);
        } 
    }
    public void SetTouchControlsVisible(bool visible)
    {
        //WIP
    }

    public void ApplyOpacity(float value)
        {
            if (tC != null)
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

    private void DisableInteraction()
    {
        tC.interactable = false;
        tC.blocksRaycasts = false;
    }
}
