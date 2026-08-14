using Fusion;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    public ShootMode shootMode;

    [SerializeField] protected Camera playerCam;
    [SerializeField] protected LayerMask layers;
    [SerializeField] protected int damage;
    [SerializeField] protected float fireRate = 0.2f;
    [SerializeField] protected int actualAmmo;
    [SerializeField] protected int maxAmmoCapacity;
    [SerializeField] protected int ammoInStock;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected NetworkPrefabRef proyectil;

    [Header("Efectos de Impacto")]
    [SerializeField] protected GameObject bulletHolePrefab;
    [SerializeField] protected ParticleSystem bloodSparksPrefab;

    public Weapon(int damage, float fireRate, int actualAmmo, int maxAmmoCapacity)
    {
        this.damage = damage;
        this.fireRate = fireRate;
        this.actualAmmo = actualAmmo;
        this.maxAmmoCapacity = maxAmmoCapacity;
    }

    public Weapon() { }

    public abstract void RigidbodyShoot();
    public abstract void RaycastShoot();

    public float GetFireRate() => fireRate;

    public virtual void Reload()
    {
        if (ammoInStock <= 0)
        {
            Debug.Log("No hay munición en reserva.");
            return;
        }
    }
}