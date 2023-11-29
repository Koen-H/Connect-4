using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class Player : INetworkSerializable
{
    public string playerName = "Player";
    public int playerId;
    private int teamID;

    public ulong ClientID;

    public int TeamID {  get { return teamID; } }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref teamID);
    }

    public void SetTeam(Team newTeam)
    {
        teamID = newTeam.TeamID;
    }

}
