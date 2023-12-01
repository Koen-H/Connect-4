
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class SingleLobbyItemUI : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI lobbyName;
    [SerializeField]
    private TextMeshProUGUI lobbyCode;

    private string joinCode;

    /// <summary>
    /// Set up the lobbyITem by changing the UI text and setting its data used to join the server
    /// </summary>
    /// <param name="lobby"></param>
    public void SetupLobbyItem(Lobby lobby)
    {
        lobbyName.text = lobby.Name;
        lobbyCode.text = lobby.Data["JOIN_CODE"].Value;
        joinCode = lobby.Data["JOIN_CODE"].Value;
    }
    
    /// <summary>
    /// Try and join the associated server.
    /// </summary>
    public void TryJoinServer()
    {
        try
        {
            ServerManager.Singleton.SetupRelayConnectionViaRelayJoincode(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }



}
