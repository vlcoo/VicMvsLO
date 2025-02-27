using Quantum;
using System.Linq;
using UnityEngine;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class StageChoosePromptSubmenu : PromptSubmenu {
        public bool success = true;
        
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
        
        public unsafe void StageSelected(VersusStageData stage) {
            success = true;
            QuantumGame game = NetworkHandler.Game;
            var allStages = game.Configurations.Simulation.AllStages;
            var selectedStage = allStages.FirstOrDefault(map => 
                ((VersusStageData)QuantumUnityDB.GetGlobalAsset(map.UserAsset)).LegalEnglishName == stage.LegalEnglishName);
            
            if (selectedStage == null) {
                Debug.LogError("Stage not found in allStages");
                Canvas.GoBack();
                return;
            }
            
            var cmd = new CommandChangeRules {
                EnabledChanges = CommandChangeRules.Rules.Stage,
                Stage = (AssetRef<Map>)selectedStage,
            };

            var slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(QuantumUtils.GetHostPlayer(game.Frames.Predicted, out _))];
            game.SendCommand(slot, cmd);
            Canvas.GoBack();
        }
    }
}