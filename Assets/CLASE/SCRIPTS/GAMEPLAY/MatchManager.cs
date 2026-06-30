using UnityEngine;
using Fusion;
using TMPro;

public class MatchManager : NetworkBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject hudprincipal;
    [SerializeField] private GameObject instruccionesCanvas;
    [SerializeField] private GameObject hudPartida;
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private TextMeshProUGUI winnerNameText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Configuración")]
    [SerializeField] private float matchDuration = 300f; 
    [SerializeField] private int killsToWin = 5;

    [Header("Sistema de Vidas")]
    [SerializeField] private GameObject[] capasVidas;

    [Networked, Capacity(4)]
    private NetworkDictionary<PlayerRef, int> playerKills => default;

    [Networked] public TickTimer matchTimer { get; set; }
    [Networked] public bool isMatchOver { get; set; }
    [Networked] public bool gameStarted { get; set; }


    [Networked, OnChangedRender(nameof(AlCambiarEstadoJuego))]
    public NetworkBool MostrarInstrucciones { get; set; }

    public override void Spawned()
    {
        if (MostrarInstrucciones)
        {
            AlCambiarEstadoJuego();
        }

        if (Object.HasStateAuthority)
        {
            matchTimer = TickTimer.CreateFromSeconds(Runner, matchDuration);
            MostrarInstrucciones = true; 
        }
    }

    void AlCambiarEstadoJuego()
    {
        if (MostrarInstrucciones && instruccionesCanvas != null)
        {
            hudprincipal.SetActive(true);

            var anim = hudprincipal.GetComponent<Animator>();
            if (anim != null) anim.Play("Canvas_Pergamino", 0, 0);

            Debug.Log("Canvas activado por red correctamente");
        }
    }

    public void FinalizarInstrucciones()
    {
        if (instruccionesCanvas != null) instruccionesCanvas.SetActive(false);
        if (hudPartida != null) hudPartida.SetActive(true);
        gameStarted = true;
    }

    public void ActualizarVidasUI(int saludActual, int saludMax)
    {
        if (capasVidas == null || capasVidas.Length == 0)
        {
            Debug.LogWarning("¡MatchManager no tiene las capas de vida asignadas en el Inspector!");
            return;
        }

        float porcentajeSalud = (float)saludActual / saludMax;
        int capasAActivar = Mathf.CeilToInt(porcentajeSalud * capasVidas.Length);

        Debug.Log($"<color=green>ACTUALIZANDO HUD:</color> Salud {saludActual}. Capas que deberían estar activas: {capasAActivar}");

        for (int i = 0; i < capasVidas.Length; i++)
        {
            if (capasVidas[i] != null)
            {
                bool estadoAnterior = capasVidas[i].activeSelf;
                bool nuevoEstado = (i < capasAActivar);

                capasVidas[i].SetActive(nuevoEstado);

                if (estadoAnterior != nuevoEstado)
                {
                    Debug.Log($"Capa de vida [{i}] cambiada a: {nuevoEstado}");
                }
            }
        }
    }


    public override void FixedUpdateNetwork()
    {
        if (isMatchOver || !gameStarted) return;

        if (matchTimer.Expired(Runner))
        {
            if (Object.HasStateAuthority)
            {
                DetermineWinner("¡TIEMPO AGOTADO!");
            }
        }
        else
        {
            float? remainingTime = matchTimer.RemainingTime(Runner);

            if (remainingTime.HasValue)
            {
                ActualizarTextoTimer(remainingTime.Value);
            }
        }
    }

    private void ActualizarTextoTimer(float tiempo)
    {
        if (timerText != null)
        {
            int minutos = Mathf.FloorToInt(tiempo / 60);
            int segundos = Mathf.FloorToInt(tiempo % 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutos, segundos);
        }
    }

    public void PlayerKilled(PlayerRef victim, PlayerRef killer)
    {
        if (!Object.HasStateAuthority || isMatchOver) return;

        int currentKills = 0;
        if (playerKills.ContainsKey(killer)) currentKills = playerKills[killer];
        currentKills++;
        playerKills.Set(killer, currentKills);

        Debug.Log($"Jugador {killer} lleva {currentKills} bajas.");

        if (currentKills >= killsToWin)
        {
            DetermineWinner("¡LÍMITE DE BAJAS ALCANZADO!");
        }
    }

    public void DetermineWinner(string reason)
    {
        if (isMatchOver) return;
        isMatchOver = true;

        string winnerName = "Empate";
        int maxKills = -1;
        PlayerRef winnerRef = PlayerRef.None;

        foreach (var entry in playerKills)
        {
            if (entry.Value > maxKills)
            {
                maxKills = entry.Value;
                winnerRef = entry.Key;
            }
        }

        if (winnerRef != PlayerRef.None)
        {
            int hostId = -1;
            foreach (var p in Runner.ActivePlayers) { hostId = p.PlayerId; break; }
            winnerName = (winnerRef.PlayerId == hostId) ? "HOST" : "INVITADO";
        }

        RPC_EndMatch(reason, winnerName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndMatch(string reason, string winner)
    {
        isMatchOver = true;
        hudPartida.SetActive(false);
        victoryCanvas.SetActive(true);
        winnerNameText.text = $"{reason}\nGANADOR: {winner}";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}