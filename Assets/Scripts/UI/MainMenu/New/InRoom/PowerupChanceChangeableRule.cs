using NSMB.Translation;
using Quantum;
using UnityEngine;
using UnityEngine.EventSystems;

public class PowerupChanceChangeableRule : SpecialToggleChangeableRule {
    [SerializeField] protected CommandChangePowerupChances.PowerupChances powerupType;

    protected override void FindValue(in GameRules rules) {
        if (ruleType != CommandChangeRules.Rules.PowerupChances) return;

        value = powerupType switch {
            CommandChangePowerupChances.PowerupChances.Mushroom => rules.ChanceMushroom != 0,
            CommandChangePowerupChances.PowerupChances.FireFlower => rules.ChanceFireFlower != 0,
            CommandChangePowerupChances.PowerupChances.IceFlower => rules.ChanceIceFlower != 0,
            CommandChangePowerupChances.PowerupChances.PropellerMushroom => rules.ChancePropellerMushroom != 0,
            CommandChangePowerupChances.PowerupChances.BlueShell => rules.ChanceBlueShell != 0,
            CommandChangePowerupChances.PowerupChances.HammerSuit => rules.ChanceHammerSuit != 0,
            CommandChangePowerupChances.PowerupChances.MiniMushroom => rules.ChanceMiniMushroom != 0,
            CommandChangePowerupChances.PowerupChances.MegaMushroom => rules.ChanceMegaMushroom != 0,
            CommandChangePowerupChances.PowerupChances.Starman => rules.ChanceStarman != 0,
        };
        
        UpdateState();
    }

    protected override unsafe void SendCommand() {
        CommandChangePowerupChances cmd = new();
        
        switch (powerupType) {
        case CommandChangePowerupChances.PowerupChances.Mushroom:
            cmd.ChanceMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.FireFlower:
            cmd.ChanceFireFlower = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.IceFlower:
            cmd.ChanceIceFlower = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.PropellerMushroom:
            cmd.ChancePropellerMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.BlueShell:
            cmd.ChanceBlueShell = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.HammerSuit:
            cmd.ChanceHammerSuit = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.MiniMushroom:
            cmd.ChanceMiniMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.MegaMushroom:
            cmd.ChanceMegaMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupChances.PowerupChances.Starman:
            cmd.ChanceStarman = ((bool) value) ? 1 : 0;
            break;
        }

        QuantumGame game = NetworkHandler.Game;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(QuantumUtils.GetHostPlayer(game.Frames.Predicted, out _))];
        game.SendCommand(slot, cmd);
    }
}