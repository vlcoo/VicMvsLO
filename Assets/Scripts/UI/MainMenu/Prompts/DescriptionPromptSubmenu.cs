using NSMB.Networking;
using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class DescriptionPromptSubmenu : PromptSubmenu {
        public bool success = true;
        public MatchSettings matchSettings;
        public TMP_InputField descriptionField;
        
        private void RefreshInteractability() {
            descriptionField.interactable = matchSettings.isHost;
        }
        
        public override void Show(bool first) {
            base.Show(first);
            success = false;
            
            matchSettings.OnRulesChangedCallback += RefreshValues;
            RefreshInteractability();
            RefreshValues();
        }
        
        public override void Hide(SubmenuHideReason hideReason) {
            matchSettings.OnRulesChangedCallback -= RefreshValues;
            base.Hide(hideReason);
        }
        
        public unsafe void DescriptionChanged() {
            var cmd = new CommandChangeRules {
                EnabledChanges = CommandChangeRules.Rules.Description,
                Description = descriptionField.text
            };
            QuantumGame game = QuantumRunner.DefaultGame;
            int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }
        
        public override bool TryGoBack(out bool playSound) {
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }

        private void RefreshValues() {
            var rules = matchSettings.rules;
            descriptionField.text = rules.Description;
        }
    }
}