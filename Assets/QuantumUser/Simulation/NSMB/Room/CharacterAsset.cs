using Quantum;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterAsset : AssetObject {

    public AssetRef<EntityPrototype> Prototype;

    public string SoundFolder;
    public string UiString;
    public string TranslationString;
    public string LegalEnglishName;
    public CharacterPalette[] Palettes;

#if QUANTUM_UNITY
    public Sprite LoadingSmallSprite;
    public Sprite LoadingLargeSprite;
    public Sprite ReadySprite;

    public RuntimeAnimatorController SmallOverrides;
    public RuntimeAnimatorController LargeOverrides;
#endif 
}

[Serializable]
public class CharacterPalette {
    public ColorRGBA ShirtColor, OverallsColor;
    public bool HatUsesOverallsColor;
}