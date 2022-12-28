using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MatchConditioner : MonoBehaviour
{
    public string a = "";
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConditionActioned(int byWhomsID, string condition)
    {
        PlayerController player = PhotonView.Find(byWhomsID).GetComponent<PlayerController>();
        ConditionActioned(player, condition);
    }

    public void ConditionActioned(PlayerController byWhom, string condition)
    {
        switch (condition)
        {
            case "GotCoin":
                break;
            
            case "GotPowerup":
                break;
            
            case "LostPowerup":
                break;
            
            case "GotStar":
                break;

            case "KnockedBack":
                break;

            case "Stomped":
                break;
            
            case "Died":
                break;

            case "Jumped":
                byWhom.Death(false, false);
                break;

            case "LookedRight":
                break;
            
            case "LookedLeft":
                break;

            case "LookedDown":
                break;

            case "LookedUp":
                break;

            case "Ran":
                break;
        }
    }
}
