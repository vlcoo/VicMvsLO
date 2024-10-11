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
#if UNITY_ANDROID
        Vibration.Init();
#endif
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
        if (!rumbleEnabled || pad == null || duration <= 0f) return;

#if UNITY_ANDROID
        Vibration.VibrateAndroid((long)(duration * 1000));
#else
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