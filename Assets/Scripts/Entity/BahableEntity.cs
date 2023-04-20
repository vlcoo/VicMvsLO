using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BahableEntity : MonoBehaviour
{
    private KillableEntity child;
    public static int BAH_STRENGTH = 8;
    
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
        child.body.velocity = new Vector2(child.body.velocity.x, BAH_STRENGTH);
        child.photonView.RPC(nameof(child.SetLeft), RpcTarget.All, !child.left);
    }
}
