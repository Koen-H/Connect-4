
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Load servers found through steam
/// </summary>
public class ServersManager : MonoBehaviour
{
    [SerializeField] private ServerItem serverItemPrefab;
    [SerializeField] private GameObject serverList;

    private void OnEnable()
    {
        LoadServers();
    }

    public void LoadServers()
    {
        for (int i = serverList.transform.childCount - 1; i >= 0; i--)
        {
            GameObject childObject = serverList.transform.GetChild(i).gameObject;
            Destroy(childObject);
        }

        LoadLobbies();
    }


    private async void LoadLobbies()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;


            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created
                    )
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach (Lobby lobby in lobbies.Results)
            {
                ServerItem serverItem = Instantiate(serverItemPrefab, serverList.transform);
                serverItem.SetupServerItem(lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}
