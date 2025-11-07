using NSMB.Networking;
using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class PowerupPromptSubmenu : PromptSubmenu {
        public bool success = true;
        private readonly Dictionary<string, Toggle> _toggles = new();
        public MatchSettings matchSettings;
        
        private void RefreshInteractability() {
            foreach (var toggle in _toggles) {
                toggle.Value.interactable = matchSettings.isHost;
            }
        }

        public override void Show(bool first) {
            base.Show(first);
            success = false;
            
            var toggles = new List<Toggle>();
            GetComponentsInChildren(true, toggles);
            _toggles.Clear();
            foreach (var toggle in toggles) {
                if (toggle.gameObject.name == "Item") continue;
                _toggles[toggle.gameObject.name] = toggle;
            }
            
            RefreshInteractability();
            RefreshValues();
        }
        
        public override bool TryGoBack(out bool playSound) {
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }
        
        public unsafe void ToggleHudRule(Toggle rule) {
            var ruleName = rule.gameObject.name;
            CommandChangePowerupsHuds.PowerupsHuds effectValue = Enum.Parse<CommandChangePowerupsHuds.PowerupsHuds>(ruleName);
            var cmd = new CommandChangePowerupsHuds { EnabledChanges = effectValue, };
            var field = cmd.GetType().GetField(ruleName);
            if (field == null) return;
            field.SetValue(cmd, rule.isOn);
            QuantumGame game = QuantumRunner.DefaultGame;
            int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        public void RefreshValues() {
            var rules = matchSettings.rules;
            foreach (var toggle in _toggles) {
                if (rules.GetType().GetField(toggle.Key).GetValue(rules) is QBoolean ruleValue) {
                    toggle.Value.SetIsOnWithoutNotify(ruleValue);
                }
            }
        }
    }
}