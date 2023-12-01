using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lobby manager is a custom made manager for keeping track of clientsManagers.
/// In combination with networkSceneManager, it will bring clients back to the main menu when they lose connection.
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    [SerializeField, Tooltip("Data used for local play")]
    private GameLobbySO gameLobby;

    private Dictionary<ulong, ClientManager> clients = new Dictionary<ulong, ClientManager>();
    public Dictionary<ulong, ClientManager> Clients { get { return clients; } }

    public static event System.Action<ClientManager> OnNewClientJoined;
    public static event System.Action<ClientManager> OnClientLeft;
    

    private static LobbyManager instance;
    public static LobbyManager Singleton
    {
        get
        {
            if (instance == null) Debug.LogError("LobbyManager is null");
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            //When in the main menu, become the new one
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                Destroy(instance.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start() 
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientConnectionLost;
    }

    public void AddClient(ulong id, ClientManager newClient)
    {
        clients.Add(id, newClient);
        OnNewClientJoined?.Invoke(newClient);

        Debug.Log($"Client({id}) connected!");
    }
    public void RemoveClient(ulong id, ClientManager leftClient)
    {
        clients.Remove(id);
        OnClientLeft?.Invoke(leftClient);
        Debug.Log($"Client({id}) disconnected!");
    }

    public ClientManager GetClient(ulong id)
    {
        if (clients.ContainsKey(id)) return clients[id];
        return null;
    }

    public void OnClientConnectionLost(ulong lostClientID)
    {
        ClientManager client = GetClient(lostClientID);
        client?.OnLeaving();

        if (NetworkManager.ServerClientId == lostClientID)//Lost connection with host
        {
            Debug.Log("Lost connection with host!");
            SceneChangeManager.Singleton.ReturnToMain();
        }
    }


    /// <summary>
    /// Get a list of client IDs and exclude one client from it.
    /// Useful for ServerRPC where there is no need to send a message back to the client that send the ServerRpc
    /// </summary>
    /// <param name="excludedClient">The client id to be excluded from the list</param>
    /// <returns>A list of client ids excluding the exluded client</returns>
    public List<ulong> GetServerRpcList(ulong excludedClient)
    {
        List<ulong> clients = new();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != excludedClient) clients.Add(clientId);
        }
        return clients;
    }

}
