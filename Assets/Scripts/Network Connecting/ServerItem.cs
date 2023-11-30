
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class ServerItem : MonoBehaviour
{
    
    [SerializeField] 
    private TextMeshProUGUI serverName;
    [SerializeField]
    private TextMeshProUGUI lobbyCode;

    private string lobbyId;
    private string joinCode;

    public void SetupServerItem(Lobby lobby)
    {
        serverName.text = lobby.Name;
        lobbyCode.text = lobby.LobbyCode;
        lobbyId = lobby.Id;
        joinCode = lobby.Data["JOIN_CODE"].Value;
    }


    public void TryJoinServer()
    {
        try
        {
            JoinServerViaJoincode(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinServerViaJoincode(string joinCode)
    {
        
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }
}
