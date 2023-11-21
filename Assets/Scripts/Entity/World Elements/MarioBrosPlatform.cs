using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class MarioBrosPlatform : MonoBehaviourPun
{
    private static readonly Vector2 BUMP_OFFSET = new(-0.25f, -0.1f);

    [Delayed] public int platformWidth = 8, samplesPerTile = 8, bumpWidthPoints = 3, bumpBlurPoints = 6;

    public float bumpDuration = 0.4f;
    public bool changeCollider = true;

    private readonly List<BumpInfo> bumps = new();
    private Texture2D displacementMap;
    private MaterialPropertyBlock mpb;
    private Color32[] pixels;
    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        Initialize();
    }

    public void Update()
    {
        for (var i = 0; i < platformWidth * samplesPerTile; i++)
            pixels[i] = new Color32(0, 0, 0, 255);

        for (var i = bumps.Count - 1; i >= 0; i--)
        {
            var bump = bumps[i];
            bump.timer -= Time.deltaTime;
            if (bump.timer <= 0)
            {
                bumps.RemoveAt(i);
                continue;
            }

            var percentageCompleted = bump.timer / bumpDuration;
            var v = Mathf.Sin(Mathf.PI * percentageCompleted);
            for (var x = -bumpWidthPoints - bumpBlurPoints; x <= bumpWidthPoints + bumpBlurPoints; x++)
            {
                var index = bump.pointX + x;
                if (index < 0 || index >= platformWidth * samplesPerTile)
                    continue;

                var color = v;
                if (x < -bumpWidthPoints || x > bumpWidthPoints)
                    color *= Mathf.SmoothStep(1, 0, (float)(Mathf.Abs(x) - bumpWidthPoints) / bumpBlurPoints);

                if (pixels[index].r / 255f >= color)
                    continue;

                pixels[index].r = (byte)(Mathf.Clamp01(color) * 255);
            }
        }

        displacementMap.SetPixels32(pixels);
        displacementMap.Apply();

        mpb.SetTexture("DisplacementMap", displacementMap);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    public void OnValidate()
    {
        ValidationUtility.SafeOnValidate(() => { Initialize(); });
    }

    private void Initialize()
    {
        if (this == null)
            //what
            return;

        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.size = new Vector2(platformWidth, 1.25f);
        if (changeCollider)
            GetComponent<BoxCollider2D>().size = new Vector2(platformWidth, 5f / 8f);

        displacementMap = new Texture2D(platformWidth * samplesPerTile, 1);
        pixels = new Color32[platformWidth * samplesPerTile];

        mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);

        mpb.SetFloat("PlatformWidth", platformWidth);
        mpb.SetFloat("PointsPerTile", samplesPerTile);
    }

    [PunRPC]
    public void Bump(int id, Vector2 worldPos)
    {
        var player = PhotonView.Find(id).GetComponent<PlayerController>();

        var localPos = transform.InverseTransformPoint(worldPos).x;

        if (Mathf.Abs(localPos) > platformWidth)
        {
            worldPos.x += GameManager.Instance.levelWidthTile / 2f;
            localPos = transform.InverseTransformPoint(worldPos).x;
        }

        localPos /= platformWidth + 1;
        localPos += 0.5f; // get rid of negative coords
        localPos *= samplesPerTile * platformWidth;

        if (displacementMap.GetPixel((int)localPos, 0).r != 0)
            return;

        player.PlaySound(Enums.Sounds.World_Block_Bump);
        if (player.photonView.IsMine)
            InteractableTile.Bump(player, InteractableTile.InteractionDirection.Up, worldPos + BUMP_OFFSET);
        bumps.Add(new BumpInfo(bumpDuration, (int)localPos));
    }

    private class BumpInfo
    {
        public readonly int pointX;
        public float timer;

        public BumpInfo(float timer, int pointX)
        {
            this.timer = timer;
            this.pointX = pointX;
        }
    }
}