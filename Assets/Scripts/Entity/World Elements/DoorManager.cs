using Photon.Pun;
using UnityEngine;

public class DoorManager : MonoBehaviourPun
{
    public bool entryAllowed = true, fadeOutMusic, isGoal;
    public DoorManager otherDoor;
    public int playersEnteringCount;
    public Animator animator;

    [PunRPC]
    public void SomeoneEntered(bool isDestination)
    {
        playersEnteringCount++;
        animator.SetBool("destination", isDestination);
        animator.SetBool("opened", true);
    }

    [PunRPC]
    public void SomeoneExited()
    {
        playersEnteringCount--;
        if (playersEnteringCount == 0) animator.SetBool("opened", false);
    }
}