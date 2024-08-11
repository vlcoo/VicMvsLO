using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Ruleset", menuName = "ScriptableObjects/Ruleset")]
public class RulesetData : ScriptableObject
{
    [Header("Custom match-inator settings")]
    [Tooltip("List of conditions to be applied. Corresponds to the action with the same index in the list below.")] [SerializeField] public string[] rulePairsConditions;
    [Tooltip("List of actions to be applied. Corresponds to the condition with the same index in the list above.")] [SerializeField] public string[] rulePairsActions;
    [Tooltip("List of persistent effects to be applied.")] [SerializeField] public string[] specials;
    [Tooltip("Should the amount of coins other players have be shown in the scoreboard?")] [SerializeField] public bool showCoins;
    [Tooltip("Should other players, and their nametags, be hidden from the track?")] [SerializeField] public bool hideTrack;

    [Header("MvL settings")]
    [Tooltip("Stars to get to win. -1 to disable or -2 to keep unchanged.")] [SerializeField] [Min(-2)] public int stars;
    [Tooltip("Coins to grab to get powerup. -1 to disable or -2 to keep unchanged.")] [SerializeField] [Min(-2)] public int coins;
    [Tooltip("Lives of each player. -1 to disable or -2 to keep unchanged.")] [SerializeField] [Min(-2)] public int lives;
    [Tooltip("Time limit in seconds. -1 to disable or -2 to keep unchanged.")] [SerializeField] [Min(-2)] public int timeSeconds;

    [Tooltip("Chances of each powerup to appear (multiplier). Can be 0, 1 or 4. - " +
             "BlueShell, FireFlower, IceFlower, MegaMushroom, MiniMushroom, Mushroom, PropellerMushroom, Star -")] [SerializeField]
    public int[] powerups = { 1, 1, 1, 1, 1, 1, 1, 1 };

    [Header("Ruleset info")]
    [Tooltip("Name shown in the menu.")] [SerializeField] public string legalName;
    [Tooltip("Extra details or help about this gamemode.")] [SerializeField] [Multiline] public string description;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (powerups.Length != 8)
        {
            Debug.LogWarning("Powerup chances array must have exactly 8 elements.");
            powerups = new int[] { 1, 1, 1, 1, 1, 1, 1, 1 };
        }

        for (var i = 0; i < powerups.Length; i++)
        {
            if (powerups[i] != 0 && powerups[i] != 1 && powerups[i] != 4)
            {
                Debug.LogWarning("Powerup chances must be 0, 1 or 4.");
                powerups[i] = 1;
            }
        }

        if (rulePairsConditions.Length != rulePairsActions.Length)
        {
            Debug.LogWarning("Rule pairs must have the same length. Please correct manually.");
        }
    }
    #endif
}
