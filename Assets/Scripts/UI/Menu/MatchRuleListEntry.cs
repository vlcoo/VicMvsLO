using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchRuleListEntry : MonoBehaviour
{
    private string Condition, Action;
    public TMP_Text lbl;

    public GameObject removeButton;
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
        lbl.text = cond + " .. " + act;
    }

    public void onRemoveButtonPressed()
    {
        
    }
}
