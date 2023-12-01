using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Custom client manager, allowing for easy acces of client specific values
/// </summary>
public class ClientManager : NetworkBehaviour
{
    //Client variables
    private NetworkVariable<ulong> clientID = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public ulong ClientID { get { return clientID.Value; } }

    public event System.Action<ClientManager> OnClientLeft;

    //Get the clientManager, that belongs to you, the client.
    private static ClientManager myClient;
    public static ClientManager MyClient
    {
        get
        {
            if (myClient == null) Debug.LogError("MyClient is null");
            return myClient;
        }
    }
    
    private void Start()
    {
        LobbyManager.Singleton.AddClient(OwnerClientId, this);
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        if (IsOwner)
        {
            myClient = this;
            clientID.Value = NetworkManager.Singleton.LocalClientId;
        }
    }

    public override void OnNetworkDespawn()
    {
        LobbyManager.Singleton.RemoveClient(OwnerClientId, this);
    }

    public void OnLeaving()
    {
        OnClientLeft?.Invoke(this);
    }

}