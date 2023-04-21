using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using UnityEngine;

public class Togglerizer : MonoBehaviour
{
    public List<string> currentEffects = new();

    void Start()
    {
        Dictionary<string, bool> currentEffectsDict;
        Utils.GetCustomProperty(Enums.NetRoomProperties.SpecialRules, out currentEffectsDict);
        currentEffects = currentEffectsDict.Keys.ToList();
    }
}
