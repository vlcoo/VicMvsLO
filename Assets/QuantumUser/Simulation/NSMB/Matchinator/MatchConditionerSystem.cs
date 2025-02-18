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
                Action = TriggerAction.Kill,
                ActionTarget = TriggerTarget.All,
                Condition = TriggerCondition.GotCoin,
                ConditionTarget = TriggerTarget.All,
                Constraint = TriggerConstraint.Always,
                ConstraintNegated = false,
                ConstraintTarget = TriggerTarget.All,
            });
        }

        private unsafe void ConditionActioned(TriggerCondition condition, Frame f, EntityRef entity) {
            foreach (var actionString in from trigger in f.ResolveList(f.Global->Rules.Triggers) where trigger.Condition == condition select trigger.Action.ToString())
            {
                GetType().GetMethod($"Act{actionString}")?.Invoke(this, new object[] { f, entity });
            }
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
        public unsafe void ActKill(Frame f, EntityRef entity) {
            f.Unsafe.GetPointer<MarioPlayer>(entity)->Death(f, entity, false);
        }
        #endregion
    }
}
