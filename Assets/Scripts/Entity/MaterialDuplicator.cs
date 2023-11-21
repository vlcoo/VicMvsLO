using UnityEngine;

public class MaterialDuplicator : MonoBehaviour
{
    [ExecuteInEditMode]
    private void Awake()
    {
        var renderer = GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
    }
}