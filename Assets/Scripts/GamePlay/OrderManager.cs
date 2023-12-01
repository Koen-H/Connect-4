using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static GameManager;

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

    private NetworkVariable<Player> currentPlayer = new();
    public NetworkVariable<Player>.OnValueChangedDelegate OnCurrentPlayerChanged { get { return currentPlayer.OnValueChanged; } set { currentPlayer.OnValueChanged = value; } }

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



    public void OnGameStart()
    {
        Debug.Log("starting the game");
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
        Player newCurrentPlayer = gameLobbyData.GetCurrentPlayer(currentTeam, teamTurns[currentIndex]);
        currentPlayer.Value = newCurrentPlayer;
        coinDropper.NetworkObject.ChangeOwnership(currentPlayer.Value.ClientID);//Grant ownership to the player that got the next turn.
        coinDropper.CreateCoinClientRpc(currentTeam.TeamID);
    }
}
