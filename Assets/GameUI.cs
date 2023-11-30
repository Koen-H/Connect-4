using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private GameObject gamePlayUI;
    [SerializeField]
    private TextMeshProUGUI currentTurnTextUI;

    [Header("After game")]
    [SerializeField]
    private GameObject afterGameUI;
    [SerializeField]
    private TextMeshProUGUI winnerTextUI;

    [Header("Always displayed")]
    [SerializeField]
    private TextMeshProUGUI connectedClientsTextUI;

    [Header("Scene Objects")]
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private OrderManager orderManager;
    [SerializeField]
    private GameBoard gameBoard;

    [Header("Scriptable Objects")]
    [SerializeField]
    private GameLobbySO gameLobby;


    private void Awake()
    {
        gameManager.OnGameStateChange += OnGameStateChange;
        orderManager.OnCurrentPlayerChanged += OnCurrentPlayerChange;
        gameBoard.OnGameWin += OnGameWon;
    }

    private void OnEnable()
    {
        LobbyManager.OnClientLeft += UpdateActiveClients;
        UpdateActiveClients();
    }
    private void OnDisable()
    {
        LobbyManager.OnClientLeft -= UpdateActiveClients;
    }

    private void OnDestroy()
    {
        gameManager.OnGameStateChange -= OnGameStateChange;
        orderManager.OnCurrentPlayerChanged -= OnCurrentPlayerChange;
        gameBoard.OnGameWin -= OnGameWon;
    }

    private void UpdateActiveClients(ClientManager leavingClient = null)
    {
        int connectedClients = LobbyManager.Singleton.Clients.Count;
        string newText = $"{connectedClients} client";
        newText += connectedClients > 1 ? "s connected" : " connected";
        connectedClientsTextUI.text = newText;
    }


    private void OnGameStateChange(GameManager.GameState oldGameState, GameManager.GameState newGameState)
    {
        //There's only two states for the UI. I didn't think it was worth making it a FSM as I dont see any future changes within this aspect.
        bool isGameplay = newGameState == GameManager.GameState.Playing;
        gamePlayUI.SetActive(isGameplay);
        afterGameUI.SetActive(!isGameplay);
    }

    private void OnCurrentPlayerChange(Player oldPlayer, Player newPlayer)
    {
        string newText = $"It's {newPlayer.PlayerName}'s";
        if (newPlayer.ClientID == NetworkManager.Singleton.LocalClientId) newText += " (your)";
        newText += " turn!";
        currentTurnTextUI.text = newText;
        currentTurnTextUI.color = gameLobby.GetTeamByID(newPlayer.TeamID).TeamColor;
    }

    private void OnGameWon(int winningTeamId)
    {
        Team winningTeam = gameLobby.GetTeamByID(winningTeamId);
        winnerTextUI.text = $"{winningTeam.TeamName} WINS!";
        winnerTextUI.color = winningTeam.TeamColor;
    }


    public void QuitGame()
    {
        NetworkManager.Singleton.Shutdown();
    }

}
