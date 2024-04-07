using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Fusion;
using NSMB.Translation;

namespace NSMB.UI.MainMenu {
    public class RoomSettingsCallbacks : MonoBehaviour {

        //---Serailized Variables
        [SerializeField] private TMP_Dropdown levelDropdown;
        [SerializeField] private TMP_InputField starsInputField, coinsInputField, livesInputField, timerInputField;
        [SerializeField] private TMP_Text playersCount, roomIdText, roomIdToggleButtonText;
        [SerializeField] private Slider playersSlider;
        [SerializeField] private Toggle privateEnabledToggle, timerEnabledToggle, livesEnabledToggle, drawEnabledToggle, teamsEnabledToggle, customPowerupsEnabledToggle, starsEnabledToggle, coinsEnabledToggle;
        [SerializeField] private TeamChooser teamSelectorButton;

        //---Properties
        private NetworkRunner Runner => NetworkHandler.Runner;
        private SessionData Room => SessionData.Instance;

        //---Private Variables
        private bool isRoomCodeVisible;

        public void OnEnable() {
            TranslationManager.OnLanguageChanged += OnLanguageChanged;
        }

        public void OnDisable() {
            TranslationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        public void UpdateAllSettings(SessionData roomData, bool level) {
            if (!roomData.Object) {
                return;
            }

            ChangePrivate(roomData.PrivateRoom);
            ChangeMaxPlayers(roomData.MaxPlayers);
            ChangeLevelIndex(roomData.Level, level);
            ChangeStarRequirement(roomData.StarRequirement);
            ChangeCoinRequirement(roomData.CoinRequirement);
            ChangeTeams(roomData.Teams);
            ChangeLives(roomData.Lives);
            ChangeTime(roomData.Timer);
            ChangeDrawOnTimeUp(roomData.DrawOnTimeUp);
            ChangeCustomPowerups(roomData.CustomPowerups);
            SetRoomIdVisibility(isRoomCodeVisible);

            if (MainMenuManager.Instance) {
                MainMenuManager.Instance.playerList.UpdateAllPlayerEntries();
                MainMenuManager.Instance.UpdateStartGameButton();
            }

            if (Runner.IsServer && Runner.Tick != 0) {
                Runner.PushHostMigrationSnapshot();
            }
        }

        #region Level Index
        public void SetLevelIndex() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int oldValue = Room.Level;
            int newValue = levelDropdown.value;
            if (newValue == oldValue || newValue < 0) {
                ChangeLevelIndex(oldValue, false);
                return;
            }

            Room.SetLevel((byte) newValue);
        }
        private void ChangeLevelIndex(int index, bool changed) {
            levelDropdown.SetValueWithoutNotify(index);
            if (changed && MainMenuManager.Instance is MainMenuManager mm) {
                ChatManager.Instance.AddSystemMessage("ui.inroom.chat.server.map", "map", mm.maps[index].translationKey);
                mm.PreviewLevel(index);
            }
        }
        #endregion

        #region Stars
        public void SetStarRequirement() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int oldValue = Room.StarRequirement;
            if (!int.TryParse(starsInputField.text, out int newValue)) {
                return;
            }

            newValue = Mathf.Clamp(newValue, 1, 25);

            if (oldValue == newValue) {
                ChangeStarRequirement(oldValue);
                return;
            }

            Room.SetStarRequirement((sbyte) newValue);
        }

        public void EnableStars() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int newValue = starsEnabledToggle.isOn ? int.Parse(starsInputField.text) : 0;

