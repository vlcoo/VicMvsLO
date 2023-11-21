using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class LooseCoin : MonoBehaviourPun
{
    public float despawn = 10;
    private Animator animator;

    private Rigidbody2D body;
    private float despawnTimer;
    private BoxCollider2D hitbox;
    private PhysicsEntity physics;
    private Vector2 prevFrameVelocity;
    private AudioSource sfx;
    private SpriteRenderer spriteRenderer;

    public bool Collected { get; set; }

    public void Start()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<BoxCollider2D>();
        physics = GetComponent<PhysicsEntity>();
        animator = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        body.velocity = Vector2.up * Random.Range(2f, 3f);
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

        physics.UpdateCollisions();
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