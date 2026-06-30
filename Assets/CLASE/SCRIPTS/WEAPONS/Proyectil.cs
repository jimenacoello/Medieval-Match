using Fusion;
using System.Threading.Tasks;
using UnityEngine;

public class Proyectil : NetworkBehaviour
{
    [SerializeField] private float _speed = 50f;
    [SerializeField] private float _lifeTime = 3f;
    [Networked] public int _damage { get; set; }

    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private ParticleSystem sparksPrefab;

    [Networked] private TickTimer _lifeTimer { get; set; }

    private PlayerRef shooter; 
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = transform.forward * _speed;

        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }


    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && _lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }




    private void OnCollisionEnter(Collision collision)
    {
        if (!Object || !Object.IsValid || !Runner.IsServer) return;

        Health targetHealth = collision.gameObject.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            if (targetHealth.Object.InputAuthority == shooter) return;

            Debug.Log($"<color=yellow>¡SERVIDOR detectó impacto en {targetHealth.gameObject.name}!</color>");
            targetHealth.RPC_TakeDamage(_damage, shooter);

            if (sparksPrefab != null)
            {
                Instantiate(sparksPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }
        }
        else
        {
            if (bulletHolePrefab != null)
            {
                ContactPoint contact = collision.contacts[0];
                GameObject hole = Instantiate(bulletHolePrefab, contact.point + (contact.normal * 0.01f), Quaternion.LookRotation(-contact.normal));
                hole.transform.SetParent(collision.transform);
                Destroy(hole, 5f);
            }
        }

        Runner.Despawn(Object);
    }

    public void SetProjectileData(PlayerRef playerref, int damage)
    {
        this.shooter = playerref;
        this._damage = damage;
    }

}