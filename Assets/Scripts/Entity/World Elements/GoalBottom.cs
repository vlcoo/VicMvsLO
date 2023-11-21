using UnityEngine;

public class GoalBottom : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.gameObject.GetComponent<PlayerController>();
        if (player is null) return;

        player.goalReachedBottom = true;
    }
}