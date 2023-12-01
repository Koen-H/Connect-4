using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Custom Network Scene Managaer which handles changes with networking.
/// </summary>
public class SceneChangeManager : NetworkBehaviour
{
    /// <summary>
    /// When a client finsihed loading, called on the server
    /// </summary>
    public Action<SceneEvent> OnLoadCompleteServerSide;
    /// <summary>
    /// When a client finsihed loading, called locally on the client
    /// </summary>
    public Action<SceneEvent> OnLoadCompleteClientSide;

    /// <summary>
    /// When a client finished unloading, called on the server
    /// </summary>
    public Action<SceneEvent> OnUnLoadCompleteServerSide;
    /// <summary>
    /// When a client finished unloading, called locally on the client
    /// </summary>
    public Action<SceneEvent> OnUnLoadCompleteClientSide;

    /// <summary>
    /// When all clients finished loading the scene, called only on the server
    /// </summary>
    public event Action<SceneEvent> OnAllLoadCompleteServerSide;
    /// <summary>
    /// When all clients finished loading the scene, called locally on the client
    /// </summary>
    public event Action<SceneEvent> OnAllLoadCompleteClientSide;

    private static SceneChangeManager instance;
    public static SceneChangeManager Singleton
    {
        get
        {
            if (instance == null) Debug.LogError("SceneChangeManager is null!");
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            //When in the main menu, become the new one
            if(SceneManager.GetActiveScene().buildIndex == 0)
            {
                Destroy(instance.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        Debug.Log("Network spawn!");
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnect;
    }


    /// <summary>
    /// When the player disconnects, load back to the main menu.
    /// </summary>
    /// <param name="clientDisconnect"></param>
    private void OnDisconnect(ulong clientDisconnect)
    {
       if(clientDisconnect == NetworkManager.Singleton.LocalClientId) ReturnToMain();
    }

    public void LoadLobby()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    /// <summary>
    /// Shut down the network connection, clean up the networkManager and load the main scene.
    /// </summary>
    public void ReturnToMain()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);//Destroy this gameobject as it's already in the mainScene
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    /// <summary>
    /// All potential scene events
    /// </summary>
    /// <param name="sceneEvent"></param>
    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        // Both client and server receive these notifications
        switch (sceneEvent.SceneEventType)
        {
            // Handle server to client Load Notifications
            case SceneEventType.Load:
                {
                    // This event provides you with the associated AsyncOperation
                    // AsyncOperation.progress can be used to determine scene loading progression
                    var asyncOperation = sceneEvent.AsyncOperation;
                    // Since the server "initiates" the event we can simply just check if we are the server here
                    if (IsServer)
                    {
                        // Handle server side load event related tasks here
                    }
                    else
                    {
                        // Handle client side load event related tasks here
                    }
                    break;
                }
            // Handle server to client unload notifications
            case SceneEventType.Unload:
                {
                    // You can use the same pattern above under SceneEventType.Load here
                    break;
                }
            // Handle client to server LoadComplete notifications
            case SceneEventType.LoadComplete:
                {
                    // This will let you know when a load is completed
                    // Server Side: receives thisn'tification for both itself and all clients
                    if (IsServer)
                    {
                        if (OnLoadCompleteServerSide != null) OnLoadCompleteServerSide.Invoke(sceneEvent);
                    }
                    else // Clients generate thisn'tification locally
                    {
                        // Handle client side LoadComplete related tasks here
                        if (OnLoadCompleteClientSide != null) OnLoadCompleteClientSide.Invoke(sceneEvent);
                    }

                    // So you can use sceneEvent.ClientId to also track when clients are finished loading a scene
                    break;
                }
            // Handle Client to Server Unload Complete Notification(s)
            case SceneEventType.UnloadComplete:
                {
                    // This will let you know when an unload is completed
                    // You can follow the same pattern above as SceneEventType.LoadComplete here
                    if (IsServer)
                    {
                        if (OnUnLoadCompleteServerSide != null) OnUnLoadCompleteServerSide.Invoke(sceneEvent);

                    }
                    else // Clients generate thisn'tification locally
                    {
                        // Handle client side LoadComplete related tasks here
                        if (OnUnLoadCompleteClientSide != null) OnUnLoadCompleteClientSide.Invoke(sceneEvent);
                    }

                    // So you can use sceneEvent.ClientId to also track when clients are finished unloading a scene
                    break;
                }
            // Handle Server to Client Load Complete (all clients finished loading notification)
            case SceneEventType.LoadEventCompleted:
                {
                    // This will let you know when all clients have finished loading a scene
                    // Received on both server and clients
                    foreach (var clientId in sceneEvent.ClientsThatCompleted)
                    {
                        // Example of parsing through the clients that completed list
                        if (IsServer && NetworkManager.LocalClientId == clientId)
                        {
                            if (OnAllLoadCompleteServerSide != null) OnAllLoadCompleteServerSide.Invoke(sceneEvent);
                        }
                        else
                        {
                            if (OnAllLoadCompleteClientSide != null) OnAllLoadCompleteClientSide.Invoke(sceneEvent);
                            // Handle any client-side tasks here
                        }
                    }
                    break;
                }
            // Handle Server to Client unload Complete (all clients finished unloading notification)
            case SceneEventType.UnloadEventCompleted:
                {
                    // This will let you know when all clients have finished unloading a scene
                    // Received on both server and clients
                    foreach (var clientId in sceneEvent.ClientsThatCompleted)
                    {
                        // Example of parsing through the clients that completed list
                        if (IsServer)
                        {
                            // Handle any server-side tasks here
                        }
                        else
                        {
                            // Handle any client-side tasks here
                        }
                    }
                    break;
                }
        }
    }
}