using ExitGames.Client.Photon;
using NSMB.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "BreakableBulletBillLauncher",
    menuName = "ScriptableObjects/Tiles/BreakableBulletBillLauncher", order = 5)]
public class BreakableBulletBillLauncher : InteractableTile
{
    public override bool Interact(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation)
    {
        if (!(interacter is PlayerController))
            return false;

        var player = (PlayerController)interacter;
        if (player.state != Enums.PowerupState.MegaMushroom)
            return false;
        if (direction == InteractionDirection.Down || direction == InteractionDirection.Up)
            return false;

        var ourLocation = Utils.WorldToTilemapPosition(worldLocation);
        var height = GetLauncherHeight(ourLocation);
        var origin = GetLauncherOrigin(ourLocation);

        var tiles = new string[height];

        for (var i = 0; i < tiles.Length; i++)
            //photon doesn't like serializing nulls
            tiles[i] = "";

        object[] parametersParticle =
        {
            (Vector2)worldLocation, direction == InteractionDirection.Right, false, new Vector2(1, height),
            "DestructableBulletBillLauncher"
        };
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnResizableParticle, parametersParticle,
            SendOptions.SendUnreliable);

        BulkModifyTilemap(origin, new Vector2Int(1, height), tiles);
        return true;
    }

    private Vector3Int GetLauncherOrigin(Vector3Int ourLocation)
    {
        var tilemap = GameManager.Instance.tilemap;
        var searchDirection = Vector3Int.down;
        var searchVector = Vector3Int.down;
        while (tilemap.GetTile<BreakableBulletBillLauncher>(ourLocation + searchVector))
            searchVector += searchDirection;
        return ourLocation + searchVector - searchDirection;
    }

    private int GetLauncherHeight(Vector3Int ourLocation)
    {
        var height = 1;
        var tilemap = GameManager.Instance.tilemap;
        var searchVector = Vector3Int.up;
        while (tilemap.GetTile<BreakableBulletBillLauncher>(ourLocation + searchVector))
        {
            height++;
            searchVector += Vector3Int.up;
        }

        searchVector = Vector3Int.down;
        while (tilemap.GetTile<BreakableBulletBillLauncher>(ourLocation + searchVector))
        {
            height++;
            searchVector += Vector3Int.down;
        }

        return height;
    }

    private void BulkModifyTilemap(Vector3Int tileOrigin, Vector2Int tileDimensions, string[] tilenames)
    {
        object[] parametersTile = { tileOrigin.x, tileOrigin.y, tileDimensions.x, tileDimensions.y, tilenames };
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, parametersTile,
            SendOptions.SendReliable);
    }
}