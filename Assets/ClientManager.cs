using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{

    //Client variables
    private NetworkVariable<ulong> clientID = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public ulong ClientID { get { return clientID.Value; } }
    public NetworkVariable<FixedString128Bytes> playerName = new("Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public event System.Action<ClientManager> OnClientLeft;

    //Get the clientManager, that belongs to you, the client.
    private static ClientManager _myClient;
    public static ClientManager MyClient
    {
        get
        {
            if (_myClient == null) Debug.LogError("MyClient is null");
            return _myClient;
        }
    }
    
    private void Start()
    {
        LobbyManager.Singleton.AddClient(OwnerClientId, this);//Do this after networkspawn so the data is synchronised
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        if (IsOwner)
        {
            _myClient = this;
            clientID.Value = NetworkManager.Singleton.LocalClientId;
            playerName.Value = $"Player {ClientID} ";
        }
        gameObject.name = $"Client ({playerName.Value})";
    }

    public override void OnNetworkDespawn()
    {
        LobbyManager.Singleton.RemoveClient(OwnerClientId, this);
    }


    public void OnLeaving()
    {
        if (OnClientLeft != null) OnClientLeft.Invoke(this);
    }

}