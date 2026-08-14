using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float distance = 0.3f;
    [SerializeField] private Transform origin;
    [SerializeField] private LayerMask groundLayers;

    public bool isGrounded;

    private Vector3 OriginPosition => origin != null ? origin.position : transform.position;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(OriginPosition, Vector3.down * distance);
    }

    public bool IsGrounded()
    {
        isGrounded = Physics.Raycast(OriginPosition, Vector3.down, distance, groundLayers);
        return isGrounded;
    }
}