using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class SlantText : MonoBehaviour
{
    [SerializeField] private float slopeAmount, firstChar, secondChar;
    private TMP_SubMeshUI subtext;

    private TMP_Text text;

    public void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    public void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(MoveVerts);
    }

    public void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(MoveVerts);
    }

    public void MoveVerts(Object a)
    {
        if (!text || a != text)
            return;

        if (!subtext)
        {
            subtext = GetComponentInChildren<TMP_SubMeshUI>();
            if (!subtext)
                return;
        }

        var info = text.textInfo;
        var mesh = subtext.mesh;
        var verts = mesh.vertices;

        var adjustment = slopeAmount < 0 ? -slopeAmount * text.textInfo.characterCount * Vector3.up : Vector3.zero;
        for (var i = 0; i < info.characterCount; i++)
        {
            var index = info.characterInfo[i].vertexIndex;
            var offset = adjustment;

            if (i == 0)
                offset.y += firstChar;
            else if (i == 1) offset.y += secondChar;
            offset.y += i * slopeAmount;

            verts[index] += offset;
            verts[index + 1] += offset;
            verts[index + 2] += offset;
            verts[index + 3] += offset;
        }

        mesh.vertices = verts;
        subtext.canvasRenderer.SetMesh(mesh);
    }
}