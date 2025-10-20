using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace NSMB.UI.MainMenu.Submenus.InRoom {
    public class PaletteButton : MonoBehaviour {
        //---Public Variables
        public byte Index;
        public CharacterPalette Palette {
            get => _palette;
            set {
                _palette = value;
                if (_palette == null) {
                    if (shirt && overalls) {
                        Destroy(shirt.gameObject);
                        Destroy(overalls.gameObject);
                    }
                    
                    return;
                }
                shirt.color = _palette.ShirtColor.AsColor;
                overalls.color = _palette.OverallsColor.AsColor;
            }
        }

        //---Serialized Variables
        [SerializeField] private Image shirt, overalls;
        
        private CharacterPalette _palette;
    }
}
