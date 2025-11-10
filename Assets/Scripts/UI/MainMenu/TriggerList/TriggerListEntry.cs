using NSMB.UI.MainMenu.Submenus.Prompts;
using NSMB.UI.MainMenu.TriggerList;
using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

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
    public TMP_Dropdown ddConditionParameter,
        ddConditionTarget,
        ddActionParameter,
        ddActionTarget,
        ddConstraint,
        ddConstraintParameter,
        ddConstraintTarget;

    public GameObject extrasDialog;
    public Button btnDelete, btnDuplicate, btnCondition, btnAction;
    
    void OnEnable() {
        if (ddConstraint.options.Count != 0) {
            return;
        }
        
        var i = 0;
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

    public void RefreshInteractability() {
        var isHost = Parent.matchSettings.isHost;
        btnDelete.interactable = isHost;
        btnDuplicate.interactable = isHost;
        btnCondition.interactable = isHost;
        btnAction.interactable = isHost;
        ddConditionTarget.interactable = isHost;
        ddConditionParameter.interactable = isHost;
        ddActionTarget.interactable = isHost;
        ddActionParameter.interactable = isHost;
    }

    public void SetTrigger(MatchConditionerTrigger newTrigger) {
        _trigger = newTrigger;
        // try to find the index of the correct option in the dropdown, that corresponds to the fields of the given new trigger.
        txtCondition.text = newTrigger.Condition.ToString();
        txtAction.text = newTrigger.Action.ToString();
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
    
    public void OnConditionChanged(TriggerCondition condition) {
        _trigger.Condition = condition;
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
    
    public void OnActionChanged(TriggerAction action) {
        _trigger.Action = action;
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
        
        if (TriggerMappings.NonPeopleConditions.Contains(Trigger.Condition)) {
            txtConditionTarget.text = "";
            return;
        }

        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (TriggerMappings.IncompatibleConditionTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            ddConditionTarget.options.Add(new DropdownTriggerOption(i++, target.ToString(), (int) target));
        }

        txtConditionTarget.text = ((DropdownTriggerOption)ddConditionTarget.options[ddConditionTarget.value]).OptionName;
    }
    
    public void RecalculateConditionParameters() {
        ddConditionParameter.ClearOptions();
        
        if (!TriggerMappings.ConditionParameters.ContainsKey(Trigger.Condition)) {
            txtConditionParameter.text = "";
            return;
        }
        
        foreach (var parameter in TriggerMappings.ConditionParameters[Trigger.Condition]) {
            ddConditionParameter.options.Add(new TMP_Dropdown.OptionData(parameter));
        }
        
        txtConditionParameter.text = ddConditionParameter.options[ddConditionParameter.value].text;
    }
    
    public void RecalculateActionTargets() {
        ddActionTarget.ClearOptions();
        
        if (TriggerMappings.NonPeopleActions.Contains(Trigger.Action)) {
            txtActionTarget.text = "";
            return;
        }
        
        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (TriggerMappings.IncompatibleActionTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            // if (TriggerMappings.NonPeopleConditions.Contains(Trigger.Condition) && new [] {TriggerTarget.Conditioner, TriggerTarget.NonConditioner, TriggerTarget.ConditionerTeam, TriggerTarget.NonConditionerTeam}.Contains((TriggerTarget) target)) {
            //     continue;
            // }
            ddActionTarget.options.Add(new DropdownTriggerOption(i++, target.ToString(), (int) target));
        }
        
        txtActionTarget.text = ((DropdownTriggerOption)ddActionTarget.options[ddActionTarget.value]).OptionName;
    }
    
    public void RecalculateActionParameters() {
        ddActionParameter.ClearOptions();
        
        if (!TriggerMappings.ActionParameters.ContainsKey(Trigger.Action)) {
            txtActionParameter.text = "";
            return;
        }
        
        foreach (var parameter in TriggerMappings.ActionParameters[Trigger.Action]) {
            ddActionParameter.options.Add(new TMP_Dropdown.OptionData(parameter));
        }
        
        txtActionParameter.text = ddActionParameter.options[ddActionParameter.value].text;
    }
    
    public void RecalculateConstraintTargets() {
        ddConstraintTarget.ClearOptions();
        
        if (TriggerMappings.NonPeopleConstraints.Contains(Trigger.Constraint)) {
            txtConstraintTarget.text = "";
            return;
        }
        
        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (TriggerMappings.IncompatibleConstraintTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            ddConstraintTarget.options.Add(new DropdownTriggerOption(i++, target.ToString(), (int) target));
        }
        
        txtConstraintTarget.text = ((DropdownTriggerOption)ddConstraintTarget.options[ddConstraintTarget.value]).OptionName;
    }
    
    public void RecalculateConstraintParameters() {
        ddConstraintParameter.ClearOptions();
        
        if (!TriggerMappings.ConstraintParameters.ContainsKey(Trigger.Constraint)) {
            txtConstraintParameter.text = "";
            return;
        }
        
        foreach (var parameter in TriggerMappings.ConstraintParameters[Trigger.Constraint]) {
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
