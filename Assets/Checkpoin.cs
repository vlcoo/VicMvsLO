using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.Serialization;

public class Checkpoin : MonoBehaviour
{
    [FormerlySerializedAs("animation")] public Animation animationComponent;
    public GameObject checkpointBody;
    
    // Start is called before the first frame update
    void Start()
    {
        animationComponent = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        
    }
}
