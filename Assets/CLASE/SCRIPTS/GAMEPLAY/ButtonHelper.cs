using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Fusion;

public class MenuButtonHelper : MonoBehaviour
{
    [Header("Referencias de Paneles (Canvas)")]
    [SerializeField] private GameObject panelSearchGame;
    [SerializeField] private GameObject panelCreateServer;
    [SerializeField] private GameObject panelJoinServer;
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelError;
    [SerializeField] private GameObject panelRegisterUser;

    [SerializeField] private GameObject panelLogin;
    [SerializeField] private GameObject panelSignIn;

    [Header("Campos de Texto - Crear Servidor")]
    [SerializeField] private TMP_InputField inputNombreCrear;
    [SerializeField] private TMP_InputField inputMaxJugadores;

    [Header("Campos de Texto - Unirse a Servidor")]
    [SerializeField] private TMP_InputField inputNombreUnirse;

    [Header("Campos de LogIn/SignIn a Menú Principal")]
    [SerializeField] private GameObject panelIntro;
    [SerializeField] private GameObject panelEncontrarPartida;
    [SerializeField] private GameObject panelMenuSignIn;
    [SerializeField] private GameObject panelMenuLogIn;

    [Header("Escena de Hub")]
    [SerializeField] private string nombreEscenaHub = "Hub";

    private PhotonManager pm;

    private void Start()
    {
        pm = Object.FindFirstObjectByType<PhotonManager>();

        if (panelError != null) panelError.SetActive(false);
    }
    public void RegresarAlHubMenu()
    {
        NetworkRunner runner = Object.FindFirstObjectByType<NetworkRunner>();
        if (runner != null)
        {
            runner.Shutdown();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaHub);
    }

    public void IrAlMenuPrincipalDesdeLogIn()
    {
        if (panelIntro != null) panelIntro.SetActive(false);
        if (panelMenuLogIn != null) panelIntro.SetActive(false);
        if (panelEncontrarPartida != null) panelEncontrarPartida.SetActive(true);
    }

    public void IrAlMenuPrincipalDesdeSignIn()
    {
        if (panelIntro != null) panelIntro.SetActive(false);
        if (panelSignIn != null) panelSignIn.SetActive(false);
        if (panelEncontrarPartida != null) panelEncontrarPartida.SetActive(true);
    }

    public void OpenLoginWindow()
    {
        if (panelSignIn != null) panelSignIn.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(true);
    }

    public void OpenSignInWindow()
    {
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelSignIn != null) panelSignIn.SetActive(true);
    }

    public void SalirJuegoDirecto()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void IniciarPartidaRapida()
    {
        if (pm != null)
        {
            pm.IniciarPartidaRapida6Caracteres();
        }
        else
        {
            Debug.LogError("No se encontró el PhotonManager en la escena.");
        }
    }

    public void IntentarCrearServer()
    {
        if (string.IsNullOrWhiteSpace(inputNombreCrear.text) || string.IsNullOrWhiteSpace(inputMaxJugadores.text))
        {
            MostrarError();
            return;
        }

        if (pm != null)
        {
            pm.HostClick();
        }
        else
        {
            Debug.LogError("No se encontró el PhotonManager en la escena.");
        }
    }

    public void IntentarJoinServer()
    {
        if (string.IsNullOrWhiteSpace(inputNombreUnirse.text))
        {
            MostrarError();
            return;
        }

        if (pm != null)
        {
            pm.JoinSelectedGame(inputNombreUnirse.text.Trim());
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

    public void CerrarError()
    {
        if (panelError != null) panelError.SetActive(false);
    }

    public void SearchGameClick() => ActivarPanel(panelSearchGame);
    public void CreateServerMenuClick() => ActivarPanel(panelCreateServer);
    public void JoinServerMenuClick() => ActivarPanel(panelJoinServer);

    public void RegisterUserMenuClick()
    {
        if (panelMainMenu != null) panelMainMenu.SetActive(false);
        ActivarPanel(panelRegisterUser);
    }

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