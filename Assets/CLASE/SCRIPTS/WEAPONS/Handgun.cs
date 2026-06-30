using Fusion;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Handgun : Weapon
{
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    public Handgun() : base(0, 0, 0, 0) { }

    private void Awake()
    {
        damage = 10;
        fireRate = 0.5f;

    }

    public override void RigidbodyShoot()
    {
        RPC_RigidBodyShoot(_firePoint.position, _firePoint.rotation);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RigidBodyShoot(Vector3 pos, Quaternion rotation, RpcInfo info = default)
    {
        Runner.Spawn(_bulletPrefab, pos, rotation, info.Source, (runner, obj) => {
            Proyectil p = obj.GetComponent<Proyectil>();
            if (p != null)
            {
                p.SetProjectileData(info.Source, damage);
                Debug.Log($"<color=green>Bala spawneada con daño: {damage}</color>");
            }
        });
    }


    public override void RaycastShoot() => RPC_RaycastShoot();

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_RaycastShoot(RpcInfo info = default)
    {
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit, 100f, layers))
        {
            Debug.Log("raycast impactó: " + hit.collider.name); 

            if (hit.collider.TryGetComponent(out Health health))
            {
                health.RPC_TakeDamage(damage, info.Source);
                SpawnImpactEffect(hit, true);
            }
            else
            {
                SpawnImpactEffect(hit, false); 
            }
        }
        else
        {
            Debug.Log("el raycast no chocó con nada");
        }
    }

    private void SpawnImpactEffect(RaycastHit hit, bool isPlayer)
    {
        if (isPlayer)
        {
            if (bloodSparksPrefab != null)
                Instantiate(bloodSparksPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            if (bulletHolePrefab != null)
            {
                GameObject hole = Instantiate(bulletHolePrefab, hit.point + (hit.normal * 0.01f), Quaternion.LookRotation(-hit.normal));
                hole.transform.SetParent(hit.collider.transform);
                Destroy(hole, 5f);
            }
        }
    }   


    public override void Reload()
    {
        base.Reload();
    }
}