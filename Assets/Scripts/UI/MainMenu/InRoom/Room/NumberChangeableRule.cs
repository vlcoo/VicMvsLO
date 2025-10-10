using NSMB.UI.Translation;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.InRoom {
    public class NumberChangeableRule : MonoBehaviour {
        //---Serialized Variables
        [SerializeField] protected int minValue = 1, maxValue = 99;
        [SerializeField] protected bool minimumValueIsOff;

        public Toggle toggle;
        public TMP_InputField inputField;
        
        public void Start() {
            QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
        }

        private unsafe void OnRulesChanged(EventRulesChanged e) {
            
        }
    
        private unsafe void OnGameStarted(CallbackGameStarted e) {
            
        }
        
        public void OnToggleChanged(bool how) {
            
        }

        public void OnInputFieldChanged(string value) {
            
        }

        private unsafe void SendCommand() {
            CommandChangeRules cmd = new CommandChangeRules {
                EnabledChanges = ruleType,
            };

            switch (ruleType) {
            case CommandChangeRules.Rules.StarsToWin:
                cmd.StarsToWin = (int) value;
                break;
            case CommandChangeRules.Rules.CoinsForPowerup:
                cmd.CoinsForPowerup = (int) value;
                break;
            case CommandChangeRules.Rules.Lives:
                cmd.Lives = (int) value;
                break;
            case CommandChangeRules.Rules.TimerMinutes:
                cmd.TimerMinutes = (int) value;
                break;
            }

            QuantumGame game = QuantumRunner.DefaultGame;
            int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            game.SendCommand(slot, cmd);
        }

        protected void UpdateLabel() {
            TranslationManager tm = GlobalController.Instance.translationManager;
            if (value is int intValue) {
                label.text = labelPrefix + ((minimumValueIsOff && intValue == minValue) ? tm.GetTranslation("ui.generic.off") : intValue);
            }
        }
    }
}