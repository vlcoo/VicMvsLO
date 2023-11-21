using System.Collections.Generic;
using UnityEngine;

public class SwingingPlatform : MonoBehaviour
{
    public int motorSpeed = 10;
    private readonly List<Collider2D> contacts = new();
    private HingeJoint2D anchor;
    private Rigidbody2D body;
    private JointMotor2D dummyMotor;

// Start is called before the first frame update
    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anchor = GetComponent<HingeJoint2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        anchor.useMotor = body.GetContacts(contacts) == 0;
        if (!anchor.useMotor) return;

        dummyMotor = anchor.motor;
        dummyMotor.motorSpeed = (body.rotation > 0 ? 1 : -1) * motorSpeed;
        anchor.motor = dummyMotor;
    }
}