using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    [SerializeField] private GameObject sessionPrefab;
    [SerializeField] private Transform viewportContent;
    [SerializeField] private GameObject noSessionMessage;

    //este meodoto controlara el que pasara dependiendo de cuantas sesiones existan, siendo
    //las unicas condiciones que la cantidad de sesion sea igual a 0 o mayor a 0
    private List<SessionInfo> sessionList = new List<SessionInfo>();

    private void Start()
    {
        // suscribirse de manera segura en el start
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.onSessionListUpdated += OnSessionListUpdated;
        }

    }

    private void OnEnable()
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.onSessionListUpdated += OnSessionListUpdated;
        }
        PrepareLobbyUI();

    }

    private void OnDisable()
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.onSessionListUpdated -= OnSessionListUpdated;
        }
    }

    public void OnSessionListUpdated(List<SessionInfo> sessionList)
    {
        this.sessionList = sessionList;

        if (this.sessionList == null || this.sessionList.Count == 0)
        {
            ClearSessionList();
            noSessionMessage.SetActive(true);
        }
        else
        {
            UpdateSessionListOnCanvas();
        }
    }

    // el que hará aparecer la lista de sesiones en el canvas
    // este medodo se mandara llamar en dos ocasiones:
    // 1. cuando se presionar el boton de refresh list
    // 2. cuando entras a la lista de sesiones

    public void UpdateSessionListOnCanvas()
    {
        noSessionMessage.SetActive(false);
        ClearSessionList();

        for (int session = 0; session < sessionList.Count; session++)
        {
            SessionInfo currentSession = sessionList[session];

            GameObject sessionGo = Instantiate(sessionPrefab, viewportContent);
            SessionEntry entryScript = sessionGo.GetComponent<SessionEntry>();

            if (entryScript != null)
            {
                entryScript.SetInfo(currentSession);
            }
        }
    }

    private void ClearSessionList()
    {
        foreach (Transform child in viewportContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnNoSessionAvaible()
    {
        ClearSessionList();
        noSessionMessage.SetActive(true);
    }

    public void PrepareLobbyUI()
    {
        ClearSessionList();
        noSessionMessage.SetActive(true); 

        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.ConnectToPhotonLobby();
        }
    }
}

    // En el session manager, cuando si encuentre sesiones y se vaya al método de UpdateSessionListOnCanvas,
    // deben de instanciar el prefab de las sesiones x cantidad de veces según las sesiones existentes. Deben
    // de usar el script SessionEntry que se encuentra en el prefab, para asignar unicamente Nombre del Servidor,
    // y cantidad de jugadores actuales/cantidad máxima de jugadores.
