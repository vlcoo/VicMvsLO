using System.Collections.Generic;
using UnityEngine;

public class SwingingPlatform : MonoBehaviour
{
    private Rigidbody2D body;
    private HingeJoint2D anchor;
    private JointMotor2D dummyMotor;
    private List<Collider2D> contacts = new();
    public int motorSpeed = 10;
    
// Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anchor = GetComponent<HingeJoint2D>();
    }

    // Update is called once per frame
    void Update()
    {
        anchor.useMotor = body.GetContacts(contacts) == 0;
        if (!anchor.useMotor) return;
        
        dummyMotor = anchor.motor;
        dummyMotor.motorSpeed = (body.rotation > 0 ? 1 : -1) * motorSpeed;
        anchor.motor = dummyMotor;
    }
}
