using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lobby manager keeps track of who's currently in the lobby and the teams
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    [SerializeField, Tooltip("Data used for local play")]
    private GameLobbySO gameLobby;


    //Lobby variables
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
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void Start() {

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientConnectionLost;
        SceneChangeManager.Singleton.OnAllLoadCompleteServerSide += OnEveryoneLoadedScene;
    }


    public void AddClient(ulong id, ClientManager newClient)
    {
        clients.Add(id, newClient);
        if (OnNewClientJoined != null) OnNewClientJoined.Invoke(newClient);

        Debug.Log($"Client({id}) connected!");
    }
    public void RemoveClient(ulong id, ClientManager leftClient)
    {
        clients.Remove(id);
        if (OnClientLeft != null) OnClientLeft.Invoke(leftClient);
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
        if (client == null) return;
        client.OnLeaving();

        if (NetworkManager.ServerClientId == lostClientID)//Lost connection with host
        {
            ReturnToMain();
        }
    }

    public void ReturnToMain(bool connectionLost = false) => NetworkManager.Singleton.Shutdown();

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



    public void OnEveryoneReady()
    {
        NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    /// <summary>
    /// Once everyone in the lobby has loaded a specific scene, do something on the scene (Server side)
    /// </summary>
    /// <param name="sceneEvent"></param>
    public void OnEveryoneLoadedScene(SceneEvent sceneEvent)
    {
        //EVeryone finished the gameScene, tell the gameManager in this scene to start loading the game
        if(sceneEvent.SceneName == "GameScene")
        {
            GameManager.Singleton.InitGame();
        }
    }


    //public void CreateTeams()
    //{

    //    int amountOfTeams = teams.Count;

    //    //Randomly place the players in a team
    //    List<Player> shuffledList = new List<Player>(players);
    //    shuffledList.Shuffle();
    //    for (int i = 0; i < shuffledList.Count; i++)
    //    {
    //        teams[i % amountOfTeams].AddPlayer(shuffledList[i]);
    //    }

    //}

}
