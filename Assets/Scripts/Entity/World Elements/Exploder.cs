using System.Collections;
using System.Collections.Generic;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Exploder : MonoBehaviourPun {
    public int explosionTileSize = 150;
    
    [PunRPC]
    public void Detonate()
    {
        Vector3Int tileLocation = Utils.WorldToTilemapPosition(Vector3.zero);
        Tilemap tm = GameManager.Instance.tilemap;
        for (int x = -explosionTileSize; x <= explosionTileSize; x++) {
            for (int y = -explosionTileSize; y <= explosionTileSize; y++) {
                if (Mathf.Abs(x) + Mathf.Abs(y) > explosionTileSize) continue;
                Vector3Int ourLocation = tileLocation + new Vector3Int(x, y, 0);
                Utils.WrapTileLocation(ref ourLocation);

                TileBase tile = tm.GetTile(ourLocation);
                if (tile is InteractableTile iTile) {
                    iTile.Interact(this, InteractableTile.InteractionDirection.Up, Utils.TilemapToWorldPosition(ourLocation));
                }
            }
        }
        PhotonNetwork.Destroy(gameObject);
    }
    
    [PunRPC]
    public void PlaySound(Enums.Sounds clip) {}
}
