using JimmysUnityUtilities;
using NSMB.Networking;
using NSMB.UI.MainMenu;
using NSMB.UI.MainMenu.Submenus.Prompts;
using NSMB.UI.MainMenu.TriggerList;
using Quantum;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class RulesetSaverLoader : MonoBehaviour
{
    private MainMenuCanvas Canvas => GetComponent<PromptSubmenu>().Canvas;
    private const string CODE_SEPARATOR = "-";
    private const int CODE_VERSION = 2;

    public void OnSavePressed() {
        GUIUtility.systemCopyBuffer = RulesetToCode();
    }

    public unsafe void OnLoadPressed() {
        QuantumGame game = NetworkHandler.Game;
        PlayerRef host = game.Frames.Predicted.Global->Host;
        if (!game.PlayerIsLocal(host)) {
            Canvas.PlaySound(SoundEffect.UI_Error);
        }
        if (CodeToRuleset(GUIUtility.systemCopyBuffer.ToUpper())) {
            // succeeded...
            Canvas.GoBack();
            Canvas.GoBack();
        } else {
            // failed!!
            Canvas.PlaySound(SoundEffect.UI_Error);
        }
    }

    public static unsafe string RulesetToCode() {
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
        
        var triggerCount = triggers.Count;
        foreach (var trigger in triggers) {
            code += (int)trigger.Condition + ",";
            if (TriggerMappings.ConditionParameters.TryGetValue(trigger.Condition, out var parameters) &&
                parameters.IndexOf(trigger.ConditionParameter) is var i and >= 0)
                code += i + ",";
            else code += ",";
            code += (int)trigger.ConditionTarget + ",";
            code += (int)trigger.Action + ",";
            if (TriggerMappings.ActionParameters.TryGetValue(trigger.Action, out parameters) &&
                parameters.IndexOf(trigger.ActionParameter) is var j and >= 0)
                code += j + ",";
            else code += ",";
            code += (int)trigger.ActionTarget + ",";
            code += (int)trigger.Constraint + ",";
            if (int.TryParse(trigger.ConstraintParameter, out _)) code += trigger.ConstraintParameter + ",";
            else if (TriggerMappings.ConstraintParameters.TryGetValue(trigger.Constraint, out parameters) &&
                parameters.IndexOf(trigger.ConstraintParameter) is var k and >= 0)
                code += k + ",";
            else code += ",";
            code += (int)trigger.ConstraintTarget + ",";
            code += trigger.DelaySeconds + ",";
            code += trigger.RepeatCount + ",";
            code += trigger.Chance;
            triggerCount--;
            if (triggerCount > 0) code += ".";
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
        code += (rules.ScoreEnabled ? "1" : "0") + CODE_SEPARATOR;
        code += (rules.SAllBricks ? "1" : "0") + CODE_SEPARATOR;
        code += CODE_VERSION + CODE_SEPARATOR;

        var sum = 0;
        foreach (var c in code) {
            sum += c;
        }
        code += (sum % 256).ToString("X2");

        return code;
    }

    private unsafe bool CodeToRuleset(string code) {
        // basically the reverse of above...
        Frame f = NetworkHandler.Game.Frames.Predicted;
        GameRules rules = f.Global->Rules;
        QuantumGame game = QuantumRunner.DefaultGame;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
        int code_version;
        
        var parts = code.Split(CODE_SEPARATOR);
        if (parts.Length < 30) return false;    // version 2 has 33 parts, version 1 has 31 parts
        if (parts.Length < 31) code_version = 1;    // version 1 didn't have version stored in code
        else code_version = int.Parse(parts[^2]);
        
        var sum = 0;
        for (int i = 0; i < code.Length - 2; i++) {
            sum += code[i];
        }
        if (((sum % 256).ToString("X2")) != parts[^1]) return false;
        
        game.SendCommand(slot, new CommandChangeRules {
            // all changes enabled.
            EnabledChanges = (CommandChangeRules.Rules)uint.MaxValue,
            Gamemode = f.SimulationConfig.AllGamemodes[int.Parse(parts[0])],
            Stage = rules.Stage,
            Laps = int.Parse(parts[1]),
            StarsToWin = int.Parse(parts[2]),
            CoinsForPowerup = int.Parse(parts[3]),
            Lives = int.Parse(parts[4]),
            TimerSeconds = int.Parse(parts[5]),
            TeamsEnabled = parts[6] == "1",
            ScoreEnabled = code_version >= 2 ? parts[31] == "1" : rules.ScoreEnabled,
            SNoReserve = parts[8] == "1",
            SNoDroppedStars = parts[9] == "1",
            SNoDefrost = parts[10] == "1",
            SNoCollisions = parts[11] == "1",
            SNoIframes = parts[12] == "1",
            SHideSeek = parts[13] == "1",
            SNoEnemies = parts[14] == "1",
            SNoCoins = parts[15] == "1",
            SNoPowerups = parts[16] == "1",
            SAllBricks = code_version >= 2 ? parts[32] == "1" : rules.SAllBricks,
        });

        game.SendCommand(new CommandChangePowerupsHuds {
            // all changes enabled.
            EnabledChanges = (CommandChangePowerupsHuds.PowerupsHuds) uint.MaxValue,
            PMushroom = parts[17] == "1",
            PFireFlower = parts[18] == "1",
            PIceFlower = parts[19] == "1",
            PPropellerMushroom = parts[20] == "1",
            PHammerSuit = parts[21] == "1",
            PBlueShell = parts[22] == "1",
            PMiniMushroom = parts[23] == "1",
            PMegaMushroom = parts[24] == "1",
            PStarman = parts[25] == "1",
            HStars = parts[26] == "1",
            HPlayers = parts[27] == "1",
            HHost = parts[28] == "1",
            HIceCubes = parts[29] == "1",
            HTeamTarget = int.Parse(parts[30]),
        });

        var triggerIndex = 0;
        game.SendCommand(new CommandChangeTriggers { RemoveAll = true });
        foreach (var triggerCode in parts[7].Split('.')) {
            var triggerParts = triggerCode.Split(',');
            if (triggerParts.Length < 12) continue;
            var condition = (TriggerCondition)int.Parse(triggerParts[0]);
            var conditionParameter = "";
            if (TriggerMappings.ConditionParameters.TryGetValue(condition, out var parameters) &&
                int.TryParse(triggerParts[1], out var i) && i >= 0 && i < parameters.Count) {
                conditionParameter = parameters[i];
            }
            var conditionTarget = (TriggerTarget)int.Parse(triggerParts[2]);
            var action = (TriggerAction)int.Parse(triggerParts[3]);
            var actionParameter = "";
            if (TriggerMappings.ActionParameters.TryGetValue(action, out parameters) &&
                int.TryParse(triggerParts[4], out var j) && j >= 0 && j < parameters.Count) {
                actionParameter = parameters[j];
            }
            var actionTarget = (TriggerTarget)int.Parse(triggerParts[5]);
            var constraint = (TriggerConstraint)int.Parse(triggerParts[6]);
            var constraintParameter = "";
            if (int.TryParse(triggerParts[7], out _)) constraintParameter = triggerParts[7];
            else if (TriggerMappings.ConstraintParameters.TryGetValue(constraint, out parameters) &&
                int.TryParse(triggerParts[7], out var k) && k >= 0 && k < parameters.Count) {
                constraintParameter = parameters[k];
            }
            var constraintTarget = (TriggerTarget)int.Parse(triggerParts[8]);
            var delaySeconds = byte.Parse(triggerParts[9]);
            var repeatCount = byte.Parse(triggerParts[10]);
            var chance = byte.Parse(triggerParts[11]);
            game.SendCommand(slot, new CommandChangeTriggers {
                Index = triggerIndex,
                TriggerCondition = (int)condition,
                TriggerConditionParameter = conditionParameter,
                TriggerConditionTarget = (int)conditionTarget,
                TriggerAction = (int)action,
                TriggerActionParameter = actionParameter,
                TriggerActionTarget = (int)actionTarget,
                TriggerConstraint = (int)constraint,
                TriggerConstraintParameter = constraintParameter,
                TriggerConstraintTarget = (int)constraintTarget,
                TriggerDelaySeconds = delaySeconds,
                TriggerRepeatCount = repeatCount,
                TriggerChance = chance,
            });
            triggerIndex++;
        }
        
        // c'est fini, everyone clapped.
        return true;
    }
}
