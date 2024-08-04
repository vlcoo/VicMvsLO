using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "TextSubmitValidator", menuName = "ScriptableObjects/TextSubmitValidator")]
public class TextSubmitValidator : TMP_InputValidator
{
    public override char Validate(ref string text, ref int pos, char ch)
    {
#if UNITY_ANDROID
        return ch;
#endif
        if (ch == '\n' || ch == '\xB')
        {
            //submit if enter pressed
            MainMenuManager.Instance.SendChat();
            return '\0';
        }

        if (text.Length >= 128)
            return '\0';

        // Ensure pos is within the valid range
        pos = Mathf.Clamp(pos, 0, text.Length);
        text = text.Insert(pos, ch.ToString());
        pos++;
        return ch;
    }
}