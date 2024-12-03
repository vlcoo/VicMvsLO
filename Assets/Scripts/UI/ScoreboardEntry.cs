using System.Collections.Generic;
using NSMB.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, valuesText;
    [SerializeField] private Image background;

    public PlayerController target;

    private int playerId, currentLives, currentStars, currentCoins;
    private bool rainbowEnabled;

    public void Start()
    {
        if (!target)
        {
            enabled = false;
            return;
        }

        playerId = target.playerId;
        nameText.text = target.photonView.Owner.GetUniqueNickname();

        var c = target.AnimationController.GlowColor;
        background.color = new Color(c.r, c.g, c.b, 0.5f);

        rainbowEnabled = target.photonView.Owner.HasSpecialName();
    }

    public void Update()
    {
        CheckForTextUpdate();

        if (rainbowEnabled)
            nameText.colorGradientPreset = Utils.GetRainbowColor();
    }

    public void CheckForTextUpdate()
    {
        if (!target)
        {
            // our target lost all lives (or dc'd)
            background.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            return;
        }

        if (target.lives == currentLives && target.stars == currentStars &&
            target.coins == currentCoins)
            // No changes.
            return;

        currentLives = target.lives;
        currentStars = target.stars;
        currentCoins = target.coins;
        UpdateText();
        ScoreboardUpdater.instance.Reposition();
    }

    private void UpdateText()
    {
        var txt = "";
        if (currentLives >= 0)
            txt += target.character.uistring + Utils.GetNumberString(currentLives.ToString());
        if (GameManager.Instance.starRequirement > 0)
            txt += " <sprite name=\"hudnumber_star\">" + Utils.GetNumberString($"{currentStars}");

        valuesText.text = txt;
    }

    public class EntryComparer : IComparer<ScoreboardEntry>
    {
        public int Compare(ScoreboardEntry x, ScoreboardEntry y)
        {
            if ((x.target == null) ^ (y.target == null)) return x.target == null ? -1 : 1;
            var comparisonResult = 0;

            // if no race level or a tie then sort by stars, if a tie then by lives, if a tie then by coins (only if enabled), if a tie then id.
            if (comparisonResult != 0) return -comparisonResult;
            comparisonResult = x.currentStars.CompareTo(y.currentStars);
            if (comparisonResult != 0) return -comparisonResult;
            comparisonResult = x.currentLives.CompareTo(y.currentLives);
            if (comparisonResult != 0) return -comparisonResult;

            comparisonResult = x.playerId.CompareTo(y.playerId);
            return -comparisonResult;
        }
    }
}