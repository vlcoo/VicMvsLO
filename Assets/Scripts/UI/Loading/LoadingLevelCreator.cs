using UnityEngine;
using TMPro;
using Quantum;

namespace NSMB.UI.Loading {

    public class LoadingLevelCreator : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private TMP_Text text;
        [SerializeField] private FieldType type;

        public void OnEnable() {
            string value = GetValueFromField();
            if (string.IsNullOrEmpty(value)) {
                text.text = "";
                return;
            }

            // No need to worry about language changes in this state...
            // or else...?
            text.text = $"Level designed by <i>{value}</i>";
        }

        private string GetValueFromField() {
            QuantumGame game = QuantumRunner.DefaultGame;
            if (game == null) {
                return "";
            }

            Frame f = game.Frames.Predicted;
            if (f == null || !f.TryFindAsset(f.Map.UserAsset, out VersusStageData stage)) {
                return "";
            }
            var shouldShow = !string.IsNullOrEmpty(stage.StageAuthor) || !string.IsNullOrEmpty(stage.MusicComposer);
            if (!shouldShow) return "";

            return type switch {
                FieldType.Author => stage.StageAuthor,
                FieldType.Composer => stage.MusicComposer,
                _ => ""
            };
        }

        public enum FieldType {
            Author,
            Composer,
        }
    }
}
