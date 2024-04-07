using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    //--- Serialized Variables
    [SerializeField] private Image progressBar;
    [SerializeField] private Button.ButtonClickedEvent action;
    [SerializeField] private float progressSpeed = 1f;
    [SerializeField] private TMP_Text label;

    //--- Private Variables
    private RectTransform rt;
    private int progressDirection = 0;
    private bool readyToInvoke;
    private string originalText;

    void OnValidate()
    {
        rt = progressBar.GetComponent<RectTransform>();
    }

    private void Start() {
        originalText = label.text;
    }

    void Update()
    {
        if (progressDirection == 0) return;

        rt.localScale = new Vector3(rt.localScale.x + progressDirection * Time.deltaTime * progressSpeed, rt.localScale.y, rt.localScale.z);

        switch (rt.localScale.x)
        {
        case > 1:
            progressDirection = 0;
            readyToInvoke = true;
            label.text = "OK!";
            break;
        case < 0:
            progressDirection = 0;
            rt.localScale = new Vector3(0, rt.localScale.y, rt.localScale.z);
            break;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (readyToInvoke) {
            action.Invoke();
            readyToInvoke = false;
            rt.localScale = new Vector3(0, rt.localScale.y, rt.localScale.z);
            label.text = originalText;
        } else {
            progressDirection = -1;
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        progressDirection = 1;
    }
}
