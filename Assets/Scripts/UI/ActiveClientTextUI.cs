using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Display the active/connected amount of clients and auto-updates via lobby manager
/// </summary>
public class ActiveClientTextUI : MonoBehaviour
{
    private TextMeshProUGUI connectedClientsTextUI;

    private void Awake()
    {
        connectedClientsTextUI = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LobbyManager.OnNewClientJoined += UpdateActiveClients;
        LobbyManager.OnClientLeft += UpdateActiveClients;
        UpdateActiveClients();
    }
    private void OnDisable()
    {
        LobbyManager.OnNewClientJoined -= UpdateActiveClients;
        LobbyManager.OnClientLeft -= UpdateActiveClients;
    }



    /// <summary>
    /// Updates the text
    /// </summary>
    /// <param name="leavingClient"></param>
    private void UpdateActiveClients(ClientManager newclient = null)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            connectedClientsTextUI.text = string.Empty;
            return;
        }
        int connectedClients = LobbyManager.Singleton.Clients.Count;
        string newText = $"{connectedClients} client";
        newText += connectedClients > 1 ? "s connected" : " connected";
        connectedClientsTextUI.text = newText;
    }
}
