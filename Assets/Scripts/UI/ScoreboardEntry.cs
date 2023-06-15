using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using NSMB.Utils;

public class ScoreboardEntry : MonoBehaviour {

    [SerializeField] private TMP_Text nameText, valuesText;
    [SerializeField] private Image background;

    public PlayerController target;

    private int playerId, currentLives, currentStars, currentLaps;
    private bool rainbowEnabled;

    public void Start() {
        if (!target) {
            enabled = false;
            return;
        }

        playerId = target.playerId;
        nameText.text = target.photonView.Owner.GetUniqueNickname();

        Color c = target.AnimationController.GlowColor;
        background.color = new(c.r, c.g, c.b, 0.5f);

        rainbowEnabled = target.photonView.Owner.HasRainbowName();
    }

    public void Update() {
        CheckForTextUpdate();

        if (rainbowEnabled)
            nameText.colorGradientPreset = Utils.GetRainbowColor();
    }

    public void CheckForTextUpdate() {
        if (!target) {
            // our target lost all lives (or dc'd)
            background.color = new(0.4f, 0.4f, 0.4f, 0.5f);
            return;
        }
        if (target.lives == currentLives && target.stars == currentStars && target.laps == currentLaps)
            // No changes.
            return;

        currentLives = target.lives;
        currentStars = target.stars;
        currentLaps = target.laps;
        UpdateText();
        ScoreboardUpdater.instance.Reposition();
    }

    public void UpdateText() {
        string txt = "";
        if (currentLives >= 0)
            txt += target.character.uistring + Utils.GetSymbolString(currentLives.ToString());
        if (GameManager.Instance.starRequirement > 0)
            txt += Utils.GetSymbolString($"S{currentStars}");
        if (GameManager.Instance.lapRequirement > 1)
            txt += Utils.GetSymbolString($"L{currentLaps}");

        valuesText.text = txt;
    }

    public class EntryComparer : IComparer<ScoreboardEntry> {
        public int Compare(ScoreboardEntry x, ScoreboardEntry y) {
            if (x.target == null ^ y.target == null)
                return x.target == null ? 1 : -1;

            if (GameManager.Instance.raceLevel)
            {
                if (x.currentLaps == y.currentLaps || x.currentLives == 0 || y.currentLives == 0)
                    return x.playerId - y.playerId;
                return y.currentLaps - x.currentLaps;
            }
            
            if (x.currentStars == y.currentStars || x.currentLives == 0 || y.currentLives == 0) {
                if (Mathf.Max(0, x.currentLives) == Mathf.Max(0, y.currentLives))
                    return x.playerId - y.playerId;

                return y.currentLives - x.currentLives;
            }

            return y.currentStars - x.currentStars;
        }
    }
}