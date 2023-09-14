using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Firebar : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Transform spriteTform;
    [FormerlySerializedAs("collider")] public BoxCollider2D colliderBar;
    public bool onlyOneSide = false;
    public int tiles = 1;
    
    public void Start() {
        Initialize();
    }

    public void OnValidate() {
        ValidationUtility.SafeOnValidate(() => {
            Initialize();
        });
    }

    private void Initialize()
    {
        sprite.size = new Vector2(0.13f * tiles, sprite.size.y);
        colliderBar.size = new Vector2(sprite.size.x, colliderBar.size.y);
        spriteTform.localPosition = onlyOneSide
            ? new Vector3(0.385f * (tiles / 2) - (tiles % 2 == 0 ? 0.1925f : 0), 0, 0)
            : new Vector3(0, 0, 0);
    }
}
