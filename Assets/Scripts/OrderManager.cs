using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Ordermanager keeps track of the order players get their turn and grants ownership to that player.
/// </summary>
public class OrderManager : NetworkBehaviour
{
    [SerializeField]
    private GameLobbySO gameLobbyData;

    [SerializeField, Tooltip("In what order will the game be played?")]
    private List<Team> teamsOrder;

    private int[] teamTurns;

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
        teamsOrder = new(gameLobbyData.Teams);
        teamTurns = new int[teamsOrder.Count];
        teamsOrder.Shuffle();
    }


    /// <summary>
    /// When the game starts, we need to spawn a coin for the current player.
    /// </summary>
    public void OnGameStart()
    {
        GrandTurn();
    }


    /// <summary>
    /// Only happens on server side.
    /// </summary>
    private void AfterCoinDrop()
    {
        currentTurn++;
        GrandTurn();
    }
    
    /// <summary>
    /// Grants the turn to the next team and player, and informs all clients to locally spawn a coin for that team.
    /// </summary>
    private void GrandTurn()
    {
        int currentIndex = currentTurn % teamsOrder.Count;
        Team currentTeam = teamsOrder[currentIndex];
        teamTurns[currentIndex]++;
        Player currentPlayer = gameLobbyData.GetCurrentPlayer(currentTeam, teamTurns[currentIndex]);
        coinDropper.NetworkObject.ChangeOwnership(currentPlayer.ClientID);//Grant ownership to the player that got the next turn.
        coinDropper.CreateCoinClientRpc(currentTeam.TeamID);
    }
}
