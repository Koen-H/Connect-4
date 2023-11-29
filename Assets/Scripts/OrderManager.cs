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
       
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer) coinDropper.OnCoinDropped += AfterCoinDrop;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(IsServer) coinDropper.OnCoinDropped -= AfterCoinDrop;
    }


    /// <summary>
    /// Creates the order the teams will play.
    /// </summary>
    public void CreateOrder()
    {
        currentTurn = 0;//Reset the current turn back to 0. Teams can keep their teamturns to allow a fair play order within the teams.
        teamsOrder = new(gameLobbyData.Teams);
        teamsOrder.Shuffle();
    }


    /// <summary>
    /// When the game starts, we need to spawn a coin for the current player.
    /// </summary>
    public void OnGameStart()
    {
        GranTurn();
    }


    /// <summary>
    /// Only happens on server side.
    /// </summary>
    private void AfterCoinDrop()
    {
        currentTurn++;
        GranTurn();
    }
    
    /// <summary>
    /// Grants the turn to the next team and player, and informs all clients to locally spawn a coin for that team.
    /// </summary>
    private void GranTurn()
    {
        Team currentTeam = teamsOrder[currentTurn % teamsOrder.Count];
        Player currentPlayer = currentTeam.GetCurrentPlayer();
        currentTeam.TeamTurn++;
        coinDropper.NetworkObject.ChangeOwnership(currentPlayer.ClientID);//Grant ownership to the player that got the next turn.
        coinDropper.CreateCoinClientRpc(currentTeam.TeamID);
    }
}
