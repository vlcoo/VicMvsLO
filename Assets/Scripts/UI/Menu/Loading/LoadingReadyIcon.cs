using System;
using NSMB.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingReadyIcon : MonoBehaviour
{
    public TMP_Text readyText;
    public TMP_ColorGradient gradientLuigiText;

    public void Start() {
        GetComponent<Image>().sprite = Utils.GetCharacterData().readySprite;
        if (Array.IndexOf<PlayerData>(GlobalController.Instance.characters, Utils.GetCharacterData()) % 2 != 0)
            readyText.colorGradientPreset = gradientLuigiText;
    }
}
