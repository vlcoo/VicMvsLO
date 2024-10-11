using UnityEngine;
using UnityEngine.UI;

public class TrackIcon : MonoBehaviour
{
    public float trackMinX, trackMaxX;
    public GameObject target;
    public bool doAnimation, targetless;
    public Sprite starSprite;
    public Animator animator;
    private bool changedSprite;

    private float flashTimer;
    private Image image;
    private Material mat;
    private PlayerController playerTarget;

    public void Start()
    {
        image = GetComponent<Image>();

        StarBouncer star;
        if (target && (star = target.GetComponent<StarBouncer>()) && star.stationary)
        {
            animator.enabled = true;
            transform.localScale = Vector2.zero;
        }

        mat = image.material;
        Update();
    }

    public void Update()
    {
        if (target == null)
        {
            if (!targetless) Destroy(gameObject);
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

        SetPositionFromLevelCoords(target.transform.position);
    }

    public void SetPositionFromLevelCoords(Vector3 position)
    {
        var gm = GameManager.Instance;
        var levelWidth = gm.GetLevelMaxX() - gm.GetLevelMinX();
        var trackWidth = trackMaxX - trackMinX;
        var percentage = (position.x - gm.GetLevelMinX()) / levelWidth;
        transform.localPosition = new Vector3(percentage * trackWidth - trackMaxX, transform.localPosition.y);
    }
}