using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class Checkpoin : MonoBehaviour
{
    private Animation animation;
    public GameObject checkpointBody;
    
    // Start is called before the first frame update
    void Start()
    {
        animation = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerController player = col.gameObject.GetComponent<PlayerController>();
        if (player is null || player.gotCheckpoint) return;

        player.gotCheckpoint = true;
        animation.Play();
        GameManager.Instance.sfx.PlayOneShot(Enums.Sounds.World_Checkpoint.GetClip());
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.ResetTiles, null, SendOptions.SendReliable);
    }
}
