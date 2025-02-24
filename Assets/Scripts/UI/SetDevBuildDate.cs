using System;
using UnityEngine;
using TMPro;

public class SetDevBuildDate : MonoBehaviour {
    private void Start() {
        TMP_Text text = GetComponent<TMP_Text>();
        text.text = $"vcmi {Application.version} (dev-1) based on vanilla from {BuildInfo.SOURCE_BUILD_TIME}";
    }
}
