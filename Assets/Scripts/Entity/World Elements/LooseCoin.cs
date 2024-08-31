using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class LooseCoin : MonoBehaviourPun
{
    private static readonly int ANY_GROUND_MASK = -1;
    public float despawn = 10;
    public bool dropped, passthrough;
    private Animator animator;

    private Rigidbody2D body;
    private float despawnTimer;
    private BoxCollider2D hitbox;
    private PhysicsEntity physics;
    private Vector2 prevFrameVelocity;
    private AudioSource sfx;
    private SpriteRenderer spriteRenderer;

    public bool Collected { get; set; }
    public bool Collectable { get; private set; }

    public void Start()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<BoxCollider2D>();
        physics = GetComponent<PhysicsEntity>();
        animator = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        var data = photonView.InstantiationData;
        if (dropped)
        {
            if (data != null)
            {
                //player dropped coin

                passthrough = true;
                spriteRenderer.color = new Color(1, 1, 1, 0.55f);
                gameObject.layer = Layers.LayerHitsNothing;
                var direction = (int)data[0];
                var left = direction <= 1;
                var fast = direction == 0 || direction == 3;
                body.velocity = new Vector2(3f * (left ? -1 : 1) * (fast ? 2f : 1f), 5f);
                body.isKinematic = false;
                hitbox.enabled = true;
            }
        }
        else
        {
            body.velocity = Vector2.up * Random.Range(2f, 3f);
        }
    }

    public void FixedUpdate()
    {
        if (GameManager.Instance && GameManager.Instance.gameover)
        {
            body.velocity = Vector2.zero;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }

        var inWall =
            Utils.IsAnyTileSolidBetweenWorldBox(body.position + hitbox.offset,
                hitbox.size * transform.lossyScale * 0.5f);
        gameObject.layer = inWall ? Layers.LayerHitsNothing : Layers.LayerLooseCoin;


        Collectable |= body.velocity.y < 0;
        physics.UpdateCollisions();
        if (dropped)
        {
            if (passthrough && Collectable && body.velocity.y <= 0 &&
                !Utils.IsAnyTileSolidBetweenWorldBox(body.position + hitbox.offset,
                    hitbox.size * transform.lossyScale) &&
                !Physics2D.OverlapBox(body.position, Vector2.one / 3, 0, ANY_GROUND_MASK))
            {
                passthrough = false;
                gameObject.layer = Layers.LayerEntity;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }

            if (Collectable)
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            if (!passthrough)
            {
                if (Utils.IsAnyTileSolidBetweenWorldBox(body.position + hitbox.offset,
                        hitbox.size * transform.lossyScale))
                    gameObject.layer = Layers.LayerHitsNothing;
                else
                    gameObject.layer = Layers.LayerEntity;
            }
        }

        if (physics.onGround)
        {
            body.velocity -= body.velocity * Time.fixedDeltaTime;
            if (physics.hitRoof && photonView.IsMine)
                PhotonNetwork.Destroy(photonView);

            if (prevFrameVelocity.y < -1f) sfx.PlayOneShot(Enums.Sounds.World_Coin_Drop.GetClip());
        }

        spriteRenderer.enabled = !(despawnTimer > despawn - 3 && despawnTimer % 0.3f >= 0.15f);

        prevFrameVelocity = body.velocity;

        if ((despawnTimer += Time.deltaTime) >= despawn)
            if (photonView.IsMine)
                PhotonNetwork.Destroy(photonView);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(body.position + hitbox.offset, hitbox.size * transform.lossyScale);
    }
}