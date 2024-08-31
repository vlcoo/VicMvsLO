using UnityEngine;

public class BahableEntity : MonoBehaviour
{
    private static readonly int BAH_STRENGTH = 4;
    private KillableEntity child;

    private void Start()
    {
        child = GetComponent<KillableEntity>();
    }

    // A bah is just a little jump (or animation) of the entity. In the original NSMB, they also change directions, but
    // we don't do that here so enemies' positions are the same for all players (since bahs happen at diff times for each person).
    public bool Bah()
    {
        if (!child.body || child.dead) return false;

        // Exceptions to the bah rule: If a koopa or spiny is in shell and moving, ignore.
        if (child.TryGetComponent(out KoopaWalk koopa))
            if (koopa.shell && child.body.velocity.x != 0)
                return false;
        if (child.TryGetComponent(out SpinyWalk spiny))
            if (spiny.shell && child.body.velocity.x != 0)
                return false;

        child.body.velocity = new Vector2(child.body.velocity.x, BAH_STRENGTH);
        return true;
    }
}