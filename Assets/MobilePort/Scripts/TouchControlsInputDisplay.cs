using NSMB;
using NSMB.UI.Game;
using Quantum;
using UnityEngine;

public class TouchControlsInputDisplay : MonoBehaviour
{
    public GameObject TouchControls;

    void Update() {
        bool anySpectating = false;

        foreach (var playerElement in PlayerElements.AllPlayerElements) {
            if (playerElement.IsSpectating) {
                anySpectating = true;
                break;
            }
        }
        QuantumRunner runner = QuantumRunner.Default;
        if (anySpectating) {
            TouchControls.SetActive(false);
        } else if (anySpectating) {
            TouchControls.SetActive(false);
        } else if (runner.Session.IsReplay) {
            TouchControls.SetActive(false);
        } else if (!Settings.Instance.GraphicsInputDisplay) {
            TouchControls.SetActive(false);
        } /*else if (!Settings.Instance.mobiletouchControls) {
            TouchControls.SetActive(false);
        }*/ else {
            TouchControls.SetActive(true);
        }
        }
    }
