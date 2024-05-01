using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Fusion;
using NSMB.Extensions;
using NSMB.Translation;

public class ChatMessage : MonoBehaviour {

    //---Public Variables
    [NonSerialized] public ChatMessageData data;

    //---Serialized Variables
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private Image image;

    //---Private Variables
    private static readonly Color SystemMessageImageColor = new Color(0.4f, 0.4f, 0.4f, 0.94f);

    public void OnValidate() {
        this.SetIfNull(ref chatText);
        this.SetIfNull(ref image);
    }

    public void OnDestroy() {
        TranslationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(TranslationManager tm) {
        chatText.text =
            "<line-height=16><font=\"DSFont\"><i> INFO</i></font>\n" +
            $"</line-height>{tm.GetTranslationWithReplacements(data.message, data.replacements)}";
        //chatText.isRightToLeftText = tm.RightToLeft;

        chatText.horizontalAlignment = tm.RightToLeft ? HorizontalAlignmentOptions.Right : HorizontalAlignmentOptions.Left;
    }

    public void Initialize(ChatMessageData data) {
        this.data = data;
        // chatText.color = data.color;

        if (data.isSystemMessage) {
            TranslationManager.OnLanguageChanged += OnLanguageChanged;
            OnLanguageChanged(GlobalController.Instance.translationManager);
        } else {
            // chatText.richText = false;
            data.player.TryGetPlayerData(out PlayerData playerData);
            chatText.text =
                $"<line-height=16><font=\"DSFont\"><i> {playerData.GetNickname()}</i></font>\n" +
                $"</line-height><noparse>{data.message.Replace("</noparse>", "")}</noparse>";
        }

        UpdateVisibleState(!Settings.Instance.GeneralDisableChat);
        UpdatePlayerColor();
    }

    public void UpdateVisibleState(bool enable) {
        gameObject.SetActive(data.isSystemMessage || enable);
    }

    public void UpdatePlayerColor() {
        if (data.isSystemMessage) {
            image.color = SystemMessageImageColor;
            chatText.color = Color.white;
        }

        // image.color = Utils.GetPlayerColor(data.player, 0.2f);
    }

    public class ChatMessageData {
        public PlayerRef player;
        public Color color;
        public bool isSystemMessage;
        public string message;
        public string[] replacements;
    }
}
