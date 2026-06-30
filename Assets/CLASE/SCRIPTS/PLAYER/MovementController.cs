using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class MovementController : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float mouseSensitivity = 10.0f;

    [SerializeField] public SimpleKCC _simpleKCC;

    public struct GameplayInput : INetworkInput
    {
        public Vector2 MoveDirection;
        public Vector2 LookRotationDelta;
        public NetworkBool IsRunning;
    }

    public override void Spawned()
    {
        if (_simpleKCC == null) _simpleKCC = GetComponent<SimpleKCC>();

        _simpleKCC.SetPosition(transform.position);
        _simpleKCC.SetActive(true);
    }

    public override void FixedUpdateNetwork()
    {
        if (FindAnyObjectByType<MatchManager>() != null && FindAnyObjectByType<MatchManager>().isMatchOver) return;

        if (GetInput(out GameplayInput input))
        {
            _simpleKCC.AddLookRotation(new Vector2(-input.LookRotationDelta.y, input.LookRotationDelta.x) * mouseSensitivity);

            // Procesar el movimiento físico
            Movement(input);

            if (HasInputAuthority)
            {
                RPC_UpdateAnimations(input.MoveDirection, input.MoveDirection.magnitude > 0.1f);
            }
        }
    }

    private void Movement(GameplayInput input)
    {
        float currentSpeed = input.IsRunning ? runSpeed : walkSpeed;

        Vector3 inputDirection = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);

        Vector3 worldDirection = transform.rotation * inputDirection;

        _simpleKCC.Move(worldDirection * currentSpeed);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_UpdateAnimations(Vector2 moveDir, bool isMoving)
    {
        if (_animator == null) return;
        _animator.SetBool("IsWalking", isMoving);
        _animator.SetFloat("WalkingZ", moveDir.y);
        _animator.SetFloat("WalkingX", moveDir.x);
    }
}