using Fusion;
using Unity.VisualScripting;
using UnityEngine;


// Weapon: Este será una clase abstracta, debera tener variables comunes de un arma.
// Como metodos ya sea abstractos o virtuales debe tener Disparo con Raycast, Disparo
// Fisico y Recargar. Tambien debe tener una variable de tipo ShootMode para escoger
// entre los tipos de disparo.

public abstract class Weapon : NetworkBehaviour
{
    public ShootMode shootMode;

    [SerializeField] protected Camera playerCam;
    [SerializeField] protected LayerMask layers;
    [SerializeField] protected int damage; //nyte
    [SerializeField] protected float fireRate;
    [SerializeField] protected int actualAmmo; //byte
    [SerializeField] protected int maxAmmoCapacity; //byte
    [SerializeField] protected int ammoInStock; //ushort
    [SerializeField] protected float reloadTime;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected NetworkPrefabRef proyectil;

    [Header("Efectos de Impacto")] //nuevo
    [SerializeField] protected GameObject bulletHolePrefab;
    [SerializeField] protected ParticleSystem bloodSparksPrefab;


    public Weapon(int damage, float fireRate, int actualAmmo, int maxAmmoCapacity)
    {
        this.damage = damage;
        this.fireRate = fireRate;
        this.actualAmmo = actualAmmo;
        this.maxAmmoCapacity = maxAmmoCapacity;
    }

    public abstract void RigidbodyShoot();
    public abstract void RaycastShoot();

    public Weapon() { }


    public virtual void Reload()
    {
        if(ammoInStock <= 0)
        {
            Debug.Log("no ammo in stock");
            return;
        }

    }


}
