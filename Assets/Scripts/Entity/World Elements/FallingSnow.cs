using System.Collections;
using UnityEngine;

public class FallingSnow : MonoBehaviour
{
    public Sprite fallingSprite;
    public bool isActive;

    private Rigidbody2D body;
    private BoxCollider2D proximityCast;
    private AudioSource sfx;
    private float wakeupTimer = 1f;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        proximityCast = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (isActive)
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                Mathf.Sin(wakeupTimer * 120f) * 4f);
            wakeupTimer -= Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (wakeupTimer < 1f) return;
        var player = col.gameObject.GetComponent<PlayerController>();
        if (player is null) return;

        isActive = true;
        player.PlaySound(Enums.Sounds.World_Falling_Snow);
        StartCoroutine(nameof(ShakeAndFall));
    }

    private IEnumerator ShakeAndFall()
    {
        yield return new WaitForSeconds(1);
        isActive = false;
        body.bodyType = RigidbodyType2D.Dynamic;
        GetComponent<SpriteRenderer>().sprite = fallingSprite;
    }
}