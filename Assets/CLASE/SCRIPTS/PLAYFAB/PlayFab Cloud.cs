using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using UnityEngine;
using System.Collections.Generic;

public class PlayFabCloud : MonoBehaviour
{
    // diferencias entre c# y javascript
    // - javascript usa tipado dinámico, no hay que especificar el tipo de variable
    // - c# usa tipado estático, donde tienes que declarar que tipo de variable usas

    // - c# es un lenguaje principalmente dedicado a programación orientada a objetos
    // - javascript es un lenguaje que puede ser dedica a objetos, o funcional (a que unicamente servirá para ejecutar instrucciones, sin encapsularlas o ligarlas a un obj especifico)




    //traducir el "helloworld"
    public void CallHelloWorld(string inputVal)
    {
        // en javascript vi que habia un "request" que guardaba los datos.
        // aca cree este objeto que sirve para lo mismo:
        // decirle a playfab que funcion quiero correr y que datos le voy a mandar.
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "helloWorld", // aca el nombre de la funcion

            // en el script original usan un "args.inputValue". 
            // como acá no se pueden mandar variables asi de una, use un diccionario 
            // para guardar el texto con la etiqueta "inputValue"
            FunctionParameter = new Dictionary<string, object>() {
                { "inputValue", inputVal }
            },
            GeneratePlayStreamEvent = true // esto es solo para que quede registrado en la pagina de playfab
        };

        // le mando la peticion que armé arriba, y le digo que si todo sale chido corra "OnHelloWorldSuccess",
        // y si algo truena corra "OnErrorShared"
        PlayFabClientAPI.ExecuteCloudScript(request, OnHelloWorldSuccess, OnErrorShared);

    }

    private void OnHelloWorldSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("el cloudscript de hello world corrió chido c:");

        // en el script original vi un "return {messageValue: message}"
        // para cachar ese mensaje aca en unity, checo si el resultado no viene vacio.
        if (result.FunctionResult != null)
        {
            // como playfab regresa json, lo convierto a texto
            // para poder ver en la consola de unity el saludo que armo el servidor.
            string resultadoTexto = result.FunctionResult.ToString();
            Debug.Log("el server me regreso esto: " + resultadoTexto);
        }
    }



    // para la segunda funcion, vi que en javascript no pide ningun parametro (args) desde el cliente.
    // asi que arme este metodo vacio en c# que no pide nada en los parentesis.
    public void CallMakeAPICall()
    {
        // igual que antes, armo la peticion.
        // solo necesito ponerle el nombre exacto de la funcion que esta en la nube.
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "makeAPICall",
            GeneratePlayStreamEvent = true
        };

        // en la funcion original vi que se creaba un request con "Statistics" y "Value: 2".
        // me di cuenta de que no tengo que escribir nada de eso en c#, porque esa modificacion 
        // de datos (subir de nivel) se hace de forma segura en el servidor de playfab.
        PlayFabClientAPI.ExecuteCloudScript(request, OnMakeAPICallSuccess, OnErrorShared);
    }

    // este metodo corre cuando la base de datos ya se actualizo en la nube.
    private void OnMakeAPICallSuccess(ExecuteCloudScriptResult result)
    {
        // no necesito recibir ningun texto de vuelta, con que me avise que se cambio el nivel a 2 estoy conforme.
        Debug.Log("se actualizaron las estadisticas en la nube con exito :D");
    }



    // este metodo lo comparten las dos funciones de arriba. 
    // si pasó algo, aca me va a avisar que fue 
    private void OnErrorShared(PlayFabError error)
    {
        Debug.LogError("valio queso el cloudscript: " + error.GenerateErrorReport());
    }
}