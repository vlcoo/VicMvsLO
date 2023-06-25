using UnityEngine;
using TMPro;

namespace NSMB.Translation {

    [RequireComponent(typeof(TMP_Text))]
    public class TMP_Translatable : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private string key;
        [SerializeField] private TMP_Text text;

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

        private void OnLanguageChanged(TranslationManager tm) {
            text.text = tm.GetTranslation(key);
            text.isRightToLeftText = tm.RightToLeft;
        }
    }
}