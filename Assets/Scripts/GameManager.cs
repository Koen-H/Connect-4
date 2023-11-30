using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{

    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private GameBoard gameBoard;

    public enum GameState {Loading, Playing, After}

    private NetworkVariable<GameState> currentGameState = new(GameState.Loading);//Default value loading as the clients are loading when the server enters the scene.
    public NetworkVariable<GameState>.OnValueChangedDelegate OnGameStateChange { get { return currentGameState.OnValueChanged; } set { currentGameState.OnValueChanged = value; } }

    private List<bool> gameLoadedOnClients;

    private void Awake()
    {
        SceneChangeManager.Singleton.OnAllLoadCompleteServerSide += OnEveryoneLoadedScene;
        gameBoard.OnGameBoardGenerated += GameReadyServerRpc;
    }

    public override void OnNetworkSpawn()
    {
        gameBoard.OnGameWin += OnGameWin;
    }

    public override void OnNetworkDespawn()
    {
        gameBoard.OnGameWin -= OnGameWin;
    }
    private void OnDestroy()
    {
        SceneChangeManager.Singleton.OnAllLoadCompleteServerSide -= OnEveryoneLoadedScene;
        gameBoard.OnGameBoardGenerated -= GameReadyServerRpc;
    }


    public void ReplayGame()
    {
        if (!IsServer) return;
        gameBoard.ResetBoardClientRpc();
    }

    public void ChangeTeams()
    {
        NetworkManager.SceneManager.LoadScene("LobbyScene",LoadSceneMode.Single);
    }


    /// <summary>
    /// Once everyone in the lobby has loaded the game scene, start the InitGame on server side
    /// </summary>
    /// <param name="sceneEvent"></param>
    public void OnEveryoneLoadedScene(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneName == "GameScene")
        {
            InitGame();
        }
    }

    /// <summary>
    /// Initialize the game, by generating the gameBoard and teams order.
    /// </summary>
    public void InitGame()
    {
        currentGameState.Value = GameState.Loading;
        gameLoadedOnClients  = new List<bool>();
        
        orderManager.CreateOrder();
        //Generate the board on all clients, this includes the server as the server is a host (server as client)
        gameBoard.GenerateBoardClientRpc();
    }

    /// <summary>
    /// As a client, let the server know that the gameboard loaded in
    /// </summary>
    [ServerRpc(RequireOwnership =false)]
    private void GameReadyServerRpc()
    {
        gameLoadedOnClients.Add(true);
        //If every client loaded the game, start the game.
        if (gameLoadedOnClients.Count == LobbyManager.Singleton.Clients.Count) StartGame();
    }

    private void StartGame()
    {
        currentGameState.Value = GameState.Playing;
        orderManager.OnGameStart();
    }

    private void OnGameWin(int winningTeamID)
    {
        currentGameState.Value = GameState.After;
    }

}
