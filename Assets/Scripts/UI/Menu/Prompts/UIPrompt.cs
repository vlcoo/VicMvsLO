using UnityEngine;
using UnityEngine.EventSystems;

public class UIPrompt : MonoBehaviour {

    //---Serialized Variables
    [SerializeField] private GameObject defaultSelectedObject;

    public void OnEnable() {
        SetDefaults();
        EventSystem.current.SetSelectedGameObject(defaultSelectedObject);
    }

    public void OnDisable() {
        SetDefaults();
    }

    protected virtual void SetDefaults() { }
}