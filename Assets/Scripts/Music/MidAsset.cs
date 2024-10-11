#if UNITY_EDITOR
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "mid")]
public class MidAsset : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var obj = ScriptableObject.CreateInstance<MidAssetData>();
        obj.Bytes = File.ReadAllBytes(ctx.assetPath);
        ctx.AddObjectToAsset("main obj", obj);
        ctx.SetMainObject(obj);
    }
}
#endif