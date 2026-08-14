using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayfabManager : MonoBehaviour
{
    [Header("Campos de Login")]
    [SerializeField] private TMP_InputField loginEmail;
    [SerializeField] private TMP_InputField loginPassword;

    [Header("Campos de Sign In (Registro)")]
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerEmail;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmPassword;

    [Header("Feedback de UI / Errores")]
    [SerializeField] private GameObject panelError;
    [SerializeField] private TMP_Text errorText;

    [Header("Paneles de Menú")]
    [SerializeField] private GameObject panelAutenticacion;
    [SerializeField] private GameObject panelMenuJuego;

    public event Action<Dictionary<string, string>> onRetriverData;
    public static PlayfabManager Instance;

    private Coroutine errorCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "1B4AF0";
        }

        if (panelError != null) panelError.SetActive(false);
    }

    public void SeleccionarColor(string colorHex)
    {
        StatManager.playerColor = colorHex;
        Debug.Log($"Color seleccionado: {colorHex}");

        if (PlayfabManager.Instance != null)
        {
            PlayfabManager.Instance.UploadDataInPlayfab();
        }
    } 

    public async void RegisterUserInPlayfab()
    {
        if (string.IsNullOrWhiteSpace(registerEmail.text) ||
            string.IsNullOrWhiteSpace(registerUsername.text) ||
            string.IsNullOrWhiteSpace(registerPassword.text) ||
            string.IsNullOrWhiteSpace(registerConfirmPassword.text))
        {
            MostrarErrorUI("Por favor llena todos los campos de registro.");
            return;
        }

        if (registerPassword.text != registerConfirmPassword.text)
        {
            MostrarErrorUI("Las contraseñas no coinciden. Verifícalas.");
            return;
        }

        try
        {
            var result = await RegisterUserInPlayfabTask();
            Debug.Log($"Usuario registrado correctamente con ID: {result.PlayFabId}");

            string passwordUsada = registerPassword.text;
            string emailUsado = registerEmail.text.Trim();
            registerPassword.text = string.Empty;
            registerConfirmPassword.text = string.Empty;

            await AutoLoginTrasRegistro(emailUsado, passwordUsada);
        }
        catch (Exception error)
        {
            MostrarErrorUI("Error al registrar cuenta: " + error.Message);
        }
    }

    public Task<RegisterPlayFabUserResult> RegisterUserInPlayfabTask()
    {
        var taskSource = new TaskCompletionSource<RegisterPlayFabUserResult>();

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {
            Email = registerEmail.text.Trim(),
            Username = registerUsername.text.Trim(),
            Password = registerPassword.text,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request,
            resultCallback => taskSource.SetResult(resultCallback),
            errorCallback => taskSource.SetException(new Exception(errorCallback.GenerateErrorReport()))
        );

        return taskSource.Task;
    }

    private async Task AutoLoginTrasRegistro(string email, string password)
    {
        try
        {
            var taskSource = new TaskCompletionSource<LoginResult>();
            LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
            {
                Email = email,
                Password = password
            };

            PlayFabClientAPI.LoginWithEmailAddress(request,
                resultCallback => taskSource.SetResult(resultCallback),
                errorCallback => taskSource.SetException(new Exception(errorCallback.GenerateErrorReport()))
            );

            await taskSource.Task;

            IngresarAlJuego();
        }
        catch (Exception error)
        {
            MostrarErrorUI("Registro exitoso, pero hubo un error al iniciar sesión automáticamente: " + error.Message);
        }
    }


    public async void LoginUserInPlayfab()
    {
        if (string.IsNullOrWhiteSpace(loginEmail.text) || string.IsNullOrWhiteSpace(loginPassword.text))
        {
            MostrarErrorUI("Ingresa tu correo y contraseña para iniciar sesión.");
            return;
        }

        try
        {
            await LoginUserInPlayfabTask();
            loginPassword.text = string.Empty;

            IngresarAlJuego();
        }
        catch (Exception error)
        {
            MostrarErrorUI("Error al iniciar sesión: " + error.Message);
        }
    }

    public Task<LoginResult> LoginUserInPlayfabTask()
    {
        var taskSource = new TaskCompletionSource<LoginResult>();

        LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
        {
            Email = loginEmail.text.Trim(),
            Password = loginPassword.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
            resultCallback => taskSource.SetResult(resultCallback),
            errorCallback => taskSource.SetException(new Exception(errorCallback.GenerateErrorReport()))
        );

        return taskSource.Task;
    }

    private void IngresarAlJuego()
    {
        if (panelAutenticacion != null) panelAutenticacion.SetActive(false);
        if (panelMenuJuego != null) panelMenuJuego.SetActive(true);

        DownloadDataFromPlayfab();
    }

    public async void UploadDataInPlayfab()
    {
        try
        {
            await UploadDataInPlayfabTask();
        }
        catch (Exception error)
        {
            Debug.LogError($"Error al subir datos vía CloudScript: {error.Message}");
        }
    }

    public Task<ExecuteCloudScriptResult> UploadDataInPlayfabTask()
    {
        var taskSource = new TaskCompletionSource<ExecuteCloudScriptResult>();

        var dataObject = new Dictionary<string, string>()
        {
            { "kill count", StatManager.Instance != null ? StatManager.Instance.killCount.ToString() : "0" },
            { "deaths count", StatManager.Instance != null ? StatManager.Instance.deathsCount.ToString() : "0" },
            { "player color", StatManager.playerColor ?? "#FFFFFF" }
        };

        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "savePlayerData",
            FunctionParameter = new { playerData = dataObject },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            resultCallback => taskSource.SetResult(resultCallback),
            errorCallback => taskSource.SetException(new Exception(errorCallback.GenerateErrorReport()))
        );

        return taskSource.Task;
    }

    public async void DownloadDataFromPlayfab()
    {
        try
        {
            var result = await DownloadDataFromPlayfabTask();

            if (result.FunctionResult != null)
            {
                string jsonResult = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).SerializeObject(result.FunctionResult);
                var responseData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).DeserializeObject<Dictionary<string, string>>(jsonResult);

                Dictionary<string, string> downloadedStats = new Dictionary<string, string>
                {
                    ["kill count"] = responseData.ContainsKey("kill count") ? responseData["kill count"] : "0",
                    ["deaths count"] = responseData.ContainsKey("deaths count") ? responseData["deaths count"] : "0",
                    ["player color"] = responseData.ContainsKey("player color") ? responseData["player color"] : null
                };

                if (StatManager.Instance != null)
                {
                    if (int.TryParse(downloadedStats["kill count"], out int kills))
                        StatManager.Instance.killCount = kills;

                    if (int.TryParse(downloadedStats["deaths count"], out int deaths))
                        StatManager.Instance.deathsCount = deaths;
                }

                string colorRemoto = responseData.ContainsKey("player color") ? responseData["player color"] : null;
                if (!string.IsNullOrEmpty(colorRemoto) && colorRemoto != "#FFFFFF")
                {
                    StatManager.playerColor = colorRemoto;
                }
                else if (string.IsNullOrEmpty(StatManager.playerColor))
                {
                    StatManager.playerColor = "#FFFFFF"; // Fallback únicamente si localmente también está vacío
                }

                Debug.Log($"Color de jugador sincronizado: {StatManager.playerColor}");
            }
        }
        catch (Exception error)
        {
            Debug.LogError($"Error al obtener datos por CloudScript: {error.Message}");
        }
    }

    public Task<ExecuteCloudScriptResult> DownloadDataFromPlayfabTask()
    {
        var taskSource = new TaskCompletionSource<ExecuteCloudScriptResult>();

        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "getPlayerData",
            FunctionParameter = null,
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            resultCallback => taskSource.SetResult(resultCallback),
            errorCallback => taskSource.SetException(new Exception(errorCallback.GenerateErrorReport()))
        );

        return taskSource.Task;
    }

    public void MostrarErrorUI(string mensaje)
    {
        if (panelError != null)
        {
            if (errorText != null)
            {
                errorText.text = mensaje;
                errorText.gameObject.SetActive(true);
            }

            panelError.SetActive(true);

            if (errorCoroutine != null) StopCoroutine(errorCoroutine);
            errorCoroutine = StartCoroutine(OcultarErrorDespuesDeTiempo(5f));
        }
        else
        {
            Debug.LogError(mensaje);
        }
    }

    private IEnumerator OcultarErrorDespuesDeTiempo(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (errorText != null)
        {
            errorText.text = string.Empty;
            errorText.gameObject.SetActive(false);
        }

        if (panelError != null)
        {
            panelError.SetActive(false);
        }
    }
}