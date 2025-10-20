using NSMB.UI.Elements;
using Quantum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Navigation = UnityEngine.UI.Navigation;

namespace NSMB.UI.MainMenu.Submenus.InRoom {
    public class PaletteChooser : MonoBehaviour, KeepChildInFocus.IFocusIgnore {

        //---Serialized Variables
        // [SerializeField] private SimulationConfig config;
        [SerializeField] private MainMenuCanvas canvas;
        [SerializeField] private GameObject template, blockerTemplate;
        [SerializeField] public GameObject content;
        [SerializeField] private Sprite clearSprite, baseSprite;
        [SerializeField] private CharacterAsset defaultCharacter;
        [SerializeField] private GameObject selectOnClose;

        [SerializeField] private Image overallsImage, shirtImage, baseImage;

        //---Private Variables
        private readonly List<PaletteButton> paletteButtons = new();
        private GameObject blocker;
        private CharacterAsset character;
        private bool initialized;

        public void OnDisable() {
            Close(false);
        }

        public unsafe void Initialize() {
            if (initialized) return;
            character = defaultCharacter;
            PopulatePaletteList();
            initialized = true;
        }

        public void PopulatePaletteList() {
            foreach (var b in paletteButtons) {
                Destroy(b.gameObject);
            }
            paletteButtons.Clear();
            
            var index = 0;
            foreach (var palette in character.Palettes) {
                var newButton = Instantiate(template, template.transform.parent);
                var cb = newButton.GetComponent<PaletteButton>();
                paletteButtons.Add(cb);
                cb.Palette = palette;
                newButton.SetActive(true);
                cb.Index = (byte) index;
                index++;
            }
        }

        public void ChangeCharacter(CharacterAsset data) {
            if (character == data) return;
            character = data;
            PopulatePaletteList();
        }

        public void SelectPalette(PaletteButton button) {
            QuantumGame game = QuantumRunner.DefaultGame;
            foreach (var slot in game.GetLocalPlayerSlots()) {
                game.SendCommand(slot, new CommandChangePlayerData { 
                    EnabledChanges = CommandChangePlayerData.Changes.Palette,
                    Palette = button.Index,
                });
            }
            
            Close(false);
            Settings.Instance.generalPalette = button.Index;
            Settings.Instance.SaveSettings();
            canvas.PlayConfirmSound();
        }

        public void Open() {
            Initialize();

            blocker = Instantiate(blockerTemplate, canvas.transform);
            blocker.SetActive(true);
            content.SetActive(true);
            canvas.PlayCursorSound();
        }

        public void Close(bool playSound) {
            Destroy(blocker);
            EventSystem.current.SetSelectedGameObject(selectOnClose);
            content.SetActive(false);

            if (playSound) {
                canvas.PlaySound(SoundEffect.UI_Back);
            }
        }
    }
}
