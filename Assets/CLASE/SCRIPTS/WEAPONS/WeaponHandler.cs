using Fusion;
using System;
using UnityEngine;





public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon actualWeapon;

    // aca se manejan el arma que el jugador tiene equipada,
    // y vamos a llamar a los metodos de disparo y recarga del
    // arma desde aqui
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
        if (!HasInputAuthority) return;

        if(GetInput(out InputInfo input))
        {
        
            if(input.isShooting && ShootAction != null)
            {
                ShootAction();
            }

            if(input.isReloading && actualWeapon != null)
            {
                actualWeapon.Reload();
            }
       
        }
    }
}