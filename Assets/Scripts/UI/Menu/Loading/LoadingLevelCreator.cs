using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingLevelCreator : MonoBehaviour {

    public TMP_Text text;
    public TMP_Text readyText;

    public void Update() {
        if (!GameManager.Instance)
            return;

        if (GameManager.Instance.levelDesigner != "") text.text = $"Level design by\n{GameManager.Instance.levelDesigner}";
        if (GameManager.Instance.MatchConditioner.count >= 8) readyText.text = "You better be ready.";
        enabled = false;
    }
}