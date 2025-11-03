using NSMB.Networking;
using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class HudPromptSubmenu : PromptSubmenu {
        public bool success = true;
        private readonly Dictionary<string, Toggle> _toggles = new();
        [HideInInspector] public GameRules rules;
        public TMP_Dropdown teamTargetDropdown;
        
        public override void Initialize() {
            base.Initialize();
            var toggles = new List<Toggle>();
            GetComponentsInChildren(true, toggles);
            foreach (var toggle in toggles) {
                if (toggle.gameObject.name == "Item") continue;
                _toggles[toggle.gameObject.name] = toggle;
            }
        }
        
        public void Start() {
            QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
            
            NetworkHandler.Client.AddCallbackTarget(this);
        }
        
        public override void Hide(SubmenuHideReason hideReason) {
            base.Hide(hideReason);
            NetworkHandler.Client.RemoveCallbackTarget(this);
        }
        
        private unsafe void OnGameStarted(CallbackGameStarted e) {
            rules = e.Game.Frames.Predicted.Global->Rules;
            RefreshValues();
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
        
        public unsafe void ToggleHudRule(Toggle rule) {
            var ruleName = rule.gameObject.name;
            CommandChangePowerupsHuds.PowerupsHuds effectValue = Enum.Parse<CommandChangePowerupsHuds.PowerupsHuds>(ruleName);
            var cmd = new CommandChangePowerupsHuds();
            var field = cmd.GetType().GetField(ruleName);
            if (field == null) return;
            field.SetValue(cmd, rule.isOn);
            QuantumGame game = QuantumRunner.DefaultGame;
            int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        public unsafe void ChangeTeamTargetRule(int value) {
            var cmd = new CommandChangePowerupsHuds();
            cmd.HTeamTarget = value;
            QuantumGame game = QuantumRunner.DefaultGame;
            int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        private unsafe void OnRulesChanged(EventRulesChanged e) {
            rules = e.Game.Frames.Predicted.Global->Rules;
            RefreshValues();
        }

        public void RefreshValues() {
            foreach (var toggle in _toggles) {
                if (rules.GetType().GetField(toggle.Key).GetValue(rules) is bool ruleValue) toggle.Value.SetIsOnWithoutNotify(ruleValue);
            }
            teamTargetDropdown.SetValueWithoutNotify(rules.HTeamTarget);
        }
    }
}