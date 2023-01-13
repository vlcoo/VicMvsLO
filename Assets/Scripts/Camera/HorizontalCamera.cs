using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HorizontalCamera : MonoBehaviour {
    public static float OFFSET_TARGET = 0f;
    public static float OFFSET_VELOCITY, OFFSET = 0f;
    private Camera ourCamera;
    
    public bool renderToTextureIfAvailable = true;
    private static float orthoSize = 1f;
    private float lerpCameraSize = 0;
    private int lerpTimer = 0;

    void Start() {
        ourCamera = GetComponent<Camera>();
        AdjustCamera();
    }
     
    private void Update()
    {
        lerpTimer++;
        if (lerpTimer > 300 && lerpTimer < 420)
        {
            lerpCameraSize += 0.0083f;
            orthoSize = Mathf.Lerp(1f, 3.5f, lerpCameraSize);
        }
        
        OFFSET = Mathf.SmoothDamp(OFFSET, OFFSET_TARGET, ref OFFSET_VELOCITY, 1f);
        AdjustCamera();
        ourCamera.targetTexture = renderToTextureIfAvailable && Settings.Instance.ndsResolution && SceneManager.GetActiveScene().buildIndex != 0 
            ? GlobalController.Instance.ndsTexture 
            : null;
    }

    private void AdjustCamera() {
        float aspect = ourCamera.aspect;
        double size = orthoSize + OFFSET;
        // double size = orthographicSize;
        // Credit: https://forum.unity.com/threads/how-to-calculate-horizontal-field-of-view.16114/#post-2961964
        double aspectReciprocals = 1d / aspect;
        ourCamera.orthographicSize = Mathf.Min((float) size, (float) (size * (16d/9d) * aspectReciprocals));
    }
}
