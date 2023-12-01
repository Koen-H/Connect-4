using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField]
    private Button replayButton;


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
        gameBoard.OnGameTied += OnGameTied;
        //If a player leaves, disable the replayButton 
        LobbyManager.OnClientLeft += DisableReplayButton;
    }

    /// <summary>
    /// Disable the interaction of the replayButton
    /// </summary>
    /// <param name="clientManager"></param>
    void DisableReplayButton(ClientManager clientManager)
    {
        replayButton.interactable = false;
    }

    private void OnDestroy()
    {
        LobbyManager.OnClientLeft -= DisableReplayButton;
        gameManager.OnGameStateChange -= OnGameStateChange;
        orderManager.OnCurrentPlayerChanged -= OnCurrentPlayerChange;
        gameBoard.OnGameWin -= OnGameWon;
        gameBoard.OnGameTied -= OnGameTied;
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

    private void OnGameTied()
    {
        winnerTextUI.text = "It's a tie!";
        winnerTextUI.color = Color.white;
    }


    public void QuitGame()
    {
        SceneChangeManager.Singleton.ReturnToMain();
    }

}
