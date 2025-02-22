using Quantum;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TriggerListEntry : MonoBehaviour {
    public MatchConditionerTrigger Trigger;
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
        TriggerCondition.XSecondRemaining,
        TriggerCondition.EveryXSecond,
        TriggerCondition.SongBahd,
    };
    private readonly List<TriggerAction> _nonPeopleActions = new() {
        TriggerAction.DrawMatch,
        TriggerAction.RespawnLevel,
        TriggerAction.ExplodeLevel,
        TriggerAction.SpawnStar
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
        TriggerTarget.All,
        TriggerTarget.Random,
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
        TriggerTarget.Random
    };
    
    // also, we have to map the parameters to each condition, action or constraint.
    private readonly Dictionary<TriggerCondition, List<string>> _conditionParameters = new() {
        { TriggerCondition.LookedXDirection, new List<string> { "Any", "Up", "Right", "Down", "Left" } },
        { TriggerCondition.XSecondRemaining, new List<string> { "60", "10" } },
        { TriggerCondition.EveryXSecond, new List<string> { "1", "5", "10", "15", "30", "60" } },
    };
    private readonly Dictionary<TriggerAction, List<string>> _actionParameters = new() {
        { TriggerAction.GiveXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerAction.SpawnXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerAction.GiveXReserve, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerAction.SpawnXEnemy, new List<string> { "Random", "Goomba", "GreenKoopa", "RedKoopa", "BlueKoopa", "BulletBill", "Boo", "Spiny", "Bobomb" } },
        { TriggerAction.BecomeXTeam, new List<string> { "Random", "A", "B", "C", "D", "E" } },
    };
    private readonly Dictionary<TriggerConstraint, List<string>> _constraintParameters = new() {
        { TriggerConstraint.IsXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
        { TriggerConstraint.IsNotXPowerup, new List<string> { "Random", "Mushroom", "FireFlower", "IceFlower", "PropellerMushroom", "MiniMushroom", "BlueShell", "HammerSuit", "MegaMushroom" } },
    };
    
    // finally, certain condition-action pairs are recursive or contradictory and are forbidden. list them here.
    private readonly Dictionary<TriggerCondition, TriggerAction> _forbiddenPairs = new() {
        { TriggerCondition.GotStar, TriggerAction.GiveStar }, { TriggerCondition.GotCoin, TriggerAction.GiveCoin },
    };
    
    void Start() {
        var i = 0;
        foreach (var condition in Enum.GetValues(typeof(TriggerCondition))) {
            ddCondition.options.Add(new DropdownTriggerOption(i++, condition.ToString(), (int) condition));
        }
        ddCondition.value = 0;
        txtCondition.text = ((DropdownTriggerOption)ddCondition.options[ddCondition.value]).OptionName;
        i = 0;
        foreach (var action in Enum.GetValues(typeof(TriggerAction))) {
            ddAction.options.Add(new DropdownTriggerOption(i++, action.ToString(), (int) action));
        }
        ddAction.value = 0;
        txtAction.text = ((DropdownTriggerOption)ddAction.options[ddAction.value]).OptionName;
        i = 0;
        foreach (var constraint in Enum.GetValues(typeof(TriggerConstraint))) {
            ddConstraint.options.Add(new DropdownTriggerOption(i++, constraint.ToString(), (int) constraint));
        }
        ddConstraint.value = 0;
        txtConstraint.text = ((DropdownTriggerOption)ddConstraint.options[ddConstraint.value]).OptionName;
        RecalculateConditionParameters();
        RecalculateActionParameters();
        RecalculateConstraintParameters();
        RecalculateConditionTargets();
        RecalculateActionTargets();
        RecalculateConstraintTargets();
    }
    
    public void OnConditionChanged() {
        var value = (DropdownTriggerOption) ddCondition.options[ddCondition.value];
        Trigger.Condition = (TriggerCondition) value.EnumValue;
        RecalculateConditionParameters();
        RecalculateConditionTargets();
    }
    
    public void OnConditionParameterChanged() {
        Trigger.ConditionParameter = ddConditionParameter.options[ddConditionParameter.value].text;
    }
    
    public void OnConditionTargetChanged() {
        var value = (DropdownTriggerOption) ddConditionTarget.options[ddConditionTarget.value];
        Trigger.ConditionTarget = (TriggerTarget) value.EnumValue;
    }
    
    public void OnActionChanged() {
        var value = (DropdownTriggerOption) ddAction.options[ddAction.value];
        Trigger.Action = (TriggerAction) value.EnumValue;
        RecalculateActionParameters();
        RecalculateActionTargets();
    }
    
    public void OnActionParameterChanged() {
        Trigger.ActionParameter = ddActionParameter.options[ddActionParameter.value].text;
    }
    
    public void OnActionTargetChanged() {
        var value = (DropdownTriggerOption) ddActionTarget.options[ddActionTarget.value];
        Trigger.ActionTarget = (TriggerTarget) value.EnumValue;
    }
    
    public void OnConstraintChanged() {
        var value = (DropdownTriggerOption) ddConstraint.options[ddConstraint.value];
        Trigger.Constraint = (TriggerConstraint) value.EnumValue;
        RecalculateConstraintParameters();
        RecalculateConstraintTargets();
    }
    
    public void OnConstraintParameterChanged() {
        Trigger.ConstraintParameter = ddConstraintParameter.options[ddConstraintParameter.value].text;
    }
    
    public void OnConstraintTargetChanged() {
        var value = (DropdownTriggerOption) ddConstraintTarget.options[ddConstraintTarget.value];
        Trigger.ConstraintTarget = (TriggerTarget) value.EnumValue;
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
