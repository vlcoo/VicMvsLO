using Photon.Pun;
using UnityEngine;

public class BahableEntity : MonoBehaviour
{
    public static int BAH_STRENGTH = 3;
    private KillableEntity child;

    // Start is called before the first frame update
    private void Start()
    {
        child = GetComponent<KillableEntity>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void bah()
    {
        if (child.body == null) return;
        if (child.TryGetComponent(out KoopaWalk koopa))
        {
            if (koopa.shell && child.body.velocity.x != 0) return;
            if (!koopa.shell) child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.FacingLeftTween);
        }
        else
        {
            child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.FacingLeftTween);
        }

        child.body.velocity = new Vector2(child.body.velocity.x, BAH_STRENGTH);
    }
}