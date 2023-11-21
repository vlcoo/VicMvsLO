using System.IO;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public static Controls controls;
    public static FileInfo file;

    public void Awake()
    {
        if (controls != null)
            return;

        controls = new Controls();
        controls.Enable();

        file = new FileInfo(Application.persistentDataPath + "/controls.json");
    }
}