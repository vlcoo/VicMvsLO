using NSMB.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CounterTip : MonoBehaviour {
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image sprite;
    [NonSerialized] public int Count = 0;
    
    void Start()
    {
        SetCount(0);
    }

    public void SetCount(int newCount) {
        this.Count = newCount;
        label.text = Utils.GetSymbolString(newCount.ToString(), Utils.smallSymbols);

        sprite.color = newCount > 0 ? Color.white : Color.clear;
        label.color = newCount > 0 ? Color.white : Color.clear;
    }
}
