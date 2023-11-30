using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu()]
public class GameLobbySO : ScriptableObject
{

    [Tooltip("List of all the teams in the game")]
    private List<Team> teams = new();
    public List<Team> Teams => teams;

    //Used for getting a team by it's unique ID
    private Dictionary<int, Team> teamsDict = new();

    private Dictionary<int, Player> playerIDDict = new();

    //It is not possible to have a list of players inside of a team because of networking limitations(?). Keep this dictionary locally...
    private Dictionary<int, List<Player>> teamPlayers = new();



    /// <summary>
    /// Loops through the player and teams networklists and stores it as normal lists, alongside dictionaries to more easily retrieve players again.
    /// </summary>
    /// <param name="newPlayers"></param>
    /// <param name="newTeams"></param>
    public void LoadData(NetworkList<Player> newPlayers, NetworkList<Team> newTeams)
    {
        teamsDict.Clear();
        playerIDDict.Clear();
        teamPlayers.Clear();

        foreach (Team team in newTeams)
        {
            teamsDict.Add(team.TeamID, team);
            teamPlayers.Add(team.TeamID, new());
        }

        foreach (Player player in newPlayers)
        {
            teamPlayers[player.TeamID].Add(player);
        }
        //Remove teams that are empty.
        List<Team> emptyTeams = new();
        foreach (KeyValuePair<int,List<Player>> playersInTeam in teamPlayers)
        {
            if (playersInTeam.Value.Count < 1) emptyTeams.Add(teamsDict[playersInTeam.Key]);
        }
        foreach (Team emptyTeam in emptyTeams)
        {
            teamsDict.Remove(emptyTeam.TeamID);
            teamPlayers.Remove(emptyTeam.TeamID);
        }
        //Create a list of the finalized dictionary
        teams = teamsDict.Values.ToList<Team>();
    }


    /// <summary>
    /// Get a team by teamID
    /// </summary>
    /// <param name="requestedTeamID">the teamID</param>
    /// <returns></returns>
    public Team GetTeamByID(int requestedTeamID)
    {
        return teamsDict[requestedTeamID];
    }

    public Player GetCurrentPlayer(Team team, int teamTurn)
    {
        int teamID = team.TeamID;
        return teamPlayers[teamID][teamTurn % teamPlayers[teamID].Count];
    }

}
