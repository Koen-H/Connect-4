using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
    private GameBoard gameBoardPrefab;
    //[SerializeField]
    //private OrderManager orderManager;



    [SerializeField]
    private GameBoard gameBoard;
    public GameBoard GameBoard { get { return gameBoard; } }

    //[SerializeField]
    //private LobbyManager lobbyManager;
    //[SerializeField]
    //private OrderManager orderManager;

    public UnityEvent OnGameBeginInitialize = new();
    public UnityEvent OnGameAfterInitialize = new();

    public UnityEvent OnGameStart = new();


    private void Awake()
    {
        if(instance != null) {
            return;
        }
        instance = this;
    }


    /// <summary>
    /// Init the game, such as making teams and the board
    /// </summary>
    public void InitGame()
    {
        //Instantiate the gameBoard
        gameBoard = Instantiate(gameBoardPrefab);

        OnGameBeginInitialize.Invoke();
        gameBoard.GenerateBoard();

        //orderManager.CreateOrder();

        OnGameAfterInitialize.Invoke();
    }

    public void StartGame()
    {
       // orderManager.OnGameStart();
        OnGameStart.Invoke();
    }
}
