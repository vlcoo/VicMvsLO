using System;
using UnityEngine;
using TMPro;

namespace NSMB.UI.MainMenu.Elements {
    [CreateAssetMenu(fileName = "MinMaxIntegerValidator", menuName = "ScriptableObjects/Input Validators/MinMaxIntegerValidator")]
    public class MinMaxIntegerValidator : TMP_InputValidator {
        public int min = 1, max = 99;

        public override char Validate(ref string text, ref int pos, char ch) {
            if (!int.TryParse(text + ch, out int number)) return '\0';
            if (number < min) {
                text = min.ToString();
                pos = text.Length;
                return '\0';
            }
            if (number > max) {
                text = max.ToString();
                pos = text.Length;
                return '\0';
            }
            text = text.Insert(pos, ch.ToString());
            pos++;
            return ch;
        }
    }
}
