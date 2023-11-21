using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchRuleDataEntry : IEquatable<MatchRuleDataEntry>
{
    public string Condition, Action;
    public List<string> Parameters;

    public MatchRuleDataEntry(string cond, string act, List<string> param)
    {
        Condition = cond;
        Action = act;
        Parameters = param;
    }

    public bool Equals(MatchRuleDataEntry other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Condition == other.Condition && Action == other.Action;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(MatchRuleDataEntry)) return false;
        return Equals((MatchRuleDataEntry)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Condition, Action);
    }
}

public class MatchRuleListEntry : MonoBehaviour, IEquatable<MatchRuleListEntry>
{
    public string Condition, Action;
    public List<string> Parameters;

    public TMP_Text lbl;
    public Selectable removeButton;

    public bool Equals(MatchRuleListEntry other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Condition == other.Condition && Action == other.Action;
    }

    public void setRules(string cond, string act)
    {
        if (cond.Equals("") || act.Equals("")) return;

        Condition = cond;
        Action = act;
        if (lbl is not null)
        {
            var sanitizedCond = Regex.Replace(cond, "(\\B[A-Z0-9])", " $1");
            var sanitizedAct = Regex.Replace(act, "(\\B[A-Z0-9])", " $1").Replace("Act ", "");
            lbl.text = sanitizedCond + " .. " + sanitizedAct;
        }
    }

    public void onRemoveButtonPressed()
    {
    }

    public void Deserialize(MatchRuleDataEntry rule)
    {
        Condition = rule.Condition;
        Action = rule.Action;
        Parameters = rule.Parameters;
    }

    public MatchRuleDataEntry Serialize()
    {
        MatchRuleDataEntry rule = new(Condition, Action, Parameters);
        return rule;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(MatchRuleListEntry)) return false;
        return Equals((MatchRuleListEntry)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Condition, Action);
    }
}