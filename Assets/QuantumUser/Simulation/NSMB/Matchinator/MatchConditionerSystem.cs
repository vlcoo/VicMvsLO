using Photon.Deterministic;
using Quantum.Collections;
using System.Linq;
using UnityEngine;
using Int32 = System.Int32;

namespace Quantum
{
    public class MatchConditionerSystem : SystemSignalsOnly, ISignalOnMarioPlayerCollectedStar, ISignalOnMarioPlayerCollectedCoin, ISignalOnMarioPlayerDied, ISignalOnEntityFreeze, ISignalOnGameStarting, ISignalOnMarioPlayerDisqualified, ISignalOnMarioPlayerJumped, ISignalOnMarioPlayerRespawned, ISignalOnMarioPlayerReceivedKnockback, ISignalOnMarioPlayerCollectedPowerup, ISignalOnMarioPlayerTookDamage, ISignalOnMarioPlayerReachedCoinLimit, ISignalOnMarioPlayerZeroedStars, ISignalOnMarioPlayerZeroedCoins {
        // TODO: initialize the trigger list in the correct place somewhere else!!
        public override unsafe void OnInit(Frame f) {
            // MOCK TRIGGER LIST for testing purposes...
            f.Global->Rules.Triggers = f.AllocateList<MatchConditionerTrigger>(10);
            var triggerMap = f.ResolveList(f.Global->Rules.Triggers);
            
            triggerMap.Add(new MatchConditionerTrigger {
                Condition = TriggerCondition.GotCoin,
                ConditionParameter = "",
                ConditionTarget = TriggerTarget.All,
                Action = TriggerAction.RemoveStar,
                ActionParameter = "",
                ActionTarget = TriggerTarget.Conditioner,
                Constraint = TriggerConstraint.Always,
                ConstraintParameter = "",
                ConstraintTarget = TriggerTarget.All,
            });
        }

        private unsafe void ConditionActioned(TriggerCondition condition, Frame f, EntityRef entity, string parameter = "") {
            foreach (var trigger in f.ResolveList(f.Global->Rules.Triggers).Where(trigger => trigger.Condition == condition)) {
                Debug.Log(
                    $"[MatchConditioner] <b>{condition}</b> succeeded... <b>{trigger.Action}</b>{(trigger.ActionParameter != "" ? (" (" + trigger.ActionParameter + ")") : "")} begin!!");
                GetType().GetMethod($"Act{trigger.Action.ToString()}")?.Invoke(this,
                    new object[] { f, entity, trigger.ActionParameter.ToString() });
            }
        }
        
        private void Err(string message) {
            Debug.LogError($"[MatchConditioner] {message}");
        }

        #region Conditions
        public void OnMarioPlayerCollectedStar(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.GotStar, f, entity);
        }
        
        public unsafe void OnMarioPlayerCollectedCoin(Frame f, EntityRef marioEntity, MarioPlayer* mario, FPVector2 worldLocation,
            QBoolean fromBlock, QBoolean downwards) {
            ConditionActioned(TriggerCondition.GotCoin, f, marioEntity);
        }

        public void a() {
            // ConditionActioned(TriggerCondition.GotCheckpoint, f, entity);
            // ConditionActioned(TriggerCondition.HitBlock, f, entity);
            // ConditionActioned(TriggerCondition.StunnedSomeone, f, entity);
            // ConditionActioned(TriggerCondition.SteppedOnEnemy, f, entity);
            // ConditionActioned(TriggerCondition.TriggeredPowerup, f, entity);
            // ConditionActioned(TriggerCondition.LookedXDirection, f, entity);
            // ConditionActioned(TriggerCondition.Ran, f, entity);
            // ConditionActioned(TriggerCondition.XSecondRemaining, f, entity);
            // ConditionActioned(TriggerCondition.SongBahd, f, entity);
            // ConditionActioned(TriggerCondition.GotStarcoin, f, entity);
            // ConditionActioned(TriggerCondition.EnteredPipe, f, entity);
            // ConditionActioned(TriggerCondition.GrabbedSomething, f, entity);
            // ConditionActioned(TriggerCondition.FinishedLap, f, entity);
            // ConditionActioned(TriggerCondition.TouchedGround, f, entity);
            // ConditionActioned(TriggerCondition.StoppedMoving, f, entity);
            // ConditionActioned(TriggerCondition.KilledSomeone, f, entity);
            // ConditionActioned(TriggerCondition.HarmedSomeone, f, entity);
        }
        
