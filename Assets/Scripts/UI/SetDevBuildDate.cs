using UnityEngine;
using TMPro;

public class SetDevBuildDate : MonoBehaviour {
    private void Start() {
        TMP_Text text = GetComponent<TMP_Text>();
        text.text = BuildInfo.GetDevVersionString();
    }
}
