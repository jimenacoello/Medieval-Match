using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class CameraController : NetworkBehaviour
{
    private InputInfo input;
    private SimpleKCC _simpleKCC;

    [Header("Camera Settings")] 
    [SerializeField] private Transform player;


    [Header("Blob Movement")]
    [SerializeField] private bool moveHead;
    [SerializeField] private float walkingSpeed = 1f;
    [SerializeField, Range(0,0.1f)] private float walkingAmplitude = 0.015f;
    [SerializeField, Range(0,0.1f)] private float runningAmplitude = 0.015f; 
    [SerializeField, Range(0,15)] private float walkingFrequency = 10.0f; 
    [SerializeField, Range(10,20)] private float runningFrequency = 18f; 
    [SerializeField] private float resetPosSpeed = 3.0f; 

    private Vector3 startPos; 
    private Vector2 head;
    private InputManager inputManager;
    


    private void Awake()
    {
        startPos = transform.localPosition;
    }


    public override void Spawned()
    {
        if (_simpleKCC == null)
        {
            _simpleKCC = GetComponentInParent<SimpleKCC>();
        }

        if (!HasInputAuthority)
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
    }

    public override void Render()
    {
        if (!HasInputAuthority) return;

        if (input.isMovingInputPressed)
        {
            float freq = input.wasRunInputPressed ? runningFrequency : walkingFrequency;
            float amp = input.wasRunInputPressed ? runningAmplitude : walkingAmplitude;

            Vector3 bobPos = Vector3.zero;
            bobPos.y = Mathf.Sin(Runner.SimulationTime * freq) * amp;
            bobPos.x = Mathf.Cos(Runner.SimulationTime * freq / 0.5f) * amp;

            transform.localPosition = startPos + bobPos;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Runner.DeltaTime);
        }
    }


    private void ResetPosition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Runner.DeltaTime);
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
    
    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
}