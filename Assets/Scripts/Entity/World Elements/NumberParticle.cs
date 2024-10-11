using TMPro;
using UnityEngine;

public class NumberParticle : MonoBehaviour
{
    public TMP_Text text;

    public void ApplyColor(Color color)
    {
        text.color = color;
        // text.ForceMeshUpdate();
        // var mr = GetComponentsInChildren<MeshRenderer>()[1];
        // MaterialPropertyBlock mpb = new();
        // mpb.SetColor("_Color", color);
        // mr.SetPropertyBlock(mpb);
    }

    public void Kill()
    {
        Destroy(transform.parent.gameObject);
    }
}