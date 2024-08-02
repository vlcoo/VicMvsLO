using System;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class BahableEntity : MonoBehaviour
{
    private static int BAH_STRENGTH = 4;
    private KillableEntity child;

    private void Start()
    {
        child = GetComponent<KillableEntity>();
    }

    public bool Bah()
    {
        if (!child.body || child.dead) return false;
        if (child.TryGetComponent(out KoopaWalk koopa))
        {
            if (koopa.shell && child.body.velocity.x != 0) return false;
            if (!koopa.shell) child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.FacingLeftTween);
        }
        else
        {
            child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.FacingLeftTween);
        }

        child.body.velocity = new Vector2(child.body.velocity.x, BAH_STRENGTH);
        return true;
    }
}