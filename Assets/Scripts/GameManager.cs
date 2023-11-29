using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public static GameManager Singleton
    {
        get
        {
            if (instance == null) Debug.LogError("GameManager is null!");
            return instance;
        }
    }

    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private GameBoard gameBoard;

    //[SerializeField]
    //private LobbyManager lobbyManager;
    //[SerializeField]
    //private OrderManager orderManager;


    List<bool> gameLoadedOnClients;

    private void Awake()
    {
        if(instance != null) {
            return;
        }
        instance = this;
        gameBoard.OnGameBoardGenerated += (GameReadyServerRpc);
    }


    /// <summary>
    /// Initialize the game, by generating the gameBoard and teams order.
    /// </summary>
    public void InitGame()
    {
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
        orderManager.OnGameStart();
    }
}
