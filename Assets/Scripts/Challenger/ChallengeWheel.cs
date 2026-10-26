using System;
using NSMB;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using NSMB.Utilities.Extensions;

public class ChallengeWheel : MonoBehaviour
{
    public String[] PossibleChallenges;
    public String CurrentChallenge = "NOTHING";
    public GameObject entryTemplate;

    private int spinningTimer = 0;
    
    void Start()
    {
        foreach (var possibleChallenge in PossibleChallenges)
        {
            var newEntry = Instantiate(entryTemplate, entryTemplate.transform.parent);
            newEntry.GetComponentInChildren<TMP_Text>().text = possibleChallenge;
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.shiftKey.isPressed)
        {
            if (keyboard[Key.Digit1].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit2].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit3].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit4].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit5].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit6].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit7].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit8].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit9].wasPressedThisFrame) SetExplicitly("NOTHING");
            if (keyboard[Key.Digit0].wasPressedThisFrame) SetExplicitly("NOTHING");
        }
        else if (keyboard.ctrlKey.isPressed)
        {
            if (keyboard[Key.Digit1].wasPressedThisFrame) SetExplicitly("Invisible Ground");
            if (keyboard[Key.Digit2].wasPressedThisFrame) SetExplicitly("Invisible Stars");
            if (keyboard[Key.Digit3].wasPressedThisFrame) SetExplicitly("Invisible Enemies & Powerups");
            if (keyboard[Key.Digit4].wasPressedThisFrame) SetExplicitly("Invisible Me");
            if (keyboard[Key.Digit5].wasPressedThisFrame) SetExplicitly("Swapped Enemies & Powerups");
            if (keyboard[Key.Digit6].wasPressedThisFrame) SetExplicitly("Offset Player Hitboxes");
            if (keyboard[Key.Digit7].wasPressedThisFrame) SetExplicitly("Swapped Left & Right Inputs");
            if (keyboard[Key.Digit8].wasPressedThisFrame) SetExplicitly("No Running");
            if (keyboard[Key.Digit9].wasPressedThisFrame) SetExplicitly("Blurry");
            if (keyboard[Key.Digit0].wasPressedThisFrame) SetExplicitly("Jumping Continuously");
        }
    }

    public void Spin()
    {
        
    }

    public void SetExplicitly(String challenge)
    {
        CurrentChallenge = challenge;
        GlobalController.Instance.sfx.PlayOneShot(SoundEffect.World_Coin_Drop);
        GlobalController.Instance.ShowTooltip($"current challenge:\n{challenge}");
    }
}
