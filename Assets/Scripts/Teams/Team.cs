using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    [SerializeField]
    private string teamName = "Team";
    [SerializeField]
    private Color teamColor = Color.black;
    public Color TeamColor { get { return teamColor; } }

    [SerializeField, Tooltip("The players in the team")]
    private List<Player> teamPlayers = new List<Player>();

    public Team(string _teamName, Color _teamColor)
    {

    }

    public void AddPlayer(Player newPlayer)
    {
        teamPlayers.Add(newPlayer);
    }

    public void RemovePlayer(Player newPlayer)
    {
        teamPlayers.Remove(newPlayer);
    }

}
