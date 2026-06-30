using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
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

    [SerializeField] private GameObject canvasUI;
    private GameMode selectedGameMode;

    [SerializeField] private UnityEvent onPlayerJoinedEvent; 
    [SerializeField] UnityEvent<List<SessionInfo>> _onSessionListUpdated;

    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private GameObject[] dontDestroyOnLoadObjs;
    [SerializeField] private GameObject canvasDelTimer;

    [Header("UI de Creación de Partida")]
    [SerializeField] private TMP_InputField serverNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;
    [SerializeField] private TMP_Text errorFeedbackTxt;

    [Header("Generador de Nombres")]
    [Min(4)]
    [SerializeField] private int sessionNameLength = 6;

    [Header("UI Panels")]
    [SerializeField] public GameObject lobbyMenuCanvas;

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

    public async void HostClick()
    {
        Debug.Log("Se ha entrado como Host");

        string nombreSesion = string.IsNullOrEmpty(serverNameInput?.text) ? "MiPartida" : serverNameInput.text;
        int maxJugadores = 4;
        if (maxPlayersInput != null && !string.IsNullOrWhiteSpace(maxPlayersInput.text))
        {
            if (int.TryParse(maxPlayersInput.text.Trim(), out int numeroConvertido))
            {
                maxJugadores = Mathf.Clamp(numeroConvertido, 2, 10);
                maxPlayersInput.text = maxJugadores.ToString();
            }
        }

        await StartRandomGame(GameMode.Host, nombreSesion, maxJugadores);
        ActivarHUD();
    }

    public async void ClienteClick()
    {
        Debug.Log("Se ha entrado como Cliente");
        await StartRandomGame(GameMode.Client, "MiPartida", 4);
        ActivarHUD();
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

        var sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties.Add("GameMode", "DoomMatch");
        sessionProperties.Add("Map", "Stadium");

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            Scene = sceneInfo,
            CustomLobbyName = "CUSTOM SERVER #" + sessionName,
            SceneManager = GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = sessionProperties
        });

        if (result.Ok)
        {
            Debug.Log("Juego iniciado correctamente");
            DisableCanvasScene();
        }
        else
        {
            Debug.LogError($"Error al iniciar: {result.ShutdownReason}");
            isStartingGame = false;
        }
    }

    public async void StartCustomGame()
    {
        if (isStartingGame) return;

        if (errorFeedbackTxt != null) errorFeedbackTxt.text = "";

        if (runner == null)
        {
            runner = FindAnyObjectByType<NetworkRunner>();
        }

        if (runner == null)
        {
            Debug.LogWarning("no se puede crear la partida pa: no tai conectado al poton");
            if (errorFeedbackTxt != null) errorFeedbackTxt.text = "horror: perate pa conectarte";
            return;
        }

        if (serverNameInput == null || string.IsNullOrWhiteSpace(serverNameInput.text))
        {
            if (errorFeedbackTxt != null) errorFeedbackTxt.text = "ojito falta el nombre del servidor";
            return;
        }
        string customServerName = serverNameInput.text.Trim();
        int customPlayerCount = 4;

        if (int.TryParse(maxPlayersInput.text.Trim(), out int parsedPlayers))
        {
            customPlayerCount = Mathf.Clamp(parsedPlayers, 2, 10);
            maxPlayersInput.text = customPlayerCount.ToString();
        }
        else
        {
            if (errorFeedbackTxt != null) errorFeedbackTxt.text = "ojito, mal parámetro pa, usa numeros.";
            return;
        }
        isStartingGame = true;
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(1);
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);

        var sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties.Add("GameMode", "DoomMatch");
        sessionProperties.Add("Map", "Stadium");

        Debug.Log($"Iniciando Custom Lobby: '{customServerName}' para [{customPlayerCount}] jugadores.");

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = customServerName,
            PlayerCount = customPlayerCount,
            Scene = sceneInfo,
            CustomLobbyName = "CUSTOM SERVER #" + customServerName,
            SceneManager = GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = sessionProperties,
        });

        if (result.Ok)
        {
            Debug.Log("Custom Lobby creado de manera exitosaAAAAAAAAAAA.");
            DisableCanvasScene();
            ActivarHUD();
        }
        else
        {
            Debug.LogError($"Error al iniciar Custom Lobby: {result.ShutdownReason}");
            if (errorFeedbackTxt != null) errorFeedbackTxt.text = $"Error de red: {result.ShutdownReason}";
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

        Debug.Log($"conectando a la sesion: {sessionName}");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = sceneInfo,
            SceneManager = GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });

        if (result.Ok)
        {
            DisableCanvasScene();
            ActivarHUD();
        }
        else
        {
            Debug.LogError($"fallo al unirse a la sesion: {result.ShutdownReason}");
            isStartingGame = false;
        }
    }

    private void DisableCanvasScene()
    {
        try
        {
            Scene canvasScene = SceneManager.GetSceneByBuildIndex(0);
            if (canvasScene.isLoaded)
            {
                GameObject[] rootObjects = canvasScene.GetRootGameObjects();
                foreach (GameObject obj in rootObjects) obj.SetActive(false);
                Debug.Log("Canvas desactivao");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo desactivar canvas por : {e.Message}");
        }
    }

    private IEnumerator UnloadCanvasScene()
    {
        yield return null;
        DisableCanvasScene();
    }

    private void LoadCanvasScene()
    {
        if (!SceneManager.GetSceneByBuildIndex(0).isLoaded)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
        }
        else
        {
            Scene canvasScene = SceneManager.GetSceneByBuildIndex(1);
            GameObject[] rootObjects = canvasScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects) obj.SetActive(true);
        }
    }

    private void ActivarHUD()
    {
        if (canvasDelTimer != null) canvasDelTimer.SetActive(true);
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
            Debug.Log($"[Photon] Jugador detectado: {player}. Instanciando Prefab...");
            SpawnLocalPlayer(runner, player);
        }
        onPlayerJoinedEvent?.Invoke();
    }

    private void SpawnLocalPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-2f, 2f), 1f, UnityEngine.Random.Range(-2f, 2f));
        NetworkObject networkedPlayer = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

        runner.SetPlayerObject(player, networkedPlayer);
        Debug.Log($"¡Prefab instanciado de forma exitosa para el jugador [{player}] en {spawnPosition}!");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Cambio de escena detectado por Fusion.");
        GameObject hudViejo = GameObject.Find("Canvas_HUD");
        if (hudViejo != null)
        {
            hudViejo.SetActive(false);
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var customInput = new MovementController.GameplayInput();

        customInput.MoveDirection.x = Input.GetAxisRaw("Horizontal");
        customInput.MoveDirection.y = Input.GetAxisRaw("Vertical");

        customInput.LookRotationDelta.x = Input.GetAxis("Mouse X");
        customInput.LookRotationDelta.y = Input.GetAxis("Mouse Y");

        customInput.IsRunning = Input.GetKey(KeyCode.LeftShift);

        input.Set(customInput);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { LoadCanvasScene(); isStartingGame = false; }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}