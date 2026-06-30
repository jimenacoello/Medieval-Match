using Fusion;
using UnityEngine;

public struct InputInfo : INetworkInput
{
    public Vector2 playerPos;
    public Vector2 lookDirection;

    public bool isMoving;
    public bool isMovingBackwards;
    public bool isMovingOnAxis;
    public bool isMovingInputPressed;

    public bool wasRunInputPressed;

    // inputs para las armas
    public bool isShooting;
    public bool isReloading;
}
