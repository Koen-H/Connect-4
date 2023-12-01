using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class ServerConnectUI : MonoBehaviour
{
    [SerializeField, Tooltip("The parent gameobject UI that has all the options to join/host")]
    private GameObject joiningSelectUI;
    [SerializeField, Tooltip("The parent gameobject UI  showcasing that it's loading the game right now.")] 
    private GameObject connectingUI;


    [SerializeField]
    private TMP_InputField joincodeInputField;

    /// <summary>
    /// When the join button is submitted, retrieve the inserted value and try to join with the joincode.
    /// </summary>
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
    /// Cancel the joining attempt by shutting down the networkManager and toggling the UI back to the overview
    /// </summary>
    public void StopJoin()
    {
        NetworkManager.Singleton.Shutdown();
        joiningSelectUI.SetActive(true);
        connectingUI.SetActive(false);
    }

}
