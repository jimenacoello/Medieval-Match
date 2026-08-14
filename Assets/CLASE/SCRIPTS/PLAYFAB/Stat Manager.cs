using UnityEngine;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("Game Loop Stats")]
    public int killCount;
    public int deathsCount;
    public static string playerColor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnEnable()
    {
        if (PlayfabManager.Instance != null)
            PlayfabManager.Instance.onRetriverData += UpdatePlayerStats;
    }

    public void OnDisable()
    {
        if (PlayfabManager.Instance != null)
            PlayfabManager.Instance.onRetriverData -= UpdatePlayerStats;
    }

    private void UpdatePlayerStats(Dictionary<string, string> data)
    {
        if (data.ContainsKey("kill count")) int.TryParse(data["kill count"], out killCount);
        if (data.ContainsKey("deaths count")) int.TryParse(data["deaths count"], out deathsCount);
        if (data.ContainsKey("player color")) playerColor = data["player color"];

        NotificarActualizacionUI();
    }

    public void RegistrarKill()
    {
        killCount++;
        Debug.Log($"Baja confirmada. Kills totales: {killCount}");
        NotificarActualizacionUI();
    }

    public void RegistrarMuerte()
    {
        deathsCount++;
        Debug.Log($"Has muerto. Muertes totales: {deathsCount}");
        NotificarActualizacionUI();
    }

    private void NotificarActualizacionUI()
    {
        MatchManager match = Object.FindFirstObjectByType<MatchManager>();
        if (match != null)
        {
            Health localHealth = Object.FindFirstObjectByType<Health>();
            int saludActual = localHealth != null ? localHealth.NetworkedHealth : 100;

            match.ActualizarVidasUI(saludActual, 100);
        }
    }

    public void SeleccionarColorDesdeUI(Color nuevoColor)
    {
        playerColor = "#" + ColorUtility.ToHtmlStringRGB(nuevoColor);
        Debug.Log($"Color seleccionado: {playerColor}");
    }
}