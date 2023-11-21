using NSMB.Utils;
using UnityEngine;
using UnityEngine.UI;

public class MarioLoader : MonoBehaviour
{
    public int scale, previousScale;
    public float scaleTimer, blinkSpeed = 0.5f;
    public PlayerData data;
    private Image image;

    public void Start()
    {
        image = GetComponent<Image>();
        if (data == null) data = Utils.GetCharacterData();
    }

    public void Update()
    {
        var scaleDisplay = scale;

        if ((scaleTimer += Time.deltaTime) < 0.5f)
        {
            if (scaleTimer % blinkSpeed < blinkSpeed / 2f)
                scaleDisplay = previousScale;
        }
        else
        {
            previousScale = scale;
        }

        if (scaleDisplay == 0)
        {
            transform.localScale = Vector3.one;
            image.sprite = data.loadingSmallSprite;
        }
        else if (scaleDisplay == 1)
        {
            transform.localScale = Vector3.one;
            image.sprite = data.loadingBigSprite;
        }
        else
        {
            transform.localScale = Vector3.one * 2;
            image.sprite = data.loadingBigSprite;
        }
    }
}