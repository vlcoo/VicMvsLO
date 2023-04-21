using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchRuleListEntry : MonoBehaviour, IEquatable<MatchRuleListEntry>
{
    public string Condition, Action;
    public TMP_Text lbl;

    public Selectable removeButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setRules(string cond, string act)
    {
        if (cond.Equals("") || act.Equals("")) return;
        
        Condition = cond;
        Action = act;
        if (lbl is not null)
        {
            string sanitizedCond = Regex.Replace(cond, "(\\B[A-Z0-9])", " $1");
            string sanitizedAct = Regex.Replace(act, "(\\B[A-Z0-9])", " $1").Replace("Act ", "");
            lbl.text = sanitizedCond + " .. " + sanitizedAct;
        }
    }

    public void onRemoveButtonPressed()
    {
        
    }

    public bool Equals(MatchRuleListEntry other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Condition == other.Condition && Action == other.Action;
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
