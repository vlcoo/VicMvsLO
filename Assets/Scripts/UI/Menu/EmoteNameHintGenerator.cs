using TMPro;
using UnityEngine;

public class EmoteNameHintGenerator : MonoBehaviour
{
    private void Start()
    {
        var label = GetComponent<TMP_Text>();
        label.text = "";

        foreach (var emoteName in GlobalController.Instance.EMOTE_NAMES)
            label.text += $"<sprite name=\"{emoteName}\"> :{emoteName}:        ";
    }
}