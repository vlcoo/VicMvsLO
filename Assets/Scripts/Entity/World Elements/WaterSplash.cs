using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class WaterSplash : MonoBehaviour
{
    [Delayed] public int widthTiles = 64, pointsPerTile = 8, splashWidth = 2;

    [Delayed] public float heightTiles = 1;

    public float tension = 40, kconstant = 1.5f, damping = 0.92f, splashVelocity = 50f, resistance, animationSpeed = 1f;
    public string splashParticle;

    public bool isWater;
    private float animTimer;
    private Color32[] colors;

    private Texture2D heightTex;
    private bool initialized;
    private float[] pointHeights, pointVelocities;
    private MaterialPropertyBlock properties;
    private SpriteRenderer spriteRenderer;
    private int totalPoints;

    private void Awake()
    {
        Initialize();
    }

    public void FixedUpdate()
    {
        if (!initialized)
        {
            Initialize();
            initialized = true;
        }

        var delta = Time.fixedDeltaTime;

        var valuesChanged = false;

        for (var i = 0; i < totalPoints; i++)
        {
            var height = pointHeights[i];
            pointVelocities[i] += tension * -height;
            pointVelocities[i] *= damping;
        }

        for (var i = 0; i < totalPoints; i++) pointHeights[i] += pointVelocities[i] * delta;
        for (var i = 0; i < totalPoints; i++)
        {
            var height = pointHeights[i];

            pointVelocities[i] -=
                kconstant * delta * (height - pointHeights[(i + totalPoints - 1) % totalPoints]); //left
            pointVelocities[i] -=
                kconstant * delta * (height - pointHeights[(i + totalPoints + 1) % totalPoints]); //right
        }

        for (var i = 0; i < totalPoints; i++)
        {
            var newR = (byte)(Mathf.Clamp01(pointHeights[i] / 20f + 0.5f) * 255f);
            valuesChanged |= colors[i].r != newR;
            colors[i].r = newR;
        }

        if (valuesChanged)
        {
            heightTex.SetPixels32(colors);
            heightTex.Apply(false);
        }

        animTimer += animationSpeed * Time.fixedDeltaTime;
        animTimer %= 8;
        properties.SetFloat("TextureIndex", animTimer);
        spriteRenderer.SetPropertyBlock(properties);
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        Instantiate(Resources.Load(splashParticle), collider.transform.position, Quaternion.identity);

        var body = collider.attachedRigidbody;
        var power = body ? body.velocity.y : -1;
        var tile = (transform.InverseTransformPoint(collider.transform.position).x / widthTiles + 0.25f) * 2f;
        var px = (int)(tile * totalPoints);
        for (var i = -splashWidth; i <= splashWidth; i++)
        {
            var pointsX = (px + totalPoints + i) % totalPoints;
            pointVelocities[pointsX] = -splashVelocity * power;
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null || !(gameObject.tag.Equals("poison") || gameObject.tag.Equals("lava")))
            return;

        collision.attachedRigidbody.velocity *= 1 - Mathf.Clamp01(resistance);
    }

    private void OnValidate()
    {
        ValidationUtility.SafeOnValidate(Initialize);
    }

    private void Initialize()
    {
        if (this == null)
            return;

        var collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        totalPoints = widthTiles * pointsPerTile;
        pointHeights = new float[totalPoints];
        pointVelocities = new float[totalPoints];

        heightTex = new Texture2D(totalPoints, 1, TextureFormat.RGBA32, false);

        Color32 gray = new(128, 0, 0, 255);
        colors = new Color32[totalPoints];
        for (var i = 0; i < totalPoints; i++)
            colors[i] = gray;

        heightTex.Apply();

        collider.offset = new Vector2(0, heightTiles * 0.25f - 0.2f);
        if (isWater) collider.size = new Vector2(widthTiles * 0.5f, collider.size.y);
        else collider.size = new Vector2(widthTiles * 0.5f, heightTiles * 0.5f - 0.1f);
        spriteRenderer.size = new Vector2(widthTiles * 0.5f, heightTiles * 0.5f + 0.5f);

        properties = new MaterialPropertyBlock();
        properties.SetTexture("Heightmap", heightTex);
        properties.SetFloat("WidthTiles", widthTiles);
        properties.SetFloat("Height", heightTiles);
        spriteRenderer.SetPropertyBlock(properties);
    }
}