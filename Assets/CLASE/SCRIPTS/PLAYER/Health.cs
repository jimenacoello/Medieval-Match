using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image healthBar;

    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int NetworkedHealth { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            NetworkedHealth = maxHealth;
        }
        ActualizarUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int _damage, PlayerRef shooter)
    {
        if (NetworkedHealth <= 0) return;

        if (shooter != PlayerRef.None && shooter == Object.InputAuthority) return;

        NetworkedHealth = Mathf.Max(0, NetworkedHealth - _damage);

        if (NetworkedHealth <= 0)
        {
            MatchManager mm = FindFirstObjectByType<MatchManager>();
            if (mm != null) mm.PlayerKilled(Object.InputAuthority, shooter);

            StartCoroutine(RespawnSequence());
        }
    }

    private void OnHealthChanged()
    {
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)NetworkedHealth / maxHealth;
        }

        if (Object.HasInputAuthority)
        {
            MatchManager mm = FindFirstObjectByType<MatchManager>();
            if (mm != null)
            {
                mm.ActualizarUIStatsLocales();
            }
        }
    }

    private System.Collections.IEnumerator RespawnSequence()
    {
        transform.position = new Vector3(0, -100, 0);

        yield return new WaitForSeconds(3f);

        MatchManager mm = FindFirstObjectByType<MatchManager>();
        if (mm != null && !mm.isMatchOver)
        {
            NetworkedHealth = maxHealth;
            transform.position = new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5));
        }
    }
}