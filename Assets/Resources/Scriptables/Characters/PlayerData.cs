using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 0)]
public class PlayerData : ScriptableObject {
    public string soundFolder, prefab, uistring, legalName;
    public Sprite loadingSmallSprite, loadingBigSprite, readySprite, silhouetteSprite;
    public RuntimeAnimatorController smallOverrides, largeOverrides;
    public bool isBowsers;
}