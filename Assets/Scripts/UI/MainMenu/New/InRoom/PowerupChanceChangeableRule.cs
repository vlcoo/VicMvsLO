using NSMB.Translation;
using Quantum;
using UnityEngine;
using UnityEngine.EventSystems;

public class PowerupChanceChangeableRule : SpecialToggleChangeableRule {
    [SerializeField] protected CommandChangePowerupsHuds.PowerupsHuds powerupType;

    protected override void FindValue(in GameRules rules) {
        if (ruleType != CommandChangeRules.Rules.PowerupChances) return;

        value = powerupType switch {
            CommandChangePowerupsHuds.PowerupsHuds.Mushroom => rules.ChanceMushroom != 0,
            CommandChangePowerupsHuds.PowerupsHuds.FireFlower => rules.ChanceFireFlower != 0,
            CommandChangePowerupsHuds.PowerupsHuds.IceFlower => rules.ChanceIceFlower != 0,
            CommandChangePowerupsHuds.PowerupsHuds.PropellerMushroom => rules.ChancePropellerMushroom != 0,
            CommandChangePowerupsHuds.PowerupsHuds.BlueShell => rules.ChanceBlueShell != 0,
            CommandChangePowerupsHuds.PowerupsHuds.HammerSuit => rules.ChanceHammerSuit != 0,
            CommandChangePowerupsHuds.PowerupsHuds.MiniMushroom => rules.ChanceMiniMushroom != 0,
            CommandChangePowerupsHuds.PowerupsHuds.MegaMushroom => rules.ChanceMegaMushroom != 0,
            CommandChangePowerupsHuds.PowerupsHuds.Starman => rules.ChanceStarman != 0,
        };
        
        UpdateState();
    }

    protected override unsafe void SendCommand() {
        CommandChangePowerupsHuds cmd = new();
        
        switch (powerupType) {
        case CommandChangePowerupsHuds.PowerupsHuds.Mushroom:
            cmd.ChanceMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.FireFlower:
            cmd.ChanceFireFlower = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.IceFlower:
            cmd.ChanceIceFlower = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.PropellerMushroom:
            cmd.ChancePropellerMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.BlueShell:
            cmd.ChanceBlueShell = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.HammerSuit:
            cmd.ChanceHammerSuit = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.MiniMushroom:
            cmd.ChanceMiniMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.MegaMushroom:
            cmd.ChanceMegaMushroom = ((bool) value) ? 1 : 0;
            break;
        case CommandChangePowerupsHuds.PowerupsHuds.Starman:
            cmd.ChanceStarman = ((bool) value) ? 1 : 0;
            break;
        }

        QuantumGame game = NetworkHandler.Game;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(game.Frames.Predicted.Global->Host)];
        game.SendCommand(slot, cmd);
    }
}