using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Gamemanager keeps track of the current state of the game, it can init, start and replay the game.
/// </summary>
public class GameManager : NetworkBehaviour
{

    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private GameBoard gameBoard;

    public enum GameState {Loading, Playing, After}

    private NetworkVariable<GameState> currentGameState = new(GameState.Loading);//Default value loading as the clients are loading when the server enters the scene.
    public NetworkVariable<GameState>.OnValueChangedDelegate OnGameStateChange { get { return currentGameState.OnValueChanged; } set { currentGameState.OnValueChanged = value; } }

    private List<bool> gameLoadedOnClients = new();

    private void Awake()
    {
        SceneChangeManager.Singleton.OnAllLoadCompleteServerSide += OnEveryoneLoadedScene;
        gameBoard.OnGameBoardGenerated += GameReadyServerRpc;
    }

    public override void OnNetworkSpawn()
    {
        gameBoard.OnGameWin += EndGame;
        gameBoard.OnGameTied += EndGame;
    }

    public override void OnNetworkDespawn()
    {
        gameBoard.OnGameWin -= EndGame;
        gameBoard.OnGameTied -= EndGame;
    }
    public void OnDisable()
    {
        
        gameBoard.OnGameBoardGenerated -= GameReadyServerRpc;
    }

    #region Invoked by buttons after game ended

    /// <summary>
    /// Inform all clients to clean up the board and set the gamestate to playing again.
    /// </summary>
    public void ReplayGame()
    {
        if (!IsServer) return;
        gameBoard.ResetBoardClientRpc();
        currentGameState.Value = GameState.Playing;
    }

    /// <summary>
    /// Loads the lobby scene again
    /// </summary>
    public void ChangeTeams()
    {
        NetworkManager.SceneManager.LoadScene("LobbyScene",LoadSceneMode.Single);
    }
    #endregion


    /// <summary>
    /// Once everyone in the lobby has loaded the game scene, Initialize the game on the server side
    /// </summary>
    /// <param name="sceneEvent"></param>
    public void OnEveryoneLoadedScene(SceneEvent sceneEvent)
    {
        Debug.Log("Everyone finished loading");
        if (sceneEvent.SceneName == "GameScene")
        {
            InitGame();
        }
        SceneChangeManager.Singleton.OnAllLoadCompleteServerSide -= OnEveryoneLoadedScene;
    }

    /// <summary>
    /// Initialize the game, by generating the gameBoard and teams order.
    /// </summary>
    public void InitGame()
    {
        currentGameState.Value = GameState.Loading;
        gameLoadedOnClients.Clear();
        
        orderManager.CreateOrder();
        gameBoard.GenerateBoardClientRpc();
    }

    /// <summary>
    /// As a client, let the server know that the gameboard loaded in and the game is ready to be played
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

    #region End game
    private void EndGame(int WinningTeamID)
    {
        if (IsServer) currentGameState.Value = GameState.After;
    }

    private void EndGame()
    {
        if(IsServer) currentGameState.Value = GameState.After;
    }
    #endregion

}
