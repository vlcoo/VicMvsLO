using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerupTile", menuName = "ScriptableObjects/Tiles/PowerupTile", order = 2)]
public class PowerupTile : BreakableBrickTile
{
    public string resultTile;

    public override bool Interact(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation)
    {
        if (base.Interact(interacter, direction, worldLocation))
            return true;

        var tileLocation = Utils.WorldToTilemapPosition(worldLocation);

        var spawnResult = "Mushroom";

        if (interacter is PlayerController || (interacter is KoopaWalk koopa && koopa.previousHolder != null))
        {
            var player = interacter is PlayerController controller
                ? controller
                : ((KoopaWalk)interacter).previousHolder;
            if (player.state == Enums.PowerupState.MegaMushroom)
            {
                //Break

                //Tilemap
                object[] parametersTile = { tileLocation.x, tileLocation.y, null };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile,
                    SendOptions.SendReliable);

                //Particle
                object[] parametersParticle =
                {
                    tileLocation.x, tileLocation.y, "BrickBreak",
                    new Vector3(particleColor.r, particleColor.g, particleColor.b)
                };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnParticle, parametersParticle,
                    SendOptions.SendUnreliable);

                if (interacter is MonoBehaviourPun pun)
                    pun.photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Block_Break);
                return true;
            }

            spawnResult = player.state <= Enums.PowerupState.Small ? "Mushroom" : "FireFlower";
        }

        Bump(interacter, direction, worldLocation);
        if (GameManager.Instance.Togglerizer.currentEffects.Contains("NoPowerups")) spawnResult = "";

        object[] parametersBump =
            { tileLocation.x, tileLocation.y, direction == InteractionDirection.Down, resultTile, spawnResult };
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.BumpTile, parametersBump, SendOptions.SendReliable);

        if (interacter is MonoBehaviourPun pun2 && spawnResult != "")
            pun2.photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Block_Powerup);
        return false;
    }
}