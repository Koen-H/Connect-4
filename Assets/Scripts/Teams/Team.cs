using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class Team : INetworkSerializable
{
    [SerializeField]
    private int teamID;
    public int TeamID { get { return teamID; } }

    [SerializeField]
    private string teamName = "Team";
    [SerializeField]
    private Color teamColor = Color.black;
    public Color TeamColor { get { return teamColor; } }


    private NetworkVariable<int> teamTurn = new NetworkVariable<int>();

    [SerializeField, Tooltip("The players in the team")]
    private List<Player> teamPlayers = new List<Player>();
    [Tooltip("The players in random order")]
    private List<Player> shuffledPlayers;


    public Team(int teamID, string _teamName, Color _teamColor)
    {

    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref teamID);
        serializer.SerializeValue(ref teamName);
        serializer.SerializeValue(ref teamColor);
    }

    public Player GetCurrentPlayer()
    {

        return teamPlayers[teamTurn.Value % teamPlayers.Count];
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
