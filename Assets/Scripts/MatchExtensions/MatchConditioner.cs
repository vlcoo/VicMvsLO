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

    //---Private Variables
    private HashSet<Rule> ActiveRules => SessionData.Instance.ActiveRules;
    private static Rule[] _forbiddenRules = {
        new(Rule.PossibleConditions.GrabbedStar, Rule.PossibleActions.GiveStar),
        new(Rule.PossibleConditions.GrabbedCoin, Rule.PossibleActions.GiveCoin),
    };

    //---Public Functions
    public static bool IsRuleForbidden(Rule rule) {
        return _forbiddenRules.Contains(rule);
    }

    public bool IsRuleActive(Rule rule) {
        return ActiveRules.Contains(rule);
    }
}
