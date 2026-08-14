using Fusion;
using System;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon actualWeapon;

    [Networked] private TickTimer cooldownDisparo { get; set; }

    private Action ShootAction;

    public override void Spawned()
    {
        if (actualWeapon == null) return;

        switch (actualWeapon.shootMode)
        {
            case ShootMode.Raycast:
                ShootAction = actualWeapon.RaycastShoot;
                break;

            case ShootMode.Rigidbody:
                ShootAction = actualWeapon.RigidbodyShoot;
                break;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out MovementController.GameplayInput input))
        {
            if (input.isShooting && ShootAction != null && cooldownDisparo.ExpiredOrNotRunning(Runner))
            {
                cooldownDisparo = TickTimer.CreateFromSeconds(Runner, actualWeapon.GetFireRate());

                ShootAction();
            }

            if (input.isReloading && actualWeapon != null)
            {
                actualWeapon.Reload();
            }
        }
    }
}