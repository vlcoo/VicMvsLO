using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class NdsCamera : MonoBehaviour
{
    private RawImage image;

    public void Start()
    {
        image = GetComponent<RawImage>();
    }

    private void LateUpdate()
    {
        image.texture = GlobalController.Instance.ndsTexture;
    }
}