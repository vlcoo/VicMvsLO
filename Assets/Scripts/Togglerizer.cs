using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using UnityEngine;

public class Togglerizer : MonoBehaviour
{
    public List<string> currentEffects = new();
    public Dictionary<string, int> powerupChanceMultipliers;

    void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.SpecialRules, out Dictionary<string, bool> currentEffectsDict);
        Utils.GetCustomProperty(Enums.NetRoomProperties.PowerupChances, out powerupChanceMultipliers);
        if (currentEffectsDict != null) currentEffects = currentEffectsDict.Keys.ToList();
        powerupChanceMultipliers ??= new Dictionary<string, int>
        {
            { "1-Up", 1 },
            { "BlueShell", 1 },
            { "FireFlower", 1 },
            { "IceFlower", 1 },
            { "MegaMushroom", 1 },
            { "MiniMushroom", 1 },
            { "Mushroom", 1 },
            { "PropellerMushroom", 1 },
            { "Star", 1 },
        };
    }
}
