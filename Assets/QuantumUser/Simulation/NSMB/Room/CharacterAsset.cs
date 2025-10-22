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
    public bool IsMinion;

#if QUANTUM_UNITY
    public Sprite LoadingSmallSprite;
    public Sprite LoadingLargeSprite;
    public Sprite ReadySprite;

    public RuntimeAnimatorController SmallOverrides;
    public RuntimeAnimatorController LargeOverrides;
#endif

    // public void OnValidate() {
    //     if (Palettes.Length == 0 || Palettes[0] != null) {
    //         // we add default palette (null on first index)
    //         var newPalettes = new CharacterPalette[Palettes.Length + 1];
    //         newPalettes[0] = null;
    //         Array.Copy(Palettes, 0, newPalettes, 1, Palettes.Length);
    //         Palettes = newPalettes;
    //     }
    // }
}

[Serializable]
public class CharacterPalette {
    public ColorRGBA ShirtColor, OverallsColor;
    public bool HatUsesOverallsColor;
}