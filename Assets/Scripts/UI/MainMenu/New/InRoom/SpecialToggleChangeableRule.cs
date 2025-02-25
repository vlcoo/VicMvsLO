using NSMB.Translation;
using Quantum;
using UnityEngine.EventSystems;

public class SpecialToggleChangeableRule : ChangeableRule {

    //---Properties
    public override bool CanIncreaseValue => !(bool) value;
    public override bool CanDecreaseValue => (bool) value;

    protected override void IncreaseValueInternal() {
        if (!(bool) value) {
            value = true;
            cursorSfx.Play();
            SendCommand();
        }
    }

    protected override unsafe void DecreaseValueInternal() {
        if ((bool) value) {
            value = false;
            cursorSfx.Play();
            SendCommand();
        }
    }
    
    public override unsafe void OnSubmit(BaseEventData eventData) {
        if (CanIncreaseValue) IncreaseValue();
        else if (CanDecreaseValue) DecreaseValue();
    }

    public override unsafe void OnPointerClick(PointerEventData eventData) {
        if (CanIncreaseValue) IncreaseValue();
        else if (CanDecreaseValue) DecreaseValue();
    }

    private unsafe void SendCommand() {
        CommandChangeRules cmd = new() {
            EnabledChanges = ruleType,
        };
        
        switch (ruleType) {
        case CommandChangeRules.Rules.SNoReserve:
            cmd.SNoReserve = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoDroppedStars:
            cmd.SNoDroppedStars = (bool) value;
            break;
        case CommandChangeRules.Rules.SInstantDeath:
            cmd.SInstantDeath = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoDefrost:
            cmd.SNoDefrost = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoCollisions:
            cmd.SNoCollisions = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoIframes:
            cmd.SNoIframes = (bool) value;
            break;
        case CommandChangeRules.Rules.SHideSeek:
            cmd.SHideSeek = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoEnemies:
            cmd.SNoEnemies = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoBahs:
            cmd.SNoBahs = (bool) value;
            break;
        case CommandChangeRules.Rules.SPitWrap:
            cmd.SPitWrap = (bool) value;
            break;
        case CommandChangeRules.Rules.SAllBricks:
            cmd.SAllBricks = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoLooping:
            cmd.SNoLooping = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoCoins:
            cmd.SNoCoins = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoPowerups:
            cmd.SNoPowerups = (bool) value;
            break;
        case CommandChangeRules.Rules.SNoMinimap:
            cmd.SNoMinimap = (bool) value;
            break;
        case CommandChangeRules.Rules.SShowCoinCount:
            cmd.SShowCoinCount = (bool) value;
            break;
        }

        QuantumGame game = NetworkHandler.Game;
        int slot = game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(QuantumUtils.GetHostPlayer(game.Frames.Predicted, out _))];
        game.SendCommand(slot, cmd);
    }

    protected override void UpdateLabel() {
        if (value is bool boolValue) {
            label.text = labelPrefix + (boolValue ? "o" : ".");
        }
    }
}