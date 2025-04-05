using Quantum;
using System;
using UnityEngine;

public class StrengthChangeableRule : ChangeableRule {
    [Flags]
    public enum RuleStrength {
        Zero = 1 << 0,
        ALotLess = 1 << 1,
        ABitLess = 1 << 2,
        Default = 1 << 3,
        ABitMore = 1 << 4,
        ALotMore = 1 << 5,
        Infinity = 1 << 6,
    }

    public int MinValue = 0;
    public int MaxValue = 10;
    public override bool CanIncreaseValue => (int) value < MaxValue;
    public override bool CanDecreaseValue => (int) value > MinValue;

    [SerializeField] protected RuleStrength includedStrengths = RuleStrength.Default;
    
    protected override void IncreaseValueInternal() {
        int intValue = (int) value;
        value = Mathf.Clamp(intValue, MinValue, MaxValue);

        if (intValue != (int) value) {
            cursorSfx.Play();
            SendCommand();
        }
    }
    
    protected override void DecreaseValueInternal() {
        int intValue = (int) value;
        value = Mathf.Clamp(intValue, MinValue, MaxValue);

        if (intValue != (int) value) {
            cursorSfx.Play();
            SendCommand();
        }
    }
    
    private unsafe void SendCommand() {
        CommandChangePowerupChances cmd = new CommandChangePowerupChances {
        };

        switch (ruleType) {
        case CommandChangeRules.Rules.PowerupChances:
            
            break;
        }

        QuantumGame game = NetworkHandler.Game;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(QuantumUtils.GetHostPlayer(game.Frames.Predicted, out _))];
        game.SendCommand(slot, cmd);
    }
    
    protected override void UpdateLabel() {
        if (value is int intValue) {
            label.text = labelPrefix + "a";
        }
    }
}