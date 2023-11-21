using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupChanceListEntry : MonoBehaviour
{
    public string powerup;
    public TMP_Text textField;
    public Button upBtn, downBtn;
    private int _chance = 1;

    public int Chance
    {
        set
        {
            _chance = value switch
            {
                2 => 4,
                3 => 1,
                _ => Math.Clamp(value, 0, 4)
            };
            textField.text = $"x{_chance}";

            upBtn.interactable = _chance != 4 && PhotonNetwork.IsMasterClient;
            downBtn.interactable = _chance != 0 && PhotonNetwork.IsMasterClient;
        }
        get => _chance;
    }
}