
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Instantiate single lobby itemUI prefabs for the lobbies found via the lobbymanager
/// </summary>
public class LobbyListUI : MonoBehaviour
{
    [SerializeField]
    private SingleLobbyItemUI singleLobbyItemUIPrefab;
    [SerializeField]
    private GameObject lobbyList;

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
        lobbyList.transform.DestroyAllChildObjects();

        List<Lobby> lobbies = await ServerManager.Singleton.GetLobbies();

        foreach (Lobby lobby in lobbies)
        {
            SingleLobbyItemUI lobbyItemInstance = Instantiate(singleLobbyItemUIPrefab, lobbyList.transform);
            lobbyItemInstance.SetupLobbyItem(lobby);
        }

        lobbiesFoundUI.text = $"{lobbies.Count} lobbies found:";
    }

}
