using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct Player : INetworkSerializable, System.IEquatable<Player>
{
    public int PlayerID => playerID;
    private int playerID;
    public string PlayerName => playerName.ToString();
    private FixedString128Bytes playerName;
    public ulong ClientID => clientID;
    private ulong clientID;
    public int TeamID => teamID;
    private int teamID; 

    public Player(int _playerID, FixedString128Bytes _playerName, ulong _clientID, int _teamID)
    {
        playerID = _playerID;
        playerName = _playerName;
        clientID = _clientID;
        teamID = _teamID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref teamID);
        serializer.SerializeValue(ref clientID);
    }

    public bool Equals(Player other)
    {
        return playerID == other.playerID;
    }
}

