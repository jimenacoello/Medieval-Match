using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform player;

    [Header("Head Bobbing")]
    [SerializeField, Range(0, 0.1f)] private float walkingAmplitude = 0.015f;
    [SerializeField, Range(0, 0.1f)] private float runningAmplitude = 0.03f;
    [SerializeField, Range(0, 15)] private float walkingFrequency = 10.0f;
    [SerializeField, Range(10, 20)] private float runningFrequency = 18f;
    [SerializeField] private float resetPosSpeed = 5.0f;

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.localPosition;
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null) cam.enabled = false;

            AudioListener listener = GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    public override void Render()
    {
        if (!HasInputAuthority) return;

        if (GetInput(out MovementController.GameplayInput input))
        {
            bool isMoving = input.MoveDirection.sqrMagnitude > 0.1f;

            if (isMoving)
            {
                float freq = input.IsRunning ? runningFrequency : walkingFrequency;
                float amp = input.IsRunning ? runningAmplitude : walkingAmplitude;

                Vector3 bobPos = Vector3.zero;
                bobPos.y = Mathf.Sin(Runner.SimulationTime * freq) * amp;
                bobPos.x = Mathf.Cos(Runner.SimulationTime * freq * 0.5f) * amp;

                transform.localPosition = startPos + bobPos;
            }
            else
            {
                ResetPosition();
            }
        }
        else
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Runner.DeltaTime);
    }
}