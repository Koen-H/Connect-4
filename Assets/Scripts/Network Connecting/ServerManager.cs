using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// Manages the server,
/// </summary>
public class ServerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The max amount of players allowed to connect to the server")]
    public int maxPlayers;

    [SerializeField]
    private bool useUnityRelayServices = false;

    [SerializeField]
    private bool useUnityLobbyServices = false;

    [SerializeField]
    private string joinCode;
    public string JoinCode => joinCode;

    private Lobby currentLobby;


    private async void StartServer()
    {
        if (useUnityRelayServices) await CreateRelayConnection();

        NetworkManager.Singleton.StartHost();
    }


    private async Task CreateRelayConnection()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        joinCode = newJoinCode;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        if (useUnityLobbyServices) await CreateLobby();

    }

    private async Task CreateLobby()
    {
        CreateLobbyOptions createLobbyOption = new CreateLobbyOptions();
        createLobbyOption.IsPrivate = false;
        createLobbyOption.Data = new Dictionary<string, DataObject>();
        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, JoinCode);

        createLobbyOption.Data.Add("JOIN_CODE", dataObject);
        currentLobby = await Lobbies.Instance.CreateLobbyAsync("Pirates killed lobby", maxPlayers, createLobbyOption);
        Debug.Log(currentLobby.LobbyCode);
        Debug.Log(currentLobby.Data["JOIN_CODE"].Value);
        StartCoroutine(HeartBeatLobbyCoroutine());

    }


    /// <summary>
    /// Send a heartbeat to the lobby to prevent it from shutting off
    /// </summary>
    /// <param name="waitTimeSeconds">Unity checks every 30 seconds, this value should be lower than 30</param>
    /// <returns></returns>
    private IEnumerator HeartBeatLobbyCoroutine(float waitTimeSeconds = 15)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            if (currentLobby == null) break;
            LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            yield return delay;
        }
    }

}
