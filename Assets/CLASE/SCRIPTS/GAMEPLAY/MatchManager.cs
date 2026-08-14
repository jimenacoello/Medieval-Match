using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    [Header("Configuración del Match")]
    [SerializeField] private float duracionMatchSegundos = 300f;
    [SerializeField] private int maxVidas = 5;

    [Header("UI Canvas de Gameplay")]
    [SerializeField] private GameObject canvasHUD;
    [SerializeField] private TMP_Text vidasText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text statsText;

    [Header("UI Instrucciones / Espera")]
    [SerializeField] private GameObject panelInstrucciones;
    [SerializeField] private GameObject panelWaitingPlayer;
    [SerializeField] private TMP_Text waitingText;

    [Header("UI Avisos y Fin de Juego")]
    [SerializeField] private GameObject panelAvisosUI;
    [SerializeField] private TMP_Text textoAviso;
    [SerializeField] private GameObject botonVolverHub;
    [SerializeField] private GameObject botonSalirJuego;

    [Networked, OnChangedRender(nameof(OnGameStartedChanged))]
    public bool GameStarted { get; set; }

    [Networked] public bool isMatchOver { get; set; }
    [Networked] private TickTimer matchTimer { get; set; }

    private bool instruccionesMostradas = false;
    private Dictionary<PlayerRef, int> playerKills = new Dictionary<PlayerRef, int>();
    private Dictionary<PlayerRef, int> playerDeaths = new Dictionary<PlayerRef, int>();

    public override void Spawned()
    {
        if (canvasHUD != null) canvasHUD.SetActive(true);
        if (panelAvisosUI != null) panelAvisosUI.SetActive(false);
        if (botonVolverHub != null) botonVolverHub.SetActive(false);
        if (botonSalirJuego != null) botonSalirJuego.SetActive(false);

        instruccionesMostradas = false;

        VerificarEstadoUI();
        ActualizarUIStatsLocales();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            int currentPlayers = Runner.SessionInfo.PlayerCount;

            if (currentPlayers >= 2 && !GameStarted && !isMatchOver)
            {
                GameStarted = true;
                matchTimer = TickTimer.CreateFromSeconds(Runner, duracionMatchSegundos);
            }
            else if (currentPlayers < 2 && GameStarted && !isMatchOver)
            {
                GameStarted = false;
            }

            if (GameStarted && !isMatchOver && matchTimer.Expired(Runner))
            {
                FinalizarMatchPorTiempo();
            }
        }
    }

    public override void Render()
    {
        if (GameStarted && !isMatchOver && timerText != null && matchTimer.IsRunning)
        {
            float tiempoRestante = matchTimer.RemainingTime(Runner) ?? 0f;
            int minutos = Mathf.FloorToInt(tiempoRestante / 60F);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    private void OnGameStartedChanged()
    {
        VerificarEstadoUI();
    }



    private void VerificarEstadoUI()
    {
        if (isMatchOver) return;

        if (!GameStarted)
        {
            if (panelWaitingPlayer != null)
            {
                panelWaitingPlayer.SetActive(true);
                if (waitingText != null) waitingText.text = "Esperando a otro jugador para iniciar...";
            }

            if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
            if (panelAvisosUI != null) panelAvisosUI.SetActive(false);
        }
        else
        {
            if (panelWaitingPlayer != null) panelWaitingPlayer.SetActive(false);
            if (!instruccionesMostradas)
            {
                instruccionesMostradas = true;

                if (panelInstrucciones != null)
                {
                    panelInstrucciones.SetActive(true);
                    Animator anim = panelInstrucciones.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.enabled = true;
                        anim.Play(0, -1, 0f);
                    }
                }
            }
        }

    }


    private void ApagarPanelInstrucciones()
    {
        if (panelInstrucciones != null)
        {
            Animator anim = panelInstrucciones.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }

            Animator[] childAnimators = panelInstrucciones.GetComponentsInChildren<Animator>();
            foreach (var childAnim in childAnimators)
            {
                childAnim.enabled = false;
            }

            panelInstrucciones.SetActive(false);

            CanvasGroup cg = panelInstrucciones.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0; cg.blocksRaycasts = false; }
        }

        if (panelWaitingPlayer != null)
        {
            panelWaitingPlayer.SetActive(false);
        }
    }


    public void FinalizarInstrucciones()
    {
        if (panelInstrucciones != null)
        {
            Animator anim = panelInstrucciones.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;

            panelInstrucciones.SetActive(false);
        }

        if (canvasHUD != null) canvasHUD.SetActive(true);
    }

    public void PlayerKilled(PlayerRef victim, PlayerRef killer) => RegistrarBaja(killer, victim);

    public void RegistrarBaja(PlayerRef killer, PlayerRef victim)
    {
        if (!Object.HasStateAuthority || isMatchOver || !GameStarted) return;

        if (!playerKills.ContainsKey(killer)) playerKills[killer] = 0;
        if (!playerDeaths.ContainsKey(victim)) playerDeaths[victim] = 0;

        playerKills[killer]++;
        playerDeaths[victim]++;

        RPC_NotificarBaja(killer, victim);

        if (playerDeaths[victim] >= maxVidas)
        {
            FinalizarMatchPorVidas(killer);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotificarBaja(PlayerRef killer, PlayerRef victim)
    {
        if (Runner.LocalPlayer == killer && StatManager.Instance != null) StatManager.Instance.RegistrarKill();
        if (Runner.LocalPlayer == victim && StatManager.Instance != null) StatManager.Instance.RegistrarMuerte();

        ActualizarUIStatsLocales();
    }

    public void ActualizarVidasUI(int currentHealth, int maxHealth) => ActualizarUIStatsLocales();

    public void ActualizarUIStatsLocales()
    {
        int muertesLocales = StatManager.Instance != null ? StatManager.Instance.deathsCount : 0;
        int killsLocales = StatManager.Instance != null ? StatManager.Instance.killCount : 0;
        int vidasRestantes = Mathf.Clamp(maxVidas - muertesLocales, 0, maxVidas);

        if (vidasText != null) vidasText.text = $"Vidas: {vidasRestantes}/{maxVidas}";
        if (statsText != null) statsText.text = $"Kills: {killsLocales} | Muertes: {muertesLocales}";
    }

    private void FinalizarMatchPorVidas(PlayerRef ganador)
    {
        isMatchOver = true;
        GameStarted = false;
        RPC_FinalizarPartida(ganador);
    }

    private void FinalizarMatchPorTiempo()
    {
        isMatchOver = true;
        GameStarted = false;

        PlayerRef ganador = PlayerRef.None;
        int maxKills = -1;

        foreach (var kvp in playerKills)
        {
            if (kvp.Value > maxKills)
            {
                maxKills = kvp.Value;
                ganador = kvp.Key;
            }
        }

        RPC_FinalizarPartida(ganador);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_FinalizarPartida(PlayerRef ganador)
    {
        isMatchOver = true;
        MostrarUIFinDeJuego(ganador);

        if (PlayfabManager.Instance != null)
        {
            PlayfabManager.Instance.UploadDataInPlayfab();
        }
    }

    private void MostrarUIFinDeJuego(PlayerRef ganador)
    {
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
        if (panelWaitingPlayer != null) panelWaitingPlayer.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelAvisosUI != null)
        {
            panelAvisosUI.SetActive(true);

            if (textoAviso != null)
            {
                if (ganador == Runner.LocalPlayer)
                    textoAviso.text = "¡VICTORIA!\nHas eliminado a tu rival.";
                else if (ganador != PlayerRef.None)
                    textoAviso.text = "DERROTA\nTe has quedado sin vidas.";
                else
                    textoAviso.text = "¡EMPATE!\nSe agotó el tiempo de partida.";
            }

            if (botonVolverHub != null) botonVolverHub.SetActive(true);
            if (botonSalirJuego != null) botonSalirJuego.SetActive(true);
        }
    }
}