using UnityEngine;
using UnityEngine.SceneManagement;

public class HorizontalCamera : MonoBehaviour
{
    public static float OFFSET_TARGET = 0f;
    public static float OFFSET_VELOCITY, OFFSET;
    private static readonly float orthoSize = 3.5f;

    public bool renderToTextureIfAvailable = true;
    private Camera ourCamera;

    private void Start()
    {
        ourCamera = GetComponent<Camera>();
        AdjustCamera();
    }

    private void Update()
    {
        OFFSET = Mathf.SmoothDamp(OFFSET, OFFSET_TARGET, ref OFFSET_VELOCITY, 1f);
        AdjustCamera();
        ourCamera.targetTexture = renderToTextureIfAvailable && Settings.Instance.ndsResolution &&
                                  SceneManager.GetActiveScene().buildIndex != 0
            ? GlobalController.Instance.ndsTexture
            : null;
    }

    private void AdjustCamera()
    {
        var aspect = ourCamera.aspect;
        double size = orthoSize + OFFSET;
        // double size = orthographicSize;
        // Credit: https://forum.unity.com/threads/how-to-calculate-horizontal-field-of-view.16114/#post-2961964
        var aspectReciprocals = 1d / aspect;
        ourCamera.orthographicSize = Mathf.Min((float)size, (float)(size * (16d / 9d) * aspectReciprocals));
    }
}