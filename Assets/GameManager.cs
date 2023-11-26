using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
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
    private GameBoard board;
    [SerializeField]
    private LobbyManager lobbyManager;
    [SerializeField]
    private OrderManager orderManager;

    public UnityEvent OnGameInitialized = new();


    private void Awake()
    {
        if(instance != null) {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }



    /// <summary>
    /// Init the game, such as making teams and the board
    /// </summary>
    public void InitGame()
    {
        board.GenerateBoard();
        lobbyManager.CreateTeams();
        orderManager.CreateOrder();

        OnGameInitialized.Invoke();
        StartGame();
    }

    public void StartGame()
    {
        orderManager.OnGameStart();
    }
}
