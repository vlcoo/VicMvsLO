using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using UnityEngine;

public class Togglerizer : MonoBehaviour
{
    public HashSet<string> currentEffects = new();
    public Dictionary<string, int> powerupChanceMultipliers;

    private void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.SpecialRules, out Dictionary<string, bool> currentEffectsDict);
        Utils.GetCustomProperty(Enums.NetRoomProperties.PowerupChances, out powerupChanceMultipliers);
        if (currentEffectsDict != null) currentEffects = currentEffectsDict.Keys.ToHashSet();
        if (powerupChanceMultipliers == null || powerupChanceMultipliers.All(pair => pair.Value == 3))
            powerupChanceMultipliers = new Dictionary<string, int>
            {
                { "BlueShell", 1 },
                { "FireFlower", 1 },
                { "IceFlower", 1 },
                { "MegaMushroom", 1 },
                { "MiniMushroom", 1 },
                { "Mushroom", 1 },
                { "PropellerMushroom", 1 },
                { "Star", 1 }
            };
        Debug.Log(string.Join("; ", powerupChanceMultipliers));
    }
}