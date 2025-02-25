using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class SpecialEffectPromptSubmenu : PromptSubmenu {
        public bool success = true;
        public CounterTip counterTip;
        private readonly List<SpecialToggleChangeableRule> _toggles = new();
        
        public override void Initialize() {
            base.Initialize();
            GetComponentsInChildren(true, _toggles);
            foreach (var rule in _toggles) {
                rule.Initialize();
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
            if (_toggles.Any(r => r.Editing)) {
                foreach (var rule in _toggles) {
                    rule.Editing = false;
                }
                playSound = true;
                return false;
            }
            
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }

        private void OnRulesChanged(EventRulesChanged e) {
            counterTip.SetCount(_toggles.Count(toggle => (bool) toggle.Value));
        }
        
        public void OnGameDestroyed(CallbackGameDestroyed e) {
            // cleanup
            counterTip.SetCount(0);
        }
    }
}