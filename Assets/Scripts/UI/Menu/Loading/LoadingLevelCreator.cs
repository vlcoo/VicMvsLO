using TMPro;
using UnityEngine;

public class LoadingLevelCreator : MonoBehaviour
{
    public TMP_Text text;
    public TMP_Text readyText;

    public void Update()
    {
        if (!GameManager.Instance)
            return;

        if (GameManager.Instance.levelDesigner != "")
            text.text = $"Level designed by {GameManager.Instance.levelDesigner}";
        enabled = false;
    }
}