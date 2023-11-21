using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class Exploder : MonoBehaviourPun
{
    public int explosionTileSize = 150;

    [PunRPC]
    public void Detonate()
    {
        var tileLocation = Utils.WorldToTilemapPosition(Vector3.zero);
        var tm = GameManager.Instance.tilemap;
        for (var x = -explosionTileSize; x <= explosionTileSize; x++)
        for (var y = -explosionTileSize; y <= explosionTileSize; y++)
        {
            if (Mathf.Abs(x) + Mathf.Abs(y) > explosionTileSize) continue;
            var ourLocation = tileLocation + new Vector3Int(x, y, 0);
            Utils.WrapTileLocation(ref ourLocation);

            var tile = tm.GetTile(ourLocation);
            if (tile is InteractableTile iTile)
                iTile.Interact(this, InteractableTile.InteractionDirection.Up,
                    Utils.TilemapToWorldPosition(ourLocation));
        }

        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public void PlaySound(Enums.Sounds clip)
    {
    }
}