using NSMB.Utils;
using UnityEngine;

public class VolumeWithDistance : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private Transform soundOrigin;
    [SerializeField] private float soundRange = 12f;

    public void Update()
    {
        var inst = GameManager.Instance;
        var listener = inst != null && inst.localPlayer
            ? inst.localPlayer.transform.position
            : Camera.main.transform.position;

        var volume =
            Utils.QuadraticEaseOut(
                1 - Mathf.Clamp01(Utils.WrappedDistance(listener, soundOrigin.position) / soundRange));

        foreach (var source in audioSources)
            source.volume = volume;
    }
}