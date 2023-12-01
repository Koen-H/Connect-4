using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ServerConnectUI : MonoBehaviour
{
    [SerializeField, Tooltip("The parent gameobject UI that has all the options to join/host")]
    private GameObject joiningSelectUI;
    [SerializeField, Tooltip("The parent gameobject UI  showcasing that it's loading the game right now.")] 
    private GameObject connectingUI;


    [SerializeField]
    private TMP_InputField joincodeInputField;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped += UnSubscribe;
    }

    private void OnDisable()
    {
        UnSubscribe();
    }

    private void UnSubscribe(bool b = false)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped -= UnSubscribe;
    }

    public void JoinServerViaUserJoincode()
    {
        joiningSelectUI.SetActive(false);
        connectingUI.SetActive(true);

        string joinCode = joincodeInputField.text;
        JoinServerViaJoincode(joinCode);
    }

    /// <summary>
    /// Automatically join a server
    /// </summary>
    public async void QuickJoin()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
        JoinServerViaJoincode(lobby.Data["JOIN_CODE"].Value);
    }


    private async void JoinServerViaJoincode(string joinCode)
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }

    public void StopJoin()
    {
        NetworkManager.Singleton.Shutdown();
        joiningSelectUI.SetActive(true);
        connectingUI.SetActive(false);
    }

    public void OnClientDisconnected(ulong clientID)
    {
        StopJoin();
    }

}
