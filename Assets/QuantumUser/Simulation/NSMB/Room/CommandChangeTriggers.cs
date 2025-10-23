using Photon.Deterministic;
using System.Collections.Generic;

namespace Quantum {
    public class CommandChangeTriggers : DeterministicCommand, ILobbyCommand {

        public int Index;
        public bool Remove;
        public int TriggerCondition = (int)Quantum.TriggerCondition.GotStar;
        public int TriggerConditionTarget = (int)TriggerTarget.Any;
        public string TriggerConditionParameter = "";
        public int TriggerAction = (int)Quantum.TriggerAction.Kill;
        public int TriggerActionTarget = (int)TriggerTarget.Conditioner;
        public string TriggerActionParameter = "";
        public int TriggerConstraint = (int)Quantum.TriggerConstraint.Always;
        public int TriggerConstraintTarget = (int)TriggerTarget.Any;
        public string TriggerConstraintParameter = "";

        public override void Serialize(BitStream stream) {
            stream.Serialize(ref Index);
            stream.Serialize(ref Remove);
            stream.Serialize(ref TriggerCondition);
            stream.Serialize(ref TriggerConditionTarget);
            stream.Serialize(ref TriggerConditionParameter);
            stream.Serialize(ref TriggerAction);
            stream.Serialize(ref TriggerActionTarget);
            stream.Serialize(ref TriggerActionParameter);
            stream.Serialize(ref TriggerConstraint);
            stream.Serialize(ref TriggerConstraintTarget);
            stream.Serialize(ref TriggerConstraintParameter);
        }

        public unsafe void Execute(Frame f, PlayerRef sender, PlayerData* playerData) {
            if (f.Global->GameState != GameState.PreGameRoom || !playerData->IsRoomHost) {
                // Only the host can change rules.
                return;
            }

            var rules = f.ResolveList(f.Global->Rules.Triggers);

            if (Remove) {
                rules.RemoveAt(Index);
            } else {
                if (Index >= rules.Count) {
                    if (rules.Count >= 80) return;
                    rules.Add(new MatchConditionerTrigger() {
                        Action = (TriggerAction) TriggerAction,
                        ActionParameter = TriggerActionParameter,
                        ActionTarget = (TriggerTarget) TriggerActionTarget,
                        Condition = (TriggerCondition) TriggerCondition,
                        ConditionParameter = TriggerConditionParameter,
                        ConditionTarget = (TriggerTarget) TriggerConditionTarget,
                        Constraint = (TriggerConstraint) TriggerConstraint,
                        ConstraintParameter = TriggerConstraintParameter,
                        ConstraintTarget = (TriggerTarget) TriggerConstraintTarget
                    });
                } else {
                    rules[Index] = new MatchConditionerTrigger() {
                        Action = (TriggerAction) TriggerAction,
                        ActionParameter = TriggerActionParameter,
                        ActionTarget = (TriggerTarget) TriggerActionTarget,
                        Condition = (TriggerCondition) TriggerCondition,
                        ConditionParameter = TriggerConditionParameter,
                        ConditionTarget = (TriggerTarget) TriggerConditionTarget,
                        Constraint = (TriggerConstraint) TriggerConstraint,
                        ConstraintParameter = TriggerConstraintParameter,
                        ConstraintTarget = (TriggerTarget) TriggerConstraintTarget
                    };
                }
            }

            f.Global->Rules.Triggers = rules;
            f.Events.TriggersChanged(f);

            if (f.Global->GameStartFrames > 0 && !QuantumUtils.IsGameStartable(f)) {
                GameLogicSystem.StopCountdown(f);
            }
        }
    }
}