using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public event Action<List<SessionInfo>> onSessionListUpdated;
    public static PhotonManager Instance;

    private NetworkRunner runner;
    private bool isStartingGame = false;

    [SerializeField] private UnityEvent onPlayerJoinedEvent;
    [SerializeField] private UnityEvent<List<SessionInfo>> _onSessionListUpdated;

    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("UI de Creación de Partida")]
    [SerializeField] private TMP_InputField serverNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;

    [Header("Generador de Nombres")]
    [Min(4)]
    [SerializeField] private int sessionNameLength = 6;

    [Header("UI Panels")]
    [SerializeField] public GameObject lobbyMenuCanvas;
    [SerializeField] private GameObject panelErrorRed;
    [SerializeField] private TMP_Text errorText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (runner == null)
        {
            runner = FindAnyObjectByType<NetworkRunner>() ?? gameObject.AddComponent<NetworkRunner>();
        }
    }

    private void Start()
    {
        if (runner != null) runner.AddCallbacks(this);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        onSessionListUpdated?.Invoke(sessionList);
        _onSessionListUpdated?.Invoke(sessionList);
    }

    public async void ConnectToPhotonLobby()
    {
        if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();
        runner.AddCallbacks(this);

        Debug.Log("Conectando al Lobby de Fusion...");
        await runner.JoinSessionLobby(SessionLobby.Custom, "MiLobbyUnicoDoomMatch");
    }

    public async void IniciarPartidaRapida6Caracteres()
    {
        string codigoAleatorio = GenerarCodigoAleatorio(sessionNameLength);
        await StartRandomGame(GameMode.Host, codigoAleatorio, 2);
    }

    private string GenerarCodigoAleatorio(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[length];
        System.Random random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

    public async void HostClick()
    {
        string nombreSesion = string.IsNullOrEmpty(serverNameInput?.text) ? GenerarCodigoAleatorio(6) : serverNameInput.text;
        int maxJugadores = 2;
        if (maxPlayersInput != null && !string.IsNullOrWhiteSpace(maxPlayersInput.text))
        {
            if (int.TryParse(maxPlayersInput.text.Trim(), out int numeroConvertido))
            {
                maxJugadores = Mathf.Clamp(numeroConvertido, 2, 10);
            }
        }

        await StartRandomGame(GameMode.Host, nombreSesion, maxJugadores);
    }

    private async Task StartRandomGame(GameMode mode, string sessionName, int maxPlayers)
    {
        if (isStartingGame) return;
        isStartingGame = true;

        if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(1);
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            Scene = sceneInfo,
            CustomLobbyName = "CUSTOM SERVER #" + sessionName,
            SceneManager = GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            MostrarErrorRed($"Error al iniciar la sesión: {result.ShutdownReason}");
            isStartingGame = false;
        }
    }

    public async void JoinSelectedGame(string sessionName)
    {
        if (isStartingGame) return;
        isStartingGame = true;

        if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(1);
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = sceneInfo,
            SceneManager = GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });

        if (!result.Ok)
        {
            MostrarErrorRed($"No se logró conectar a la partida: {result.ShutdownReason}");
            isStartingGame = false;
        }
    }

    public async void VolverAlMenuPrincipal()
    {
        if (runner != null)
        {
            await runner.Shutdown();
        }
        SceneManager.LoadScene(0);
    }

    private void MostrarErrorRed(string mensaje)
    {
        if (panelErrorRed != null)
        {
            panelErrorRed.SetActive(true);
            if (errorText != null) errorText.text = mensaje;
        }
        else
        {
            Debug.LogError(mensaje);
        }
    }

    public async void SalirClick()
    {
        if (runner != null) await runner.Shutdown();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            SpawnLocalPlayer(runner, player);
        }
        onPlayerJoinedEvent?.Invoke();
    }

    private void SpawnLocalPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-2f, 2f), 1f, UnityEngine.Random.Range(-2f, 2f));
        NetworkObject networkedPlayer = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        runner.SetPlayerObject(player, networkedPlayer);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (shutdownReason != ShutdownReason.Ok)
        {
            string razonTexto = shutdownReason.ToString();
            string mensajeError = $"Se perdió la conexión con la partida ({razonTexto}).";

            // Evaluamos por texto para evitar errores de compilación según la versión del SDK
            if (razonTexto.Contains("Host") || razonTexto.Contains("Server") || razonTexto.Contains("Disconnected"))
            {
                mensajeError = "La partida fue cerrada por el servidor/host o te has desconectado.";
            }

            MostrarErrorRed(mensajeError);
        }
        isStartingGame = false;
    }


    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        MostrarErrorRed($"Desconectado del servidor. Razón: {reason}");
    }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var customInput = new MovementController.GameplayInput
        {
            MoveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            LookRotationDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")),
            IsRunning = Input.GetKey(KeyCode.LeftShift),
            // IMPORTANTE: Capturamos el disparo aquí para transmitirlo por red
            isShooting = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0),
            isReloading = Input.GetKeyDown(KeyCode.R)
        };

        input.Set(customInput);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}