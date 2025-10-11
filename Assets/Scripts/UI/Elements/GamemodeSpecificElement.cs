using Quantum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.Elements {
    public class GamemodeSpecificElement : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private List<AssetRef<GamemodeAsset>> gamemodes;
        private List<Selectable> selectables = new();

        public void Awake() {
            GetComponentsInChildren(selectables);
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
            var active = gamemodes.Contains(f.Global->Rules.Gamemode);
            foreach (var selectable in selectables) {
                selectable.interactable = active;
            }
        }

        private void OnGameStarted(CallbackGameStarted e) {
            Apply(e.Game);
        }

        private void OnRulesChanged(EventRulesChanged e) {
            Apply(e.Game);
        }
    }
}