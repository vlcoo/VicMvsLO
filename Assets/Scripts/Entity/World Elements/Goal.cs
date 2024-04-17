using Fusion;
using NSMB.Entities.Player;
using NSMB.Extensions;
using NSMB.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Goal : NetworkBehaviour, IPlayerInteractable
{
    //--- Networked Variables
    [Networked] public PlayerController WinningPlayer { get; set; }
    [Networked] public NetworkBool Collectable { get; set; } = true;
    [Networked] public TickTimer EndGameTimer { get; set; }
    [Networked] public int HangingPlayersCount {
        get => _hangingPlayersCount;
        set {
            if (value > 0 && !EndGameTimer.IsRunning) EndGameTimer = TickTimer.CreateFromSeconds(Runner, 4);
            _hangingPlayersCount = math.max(0, value);
        }
    }

    //---Private Variables
    private int _hangingPlayersCount;

    public override void FixedUpdateNetwork() {
        if (HangingPlayersCount == 0 || !Collectable) return;

        int activePlayersCount = Runner.ActivePlayers
            .Select(pr => pr.GetPlayerData())
            .Count(pd => pd && !pd.IsCurrentlySpectating);

        if (EndGameTimer.Expired(Runner) || HangingPlayersCount >= activePlayersCount) {
            Debug.Log("goal reached and game ended!");
            EndGameTimer = TickTimer.None;
            // TODO vcmi: Implement winner checking for laps.
            GameManager.Instance.Rpc_EndGame(0);
            Collectable = false;
            return;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_LapCollected(PlayerController collector) {
        if (GameManager.Instance && GameManager.Instance.PlaySounds) {
            collector.PlaySoundEverywhere(Enums.Sounds.World_Block_Bump);
        }

        if (collector.cameraController.IsControllingCamera) {
            GlobalController.Instance.rumbleManager.RumbleForSeconds(0f, 0.8f, 0.1f, RumbleManager.RumbleSetting.High);
        }

        Instantiate(PrefabList.Instance.Particle_EnemySpecialKill, collector.body.Position, Quaternion.identity);
    }

    //---IPlayerInteractable overrides
    public void InteractWithPlayer(PlayerController player, PhysicsDataStruct.IContactStruct contact) {
        if (player.IsDead || player.IsGoalHanging) return;
        if (!Collectable) return;

        if (HasStateAuthority) {
            Rpc_LapCollected(player);
        }

        // We can lap
        player.Laps = (byte) Mathf.Min(player.Laps + 1, SessionData.Instance.LapRequirement);
        if (player.Laps >= SessionData.Instance.LapRequirement) {
            player.IsGoalHanging = true;
            if (HangingPlayersCount == 0) WinningPlayer = player;
            HangingPlayersCount++;
        } else {
            Vector3 spawnpoint = player.Spawnpoint;
            player.body.Position = spawnpoint;
            player.cameraController.Recenter(spawnpoint);
        }


        // Game mechanics
        if (HasStateAuthority) {
            GameManager.Instance.tileManager.ResetMap();
        }
    }
}
