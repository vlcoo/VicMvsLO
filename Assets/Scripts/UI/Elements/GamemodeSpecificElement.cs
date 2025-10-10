using Quantum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.Elements {
    public class GamemodeSpecificElement : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private List<AssetRef<GamemodeAsset>> gamemodes;
        private Selectable selectable;

        public void Awake() {
            selectable = GetComponent<Selectable>();
            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
            QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);

            QuantumGame game;
            if ((game = QuantumRunner.DefaultGame) != null) {
                Apply(game);
            }
        }

        public unsafe void Apply(QuantumGame game) {
            Frame f = game.Frames.Predicted;
            // gameObject.SetActive(gamemodes.Contains(f.Global->Rules.Gamemode));
            selectable.interactable = gamemodes.Contains(f.Global->Rules.Gamemode);
        }

        private void OnGameStarted(CallbackGameStarted e) {
            Apply(e.Game);
        }

        private void OnRulesChanged(EventRulesChanged e) {
            Apply(e.Game);
        }
    }
}