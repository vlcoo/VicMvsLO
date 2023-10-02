using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerupChanceListEntry : MonoBehaviour
{
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
        }
        get => _chance;
    }
    
    public String powerup;
    public TMP_Text textField;
}
