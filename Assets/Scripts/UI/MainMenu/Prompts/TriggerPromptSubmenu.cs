using NSMB.Networking;
using Quantum;
using Quantum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class TriggerPromptSubmenu : PromptSubmenu {
        public GameObject triggerTemplate;
        public GameObject triggersParent;
        public GameObject popupCondition, popupAction;
        // public CounterTip counterTip;
        public bool success = true;
        public List<TriggerListEntry> triggers = new();
        public TriggerListEntry currentEditingEntry = null;
        public Button btnAdd, btnClear;
        public MatchSettings matchSettings;

        public void Start() {
            QuantumEvent.Subscribe<EventTriggersChanged>(this, OnTriggersChanged);
            QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
        }

        public override unsafe void Show(bool first) {
            base.Show(first);
            success = false;
            NetworkHandler.Client.AddCallbackTarget(this);
            var f = NetworkHandler.Game.Frames.Predicted;
            var newTriggers = f.ResolveList(f.Global->Rules.Triggers);
            RefreshInteractability();
            RefreshValues(newTriggers);
        }
        
        public override void Hide(SubmenuHideReason hideReason) {
            base.Hide(hideReason);
            NetworkHandler.Client.RemoveCallbackTarget(this);
        }
        
        public override bool TryGoBack(out bool playSound) {
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }

        public unsafe void OnGameStarted(CallbackGameStarted e) {
            var f = e.Game.Frames.Predicted;
            var newTriggers = f.ResolveList(f.Global->Rules.Triggers);
            RefreshValues(newTriggers);
        }

        public unsafe void OnTriggersChanged(EventTriggersChanged e) {
            var f = e.Frame;
            var newTriggers = f.ResolveList(f.Global->Rules.Triggers);
            RefreshValues(newTriggers);
        }
        
        private void RefreshInteractability() {
            btnAdd.interactable = matchSettings.isHost;
            btnClear.interactable = matchSettings.isHost;
            foreach (var trigger in triggers) {
                trigger.RefreshInteractability();
            }
        }
        
        private void RefreshValues(QList<MatchConditionerTrigger> newTriggers) {
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
                    newEntryScript.RefreshInteractability();
                }
                triggers[i].Trigger = newTriggers[i];
            }
            for (int i = triggers.Count - 1; i >= newTriggers.Count; i--) {
                Destroy(triggers[i].gameObject);
                triggers.RemoveAt(i);
            }
            
            // counterTip.SetCount(triggers.Count);
        }

        public unsafe void TriggersCleared() {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                RemoveAll = true,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        public unsafe void TriggerAdded() {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = triggers.Count,
                RemoveSingle = false,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        public unsafe void TriggerDuplicated(TriggerListEntry entry) {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = triggers.Count,
                TriggerAction = (int)entry.Trigger.Action,
                TriggerActionParameter = entry.Trigger.ActionParameter,
                TriggerActionTarget = (int)entry.Trigger.ActionTarget,
                TriggerCondition = (int)entry.Trigger.Condition,
                TriggerConditionParameter = entry.Trigger.ConditionParameter,
                TriggerConditionTarget = (int)entry.Trigger.ConditionTarget,
                TriggerConstraint = (int)entry.Trigger.Constraint,
                TriggerConstraintParameter = entry.Trigger.ConstraintParameter,
                TriggerConstraintTarget = (int)entry.Trigger.ConstraintTarget,
                TriggerDelaySeconds = entry.Trigger.DelaySeconds,
                TriggerRepeatCount = entry.Trigger.RepeatCount,
                TriggerChance = entry.Trigger.Chance,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }
        
        public unsafe void TriggerRemoved(TriggerListEntry entry) {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = entry.Index,
                RemoveSingle = true,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }
        
        public unsafe void TriggerEdited(TriggerListEntry entry) {
            QuantumGame game = NetworkHandler.Game;
            
            var cmd = new CommandChangeTriggers {
                Index = entry.Index,
                TriggerAction = (int)entry.Trigger.Action,
                TriggerActionParameter = entry.Trigger.ActionParameter,
                TriggerActionTarget = (int)entry.Trigger.ActionTarget,
                TriggerCondition = (int)entry.Trigger.Condition,
                TriggerConditionParameter = entry.Trigger.ConditionParameter,
                TriggerConditionTarget = (int)entry.Trigger.ConditionTarget,
                TriggerConstraint = (int)entry.Trigger.Constraint,
                TriggerConstraintParameter = entry.Trigger.ConstraintParameter,
                TriggerConstraintTarget = (int)entry.Trigger.ConstraintTarget,
                TriggerDelaySeconds = entry.Trigger.DelaySeconds,
                TriggerRepeatCount = entry.Trigger.RepeatCount,
                TriggerChance = entry.Trigger.Chance,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        public void OnConditionClicked(TriggerListEntry entry) {
            currentEditingEntry = entry;
            popupCondition.SetActive(true);
        }

        public void OnConditionSelected(string condition) {
            currentEditingEntry.OnConditionChanged(Enum.Parse<TriggerCondition>(condition));
            ClosePopups();
            Canvas.PlayConfirmSound();
        }
        
        public void OnActionClicked(TriggerListEntry entry) {
            currentEditingEntry = entry;
            popupAction.SetActive(true);
        }
        
        public void OnActionSelected(string action) {
            currentEditingEntry.OnActionChanged(Enum.Parse<TriggerAction>(action));
            ClosePopups();
            Canvas.PlayConfirmSound();
        }
        
        public void ClosePopups() {
            popupCondition.SetActive(false);
            popupAction.SetActive(false);
            currentEditingEntry = null;
        }
        
        public void OnGameDestroyed(CallbackGameDestroyed e) {
            // cleanup
            foreach (var trigger in triggers) {
                Destroy(trigger.gameObject);
            }
            triggers.Clear();
            // counterTip.SetCount(0);
        }
    }
}