using NSMB.UI.MainMenu.Submenus.Prompts;
using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TriggerListEntry : MonoBehaviour {
    public int Index;
    public TriggerPromptSubmenu Parent;
    private MatchConditionerTrigger _trigger;
    public MatchConditionerTrigger Trigger {
        get => _trigger;
        set => SetTrigger(value);
    }
    
    public TMP_Text txtCondition,
        txtConditionParameter,
        txtConditionTarget,
        txtAction,
        txtActionParameter,
        txtActionTarget,
        txtConstraint,
        txtConstraintParameter,
        txtConstraintTarget;
    public TMP_Dropdown ddCondition,
        ddConditionParameter,
        ddConditionTarget,
        ddAction,
        ddActionParameter,
        ddActionTarget,
        ddConstraint,
        ddConstraintParameter,
        ddConstraintTarget;
    
    // some conditions, actions and constraints are global (not referring to a player, but rather the stage or match itself)
    // they can't have a target.
    private readonly List<TriggerCondition> _nonPeopleConditions = new() {
        TriggerCondition.MatchStarted,
        // TriggerCondition.XSecondRemaining,
        // TriggerCondition.EveryXSecond,
        // TriggerCondition.SongBahd,
    };
    private readonly List<TriggerAction> _nonPeopleActions = new() {
        TriggerAction.DrawMatch,
        TriggerAction.RespawnLevel,
        // TriggerAction.ExplodeLevel,
        // TriggerAction.SpawnStar
    };
    private readonly List<TriggerConstraint> _nonPeopleConstraints = new() {
        TriggerConstraint.Always,
        TriggerConstraint.TimerIsLessThanX,
        TriggerConstraint.TimerIsMoreThanX,
        TriggerConstraint.StarsExist,
        TriggerConstraint.StarsNotExist,
        TriggerConstraint.EnemiesExist,
        TriggerConstraint.EnemiesNotExist,
        TriggerConstraint.CoinsExist,
        TriggerConstraint.CoinsNotExist,
        TriggerConstraint.XPlayersRemaining,
        TriggerConstraint.LessThanXPlayersRemaining,
        TriggerConstraint.MoreThanXPlayersRemaining,
    };
    
    // some targets are exclusive to either the conditions, the actions or the constraints.
    private readonly List<TriggerTarget> _incompatibleConditionTargets = new() {
        TriggerTarget.Everyone,
        TriggerTarget.OneRandom,
        TriggerTarget.Randoms,
        TriggerTarget.Conditioner, 
        TriggerTarget.ConditionerTeam, 
        TriggerTarget.NonConditioner,
        TriggerTarget.NonConditionerTeam,
        TriggerTarget.Actioner,
        TriggerTarget.ActionerTeam,
        TriggerTarget.NonActioner,
        TriggerTarget.NonActionerTeam,
    };
    private readonly List<TriggerTarget> _incompatibleActionTargets = new() {
        TriggerTarget.Any,
        TriggerTarget.Actioner,
        TriggerTarget.ActionerTeam,
        TriggerTarget.NonActioner,
        TriggerTarget.NonActionerTeam,
    };
    private readonly List<TriggerTarget> _incompatibleConstraintTargets = new() {
        TriggerTarget.OneRandom,
        TriggerTarget.Randoms
    };
    
    // also, we have to map the parameters to each condition, action or constraint.
    private readonly Dictionary<TriggerCondition, List<string>> _conditionParameters = new() {
        // { TriggerCondition.LookedXDirection, new List<string> { "Any", "Up", "Right", "Down", "Left" } },
        // { TriggerCondition.XSecondRemaining, new List<string> { "60", "10" } },
        // { TriggerCondition.EveryXSecond, new List<string> { "1", "5", "10", "15", "30", "60" } },
    };
    private readonly Dictionary<TriggerAction, List<string>> _actionParameters = new() {
        { TriggerAction.GiveXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerAction.SpawnXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerAction.GiveXReserve, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        // { TriggerAction.SpawnXEnemy, new List<string> { "Random", "Goomba", "GreenKoopa", "RedKoopa", "BlueKoopa", "BulletBill", "Boo", "Spiny", "Bobomb" } },
        // { TriggerAction.BecomeXTeam, new List<string> { "Random", "A", "B", "C", "D", "E" } },
        { TriggerAction.Stun, new List<string> { "Bump", "Knockback", "HardKnockback", "ForcefulKnockback" } },
    };
    private readonly Dictionary<TriggerConstraint, List<string>> _constraintParameters = new() {
        { TriggerConstraint.IsXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerConstraint.IsNotXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
    };
    
    // finally, certain condition-action pairs are recursive or contradictory and are forbidden. list them here.
    private readonly Dictionary<TriggerCondition, TriggerAction> _forbiddenPairs = new() {
        { TriggerCondition.GotStar, TriggerAction.GiveStar }, { TriggerCondition.GotCoin, TriggerAction.GiveCoin },
    };
    
    void OnEnable() {
        if (ddCondition.options.Count != 0 && ddAction.options.Count != 0 && ddConstraint.options.Count != 0) {
            return;
        }
        
        var i = 0;
        foreach (var condition in Enum.GetValues(typeof(TriggerCondition))) {
            ddCondition.options.Add(new DropdownTriggerOption(i++, condition.ToString(), (int) condition));
        }
        txtCondition.text = ((DropdownTriggerOption)ddCondition.options[ddCondition.value]).OptionName;
        i = 0;
        foreach (var action in Enum.GetValues(typeof(TriggerAction))) {
            ddAction.options.Add(new DropdownTriggerOption(i++, action.ToString(), (int) action));
        }
        txtAction.text = ((DropdownTriggerOption)ddAction.options[ddAction.value]).OptionName;
        i = 0;
        foreach (var constraint in Enum.GetValues(typeof(TriggerConstraint))) {
            ddConstraint.options.Add(new DropdownTriggerOption(i++, constraint.ToString(), (int) constraint));
        }
        txtConstraint.text = ((DropdownTriggerOption)ddConstraint.options[ddConstraint.value]).OptionName;
        RecalculateConditionParameters();
        RecalculateActionParameters();
        RecalculateConstraintParameters();
        RecalculateConditionTargets();
        RecalculateActionTargets();
        RecalculateConstraintTargets();
    }
    
    public void OnDeleted() {
        Parent.TriggerRemoved(this);
    }

    public void SetTrigger(MatchConditionerTrigger newTrigger) {
        _trigger = newTrigger;
        // try to find the index of the correct option in the dropdown, that corresponds to the fields of the given new trigger.
        ddCondition.SetValueWithoutNotify(ddCondition.options.FindIndex(o =>
            ((DropdownTriggerOption) o).EnumValue == (int) newTrigger.Condition));
        ddAction.SetValueWithoutNotify(ddAction.options.FindIndex(o =>
            ((DropdownTriggerOption) o).EnumValue == (int) newTrigger.Action));
        ddConstraint.SetValueWithoutNotify(ddConstraint.options.FindIndex(o =>
            ((DropdownTriggerOption) o).EnumValue == (int) newTrigger.Constraint));
        RecalculateConditionParameters();
        RecalculateActionParameters();
        RecalculateConstraintParameters();
        RecalculateConditionTargets();
        RecalculateActionTargets();
        RecalculateConstraintTargets();
        ddConditionParameter.SetValueWithoutNotify(ddConditionParameter.options.FindIndex(o =>
            o.text == newTrigger.ConditionParameter));
        ddActionParameter.SetValueWithoutNotify(ddActionParameter.options.FindIndex(o => 
            o.text == newTrigger.ActionParameter));
        ddConstraintParameter.SetValueWithoutNotify(ddConstraintParameter.options.FindIndex(o =>
            o.text == newTrigger.ConstraintParameter));
        ddConditionTarget.SetValueWithoutNotify(ddConditionTarget.options.FindIndex(o =>
            ((DropdownTriggerOption) o).EnumValue == (int) newTrigger.ConditionTarget));
        ddActionTarget.SetValueWithoutNotify(ddActionTarget.options.FindIndex(o =>
            ((DropdownTriggerOption) o).EnumValue == (int) newTrigger.ActionTarget));
        ddConstraintTarget.SetValueWithoutNotify(ddConstraintTarget.options.FindIndex(o =>
            ((DropdownTriggerOption) o).EnumValue == (int) newTrigger.ConstraintTarget));
    }
    
    public void OnConditionChanged() {
        var value = (DropdownTriggerOption) ddCondition.options[ddCondition.value];
        _trigger.Condition = (TriggerCondition) value.EnumValue;
        RecalculateConditionParameters();
        RecalculateConditionTargets();
        Parent.TriggerEdited(this);
    }
    
    public void OnConditionParameterChanged() {
        _trigger.ConditionParameter = ddConditionParameter.options[ddConditionParameter.value].text;
        Parent.TriggerEdited(this);
    }
    
    public void OnConditionTargetChanged() {
        var value = (DropdownTriggerOption) ddConditionTarget.options[ddConditionTarget.value];
        _trigger.ConditionTarget = (TriggerTarget) value.EnumValue;
        Parent.TriggerEdited(this);
    }
    
    public void OnActionChanged() {
        var value = (DropdownTriggerOption) ddAction.options[ddAction.value];
        _trigger.Action = (TriggerAction) value.EnumValue;
        RecalculateActionParameters();
        RecalculateActionTargets();
        Parent.TriggerEdited(this);
    }
    
    public void OnActionParameterChanged() {
        _trigger.ActionParameter = ddActionParameter.options[ddActionParameter.value].text;
        Parent.TriggerEdited(this);
    }
    
    public void OnActionTargetChanged() {
        var value = (DropdownTriggerOption) ddActionTarget.options[ddActionTarget.value];
        _trigger.ActionTarget = (TriggerTarget) value.EnumValue;
        Parent.TriggerEdited(this);
    }
    
    public void OnConstraintChanged() {
        var value = (DropdownTriggerOption) ddConstraint.options[ddConstraint.value];
        _trigger.Constraint = (TriggerConstraint) value.EnumValue;
        RecalculateConstraintParameters();
        RecalculateConstraintTargets();
        Parent.TriggerEdited(this);
    }
    
    public void OnConstraintParameterChanged() {
        _trigger.ConstraintParameter = ddConstraintParameter.options[ddConstraintParameter.value].text;
        Parent.TriggerEdited(this);
    }
    
    public void OnConstraintTargetChanged() {
        var value = (DropdownTriggerOption) ddConstraintTarget.options[ddConstraintTarget.value];
        _trigger.ConstraintTarget = (TriggerTarget) value.EnumValue;
        Parent.TriggerEdited(this);
    }
    
    public void RecalculateConditionTargets() {
        ddConditionTarget.ClearOptions();
        
        if (_nonPeopleConditions.Contains(Trigger.Condition)) {
            txtConditionTarget.text = "";
            return;
        }

        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (_incompatibleConditionTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            ddConditionTarget.options.Add(new DropdownTriggerOption(i++, target.ToString(), (int) target));
        }

        txtConditionTarget.text = ((DropdownTriggerOption)ddConditionTarget.options[ddConditionTarget.value]).OptionName;
    }
    
    public void RecalculateConditionParameters() {
        ddConditionParameter.ClearOptions();
        
        if (!_conditionParameters.ContainsKey(Trigger.Condition)) {
            txtConditionParameter.text = "";
            return;
        }
        
        foreach (var parameter in _conditionParameters[Trigger.Condition]) {
            ddConditionParameter.options.Add(new TMP_Dropdown.OptionData(parameter));
        }
        
        txtConditionParameter.text = ddConditionParameter.options[ddConditionParameter.value].text;
    }
    
    public void RecalculateActionTargets() {
        ddActionTarget.ClearOptions();
        
        if (_nonPeopleActions.Contains(Trigger.Action)) {
            txtActionTarget.text = "";
            return;
        }
        
        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (_incompatibleActionTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            // if (_nonPeopleConditions.Contains(Trigger.Condition) && new [] {TriggerTarget.Conditioner, TriggerTarget.NonConditioner, TriggerTarget.ConditionerTeam, TriggerTarget.NonConditionerTeam}.Contains((TriggerTarget) target)) {
            //     continue;
            // }
            ddActionTarget.options.Add(new DropdownTriggerOption(i++, target.ToString(), (int) target));
        }
        
        txtActionTarget.text = ((DropdownTriggerOption)ddActionTarget.options[ddActionTarget.value]).OptionName;
    }
    
    public void RecalculateActionParameters() {
        ddActionParameter.ClearOptions();
        
        if (!_actionParameters.ContainsKey(Trigger.Action)) {
            txtActionParameter.text = "";
            return;
        }
        
        foreach (var parameter in _actionParameters[Trigger.Action]) {
            ddActionParameter.options.Add(new TMP_Dropdown.OptionData(parameter));
        }
        
        txtActionParameter.text = ddActionParameter.options[ddActionParameter.value].text;
    }
    
    public void RecalculateConstraintTargets() {
        ddConstraintTarget.ClearOptions();
        
        if (_nonPeopleConstraints.Contains(Trigger.Constraint)) {
            txtConstraintTarget.text = "";
            return;
        }
        
        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (_incompatibleConstraintTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            ddConstraintTarget.options.Add(new DropdownTriggerOption(i++, target.ToString(), (int) target));
        }
        
        txtConstraintTarget.text = ((DropdownTriggerOption)ddConstraintTarget.options[ddConstraintTarget.value]).OptionName;
    }
    
    public void RecalculateConstraintParameters() {
        ddConstraintParameter.ClearOptions();
        
        if (!_constraintParameters.ContainsKey(Trigger.Constraint)) {
            txtConstraintParameter.text = "";
            return;
        }
        
        foreach (var parameter in _constraintParameters[Trigger.Constraint]) {
            ddConstraintParameter.options.Add(new TMP_Dropdown.OptionData(parameter));
        }
        
        txtConstraintParameter.text = ddConstraintParameter.options[ddConstraintParameter.value].text;
    }

    private class DropdownTriggerOption : TMP_Dropdown.OptionData {
        public int Index { get; }
        public string OptionName { get; }
        public int EnumValue { get; }
        
        public DropdownTriggerOption(int index, string optionName, int enumValue) {
            Index = index;
            OptionName = optionName;
            EnumValue = enumValue;
            text = optionName;
        }
    }
}
