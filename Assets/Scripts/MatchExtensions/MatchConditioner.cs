using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manager for the custom rulesets pairs. Basically the heart of the Custom Match-inator.
// Uses the ActiveRules list from SessionData. Defines the functions for each action.
// Any entity can trigger a condition by calling the corresponding function in here, with the condition as parameter.
// If the condition is in the list of active rules, the corresponding action is executed.
public class MatchConditioner : Singleton<MatchConditioner>
{
    public class RuleEqualityComparer : IEqualityComparer<Rule> {
        public bool Equals(Rule x, Rule y)
        {
            if (ReferenceEquals(x, y)) {
                return true;
            }

            if (ReferenceEquals(x, null)) {
                return false;
            }

            if (ReferenceEquals(y, null)) {
                return false;
            }

            if (x.GetType() != y.GetType()) {
                return false;
            }

            return x.Condition == y.Condition && x.Action == y.Action && x.ConditionTarget == y.ConditionTarget &&
                   x.ActionTarget == y.ActionTarget && x.ConditionParameter == y.ConditionParameter &&
                   x.ActionParameter == y.ActionParameter;
        }

        public int GetHashCode(Rule obj) {
            return HashCode.Combine((int) obj.Condition, (int) obj.Action, (int) obj.ConditionTarget,
                (int) obj.ActionTarget, obj.ConditionParameter, obj.ActionParameter);
        }
    }
    public static readonly RuleEqualityComparer RuleComparer = new();

    //---Private Variables
    private HashSet<Rule> _activeRules = new(RuleComparer);
    private static readonly Rule[] ForbiddenRules = {
        new(Rule.PossibleConditions.GrabbedStar, Rule.PossibleActions.GiveStar),
        new(Rule.PossibleConditions.GrabbedCoin, Rule.PossibleActions.GiveCoin),
    };

    public void Awake() {
        Set(this);
    }

    //---Public Functions
    public void SetActiveRules(HashSet<Rule> rules) {
        _activeRules = rules;
    }

    public string ActiveRulesToString() {
        return string.Join(", ", _activeRules.Select(rule => rule.ToString()));
    }

    public bool AddRule(Rule rule) {
        if (IsRuleForbidden(rule) || !_activeRules.Add(rule))
            return false;

        SessionData.Instance.SetActiveRules(_activeRules);
        return true;
    }

    public void RemoveRule(Rule rule) {
        _activeRules.Remove(rule);
        SessionData.Instance.SetActiveRules(_activeRules);
    }

    public void ClearRules() {
        _activeRules.Clear();
        SessionData.Instance.SetActiveRules(_activeRules);
    }

    public static bool IsRuleForbidden(Rule rule) {
        return ForbiddenRules.Contains(rule);
    }

    public bool IsRuleActive(Rule rule) {
        return _activeRules.Contains(rule);
    }
}
