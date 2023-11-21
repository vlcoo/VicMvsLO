using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "BreakablePipeTile", menuName = "ScriptableObjects/Tiles/BreakablePipeTile", order = 4)]
public class BreakablePipeTile : InteractableTile
{
    public string leftDestroy, rightDestroy, pipeParticle;
    public bool upsideDownPipe, leftOfPipe;

    public override bool Interact(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation)
    {
        if (!(interacter is PlayerController))
            return false;

        var player = (PlayerController)interacter;
        if (player.state != Enums.PowerupState.MegaMushroom)
            return false;

        if ((upsideDownPipe && direction == InteractionDirection.Down) ||
            (!upsideDownPipe && direction == InteractionDirection.Up))
            //we've hit the underside of the pipe
            return false;


        var tilemap = GameManager.Instance.tilemap;
        var ourLocation = Utils.WorldToTilemapPosition(worldLocation);

        if (leftOfPipe && direction == InteractionDirection.Left)
            if (Utils.GetTileAtTileLocation(ourLocation + Vector3Int.right) is InteractableTile otherPipe)
                return otherPipe.Interact(interacter, direction, worldLocation + Vector3.right * 0.5f);
        if (!leftOfPipe && direction == InteractionDirection.Right)
            if (Utils.GetTileAtTileLocation(ourLocation + Vector3Int.left) is InteractableTile otherPipe)
                return otherPipe.Interact(interacter, direction, worldLocation + Vector3.left * 0.5f);

        var height = GetPipeHeight(ourLocation);
        var origin = GetPipeOrigin(ourLocation);
        var pipeDirection = upsideDownPipe ? Vector3Int.up : Vector3Int.down;
        var hat = origin - pipeDirection * (height - 1);

        if (ourLocation.y == GameManager.Instance.levelMinTileY + 1)
            //exception: dont break out of bounds.
            return false;

        var bottom = false;

        if (origin.y < GameManager.Instance.cameraMinY - 9f || origin.y + height >=
            GameManager.Instance.levelMinTileY + GameManager.Instance.levelHeightTile)
            bottom = true;

        int tileHeight;
        var shrink = false;
        var addHat = true;

        if (direction == InteractionDirection.Down || direction == InteractionDirection.Up)
        {
            //hit top/bottom of pipe.
            if (hat == origin || height <= 1)
                return false;

            //shrink the pipe by 1. simple as moving the hat tiles up/down one
            tileHeight = 2;
            shrink = true;
        }
        else
        {
            //hit left/right side of pipe

            Vector2 world = worldLocation;
            var alreadyDestroyed = tilemap.GetTile(hat).name.EndsWith("D");

            if (upsideDownPipe)
            {
                if (ourLocation == origin)
                    addHat = false;

                tileHeight = Mathf.Abs(hat.y - ourLocation.y) + (addHat ? 2 : 1);

                if (bottom && ourLocation == origin && (tileHeight != 1 || alreadyDestroyed))
                    return false;
            }
            else
            {
                addHat = bottom;
                tileHeight = GetPipeHeight(ourLocation);

                world -= (Vector2)((Vector3)(ourLocation - origin) / 2f);

                if (bottom)
                    world += Vector2.up * 0.5f;
            }


            object[] parametersParticle =
            {
                world + (leftOfPipe ? Vector2.zero : Vector2.left * 0.5f), leftOfPipe, upsideDownPipe,
                new Vector2(2, tileHeight - (addHat ? 1 : 0)), pipeParticle + (alreadyDestroyed ? "-D" : "")
            };
            GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnResizableParticle, parametersParticle,
                SendOptions.SendUnreliable);
        }

        var tiles = new string[tileHeight * 2];

        var start = upsideDownPipe ? tileHeight * 2 - 2 : 0;
        if (addHat)
        {
            if (leftOfPipe)
            {
                //we're the left side. modify the right side too.
                if (shrink)
                {
                    tiles[start] = "SpecialPipes/" + tilemap.GetTile(hat).name;
                    tiles[start + 1] = "SpecialPipes/" + tilemap.GetTile(hat + Vector3Int.right).name;
                }
                else
                {
                    tiles[start] = "SpecialPipes/" + leftDestroy;
                    tiles[start + 1] = "SpecialPipes/" + rightDestroy;
                }
            }
            else
            {
                //we're the right side. modify the left side too.
                if (shrink)
                {
                    tiles[start] = "SpecialPipes/" + tilemap.GetTile(hat + Vector3Int.left).name;
                    tiles[start + 1] = "SpecialPipes/" + tilemap.GetTile(hat).name;
                }
                else
                {
                    tiles[start] = "SpecialPipes/" + leftDestroy;
                    tiles[start + 1] = "SpecialPipes/" + rightDestroy;
                }
            }
        }

        for (var i = 0; i < tiles.Length; i++)
            //photon doesn't like serializing nulls
            if (tiles[i] == null)
                tiles[i] = "";

        var offset = upsideDownPipe ? Vector3Int.zero : pipeDirection * (tileHeight - 1);
        BulkModifyTilemap(hat + offset + (leftOfPipe ? Vector3Int.zero : Vector3Int.left),
            new Vector2Int(2, tileHeight), tiles);

        player.photonView.RPC(nameof(PlayerController.PlaySound), RpcTarget.All,
            Enums.Sounds.Powerup_MegaMushroom_Break_Pipe);
        return true;
    }

    private Vector3Int GetPipeOrigin(Vector3Int ourLocation)
    {
        var tilemap = GameManager.Instance.tilemap;
        var searchDirection = upsideDownPipe ? Vector3Int.up : Vector3Int.down;
        var searchVector = upsideDownPipe ? Vector3Int.up : Vector3Int.down;
        while (tilemap.GetTile<BreakablePipeTile>(ourLocation + searchVector))
            searchVector += searchDirection;
        return ourLocation + searchVector - searchDirection;
    }

    private int GetPipeHeight(Vector3Int ourLocation)
    {
        var height = 1;
        var tilemap = GameManager.Instance.tilemap;
        var searchVector = Vector3Int.up;
        while (tilemap.GetTile<BreakablePipeTile>(ourLocation + searchVector))
        {
            height++;
            searchVector += Vector3Int.up;
        }

        searchVector = Vector3Int.down;
        while (tilemap.GetTile<BreakablePipeTile>(ourLocation + searchVector))
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