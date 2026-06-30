using UnityEngine;
using TMPro; // <--- Importante para los InputFields de TextMeshPro
using UnityEngine.UI;

public class MenuButtonHelper : MonoBehaviour
{
    [Header("Referencias de Paneles (Canvas)")]
    [SerializeField] private GameObject panelSearchGame;
    [SerializeField] private GameObject panelCreateServer;
    [SerializeField] private GameObject panelJoinServer;
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelError; // El canvas/imagen de error que se activa

    [Header("Campos de Texto - Crear Servidor")]
    [SerializeField] private TMP_InputField inputNombreCrear;
    [SerializeField] private TMP_InputField inputMaxJugadores;

    [Header("Campos de Texto - Unirse a Servidor")]
    [SerializeField] private TMP_InputField inputNombreUnirse;

    private PhotonManager pm;

    private void Start()
    {
        pm = Object.FindFirstObjectByType<PhotonManager>();

        // Aseguramos que el panel de error inicie apagado
        if (panelError != null) panelError.SetActive(false);
    }

    // ==========================================
    // VALIDACIÓN Y ACCIONES DE BOTONES
    // ==========================================

    // Botón definitivo para CREAR SERVER
    public void IntentarCrearServer()
    {
        // Validamos que los campos no estén vacíos o con puros espacios
        if (string.IsNullOrWhiteSpace(inputNombreCrear.text) || string.IsNullOrWhiteSpace(inputMaxJugadores.text))
        {
            MostrarError();
            return;
        }

        // Si todo está bien, llamamos al manager para que arme la partida
        if (pm != null)
        {
            pm.HostClick();
        }
        else
        {
            Debug.LogError("No se encontró el PhotonManager en la escena.");
        }
    }

    // Botón definitivo para UNIRSE A SERVER
    public void IntentarJoinServer()
    {
        // Validamos que el campo de nombre no esté vacío
        if (string.IsNullOrWhiteSpace(inputNombreUnirse.text))
        {
            MostrarError();
            return;
        }

        // Si todo está correcto, nos unimos
        if (pm != null)
        {
            pm.ClienteClick();
        }
        else
        {
            Debug.LogError("No se encontró el PhotonManager en la escena.");
        }
    }

    private void MostrarError()
    {
        if (panelError != null)
        {
            panelError.SetActive(true);
        }
        else
        {
            Debug.LogError("Falta referenciar el Panel de Error en el Inspector.");
        }
    }

    // Para cerrar el mensaje de error con un botón de "OK" o "Cerrar"
    public void CerrarError()
    {
        if (panelError != null) panelError.SetActive(false);
    }

    // ==========================================
    // NAVEGACIÓN BÁSICA DE MENÚS
    // ==========================================

    public void SearchGameClick() => ActivarPanel(panelSearchGame);
    public void CreateServerMenuClick() => ActivarPanel(panelCreateServer);
    public void JoinServerMenuClick() => ActivarPanel(panelJoinServer);

    public void Return_MainMenu()
    {
        if (panelSearchGame != null) panelSearchGame.SetActive(false);
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
    }

    public void Return_ServerMenu_Create()
    {
        if (panelCreateServer != null) panelCreateServer.SetActive(false);
        if (panelSearchGame != null) panelSearchGame.SetActive(true);
    }

    public void Return_ServerMenu_Join()
    {
        if (panelJoinServer != null) panelJoinServer.SetActive(false);
        if (panelSearchGame != null) panelSearchGame.SetActive(true);
    }

    public void SalirClick()
    {
        if (pm != null) pm.SalirClick();
        else Application.Quit();
    }

    private void ActivarPanel(GameObject panelAActivar)
    {
        if (panelAActivar != null) panelAActivar.SetActive(true);
    }
}