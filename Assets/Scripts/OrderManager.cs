using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Ordermanager keeps track of the order players get their turn and grants ownership to that player.
/// </summary>
public class OrderManager : NetworkBehaviour
{
    [SerializeField, Tooltip("Reference to the gameLobbyData")]
    private GameLobbySO gameLobbyData;

    [SerializeField, Tooltip("In what order will the game be played?")]
    private List<Team> teamsOrder;

    private CoinDropper coinDropper;

    //Keep track on how many turns there have been
    private int currentTurn = 0;

    private void Awake()
    {
        coinDropper = GetComponent<CoinDropper>();
        //coinDropper.OnCoinDropped.AddListener(AfterCoinDrop);
    }

    /// <summary>
    /// Creates the order the teams will play.
    /// </summary>
    public void CreateOrder()
    {
        teamsOrder = new(gameLobbyData.Teams);
        teamsOrder.Shuffle();
    }


    /// <summary>
    /// When the game starts, we need to spawn a coin for the current player.
    /// </summary>
    public void OnGameStart()
    {
        Team currentTeam = teamsOrder[currentTurn % teamsOrder.Count];
        coinDropper.CreateCoin(currentTeam);
    }


    private void AfterCoinDrop()
    {
        currentTurn++;
        Team currentTeam = teamsOrder[currentTurn % teamsOrder.Count];
        Player currentPlayer = currentTeam.GetCurrentPlayer();
        coinDropper.CreateCoin(currentTeam);
    }
}
