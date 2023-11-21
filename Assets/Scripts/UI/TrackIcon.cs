using UnityEngine;
using UnityEngine.UI;

public class TrackIcon : MonoBehaviour
{
    public float trackMinX, trackMaxX;
    public GameObject target;
    public bool doAnimation;
    public Sprite starSprite;
    private bool changedSprite;

    private float flashTimer;
    private Image image;
    private Material mat;
    private PlayerController playerTarget;

    public void Start()
    {
        image = GetComponent<Image>();

        StarBouncer star;
        if ((star = target.GetComponent<StarBouncer>()) && star.stationary)
        {
            GetComponent<Animator>().enabled = true;
            transform.localScale = Vector2.zero;
        }

        mat = image.material;
        Update();
    }

    public void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (playerTarget || target.CompareTag("Player"))
        {
            if (!playerTarget)
            {
                playerTarget = target.GetComponent<PlayerController>();
                image.color = playerTarget.AnimationController.GlowColor;
                mat.SetColor("OverlayColor", playerTarget.AnimationController.GlowColor);
            }

            // OPTIMIZE:
            if (playerTarget.dead)
            {
                flashTimer += Time.deltaTime;
                image.enabled = flashTimer % 0.2f <= 0.1f;
            }
            else
            {
                flashTimer = 0;
                image.enabled = true;
            }

            transform.localScale = playerTarget.cameraController.IsControllingCamera
                ? new Vector3(1, -1, 1)
                : Vector3.one * (2f / 3f);
        }
        else if (!changedSprite)
        {
            image.sprite = starSprite;
            image.enabled = true;
            changedSprite = true;
        }

        var gm = GameManager.Instance;
        var levelWidth = gm.GetLevelMaxX() - gm.GetLevelMinX();
        var trackWidth = trackMaxX - trackMinX;
        var percentage = (target.transform.position.x - gm.GetLevelMinX()) / levelWidth;
        transform.localPosition = new Vector3(percentage * trackWidth - trackMaxX, transform.localPosition.y);
    }
}