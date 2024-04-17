using UnityEngine;
using TMPro;

namespace NSMB.Translation {

    [RequireComponent(typeof(TMP_Text))]
    public class TMP_Translatable : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private string key;
        [SerializeField] private TMP_Text text;

        //---Private Variables
        private HorizontalAlignmentOptions originalTextAlignment;

        public void Awake() {
            originalTextAlignment = text.horizontalAlignment;
        }

        public void OnValidate() {
            if (!text) text = GetComponent<TMP_Text>();
        }

        public void OnEnable() {
            TranslationManager.OnLanguageChanged += OnLanguageChanged;
            OnLanguageChanged(GlobalController.Instance.translationManager);
        }

        public void OnDisable() {
            TranslationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        // TODO vcmi: Don't forget translation server for labels is commented out.
        private void OnLanguageChanged(TranslationManager tm) {
            // text.text = tm.GetTranslation(key);
            //
            // if (originalTextAlignment == HorizontalAlignmentOptions.Left) {
            //     text.horizontalAlignment = tm.RightToLeft ? HorizontalAlignmentOptions.Right : HorizontalAlignmentOptions.Left;
            // } else if (originalTextAlignment == HorizontalAlignmentOptions.Right) {
            //     text.horizontalAlignment = tm.RightToLeft ? HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Right;
            // }
        }
    }
}
