using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettings : MonoBehaviour {
    public Toggle lapsToggle, starsToggle, coinsToggle, livesToggle, timerToggle, teamsToggle;
    public TMP_InputField lapsInput, starsInput, coinsInput, livesInput, timerInput;
    public GameRules rules;
    
    public void Start() {
        QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
        QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
    }

    private unsafe void OnRulesChanged(EventRulesChanged e) {
        rules = e.Game.Frames.Predicted.Global->Rules;
        RefreshValues();
    }
    
    private unsafe void OnGameStarted(CallbackGameStarted e) {
        rules = e.Game.Frames.Predicted.Global->Rules;
        RefreshValues();
    }

    public void OnInputEndEdit() {
        RefreshValues();    // this will reset any empty fields
    }

    public void RefreshValues() {
        lapsToggle.SetIsOnWithoutNotify(rules.Laps > 0);
        lapsInput.interactable = rules.Laps > 0;
        if (rules.Laps > 0 || lapsInput.text.Length == 0) lapsInput.SetTextWithoutNotify(rules.Laps.ToString());
        starsToggle.SetIsOnWithoutNotify(rules.StarsToWin > 0);
        starsInput.interactable = rules.StarsToWin > 0;
        if (rules.StarsToWin > 0 || starsInput.text.Length == 0) starsInput.SetTextWithoutNotify(rules.StarsToWin.ToString());
        coinsToggle.SetIsOnWithoutNotify(rules.CoinsForPowerup > 0);
        coinsInput.interactable = rules.CoinsForPowerup > 0;
        if (rules.CoinsForPowerup > 0 || coinsInput.text.Length == 0) coinsInput.SetTextWithoutNotify(rules.CoinsForPowerup.ToString());
        livesToggle.SetIsOnWithoutNotify(rules.Lives > 0);
        livesInput.interactable = rules.Lives > 0;
        if (rules.Lives > 0 || livesInput.text.Length == 0) livesInput.SetTextWithoutNotify(rules.Lives.ToString());
        timerToggle.SetIsOnWithoutNotify(rules.TimerSeconds > 0);
        timerInput.interactable = rules.TimerSeconds > 0;
        if (rules.TimerSeconds > 0 || timerInput.text.Length == 0) timerInput.SetTextWithoutNotify(rules.TimerSeconds.ToString());
        teamsToggle.SetIsOnWithoutNotify(rules.TeamsEnabled);
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
                           | CommandChangeRules.Rules.TeamsEnabled,
            Laps = lapsToggle.isOn ? int.Parse(lapsInput.text) : 0,
            StarsToWin = starsToggle.isOn ? int.Parse(starsInput.text) : 0,
            CoinsForPowerup = coinsToggle.isOn ? int.Parse(coinsInput.text) : 0,
            Lives = livesToggle.isOn ? int.Parse(livesInput.text) : 0,
            TimerSeconds = timerToggle.isOn ? int.Parse(timerInput.text) : 0,
            TeamsEnabled = teamsToggle.isOn,
        };
        QuantumGame game = QuantumRunner.DefaultGame;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
        game.SendCommand(slot, cmd);
    }
}
