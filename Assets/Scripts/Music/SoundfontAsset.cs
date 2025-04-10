#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

[ScriptedImporter(1, "sf2")]
public class SoundfontAsset : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx) {
        var rawBytes = File.ReadAllBytes(ctx.assetPath);
        var compressedBytes = Compress(rawBytes);
        var obj = ScriptableObject.CreateInstance<SoundfontAssetData>();
        obj.Bytes = compressedBytes;
        ctx.AddObjectToAsset("main obj", obj);
        ctx.SetMainObject(obj);
    }

    private static byte[] Compress(byte[] data) {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) {
            gzip.Write(data, 0, data.Length);
        } 
        return output.ToArray(); 
    }
}
#endif