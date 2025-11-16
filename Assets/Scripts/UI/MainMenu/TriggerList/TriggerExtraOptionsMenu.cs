using NSMB.UI.MainMenu.Submenus.Prompts;
using NSMB.UI.MainMenu.TriggerList;
using Quantum;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerExtraOptionsMenu : MonoBehaviour {
    public TriggerPromptSubmenu Parent;

    public TMP_Text txtConstraint,
        txtConstraintParameter,
        txtConstraintTarget,
        txtChance,
        txtDelay,
        txtRepeat;
    public TMP_Dropdown ddConstraint,
        ddConstraintParameter,
        ddConstraintTarget;
    public Slider sChance,
        sDelay,
        sRepeat;
    public TMP_InputField inConstraintParameter;

    private TriggerConstraint _currentConstraint;
    private TriggerTarget _currentTarget;
    private string _currentParameter;

    public void RefreshValues() {
        var newTrigger = Parent.currentEditingEntry.Trigger;
        _currentConstraint = newTrigger.Constraint;
        _currentTarget = newTrigger.ConstraintTarget;
        _currentParameter = newTrigger.ConstraintParameter;
        sChance.value = newTrigger.Chance;
        sDelay.value = newTrigger.DelaySeconds;
        sRepeat.value = newTrigger.RepeatCount;
        
        if (ddConstraint.options.Count == 0) {
            var i = 0;
            foreach (var constraint in Enum.GetValues(typeof(TriggerConstraint))) {
                ddConstraint.options.Add(new TriggerListEntry.DropdownTriggerOption(i++, constraint.ToString(), (int) constraint));
            }
            txtConstraint.text = ((TriggerListEntry.DropdownTriggerOption)ddConstraint.options[ddConstraint.value]).OptionName;
        }
        
        ddConstraint.SetValueWithoutNotify(ddConstraint.options.FindIndex(o =>
            ((TriggerListEntry.DropdownTriggerOption) o).EnumValue == (int) _currentConstraint));
        RecalculateConstraintParameters();
        RecalculateConstraintTargets();
        if (ddConstraintParameter.gameObject.activeSelf) 
            ddConstraintParameter.SetValueWithoutNotify(ddConstraintParameter.options.FindIndex(o =>
            o.text == _currentParameter));
        if (inConstraintParameter.gameObject.activeSelf)
            inConstraintParameter.text = _currentParameter;
        ddConstraintTarget.SetValueWithoutNotify(ddConstraintTarget.options.FindIndex(o =>
            ((TriggerListEntry.DropdownTriggerOption) o).EnumValue == (int) _currentTarget));
    }
    
    public void RefreshInteractability() {
        var isHost = Parent.matchSettings.isHost;
        ddConstraint.interactable = isHost;
        ddConstraintParameter.interactable = isHost;
        ddConstraintTarget.interactable = isHost;
        sChance.interactable = isHost;
        sDelay.interactable = isHost;
        sRepeat.interactable = isHost;
        inConstraintParameter.interactable = isHost;
    }

    public void OnConfirm() {
        Parent.currentEditingEntry.OnExtrasChanged(
            (byte) sChance.value, (byte) sDelay.value, (byte) sRepeat.value,
            _currentConstraint, _currentTarget, _currentParameter
        );
    }
    
    public void OnConstraintChanged() {
        _currentConstraint = (TriggerConstraint) ((TriggerListEntry.DropdownTriggerOption) ddConstraint.options[ddConstraint.value]).EnumValue;
        RecalculateConstraintParameters();
        RecalculateConstraintTargets();
    }
    
    public void OnConstraintParameterChanged() {
        _currentParameter = ddConstraintParameter.options[ddConstraintParameter.value].text;
    }
    
    public void OnConstraintNumberParameterChanged() {
        _currentParameter = inConstraintParameter.text;
    }
    
    public void OnConstraintTargetChanged() {
        _currentTarget = (TriggerTarget) ((TriggerListEntry.DropdownTriggerOption) ddConstraintTarget.options[ddConstraintTarget.value]).EnumValue;
    }
    
    public void OnChanceChanged() {
        txtChance.text = sChance.value + "%";
    }
    
    public void OnDelayChanged() {
        txtDelay.text = sDelay.value + "s";
    }
    
    public void OnRepeatChanged() {
        txtRepeat.text = sRepeat.value + "x";
    }
    
    public void RecalculateConstraintTargets() {
        ddConstraintTarget.ClearOptions();
        
        if (TriggerMappings.NonPeopleConstraints.Contains(_currentConstraint)) {
            txtConstraintTarget.text = "";
            return;
        }
        
        var i = 0;
        foreach (var target in Enum.GetValues(typeof(TriggerTarget))) {
            if (TriggerMappings.IncompatibleConstraintTargets.Contains((TriggerTarget) target)) {
                continue;
            }
            ddConstraintTarget.options.Add(new TriggerListEntry.DropdownTriggerOption(i++, target.ToString(), (int) target));
        }
        
        ddConstraintTarget.SetValueWithoutNotify(ddConstraintTarget.options.FindIndex(o =>
            ((TriggerListEntry.DropdownTriggerOption) o).EnumValue == (int) _currentTarget));
        txtConstraintTarget.text = ((TriggerListEntry.DropdownTriggerOption)ddConstraintTarget.options[ddConstraintTarget.value]).OptionName;
    }
    
    public void RecalculateConstraintParameters() {
        if (TriggerMappings.ConstraintNumberParameters.Contains(_currentConstraint)) {
            // show input field
            ddConstraintParameter.gameObject.SetActive(false);
            inConstraintParameter.gameObject.SetActive(true);
            if (!int.TryParse(_currentParameter, out _)) {
                inConstraintParameter.text = "1";
                _currentParameter = "1";
            }
        } else {
            // preset parameters
            ddConstraintParameter.gameObject.SetActive(true);
            inConstraintParameter.gameObject.SetActive(false);
            ddConstraintParameter.ClearOptions();
            
            if (!TriggerMappings.ConstraintParameters.TryGetValue(_currentConstraint, out var constraintParameters)) {
                txtConstraintParameter.text = "";
                return;
            }
            
            foreach (var parameter in constraintParameters) {
                ddConstraintParameter.options.Add(new TMP_Dropdown.OptionData(parameter));
            }
            
            txtConstraintParameter.text = ddConstraintParameter.options[ddConstraintParameter.value].text;
            // _currentParameter = "";
        }
    }
}
