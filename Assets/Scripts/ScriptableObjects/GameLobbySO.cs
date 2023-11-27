using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GameLobbySO : ScriptableObject
{
    [SerializeField]
    public List<Player> Players;

    [SerializeField]
    public List<Team> Teams;


    /// <summary>
    /// Generate teams with random players
    /// </summary>
    public void GenerateTeams(int teamsAmount)
    {
        //Randomly place the players in a team
        List<Player> shuffledList = new List<Player>(Players);
        shuffledList.Shuffle();
        for (int i = 0; i < shuffledList.Count; i++)
        {
            Teams[i % teamsAmount].AddPlayer(shuffledList[i]);
        }
    }

}
