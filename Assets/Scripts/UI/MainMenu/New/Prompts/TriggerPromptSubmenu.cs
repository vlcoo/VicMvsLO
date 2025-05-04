using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class TriggerPromptSubmenu : PromptSubmenu {
        public GameObject triggerTemplate;
        public GameObject triggersParent;
        public CounterTip counterTip;
        public bool success = true;
        public List<TriggerListEntry> triggers = new();

        public void Start() {
            QuantumEvent.Subscribe<EventTriggersChanged>(this, OnTriggersChanged);
            QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
        }

        public override void Show(bool first) {
            base.Show(first);
            success = false;
        }
        
        public override bool TryGoBack(out bool playSound) {
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }

        public unsafe void OnTriggersChanged(EventTriggersChanged e) {
            var f = e.Frame;
            var newTriggers = f.ResolveList(f.Global->Rules.Triggers);
            
            // go through all the new triggers. we can reuse existing entries in the ui list by simply editing them.
            // if there are more triggers than entries, we need to create new entries.
            // if there are less, we have to remove the excess entries.
            // TODO: can be massively optimized! will iterate on its implementation after checking this fully works.
            
            for (int i = 0; i < newTriggers.Count; i++) {
                if (i >= triggers.Count) {
                    var newEntry = Instantiate(triggerTemplate, triggersParent.transform, false);
                    var newEntryScript = newEntry.GetComponent<TriggerListEntry>();
                    triggers.Add(newEntryScript);
                    newEntryScript.Index = triggers.Count - 1;
                    newEntryScript.Parent = this;
                    newEntry.SetActive(true);
                }
                triggers[i].Trigger = newTriggers[i];
            }
            for (int i = triggers.Count - 1; i >= newTriggers.Count; i--) {
                Destroy(triggers[i].gameObject);
                triggers.RemoveAt(i);
            }
            
            counterTip.SetCount(triggers.Count);
        }

        public unsafe void TriggerAdded() {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = triggers.Count,
                Remove = false,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }
        
        public unsafe void TriggerRemoved(TriggerListEntry entry) {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = entry.Index,
                Remove = true,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }
        
        public unsafe void TriggerEdited(TriggerListEntry entry) {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = entry.Index,
                Remove = false,
                TriggerAction = (int)entry.Trigger.Action,
                TriggerActionParameter = entry.Trigger.ActionParameter,
                TriggerActionTarget = (int)entry.Trigger.ActionTarget,
                TriggerCondition = (int)entry.Trigger.Condition,
                TriggerConditionParameter = entry.Trigger.ConditionParameter,
                TriggerConditionTarget = (int)entry.Trigger.ConditionTarget,
                TriggerConstraint = (int)entry.Trigger.Constraint,
                TriggerConstraintParameter = entry.Trigger.ConstraintParameter,
                TriggerConstraintTarget = (int)entry.Trigger.ConstraintTarget,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }
        
        public void OnGameDestroyed(CallbackGameDestroyed e) {
            // cleanup
            foreach (var trigger in triggers) {
                Destroy(trigger.gameObject);
            }
            triggers.Clear();
            counterTip.SetCount(0);
        }
    }
}