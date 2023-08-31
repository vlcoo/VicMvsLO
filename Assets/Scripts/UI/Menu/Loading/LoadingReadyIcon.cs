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
        // if (Utils.GetCharacterData().prefab.Equals("PlayerLuigi"))
        //     readyText.colorGradientPreset = gradientLuigiText;
    }
}