            Room.SetStarRequirement((sbyte) newValue);
        }

        private void ChangeStarRequirement(int stars) {
            bool enabled = stars > 0;
            starsEnabledToggle.SetIsOnWithoutNotify(enabled);
            starsInputField.interactable = enabled;

            if (enabled) {
                starsInputField.SetTextWithoutNotify(stars.ToString());
            }
        }
        #endregion

        #region Coins
        public void SetCoinRequirement() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int oldValue = Room.CoinRequirement;
            if (!int.TryParse(coinsInputField.text, out int newValue)) {
                return;
            }

            newValue = Mathf.Clamp(newValue, 3, 25);

            if (newValue == oldValue) {
                ChangeCoinRequirement(oldValue);
                return;
            }

            Room.SetCoinRequirement((byte) newValue);
        }

        public void EnableCoins() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int newValue = coinsEnabledToggle.isOn ? int.Parse(coinsInputField.text) : 0;

            Room.SetCoinRequirement((byte) newValue);
        }

        private void ChangeCoinRequirement(int coins) {
            bool enabled = coins > 0;
            coinsEnabledToggle.SetIsOnWithoutNotify(enabled);
            coinsInputField.interactable = enabled;

            if (enabled) {
                coinsInputField.SetTextWithoutNotify(coins.ToString());
            }
        }
        #endregion

        #region Lives
        public void SetLives() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int oldValue = Room.Lives;
            if (!int.TryParse(livesInputField.text, out int newValue)) {
                return;
            }

            newValue = Mathf.Clamp(newValue, 1, 25);

            if (newValue == oldValue) {
                ChangeLives(oldValue);
                return;
            }

            Room.SetLives((byte) newValue);
        }

        public void EnableLives() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int newValue = livesEnabledToggle.isOn ? int.Parse(livesInputField.text) : 0;

            Room.SetLives((byte) newValue);
        }

        private void ChangeLives(int lives) {
            bool enabled = lives > 0;
            livesEnabledToggle.SetIsOnWithoutNotify(enabled);
            livesInputField.interactable = enabled;

            if (enabled) {
                livesInputField.SetTextWithoutNotify(lives.ToString());
            }
        }
        #endregion

        #region Timer
        public void SetTime() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int oldValue = Room.Timer;
            if (!int.TryParse(timerInputField.text.Split(':')[0], out int newValue)) {
                return;
            }

            newValue = Mathf.Clamp(newValue, 1, 99);

            if (newValue == oldValue) {
                ChangeTime(oldValue);
                return;
            }

            Room.SetTimer((byte) newValue);
        }

        public void EnableTime() {
            if (!Room.HasStateAuthority) {
                return;
            }

            if (!int.TryParse(timerInputField.text.Split(':')[0], out int newValue)) {
                return;
            }

            newValue = timerEnabledToggle.isOn ? Mathf.Clamp(newValue, 1, 99) : 0;

            Room.SetTimer((byte) newValue);
        }

        private void ChangeTime(int time) {
            timerEnabledToggle.SetIsOnWithoutNotify(time > 0);
            timerInputField.interactable = time > 0;
            drawEnabledToggle.interactable = time > 0;

            if (time <= 0) {
                return;
            }

            timerInputField.SetTextWithoutNotify($"{time}:00");
        }
        #endregion

        #region DrawOnTimeUp
        public void SetDrawOnTimeUp() {
            if (!Room.HasStateAuthority) {
                return;
            }

            bool newValue = drawEnabledToggle.isOn;

            Room.SetDrawOnTimeUp(newValue);
        }
        private void ChangeDrawOnTimeUp(bool value) {
            drawEnabledToggle.SetIsOnWithoutNotify(value);
        }
        #endregion

        #region Teams
        public void SetTeams() {
            if (!Room.HasStateAuthority) {
                return;
            }

            bool newValue = teamsEnabledToggle.isOn;

            Room.SetTeams(newValue);

            if (MainMenuManager.Instance) {
                MainMenuManager.Instance.playerList.UpdateAllPlayerEntries();
            }
        }
        private void ChangeTeams(bool value) {
            teamsEnabledToggle.SetIsOnWithoutNotify(value);
            teamSelectorButton.SetEnabled(value);

            if (!teamsEnabledToggle.isOn && value) {
                MainMenuManager.Instance.playerList.UpdateAllPlayerEntries();
            }
        }
        #endregion

        #region Custom Powerups
        public void SetCustomPowerups() {
            if (!Room.HasStateAuthority) {
                return;
            }

            bool newValue = customPowerupsEnabledToggle.isOn;

            Room.SetCustomPowerups(newValue);
        }
        private void ChangeCustomPowerups(bool value) {
            customPowerupsEnabledToggle.SetIsOnWithoutNotify(value);
        }
        #endregion

        #region Players
        public void SetMaxPlayers() {
            if (!Room.HasStateAuthority) {
                return;
            }

            int oldValue = Room.MaxPlayers;
            int newValue = (int) playersSlider.value;

            newValue = Mathf.Clamp(newValue, Mathf.Max(2, Runner.SessionInfo.PlayerCount), 10);

            if (newValue == oldValue) {
                ChangeMaxPlayers(oldValue);
                return;
            }

            Room.SetMaxPlayers((byte) newValue);
            ChangeMaxPlayers(newValue);
        }
        private void ChangeMaxPlayers(int value) {
            playersSlider.SetValueWithoutNotify(value);
            playersCount.text = value.ToString();
        }
        #endregion

        #region Win Counter
        public void ClearWinCounters() {
            if (!Room.HasStateAuthority) {
                return;
            }

            foreach ((_, PlayerData data) in Room.PlayerDatas) {
                data.Wins = 0;
            }
        }

        #endregion

        #region Private
        public void SetPrivate() {
            if (!Room.HasStateAuthority) {
                return;
            }

            bool newValue = privateEnabledToggle.isOn;

            Room.SetPrivateRoom(newValue);
        }
        private void ChangePrivate(bool value) {
            privateEnabledToggle.SetIsOnWithoutNotify(value);
        }
        public void CopyRoomCode() {
            TextEditor te = new() {
                text = Runner.SessionInfo.Name
            };
            te.SelectAll();
            te.Copy();
        }
        #endregion

        #region Room ID
        public void ToggleRoomIdVisibility() {
            SetRoomIdVisibility(!isRoomCodeVisible);
        }

        public void SetRoomIdVisibility(bool newValue) {
            isRoomCodeVisible = newValue;
            roomIdToggleButtonText.text = GlobalController.Instance.translationManager.GetTranslation(isRoomCodeVisible ? "ui.generic.hide" : "ui.generic.show");
            roomIdText.text = GlobalController.Instance.translationManager.GetTranslationWithReplacements("ui.inroom.settings.room.roomid", "id", isRoomCodeVisible ? Runner.SessionInfo.Name : "ui.inroom.settings.room.roomidhidden");
        }
        #endregion

        //---Callbacks
        private void OnLanguageChanged(TranslationManager tm) {
            SetRoomIdVisibility(isRoomCodeVisible);
            roomIdText.horizontalAlignment = tm.RightToLeft ? HorizontalAlignmentOptions.Right : HorizontalAlignmentOptions.Left;
        }
    }
}
