using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "mid")]
public class MidAsset : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var obj = ScriptableObject.CreateInstance<MidAssetData>();
        obj.SetData(System.IO.File.ReadAllText(ctx.assetPath));
        ctx.AddObjectToAsset("main obj", obj);
        ctx.SetMainObject(obj);
    }
}