        public void OnMarioPlayerDied(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.Died, f, entity);
        }

        public void OnEntityFreeze(Frame f, EntityRef entity, EntityRef iceBlock) {
            if (!f.Has<MarioPlayer>(entity)) return;
            ConditionActioned(TriggerCondition.Frozen, f, entity);
        }

        public void OnGameStarting(Frame f) {
            ConditionActioned(TriggerCondition.MatchStarted, f, default);
        }
        
        public void OnMarioPlayerDisqualified(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.Disqualified, f, entity);
        }

        public void OnMarioPlayerJumped(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.Jumped, f, entity);
        }

        public void OnMarioPlayerRespawned(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.Spawned, f, entity);
        }

        public void OnMarioPlayerReceivedKnockback(Frame f, EntityRef entity, EntityRef attacker, Int32 strength) {
            switch (strength) {
                case 0: ConditionActioned(TriggerCondition.Stunned, f, entity, "Bump"); break;
                case 1: ConditionActioned(TriggerCondition.Stunned, f, entity, "Knockback"); break;
                case 2: ConditionActioned(TriggerCondition.Stunned, f, entity, "HardKnockback"); break;
            }
        }

        public unsafe void OnMarioPlayerCollectedPowerup(Frame f, EntityRef marioEntity, MarioPlayer* mario) {
            ConditionActioned(TriggerCondition.GotPowerup, f, marioEntity);
        }

        public void OnMarioPlayerTookDamage(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.LostPowerup, f, entity);
        }
        
        public unsafe void OnMarioPlayerReachedCoinLimit(Frame f, EntityRef marioEntity, MarioPlayer* mario) {
            ConditionActioned(TriggerCondition.ReachedCoinLimit, f, marioEntity);
        }

        public void OnMarioPlayerZeroedStars(Frame f, EntityRef entity) {
            ConditionActioned(TriggerCondition.ReachedZeroStars, f, entity);
        }

        public unsafe void OnMarioPlayerZeroedCoins(Frame f, EntityRef marioEntity, MarioPlayer* mario) {
            ConditionActioned(TriggerCondition.ReachedZeroCoins, f, marioEntity);
        }
        #endregion

        #region Actions
        public unsafe void ActKill(Frame f, EntityRef entity, string parameter) {
            f.Unsafe.GetPointer<MarioPlayer>(entity)->Death(f, entity, false);
        }

        public unsafe void ActGiveStar(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            mario->Stars++;
            f.Signals.OnMarioPlayerCollectedStar(entity);
            f.Events.MarioPlayerCollectedStar(f, entity, *mario, f.Unsafe.GetPointer<Transform2D>(entity)->Position);
        }

        public unsafe void ActGiveCoin(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            f.Signals.OnMarioPlayerCollectedCoin(entity, mario, f.Unsafe.GetPointer<Transform2D>(entity)->Position, false, false);
        }

        public unsafe void ActRemoveStar(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            if (mario->Stars == 0) return;
            mario->Stars--;
            if (mario->Stars == 0) f.Signals.OnMarioPlayerZeroedStars(entity);
        }

        public unsafe void ActRemoveCoin(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            if (mario->Coins == 0) return;
            mario->Coins--;
            if (mario->Coins == 0) f.Signals.OnMarioPlayerZeroedCoins(entity, mario);
        }

        public unsafe void ActGiveXPowerup(Frame f, EntityRef entity, string parameter) {
            PowerupAsset newScriptable = parameter == "random"
                ? f.SimulationConfig.AllPowerups[f.RNG->Next(0, f.SimulationConfig.AllPowerups.Length)]
                : f.SimulationConfig.AllPowerups.FirstOrDefault(p => p.State.ToString() == parameter);
            if (newScriptable == null) { Err("powerup asset was null!!"); return; }
            PowerupReserveResult result = PowerupSystem.CollectPowerup(f, entity, f.Unsafe.GetPointer<MarioPlayer>(entity), f.Unsafe.GetPointer<PhysicsObject>(entity), newScriptable, true);
            f.Events.MarioPlayerCollectedPowerup(f, entity, result, newScriptable);
        }

        public unsafe void ActGiveLife(Frame f, EntityRef entity, string parameter) {
            if (!f.Global->Rules.IsLivesEnabled) return;
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            mario->Lives++;
        }

        public unsafe void ActWin(Frame f, EntityRef entity, string parameter) {
            GameLogicSystem.EndGame(f, f.Unsafe.GetPointer<MarioPlayer>(entity)->GetTeam(f));
        }

        public unsafe void ActDrawMatch(Frame f, EntityRef entity, string parameter) {
            GameLogicSystem.EndGame(f, null);
        }

        public unsafe void ActDisqualify(Frame f, EntityRef entity, string parameter) {
            f.Signals.OnMarioPlayerDisqualified(entity);
            f.Destroy(entity);
            GameLogicSystem.CheckForGameEnd(f);
        }

        public unsafe void ActStun(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            switch (parameter) {
                case "Bump":
                    mario->DoKnockback(f, entity, mario->FacingRight, 1, true, entity);
                    break;
                case "Knockback":
                    mario->DoKnockback(f, entity, mario->FacingRight, 1, false, entity);
                    break;
                case "HardKnockback":
                    mario->DoKnockback(f, entity, mario->FacingRight, 3, false, entity);
                    break;
                case "ForcefulKnockback":
                    mario->DoKnockback(f, entity, mario->FacingRight, 1, false, entity, true);
                    break;
                default:
                    Err($"invalid param {parameter}!!"); break;
            }
        }

        public unsafe void ActDive(Frame f, EntityRef entity, string parameter) {
            
        }

        public unsafe void ActLaunch(Frame f, EntityRef entity, string parameter) {
            
        }

        public unsafe void ActFreeze(Frame f, EntityRef entity, string parameter) {
            IceBlockSystem.Freeze(f, entity);
        }

        public unsafe void ActHarm(Frame f, EntityRef entity, string parameter) {
            f.Unsafe.GetPointer<MarioPlayer>(entity)->Powerdown(f, entity, parameter == "force");
        }

        public unsafe void ActSpawnXPowerup(Frame f, EntityRef entity, string parameter) {
            PowerupAsset newScriptable = f.SimulationConfig.AllPowerups.FirstOrDefault(p => p.State.ToString() == parameter);
            if (newScriptable == null) { Err("powerup asset was null!!"); return; }
            MarioPlayerSystem.SpawnItem(f, entity, f.Unsafe.GetPointer<MarioPlayer>(entity), parameter == "random" ? default : newScriptable.Prefab);
        }

        public unsafe void ActSpawnXEnemy(Frame f, EntityRef entity, string parameter) {
            
        }

        public unsafe void ActRespawnLevel(Frame f, EntityRef entity, string parameter) {
            f.FindAsset<VersusStageData>(f.Map.UserAsset).ResetStage(f, false);
        }

        public unsafe void ActExplodeLevel(Frame f, EntityRef entity, string parameter) {
            
        }

        public unsafe void ActRandomTeleport(Frame f, EntityRef entity, string parameter) {
            
        }

        public unsafe void ActRemoveReserve(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            mario->ReserveItem = default;
        }

        public unsafe void ActGiveXReserve(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            PowerupAsset newScriptable = parameter == "random"
                ? f.SimulationConfig.AllPowerups[f.RNG->Next(0, f.SimulationConfig.AllPowerups.Length)]
                : f.SimulationConfig.AllPowerups.FirstOrDefault(p => p.State.ToString() == parameter);
            if (newScriptable == null) { Err("powerup asset was null!!"); return; }
            mario->ReserveItem = newScriptable;
            f.Events.MarioPlayerCollectedPowerup(f, entity, PowerupReserveResult.ReserveNewPowerup, newScriptable);
        }

        public unsafe void ActGiveIFrames(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            mario->DamageInvincibilityFrames = 2 * 60;
        }

        public unsafe void ActSpawnLooseCoin(Frame f, EntityRef entity, string parameter) {
            EntityRef coinEntity = f.Create(f.SimulationConfig.LooseCoinPrototype);
            f.Unsafe.GetPointer<Transform2D>(coinEntity)->Position = f.Unsafe.GetPointer<Transform2D>(entity)->Position + f.Unsafe.GetPointer<PhysicsCollider2D>(entity)->Shape.Centroid;
            f.Unsafe.GetPointer<PhysicsObject>(coinEntity)->Velocity =
                new FPVector2(
                    f.RNG->Next(Constants._3_50, 4) * (f.Unsafe.GetPointer<MarioPlayer>(entity)->FacingRight ? -1 : 1),
                    f.RNG->Next(Constants._4_50, 5));
        }

        public unsafe void ActSpawnLooseStar(Frame f, EntityRef entity, string parameter) {
            EntityRef newStarEntity = f.Create(f.SimulationConfig.BigStarPrototype);
            f.Unsafe.GetPointer<Transform2D>(newStarEntity)->Position = f.Unsafe.GetPointer<Transform2D>(entity)->Position;
            f.Unsafe.GetPointer<BigStar>(newStarEntity)->InitializeMovingStar(f,
                f.FindAsset<VersusStageData>(f.Map.UserAsset), newStarEntity,
                f.Unsafe.GetPointer<MarioPlayer>(entity)->FacingRight ? 1 : 2);
        }

        public unsafe void ActSpawnStar(Frame f, EntityRef entity, string parameter) {
            
        }

        public unsafe void ActZeroCoins(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            mario->Coins = 0;
        }

        public unsafe void ActZeroStars(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            mario->Stars = 0;
        }

        public unsafe void ActMaxCoins(Frame f, EntityRef entity, string parameter) {
            // maybe unused.
        }

        public unsafe void ActBecomeXTeam(Frame f, EntityRef entity, string parameter) {
            
        }
        #endregion
    }
}
