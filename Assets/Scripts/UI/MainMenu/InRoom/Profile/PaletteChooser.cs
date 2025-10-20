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
        private readonly List<Button> buttons = new();
        private GameObject blocker;
        private CharacterAsset character;
        private int selected;
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
            foreach (var cb in paletteButtons) {
                Destroy(cb.gameObject);
            }
            paletteButtons.Clear();
            buttons.Clear();
            foreach (var palette in character.Palettes) {
                var newButton = Instantiate(template, template.transform.parent);
                var cb = newButton.GetComponent<PaletteButton>();
                paletteButtons.Add(cb);
                cb.Palette = palette;

                Button b = newButton.GetComponent<Button>();
                // newButton.name = palette ? palette.name : "Reset";
                // if (!palette) {
                //     b.image.sprite = clearSprite;
                // }

                newButton.SetActive(true);
                buttons.Add(b);
            }
        }

        public void ChangeCharacter(CharacterAsset data) {
            if (character == data) return;
            character = data;
            PopulatePaletteList();
        }

        // public void ChangePaletteButton(int index) {
        //     selected = index;
        //     AssetRef<PaletteSet>[] palettes = GlobalController.Instance.config.Palettes;
        //     PaletteSet palette = null;
        //
        //     if (index >= 0 && index < palettes.Length) {
        //         palette = QuantumUnityDB.GetGlobalAsset(palettes[index]);
        //     }
        //
        //     if (palette) {
        //         overallsImage.enabled = true;
        //         overallsImage.color = palette.GetPaletteForCharacter(character).OverallsColor.AsColor;
        //         shirtImage.enabled = true;
        //         shirtImage.color = palette.GetPaletteForCharacter(character).ShirtColor.AsColor;
        //         baseImage.sprite = baseSprite;
        //     } else {
        //         overallsImage.enabled = false;
        //         shirtImage.enabled = false;
        //         baseImage.sprite = clearSprite;
        //     }
        // }

        public void SelectPalette(Button button) {
            int newIndex = buttons.IndexOf(button);
            QuantumGame game = QuantumRunner.DefaultGame;
            foreach (var slot in game.GetLocalPlayerSlots()) {
                game.SendCommand(slot, new CommandChangePlayerData { 
                    EnabledChanges = CommandChangePlayerData.Changes.Palette,
                    Palette = (byte) newIndex,
                });
            }
            
            Close(false);
            selected = newIndex;
            Settings.Instance.generalPalette = selected;
            Settings.Instance.SaveSettings();
            canvas.PlayConfirmSound();
        }

        public void Open() {
            Initialize();

            blocker = Instantiate(blockerTemplate, canvas.transform);
            blocker.SetActive(true);
            content.SetActive(true);
            canvas.PlayCursorSound();

            EventSystem.current.SetSelectedGameObject(buttons[selected].gameObject);
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
