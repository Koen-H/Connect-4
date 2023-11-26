using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lobby manager keeps track of who's currently in the lobby
/// </summary>
public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private List<Player> players = new List<Player>();

    [SerializeField]
    private List<Team> teams = new List<Team>();

    public List<Team> Teams { get { return teams; } }
    
    public void CreateTeams()
    {
        //teams = new List<Team>();
        //for(int t = 0; t < teamsAmount; t++)
        //{
        //    teams.Add(new Team())

        //}

        int amountOfTeams = teams.Count;

        //Randomly place the players in a team
        List<Player> shuffledList = new List<Player>(players);
        shuffledList.Shuffle();
        for (int i = 0; i < shuffledList.Count; i++)
        {
            teams[i % amountOfTeams].AddPlayer(shuffledList[i]);
        }

    }

}
