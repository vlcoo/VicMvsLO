using UnityEngine;

public class GoalBottom : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.gameObject.GetComponent<PlayerController>();
        if (player is null) return;

        player.goalReachedBottom = true;
    }
}