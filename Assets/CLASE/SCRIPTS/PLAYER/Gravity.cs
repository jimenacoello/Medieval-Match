using Fusion;
using System;
using UnityEngine;

public class Gravity : NetworkBehaviour
{
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] private bool usesGroundCheck;

    [Networked] private float acceleration { get; set; }

    private GroundCheck groundCheck;
    private Rigidbody rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        if (usesGroundCheck)
        {
            groundCheck = GetComponent<GroundCheck>();
        }
    }

    public override void FixedUpdateNetwork()
    {
        ApplyGravityLogic();
    }

    private void ApplyGravityLogic()
    {
        if (usesGroundCheck && groundCheck != null)
        {
            if (groundCheck.IsGrounded())
            {
                acceleration = 0;
                return;
            }
        }

        acceleration += Runner.DeltaTime;

        Vector3 gravityVector = Vector3.down * gravityForce * acceleration;
        rb.AddForce(gravityVector, ForceMode.Acceleration);
    }
}