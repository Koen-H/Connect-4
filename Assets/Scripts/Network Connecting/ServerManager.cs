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
/// Will create a server and relay + lobby using unity services
/// </summary>
public class ServerManager : MonoBehaviour
{


    [Header("Unity Services"), Tooltip("Unity services have a free limit, can be disabled when testing locally.")]
    [SerializeField]
    private bool useUnityRelayServices = false;
    [SerializeField]
    private bool useUnityLobbyServices = false;

    [Header("Server settings")]
    [SerializeField, Tooltip("The max amount of players allowed to connect to the server")]
    public int maxPlayers;

    [SerializeField]
    private string lobbyName = "MyLobby";

    private static string joinCode;
    public static string JoinCode => joinCode;

    private Lobby currentLobby;
    private static string lobbyCode;
    public static string LobbyCode => lobbyCode;

    private static ServerManager instance;
    public static ServerManager Singleton
    {
        get
        {
            if (instance == null) Debug.LogError("ServerManager is null!");
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Start a server, create a relay if not local
    /// </summary>
    /// <param name="isLocal">Whether it is local only and if it should create a relay and lobby for online play</param>
    public async void StartServer(bool isLocal = false)
    {
        if (useUnityRelayServices && !isLocal) await CreateRelayConnection();

        NetworkManager.Singleton.StartHost();
        SceneChangeManager.Singleton.LoadLobby();
    }

    /// <summary>
    /// Creates a unity relay connection
    /// </summary>
    /// <returns></returns>
    private async Task CreateRelayConnection()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsAuthorized) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        joinCode = newJoinCode;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetHostRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData);

        if (useUnityLobbyServices) await CreateLobby();

    }

    /// <summary>
    /// Creates a unity lobby, lobbies can be retrieved
    /// </summary>
    /// <returns></returns>
    private async Task CreateLobby()
    {
        CreateLobbyOptions createLobbyOption = new CreateLobbyOptions();
        createLobbyOption.IsPrivate = false;
        createLobbyOption.Data = new Dictionary<string, DataObject>();

        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, JoinCode);
        createLobbyOption.Data.Add("JOIN_CODE", dataObject);
        currentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOption);
        lobbyCode = currentLobby.LobbyCode;

        StartCoroutine(HeartBeatLobbyCoroutine());
#if UNITY_EDITOR
        Debug.Log($"LobbyCode = {currentLobby.LobbyCode}");
        Debug.Log($"Joincode = {currentLobby.Data["JOIN_CODE"].Value}");
#endif
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

    /// <summary>
    /// Get a list of the current lobies
    /// </summary>
    /// <returns></returns>
    public async Task<List<Lobby>> GetLobbies()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created
                    )
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            return lobbies.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        return null;
    }

    /// <summary>
    /// Set up a relay connection with a server using a joinCode of the relay
    /// </summary>
    /// <param name="joinCode">The joincode of the relay</param>
    public async void SetupRelayConnectionViaRelayJoincode(string joinCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetClientRelayData(allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }

}
