using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct Team : INetworkSerializable, System.IEquatable<Team>
{
    [SerializeField]
    private int teamID;
    public int TeamID => teamID;

    [SerializeField]
    private FixedString128Bytes teamName;
    public FixedString128Bytes TeamName {  get { return teamName; } set { teamName = value; } }


    [SerializeField]
    private Color teamColor;
    public Color TeamColor { get { return teamColor; } }

    public Team(int _teamID, FixedString128Bytes _teamName, Color _teamColor)
    {
        teamID = _teamID;
        teamName = _teamName;
        teamColor = _teamColor;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref teamID);
        serializer.SerializeValue(ref teamName);
        serializer.SerializeValue(ref teamColor);
    }
    public bool Equals(Team other)
    {
        return teamID == other.teamID;
    }

}
