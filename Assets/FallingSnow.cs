using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSnow : MonoBehaviour
{
    public Sprite fallingSprite;
    
    private Rigidbody2D body;
    private BoxCollider2D proximityCast;
    public bool isActive = false;
    private float wakeupTimer = 1f;
    private AudioSource sfx;
    
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        proximityCast = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isActive)
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                Mathf.Sin(wakeupTimer * 20f));
            wakeupTimer -= Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (wakeupTimer < 1f) return;
        PlayerController player = col.gameObject.GetComponent<PlayerController>();
        if (player is null) return;

        isActive = true;
        sfx.PlayOneShot(Enums.Sounds.World_Falling_Snow.GetClip());
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
