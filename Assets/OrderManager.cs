using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField]
    private List<Team> teamsOrder;

    [SerializeField]
    CoinDropper coinDropper;

    [SerializeField]
    LobbyManager lobbyManager;

    private int currentTurn = 0;


    private void Awake()
    {
        if(coinDropper == null) GetComponent<CoinDropper>();
        coinDropper.OnCoinDropped.AddListener(AfterCoinDrop);
    }


    public void CreateOrder()
    {
        teamsOrder = new (lobbyManager.Teams);
        teamsOrder.Shuffle();
    }

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

}
