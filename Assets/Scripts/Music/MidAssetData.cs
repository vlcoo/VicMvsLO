using System;
using UnityEngine;

public class MidAssetData : ScriptableObject
{
    [NonSerialized] public TextAsset data;

    public void SetData(string midData) => this.data = new TextAsset(midData);
}