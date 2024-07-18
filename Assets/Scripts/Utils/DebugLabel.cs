using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLabel : MonoBehaviour {
    private TMP_Text _label;

    void Start() {
        _label = GetComponent<TMP_Text>();
    }

    void Update() {
        //_label.text = MatchConditioner.Instance.ActiveRulesToString() + "\n" + SessionData.Instance.ActiveRulesJson;
    }
}
