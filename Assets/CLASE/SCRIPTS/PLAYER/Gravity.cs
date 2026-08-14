using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class Gravity : NetworkBehaviour
{
    [SerializeField] private float gravityForce = 19.6f;
    [SerializeField] private bool usesGroundCheck;

    [Networked] private float acceleration { get; set; }

    private GroundCheck groundCheck;
    private SimpleKCC kcc;
    private Rigidbody rb;

    public override void Spawned()
    {
        kcc = GetComponent<SimpleKCC>();
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
        if (usesGroundCheck && groundCheck != null && groundCheck.IsGrounded())
        {
            acceleration = 0;
            return;
        }

        acceleration += Runner.DeltaTime;

        if (kcc != null)
        {
            kcc.SetGravity(-gravityForce * acceleration);
        }
        else if (rb != null && !rb.isKinematic)
        {
            Vector3 gravityVector = Vector3.down * gravityForce * acceleration;
            rb.AddForce(gravityVector, ForceMode.Acceleration);
        }
    }
}