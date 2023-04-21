using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BahableEntity : MonoBehaviour
{
    private KillableEntity child;
    public static int BAH_STRENGTH = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        child = GetComponent<KillableEntity>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void bah()
    {
        if (child.body == null) return;
        if (child.TryGetComponent(out KoopaWalk koopa))
        {
            if (koopa.shell && child.body.velocity.x != 0) return;
            if (!koopa.shell) child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.left);
        }
        else child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.left);
        child.body.velocity = new Vector2(child.body.velocity.x, BAH_STRENGTH);
    }
}
