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


    public void JoinServerViaUserJoincode()
    {
        joiningSelectUI.SetActive(false);
        connectingUI.SetActive(true);

        string joinCode = joincodeInputField.text;
        try
        {
            ServerManager.Singleton.SetupRelayConnectionViaRelayJoincode(joinCode);
        }
        catch(Exception e) { Debug.LogError(e); }
    }


    public void OnClientDisconnected(ulong clientID)
    {
        StopJoin();
    }

    /// <summary>
    /// Cancel the joining attempt by shutting down the networkManager and toggling the UI
    /// </summary>
    public void StopJoin()
    {
        NetworkManager.Singleton.Shutdown();
        joiningSelectUI.SetActive(true);
        connectingUI.SetActive(false);
    }

}
