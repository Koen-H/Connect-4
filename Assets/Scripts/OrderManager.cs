using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Ordermanager keeps track of the order players get their turn
/// </summary>
public class OrderManager : MonoBehaviour
{
    [SerializeField]
    GameLobbySO gameLobby;

    [SerializeField, Tooltip("In what order will the game be played?")]
    private List<Team> teamsOrder;

    [SerializeField]
    private List<Player> playerOrder;

    [SerializeField]
    CoinDropper coinDropper;

    //Keep track on how many turns there have been
    private int currentTurn = 0;

    public UnityEvent<List<Player>> OnOrderChanged;


    private void Awake()
    {
        if(coinDropper == null) GetComponent<CoinDropper>();
        coinDropper.OnCoinDropped.AddListener(AfterCoinDrop);
    }


    public void CreateOrder()
    {
        //Create a team order
        teamsOrder = new (gameLobby.Teams);
        teamsOrder.Shuffle();

        playerOrder = new();

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
        coinDropper.CreateCoin(currentTeam);
    }

    /// <summary>
    /// Change the turn to the next player
    /// </summary>
    private void ChangeTurn()
    {
        //Put the player that's currently first back to the end
        Player firstPlayer = playerOrder[0];
        playerOrder.RemoveAt(0);
        playerOrder.Add(firstPlayer);

        OnOrderChanged.Invoke(playerOrder);


    }

}
