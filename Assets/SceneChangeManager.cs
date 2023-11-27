using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Custom scene managaer which handles changes with networking.
/// </summary>
public class SceneChangeManager : NetworkBehaviour
{

    public System.Action<SceneEvent> OnLoadCompleteServerSide;
    public System.Action<SceneEvent> OnLoadCompleteClientSide;

    public System.Action<SceneEvent> OnUnLoadCompleteServerSide;
    public System.Action<SceneEvent> OnUnLoadCompleteClientSide;

    /// <summary>
    /// When all clients finished loading the scene.
    /// </summary>
    public event System.Action<SceneEvent> OnAllLoadCompleteServerSide;
    public event System.Action<SceneEvent> OnAllLoadCompleteClientSide;

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
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
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