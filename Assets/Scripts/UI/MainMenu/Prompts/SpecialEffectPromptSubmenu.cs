using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class SpecialEffectPromptSubmenu : PromptSubmenu {
        public bool success = true;
        public CounterTip counterTip;
        private readonly Dictionary<string, Toggle> _toggles = new();
        [HideInInspector] public GameRules rules;
        
        public override void Initialize() {
            base.Initialize();
            var toggles = new List<Toggle>();
            GetComponentsInChildren(true, toggles);
            foreach (var toggle in toggles) {
                _toggles[toggle.gameObject.name] = toggle;
            }
        }

        public void Start() {
            QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
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

        public unsafe void ToggleSpecialEffect(Toggle effect) {
            var effectName = effect.gameObject.name;
            CommandChangeRules.Rules effectValue = Enum.Parse<CommandChangeRules.Rules>(effectName);
            var cmd = new CommandChangeRules {
                EnabledChanges = effectValue,
            };
            var field = cmd.GetType().GetField(effectName);
            if (field == null) return;
            Debug.Log(rules.SNoCoins);
            field.SetValue(cmd, effect.isOn);
            QuantumGame game = QuantumRunner.DefaultGame;
            int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        private unsafe void OnRulesChanged(EventRulesChanged e) {
            rules = e.Game.Frames.Predicted.Global->Rules;
            foreach (var toggle in _toggles) {
                if (rules.GetType().GetField(toggle.Key).GetValue(rules) is bool ruleValue) toggle.Value.SetIsOnWithoutNotify(ruleValue);
            }
            counterTip.SetCount(_toggles.Count(toggle => toggle.Value.isOn));
        }
        
        public void OnGameDestroyed(CallbackGameDestroyed e) {
            // cleanup
            counterTip.SetCount(0);
        }
    }
}