using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.AssetImporters;

[ScriptedImporter(1, "mid")]
public class MidAsset : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var obj = ScriptableObject.CreateInstance<MidAssetData>();
        obj.Bytes = System.IO.File.ReadAllBytes(ctx.assetPath);
        ctx.AddObjectToAsset("main obj", obj);
        ctx.SetMainObject(obj);
    }
}
#endif
