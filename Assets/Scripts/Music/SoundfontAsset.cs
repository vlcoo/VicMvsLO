using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.AssetImporters;

[ScriptedImporter(1, "sf2")]
public class SoundfontAsset : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var obj = ScriptableObject.CreateInstance<SoundfontAssetData>();
        obj.Bytes = System.IO.File.ReadAllBytes(ctx.assetPath);
        ctx.AddObjectToAsset("main obj", obj);
        ctx.SetMainObject(obj);
    }
}
#endif
