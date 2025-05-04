using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus {
    public class ProfilePanel : InRoomSubmenuPanel {

        //---Properties
        public override bool IsInSubmenu => teamChooser.content.activeSelf || paletteChooser.content.activeSelf;

        //---Serialized Variables
        [SerializeField] private Image paletteBackground, characterImage;
        [SerializeField] private PaletteChooser paletteChooser;
        [SerializeField] private TeamChooser teamChooser;
        [SerializeField] private SpriteChangingToggle spectateToggle;

        //---Private Variables
        private int currentCharacterIndex;

        public override void Initialize() {
            paletteChooser.Initialize();
            teamChooser.Initialize();

            QuantumEvent.Subscribe<EventPlayerDataChanged>(this, OnPlayerDataChanged);
        }

        public override bool TryGoBack(out bool playSound) {
            if (teamChooser.content.activeSelf) {
                teamChooser.Close(true);
                playSound = false;
                return false;
            }

            if (paletteChooser.content.activeSelf) {
                paletteChooser.Close(true);
                playSound = false;
                return false;
            }

            return base.TryGoBack(out playSound);
        }

        public void OnSpectateToggled() {
            QuantumGame game = NetworkHandler.Runner.Game;
            foreach (var slot in game.GetLocalPlayerSlots()) {
                game.SendCommand(slot, new CommandChangePlayerData {
                    EnabledChanges = CommandChangePlayerData.Changes.Spectating,
                    Spectating = spectateToggle.isOn,
                });
            }
            menu.Canvas.PlayConfirmSound();
        }

        private void SetCharacterButtonState(Frame f, int index, bool sound) {
            bool changed = currentCharacterIndex != index;
            currentCharacterIndex = index;

            SimulationConfig config = f.SimulationConfig;
            CharacterAsset characterAsset = f.FindAsset(config.CharacterDatas[Mathf.Clamp(index, 0, config.CharacterDatas.Length)]);
            paletteChooser.ChangeCharacter(characterAsset);
            characterImage.sprite = characterAsset.ReadySprite;

            if (changed) {
                Settings.Instance.generalCharacter = index;
                Settings.Instance.SaveSettings();
            }

            if (sound && changed) {
                menu.Canvas.PlaySound(SoundEffect.Player_Voice_Selected, characterAsset);
            }
        }

        private void SetPaletteButtonState(int index) {
            paletteChooser.ChangePaletteButton(index);
        }

        //---Callbacks
        private unsafe void OnPlayerDataChanged(EventPlayerDataChanged e) {
            if (!e.Game.PlayerIsLocal(e.Player)) {
                return;
            }

            Frame f = e.Game.Frames.Predicted;

            // Set character button to the correct state
            PlayerData* data = QuantumUtils.GetPlayerData(f, e.Player);
            SetPaletteButtonState(data->Palette);
            SetCharacterButtonState(f, data->Character, false);
            spectateToggle.SetIsOnWithoutNotify(data->ManualSpectator);
        }
    }
}
