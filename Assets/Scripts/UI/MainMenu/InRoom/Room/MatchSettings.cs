using Quantum;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class MatchSettings : MonoBehaviour {
    public Toggle starsToggle, coinsToggle, livesToggle, timerToggle, teamsToggle, scoreToggle;
    public TMP_InputField lapsInput, starsInput, coinsInput, livesInput, timerInput;
    [HideInInspector] public GameRules rules;
    [HideInInspector] public bool isHost;
    public Button btnGamemode;
    public Action OnRulesChangedCallback;
    
    public void Start() {
        QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
        QuantumEvent.Subscribe<EventHostChanged>(this, OnHostChanged);
        QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
    }

    private unsafe void OnRulesChanged(EventRulesChanged e) {
        rules = e.Game.Frames.Predicted.Global->Rules;
        RefreshValues();
        OnRulesChangedCallback?.Invoke();
    }
    
    private unsafe void OnGameStarted(CallbackGameStarted e) {
        isHost = false;
        rules = e.Game.Frames.Predicted.Global->Rules;
        RefreshValues();
    }
    
    private void OnHostChanged(EventHostChanged e) {
        isHost = e.Game.PlayerIsLocal(e.NewHost);
        RefreshValues();
    }

    public void OnInputEndEdit() {
        RefreshValues();    // this will reset any empty fields
    }

    public void RefreshValues() {
        var isCampaignMap = false;
        if (QuantumUnityDB.TryGetGlobalAsset(rules.Stage, out Map map)
            && QuantumUnityDB.TryGetGlobalAsset(map.UserAsset, out VersusStageData stage))
            isCampaignMap = stage.IsCampaignMap;
        
        lapsInput.interactable = isCampaignMap && isHost;
        if (rules.Laps > 0 || lapsInput.text.Length == 0) lapsInput.SetTextWithoutNotify(rules.Laps.ToString());
        starsToggle.interactable = isHost;
        starsToggle.SetIsOnWithoutNotify(rules.StarsToWin > 0);
        starsInput.interactable = rules.StarsToWin > 0 && isHost;
        if (rules.StarsToWin > 0 || starsInput.text.Length == 0) starsInput.SetTextWithoutNotify(rules.StarsToWin.ToString());
        coinsToggle.interactable = isHost;
        coinsToggle.SetIsOnWithoutNotify(rules.CoinsForPowerup > 0);
        coinsInput.interactable = rules.CoinsForPowerup > 0 && isHost;
        if (rules.CoinsForPowerup > 0 || coinsInput.text.Length == 0) coinsInput.SetTextWithoutNotify(rules.CoinsForPowerup.ToString());
        livesToggle.interactable = isHost;
        livesToggle.SetIsOnWithoutNotify(rules.Lives > 0);
        livesInput.interactable = rules.Lives > 0 && isHost;
        if (rules.Lives > 0 || livesInput.text.Length == 0) livesInput.SetTextWithoutNotify(rules.Lives.ToString());
        timerToggle.interactable = isHost;
        timerToggle.SetIsOnWithoutNotify(rules.TimerSeconds > 0);
        timerInput.interactable = rules.TimerSeconds > 0 && isHost;
        if (rules.TimerSeconds > 0 || timerInput.text.Length == 0) timerInput.SetTextWithoutNotify(rules.TimerSeconds.ToString());
        teamsToggle.interactable = isHost;
        teamsToggle.SetIsOnWithoutNotify(rules.TeamsEnabled);
        scoreToggle.interactable = isHost;
        scoreToggle.SetIsOnWithoutNotify(rules.ScoreEnabled);
        btnGamemode.interactable = isHost;
    }

    public unsafe void ChangeRuleValue() {
        if (lapsInput.text.Length == 0 || starsInput.text.Length == 0 || coinsInput.text.Length == 0
            || livesInput.text.Length == 0 || timerInput.text.Length == 0) {
            return;
        }
        var cmd = new CommandChangeRules {
            EnabledChanges = CommandChangeRules.Rules.Laps
                           | CommandChangeRules.Rules.StarsToWin
                           | CommandChangeRules.Rules.CoinsForPowerup
                           | CommandChangeRules.Rules.Lives
                           | CommandChangeRules.Rules.TimerSeconds
                           | CommandChangeRules.Rules.TeamsEnabled
                           | CommandChangeRules.Rules.ScoreEnabled,
            Laps = int.Parse(lapsInput.text),
            StarsToWin = starsToggle.isOn ? int.Parse(starsInput.text) : 0,
            CoinsForPowerup = coinsToggle.isOn ? int.Parse(coinsInput.text) : 0,
            Lives = livesToggle.isOn ? int.Parse(livesInput.text) : 0,
            TimerSeconds = timerToggle.isOn ? int.Parse(timerInput.text) : 0,
            TeamsEnabled = teamsToggle.isOn,
            ScoreEnabled = scoreToggle.isOn,
        };
        QuantumGame game = QuantumRunner.DefaultGame;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
        game.SendCommand(slot, cmd);
    }
}
