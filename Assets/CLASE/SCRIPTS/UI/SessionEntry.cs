using UnityEngine;
using TMPro;
using Fusion;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] TMP_Text serverNameLbl;
    [SerializeField] TMP_Text gameModeLbl;
    [SerializeField] TMP_Text mapNameLbl;
    [SerializeField] TMP_Text playerCountLbl;

    private string serverName;
    private string gameMode;
    private string mapName;
    private int playerInGame;
    private int maxPlayers;

    public void SetInfo(SessionInfo sessionInfo)
    {
        this.serverName = sessionInfo.Name;
        this.playerInGame = sessionInfo.PlayerCount;
        this.maxPlayers = sessionInfo.MaxPlayers;

        if (sessionInfo.Properties != null)
        {
            if (sessionInfo.Properties.TryGetValue("GameMode", out SessionProperty modeVal))
            {
                this.gameMode = (string)modeVal;
            }
            else { this.gameMode = "Desconocido"; }

            if (sessionInfo.Properties.TryGetValue("Map", out SessionProperty mapVal))
            {
                this.mapName = (string)mapVal;
            }
            else { this.mapName = "Desconocido"; }
        }
        if (serverNameLbl != null) serverNameLbl.text = this.serverName;
        if (gameModeLbl != null) gameModeLbl.text = this.gameMode;
        if (mapNameLbl != null) mapNameLbl.text = this.mapName;
        if (playerCountLbl != null) playerCountLbl.text = $"{this.playerInGame}/{this.maxPlayers}";
    }

    public void JoinGame()
    {
        if (PhotonManager.Instance != null && !string.IsNullOrEmpty(serverName))
        {
            Debug.Log($"Intentando unirse a la sesión: {serverName}");
            PhotonManager.Instance.JoinSelectedGame(serverName);
        }
    }
}