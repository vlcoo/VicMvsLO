using NSMB.Utils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockBump : MonoBehaviour
{
    public Sprite sprite;
    public Vector2 spawnOffset = Vector2.zero;
    public string resultTile = "", resultPrefab;
    public bool fromAbove;

    private SpriteRenderer spriteRenderer;

    #region Unity Methods

    public void Start()
    {
        var anim = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        anim.SetBool("down", fromAbove);
        anim.SetTrigger("start");

        if (resultPrefab == "Coin")
        {
            var coin = (GameObject)Instantiate(Resources.Load("Prefabs/Particle/CoinFromBlock"),
                transform.position + new Vector3(0, fromAbove ? -0.25f : 0.5f), Quaternion.identity);
            coin.GetComponentInChildren<Animator>().SetBool("down", fromAbove);
        }

        var hitbox = GetComponentInChildren<BoxCollider2D>();
        hitbox.size = sprite.bounds.size;
        hitbox.offset = (hitbox.size - Vector2.one) * new Vector2(1 / 2f, -1 / 2f);
    }

    #endregion

    #region Animator Methods

    public void Kill()
    {
        Destroy(gameObject);

        var tm = GameManager.Instance.tilemap;
        var loc = Utils.WorldToTilemapPosition(transform.position);

        var tile = Resources.Load("Tilemaps/Tiles/" + resultTile);
        if (tile is AnimatedTile animatedTile)
            tm.SetTile(loc, animatedTile);
        else if (tile is Tile normalTile) tm.SetTile(loc, normalTile);

        if (!PhotonNetwork.IsMasterClient || resultPrefab == null || resultPrefab == "" || resultPrefab == "Coin")
            return;

        var pos = transform.position + Vector3.up * (fromAbove ? -0.7f : 0.25f);
        PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/" + resultPrefab, pos + (Vector3)spawnOffset,
            Quaternion.identity);
    }

    #endregion
}