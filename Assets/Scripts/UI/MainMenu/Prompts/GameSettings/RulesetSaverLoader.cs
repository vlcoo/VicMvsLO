using JimmysUnityUtilities;
using NSMB.Networking;
using NSMB.UI.MainMenu;
using Quantum;
using System.Text;
using UnityEngine;

public class RulesetSaverLoader : MonoBehaviour
{
    [SerializeField] private MainMenuCanvas canvas;
    private string CODE_SEPARATOR = "-";

    public void OnSavePressed() {
        GUIUtility.systemCopyBuffer = RulesetToCode();
    }

    public unsafe void OnLoadPressed() {
        QuantumGame game = NetworkHandler.Game;
        PlayerRef host = game.Frames.Predicted.Global->Host;
        if (!game.PlayerIsLocal(host)) {
            canvas.PlaySound(SoundEffect.UI_Error);
        }
        if (CodeToRuleset(GUIUtility.systemCopyBuffer.ToUpper())) {
            // succeeded...
            canvas.GoBack();
        } else {
            // failed!!
            canvas.PlaySound(SoundEffect.UI_Error);
        }
    }

    private unsafe string RulesetToCode() {
        var code = "";
        Frame f = NetworkHandler.Game.Frames.Predicted;
        GameRules rules = f.Global->Rules;
        var triggers = f.ResolveList(rules.Triggers);

        code += f.SimulationConfig.AllGamemodes.IndexOf(rules.Gamemode) + CODE_SEPARATOR;
        code += rules.Laps + CODE_SEPARATOR;
        code += rules.StarsToWin + CODE_SEPARATOR;
        code += rules.CoinsForPowerup + CODE_SEPARATOR;
        code += rules.Lives + CODE_SEPARATOR;
        code += rules.TimerSeconds + CODE_SEPARATOR;
        code += (rules.TeamsEnabled ? "1" : "0") + CODE_SEPARATOR;
        
        foreach (var trigger in triggers) {
            code += trigger.Condition + ",";
            code += trigger.ConditionParameter + ",";
            code += trigger.ConditionTarget + ",";
            code += trigger.Action + ",";
            code += trigger.ActionParameter + ",";
            code += trigger.ActionTarget + ",";
            code += trigger.Constraint + ",";
            code += trigger.ConstraintParameter + ",";
            code += trigger.ConstraintTarget + ",";
            code += trigger.DelaySeconds + ",";
            code += trigger.RepeatCount + ",";
            code += trigger.Chance + ",";
        }
        code += CODE_SEPARATOR;

        code += (rules.SNoReserve ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoDroppedStars ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoDefrost ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoCollisions ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoIframes ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SHideSeek ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoEnemies ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoCoins ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SNoPowerups ? "1" : "0") + CODE_SEPARATOR;
        
        code += (rules.PMushroom ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PFireFlower ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PIceFlower ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PPropellerMushroom ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PHammerSuit ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PBlueShell ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PMiniMushroom ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PMegaMushroom ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.PStarman ? "1" : "0") + CODE_SEPARATOR;
        
        code += (rules.HStars ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.HPlayers ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.HHost ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.HIceCubes ? "1" : "0") + CODE_SEPARATOR;
        code += rules.HTeamTarget + CODE_SEPARATOR;

        Debug.Log(code);
        return code;
    }

    private bool CodeToRuleset(string code) {
        return true;
    }
}
