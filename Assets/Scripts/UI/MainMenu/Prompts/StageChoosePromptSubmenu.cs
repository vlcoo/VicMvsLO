using NSMB.Networking;
using Quantum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class StageChoosePromptSubmenu : PromptSubmenu {
        public bool success = true;
        private List<Button> stageButtons = new();
        public MatchSettings matchSettings;

        public override void OnValidate() {
            
        }
        
        public override void Show(bool first) {
            base.Show(first);
            success = false;
            RefreshInteractability();
        }
        
        public override bool TryGoBack(out bool playSound) {
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }
        
        private void RefreshInteractability() {
            if (stageButtons.Count == 0) {
                foreach (var button in GetComponentsInChildren<Button>()) {
                    if (button.interactable && button.gameObject != BackButton) stageButtons.Add(button);
                }
            }
            
            foreach (var button in stageButtons) {
                button.interactable = matchSettings.isHost;
            }
        }
        
        public unsafe void StageSelected(VersusStageData stage) {
            success = true;
            QuantumGame game = NetworkHandler.Game;
            int index = game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host);
            var allStages = game.Configurations.Simulation.AllStages;
            var selectedStage = allStages.FirstOrDefault(map => 
                ((VersusStageData)QuantumUnityDB.GetGlobalAsset(game.Frames.Predicted.FindAsset(map).UserAsset)).LegalEnglishName == stage.LegalEnglishName);
            
            if (selectedStage == null) {
                Debug.LogError("Stage not found in allStages");
                // Canvas.GoBack();
                return;
            }
            
            var cmd = new CommandChangeRules {
                EnabledChanges = CommandChangeRules.Rules.Stage,
                Stage = selectedStage,
            };

            // var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
            // game.SendCommand(slot, cmd);
            if (index != -1) {
                int slot = game.GetLocalPlayerSlots()[index];
                game.SendCommand(slot, cmd);
                // Canvas.PlayConfirmSound();
            } else {
                Canvas.PlaySound(SoundEffect.UI_Error);
            }
            Canvas.GoBack();
        }
    }
}