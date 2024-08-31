using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Ruleset", menuName = "ScriptableObjects/Ruleset")]
public class RulesetData : ScriptableObject
{
    [Header("Custom match-inator settings")]
    [Tooltip("List of conditions to be applied. Corresponds to the action with the same index in the list below.")]
    [SerializeField]
    public string[] rulePairsConditions;

    [Tooltip("List of actions to be applied. Corresponds to the condition with the same index in the list above.")]
    [SerializeField]
    public string[] rulePairsActions;

    [Tooltip("List of persistent effects to be applied.")] [SerializeField]
    public string[] specials;

    [Tooltip("Should the amount of coins other players have be shown in the scoreboard?")] [SerializeField]
    public bool showCoins;

    [Tooltip("Should other players, and their nametags, be hidden from the track?")] [SerializeField]
    public bool hideTrack;

    [Header("MvL settings")] [Tooltip("Laps to do to win. -1 to keep unchanged.")] [SerializeField] [Min(-1)]
    public int laps = -1;

    [Tooltip("Stars to get to win. 0 to disable or -1 to keep unchanged.")] [SerializeField] [Min(-1)]
    public int stars = -1;

    [Tooltip("Coins to grab to get powerup. 0 to disable or -1 to keep unchanged.")] [SerializeField] [Min(-1)]
    public int coins = -1;

    [Tooltip("Lives of each player. 0 to disable or -1 to keep unchanged.")] [SerializeField] [Min(-1)]
    public int lives = -1;

    [Tooltip("Time limit in seconds. 0 to disable or -1 to keep unchanged.")] [SerializeField] [Min(-1)]
    public int timeSeconds = -1;

    [Tooltip("Chances of each powerup to appear (multiplier). Can be 0, 1 or 4. - " +
             "BlueShell, FireFlower, IceFlower, MegaMushroom, MiniMushroom, Mushroom, PropellerMushroom, Star -")]
    [SerializeField]
    public int[] powerups = { 1, 1, 1, 1, 1, 1, 1, 1 };

    [Header("Ruleset info")] [Tooltip("Name shown in the menu.")] [SerializeField]
    public string legalName;

    [Tooltip("Extra details or help about this gamemode.")] [SerializeField] [Multiline]
    public string description;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (powerups.Length != 8)
        {
            Debug.LogWarning("Powerup chances array must have exactly 8 elements.");
            powerups = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
        }

        for (var i = 0; i < powerups.Length; i++)
            if (powerups[i] != 0 && powerups[i] != 1 && powerups[i] != 4)
            {
                Debug.LogWarning("Powerup chances must be 0, 1 or 4.");
                powerups[i] = 1;
            }

        if (rulePairsConditions.Length != rulePairsActions.Length)
            Debug.LogWarning("Rule pairs must have the same length. Please correct manually.");
    }
#endif

    public bool IsValid()
    {
        var v = rulePairsActions.Length == rulePairsConditions.Length
                && powerups.Length == 8
                && powerups.All(x => x is 0 or 1 or 4)
                && stars <= 99
                && coins <= 99
                && lives <= 99
                && laps <= 99
                && laps != 0
                && timeSeconds <= 3599
                && (rulePairsActions.Length == 0 ||
                    rulePairsActions.Any(MainMenuManager.Instance.POSSIBLE_ACTIONS.Contains))
                && (rulePairsConditions.Length == 0 ||
                    rulePairsConditions.Any(MainMenuManager.Instance.POSSIBLE_CONDITIONS.Contains));

        if (!v) Debug.LogWarning("Ruleset is invalid.");
        return v;
    }
}