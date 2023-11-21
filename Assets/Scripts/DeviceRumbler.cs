using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceRumbler : MonoBehaviour
{
    public bool rumbleEnabled = true;
    private Coroutine currentlyRumbling;
    private Gamepad pad;

    private void Start()
    {
        rumbleEnabled = GlobalController.Instance.settings.rumbleController;
    }

    private void OnEnable()
    {
        UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
        pad = Gamepad.current;
    }

    private void OnDisable()
    {
        UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad) return;
        pad?.ResetHaptics();
        pad = gamepad;
    }

    public void RumbleForSeconds(float bassStrength, float trebleStrength, float duration)
    {
#if UNITY_ANDROID
        if (rumbleEnabled) Handheld.Vibrate();
#else
        if (!rumbleEnabled || pad == null) return;
        if (currentlyRumbling != null) StopCoroutine(currentlyRumbling);
        currentlyRumbling = StartCoroutine(Rumble(bassStrength, trebleStrength, duration));
#endif
    }

    private IEnumerator Rumble(float lowFreq, float highFreq, float duration)
    {
        pad.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0f, 0f);
    }
}