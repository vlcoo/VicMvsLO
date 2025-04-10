using Quantum;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class CharacterChoosePromptSubmenu : PromptSubmenu {
        public bool success = true;
        public GameObject characterPreviews;
        public PreviewPlayerAnimator currentCharacterPreview;
        public Image characterImage;
        
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

        public override void Initialize() {
            QuantumEvent.Subscribe<EventPlayerDataChanged>(this, OnPlayerDataChanged);
        }

        public unsafe void CharacterSelected(CharacterAsset character) {
            var game = NetworkHandler.Runner.Game;
            var allCharacters = game.Configurations.Simulation.CharacterDatas;
            var selectedCharacter = allCharacters.IndexOf(chara => 
                chara.LegalEnglishName == character.LegalEnglishName);
            foreach (int slot in game.GetLocalPlayerSlots()) {
                game.SendCommand(slot, new CommandChangePlayerData {
                    EnabledChanges = CommandChangePlayerData.Changes.Character,
                    Character = (byte) selectedCharacter,
                });
            }

            characterImage.sprite = character.ReadySprite;
            if (currentCharacterPreview != null) currentCharacterPreview.SetVisible(false);
            currentCharacterPreview = GetCharacterPreview(character);
            currentCharacterPreview.SetSelected();
        }

        private PreviewPlayerAnimator GetCharacterPreview(CharacterAsset character) {
            for (int i = 0; i < characterPreviews.transform.childCount; i++) {
                var child = characterPreviews.transform.GetChild(i);
                if (child.gameObject.name.Contains(character.LegalEnglishName)) {
                    return child.GetComponent<PreviewPlayerAnimator>();
                }
            }

            return null;
        }
        
        //---Callbacks
        private unsafe void OnPlayerDataChanged(EventPlayerDataChanged e) {
            if (!e.Game.PlayerIsLocal(e.Player)) {
                return;
            }

            Frame f = e.Game.Frames.Predicted;
            SimulationConfig config = f.SimulationConfig;
            PlayerData* data = QuantumUtils.GetPlayerData(f, e.Player);
            CharacterAsset characterAsset = config.CharacterDatas[Mathf.Clamp(data->Character, 0, config.CharacterDatas.Length)];
            currentCharacterPreview = GetCharacterPreview(characterAsset);
            currentCharacterPreview.SetVisible(true);
        }
    }
}