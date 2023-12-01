
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Load servers found through unity's lobby
/// </summary>
public class ServersManager : MonoBehaviour
{
    [SerializeField]
    private SingleLobbyItemUI singleLobbyItemUIPrefab;
    [SerializeField]
    private GameObject serverList;

    [SerializeField]
    private TextMeshProUGUI lobbiesFoundUI;

    private void Awake()
    {
        LoadLobbies();
    }


    /// <summary>
    /// Request the lobbies and display them in the lobbyList
    /// </summary>
    public async void LoadLobbies()
    {
        lobbiesFoundUI.text = "Loading lobbies...";
        serverList.transform.DestroyAllChildObjects();

        List<Lobby> lobbies = await ServerManager.Singleton.GetLobbies();

        foreach (Lobby lobby in lobbies)
        {
            SingleLobbyItemUI lobbyItemInstance = Instantiate(singleLobbyItemUIPrefab, serverList.transform);
            lobbyItemInstance.SetupLobbyItem(lobby);
        }

        lobbiesFoundUI.text = $"{lobbies.Count} lobbies found:";
    }

}
