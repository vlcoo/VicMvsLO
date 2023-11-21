using TMPro;
using UnityEngine;

public class VersionNamer : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TMP_Text>().text = "v" + Application.version;
    }
}