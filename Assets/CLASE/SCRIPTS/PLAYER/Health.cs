using UnityEngine;
using Fusion;
using UnityEngine.Events;
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
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int _damage, PlayerRef shooter)
    {
        if (NetworkedHealth <= 0) return;

        Debug.Log($"<color=red>DAÑO RECIBIDO:</color> {_damage} puntos de {shooter}. Vida antes: {NetworkedHealth}");

        NetworkedHealth -= _damage;

        Debug.Log($"<color=orange>VIDA ACTUALIZADA EN RED:</color> {NetworkedHealth}");

        if (NetworkedHealth <= 0)
        {
            Debug.Log("<color=black><b>MUERTE DETECTADA</b></color>");
            MatchManager mm = FindFirstObjectByType<MatchManager>();
            if (mm != null) mm.PlayerKilled(Object.InputAuthority, shooter);
            StartCoroutine(RespawnSequence());
        }
    }

    void OnHealthChanged()
    {
        Debug.Log($"<color=cyan>OnHealthChanged disparado.</color> Nueva vida: {NetworkedHealth} para el jugador {Object.InputAuthority}");

        if (healthBar != null)
        {
            healthBar.fillAmount = (float)NetworkedHealth / maxHealth;
        }

        if (Object.HasInputAuthority)
        {
            MatchManager mm = FindFirstObjectByType<MatchManager>();
            if (mm != null)
            {
                mm.ActualizarVidasUI(NetworkedHealth, maxHealth);
            }
        }
    }

    private System.Collections.IEnumerator RespawnSequence()
    {
        transform.position = new Vector3(0, -100, 0);

        yield return new WaitForSeconds(3f); // tiempo de espera pa revivir

        NetworkedHealth = maxHealth;

        // respawn en otra locacion 
        transform.position = new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5));
    }

}
