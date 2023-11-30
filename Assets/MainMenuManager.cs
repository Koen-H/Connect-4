using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple main menu manager
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    
    /// <summary>
    /// Playing locally will still create a host instance, but will not be shown public
    /// </summary>
    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }



}
