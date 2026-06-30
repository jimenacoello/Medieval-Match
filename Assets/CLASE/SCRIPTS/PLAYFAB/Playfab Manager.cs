using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System.Collections;
using System;


public class PlayfabManager : MonoBehaviour
{


    private void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId)) // para verificar que tengamos un titleid
        {
            PlayFabSettings.TitleId = "1B4AF0";
        }

        if (string.IsNullOrEmpty(PlayFabSettings.DeveloperSecretKey)) // para verificar que tengamos un developer secret key
        {
            PlayFabSettings.DeveloperSecretKey = "DC8JHG5II35G3GOX5W8UGS4RSWBR39R11GYEYIM45XB3QR97HY";
        }
    }

    //crear un metodo donde sellevara a cabo la logica para crear un usuario
    public void RegisterUser()
    {
       RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {
            Email = "",
            Username = "",
            Password = "",
            RequireBothUsernameAndEmail = true,

        };
        
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterUserSuccess, OnPlayFabError); //solicitud, lo que va a pasar si sale bn, lo que pasa si sale mal ;(
        // cada solicitud tiene el que pasa si salio bien o si salio mal 
    }



    public void OnRegisterUserSuccess (RegisterPlayFabUserResult result) // en la variable de result playfab nos manda datos de nuestra cuenta
    {
       
    }

    public void OnPlayFabError(PlayFabError error) // en la variable de error va recibir un msj de playfab indicando el horror
    {
        Debug.Log(error);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public async void RegisterUserInPlayfab() // este metodo async se va a mandar a llamar desde un boton
    {
        try
        {
            var registerTask = RegisterUserInPlayfabTask(); // esperamos a que se complete la tarea
            await RegisterUserInPlayfabTask();// esperar a que se realice la conexión o intento de registro de usuario

            Debug.Log("se ha iniciado sesion correctamente pa");
        }
        catch (Exception error)
        {
            Debug.Log(error.Message);
        }
    }


    public async Task<RegisterPlayFabUserResult> RegisterUserInPlayfabTask()
    {
        var taskSource = new TaskCompletionSource<RegisterPlayFabUserResult>();
        // nos crea una variable que espera un tipo de resultado esp para la tarea
        // pero dentro de esa variable se guarda tanto el resultado como el error
        // taskcompletionsource te la toma en cuenta como si fuera el tipo de dato esperado

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {
            Email = "",
            Username = "",
            Password = "",
            RequireBothUsernameAndEmail = true,

        };

        PlayFabClientAPI.RegisterPlayFabUser(request, resultCallback => taskSource.SetResult(resultCallback), 
            errorCallback => taskSource.SetException(new Exception(errorCallback.GenerateErrorReport())));
        // el resultado que pudiste mandar a un metodo, mejor mandarlo a mi variable
        // una exception es la viarable predilecta de csharp para guardar o manejar errores 
        // yo necesito que errorCallBack el cual es una variable tipo PlayFabError lo transforme a Exception
        // dentro del parentesis de SetException, no puedo poner un PlayFabError, por eso la tengo que convertir
        // GenerateErrorReport me guarda en un Exception lo que originalmente tiempotiene el PlayFabError

        return await taskSource.Task; // devuelve ya sea el error 
    }

    IEnumerator Corrutina()
    {
        yield return new WaitUntil(() => 18 > 25);
    }




}
