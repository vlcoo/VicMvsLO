using System;
using UnityEngine;

public class SoundfontAssetData : ScriptableObject
{
    [NonSerialized] public TextAsset data;

    public void SetData(string sfData) => this.data = new TextAsset(sfData);
}