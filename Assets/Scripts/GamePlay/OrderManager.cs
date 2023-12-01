using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static GameManager;

/// <summary>
/// Ordermanager keeps track of the order players get their turn and grants ownership to that player.
/// </summary>
public class OrderManager : NetworkBehaviour
{
    [SerializeField]
    private GameLobbySO gameLobby;

    [SerializeField, Tooltip("In what order will the game be played?")]
    private List<Team> teamsOrder;

    private int[] teamTurns;

    private CoinDropper coinDropper;

    //Keep track on how many turns there have been
    private int currentTurn = 0;

    private ClientManager currentClient;

    private NetworkVariable<Player> currentPlayer = new();
    public NetworkVariable<Player>.OnValueChangedDelegate OnCurrentPlayerChanged { get { return currentPlayer.OnValueChanged; } set { currentPlayer.OnValueChanged = value; } }

    private void Awake()
    {
        coinDropper = GetComponent<CoinDropper>();
        if (gameLobby == null) Debug.LogError("The gamelobbySO is not set");
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
        teamsOrder = new(gameLobby.Teams);
        teamTurns = new int[teamsOrder.Count];
        teamsOrder.Shuffle();
    }

    public void OnGameStart()
    {
        GrandTurn();
    }


    /// <summary>
    /// Increases the current turn and will grand the turn to the next player, should only happen on server side.
    /// </summary>
    private void AfterCoinDrop()
    {
        currentTurn++;
        if(currentClient != null) currentClient.OnClientLeft -= OnCurrentPlayerLeave;
        GrandTurn();
    }
    
    /// <summary>
    /// Grants the turn to the next team and gives ownership of the controls to the next player's client. 
    /// Informs all clients to locally spawn a coin for that team.
    /// </summary>
    private void GrandTurn()
    {
        int currentIndex = currentTurn % teamsOrder.Count;
        Team currentTeam = teamsOrder[currentIndex];
        teamTurns[currentIndex]++;

        Player newCurrentPlayer = gameLobby.GetCurrentPlayer(currentTeam, teamTurns[currentIndex]);
        currentPlayer.Value = newCurrentPlayer;
        ulong currentClientID = currentPlayer.Value.ClientID;
        coinDropper.CreateCoinClientRpc(currentTeam.TeamID);
        if (!LobbyManager.Singleton.Clients.ContainsKey(currentClientID))
        {
            coinDropper.NetworkObject.RemoveOwnership();//Get ownership to server!
            coinDropper.StartRandomDrop();
        }
        else
        {
            currentClient = LobbyManager.Singleton.GetClient(currentClientID);
            currentClient.OnClientLeft += OnCurrentPlayerLeave;
            coinDropper.NetworkObject.ChangeOwnership(currentClientID);
        }//Grant ownership to the player that got the next turn.
    }

    /// <summary>
    /// When a client leaves while it's their turn, catch it and use a random drop.
    /// </summary>
    /// <param name="clientManger"></param>
    private void OnCurrentPlayerLeave(ClientManager clientManger)
    {
        coinDropper.NetworkObject.RemoveOwnership();//Get ownership to server!
        coinDropper.StartRandomDrop();
    }
}
