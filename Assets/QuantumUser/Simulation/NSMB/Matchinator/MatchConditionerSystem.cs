using Photon.Deterministic;
using Quantum.Collections;
using System.Linq;
using UnityEngine;

namespace Quantum
{
    public class MatchConditionerSystem : SystemSignalsOnly, ISignalOnMarioPlayerCollectedStar, ISignalOnMarioPlayerCollectedCoin {
        // TODO: initialize the trigger list in the correct place somewhere else!!
        public override unsafe void OnInit(Frame f) {
            // MOCK TRIGGER LIST for testing purposes...
            f.Global->Rules.Triggers = f.AllocateList<MatchConditionerTrigger>(10);
            var triggerMap = f.ResolveList(f.Global->Rules.Triggers);
            
            triggerMap.Add(new MatchConditionerTrigger {
                Condition = TriggerCondition.GotStar,
                ConditionParameter = "",
                ConditionTarget = TriggerTarget.All,
                Action = TriggerAction.ZeroCoins,
                ActionParameter = "",
                ActionTarget = TriggerTarget.Conditioner,
                Constraint = TriggerConstraint.Always,
                ConstraintParameter = "",
                ConstraintTarget = TriggerTarget.All,
            });
        }

        private unsafe void ConditionActioned(TriggerCondition condition, Frame f, EntityRef entity) {
            foreach (var trigger in f.ResolveList(f.Global->Rules.Triggers).Where(trigger => trigger.Condition == condition))
            {
                GetType().GetMethod($"Act{trigger.Action.ToString()}")?.Invoke(this, new object[] { f, entity, trigger.ActionParameter.ToString() });
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
        }

        public unsafe void ActRemoveCoin(Frame f, EntityRef entity, string parameter) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(entity);
            if (mario->Coins == 0) return;
            mario->Coins--;
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
            f.Destroy(entity);
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
