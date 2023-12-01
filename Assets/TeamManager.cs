using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Teammanager handles the input from the UITeamManagers and does the networking.
/// Networking inside the UI itself is not great because it would mean the whole canvas would have to be a networkobject.
/// </summary>
public class TeamManager : NetworkBehaviour
{

    [SerializeField, Tooltip("The UIManager for the team, inserted in the right order of team 1 to 4")]
    private List<UITeamManager> uiManagers;

    [SerializeField]
    private TextMeshProUGUI joinCodeTextUI;

    [SerializeField]
    private Button startButton;
    [SerializeField]
    private TextMeshProUGUI startButtonText;

    [SerializeField]
    private TeamColorsSO teamColors;

    private NetworkList<Team> teams;

    //Used for providing ID's for players, never goes down!
    private int uniquePlayerCount = 1;
    private NetworkList<Player> players;

    [SerializeField,Tooltip("NetworkedLists will be deleted after we leave this scene, therefore we store it in a SO")]
    private GameLobbySO gameLobby;


    private Dictionary<int, Player> playerIDDict = new();

    //It is not possible to have a list of players inside of a team because of networking limitations. Keep this dictionary locally on the server side.
    private Dictionary<int, List<Player>> teamPlayers = new();

    //Server side only
    private List<ulong> gameLobbyLoadedOnClients = new();



    private void Awake()
    {
        teams = new();
        players = new();

        teams.OnListChanged += OnTeamsListChange;
        players.OnListChanged += OnPlayersListChange;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        for (int i = 0; i < uiManagers.Count; i++)
        {
            if (IsServer)
            {
                Team team = new Team(i, $"Team {i + 1}", teamColors.TeamColors[i]);
                teams.Add(team);
            }
            teamPlayers.Add(i, new());
            uiManagers[i].SetTeamRef(teams[i]);
        }
        //If the client is joined after the list changed, update the data on screen by calling the methods manually.
        DisplayTeamNames();
        DisplayPlayers();
        DisplayJoinCodes();
       
    }


    /// <summary>
    /// Retrieves and displays the joincode and lobbycode on the server side, if set.
    /// </summary>
    public void DisplayJoinCodes()
    {
        string joinCodeTextStr = string.Empty;
        if (!string.IsNullOrEmpty(ServerManager.JoinCode))
        {
            joinCodeTextStr = $"Joincode: {ServerManager.JoinCode} (share)";
            if (!string.IsNullOrEmpty(ServerManager.LobbyCode))
            {
                joinCodeTextStr += $"\nLobbycode: {ServerManager.LobbyCode}";
            }
        }
        joinCodeTextUI.text = joinCodeTextStr;
    }



    [ServerRpc(RequireOwnership = false)]
    public void SetTeamNameServerRpc(int teamID, string newTeamName)
    {
        Team oldTeam = teams[teamID];
        //Add team as new as editing networkedlist variables don't sync over the network.
        teams.RemoveAt(teamID);
        Team updatedTeam = new Team(teamID, newTeamName, oldTeam.TeamColor);
        teams.Insert(teamID, updatedTeam);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(string playerName, ulong clientID, int teamID)
    {
        Player newPlayer = new Player(uniquePlayerCount, playerName, clientID, teamID);
        playerIDDict.Add(uniquePlayerCount, newPlayer);
        teamPlayers[teamID].Add(newPlayer);
        players.Add(newPlayer);
        uniquePlayerCount++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerServerRpc(int playerID)
    {
        Player playerToRemove = playerIDDict[playerID];
        teamPlayers[playerToRemove.TeamID].Remove(playerToRemove);
        playerIDDict.Remove(playerID);
        players.Remove(playerToRemove);
    }


    private void OnTeamsListChange(NetworkListEvent<Team> changeEvent)
    {
        DisplayTeamNames();
    }
    private void DisplayTeamNames()
    {
        foreach (Team team in teams)
        {
            uiManagers[team.TeamID].DisplayNewTeamName(team.TeamName.ToString());
        }
    }

    private void OnPlayersListChange(NetworkListEvent<Player> changeEvent)
    {
        DisplayPlayers();
        if (IsServer) ValidateTeams();
    }

    private void DisplayPlayers()
    {
        foreach (UITeamManager uiTeamManager in uiManagers)
        {
            uiTeamManager.DestroyPlayerList();
        }
        foreach (Player player in players)
        {
            uiManagers[player.TeamID].AddPlayerUICard(player);
        }
    }


    /// <summary>
    /// Validates the teams, there must be atleast two teams with each one player to start.
    /// </summary>
    private void ValidateTeams()
    {
        int validTeams = 0;
        foreach(List<Player> playersInTeam in teamPlayers.Values)
        {
            if (playersInTeam.Count > 0) validTeams++;
        }

        bool isReady = validTeams > 1;

        startButtonText.text = isReady ? "Start game!" : "Atleast two teams with one player is required before starting the game!";
        startButton.interactable = isReady;
    }

    public void StartGameButton()
    {
        gameLobbyLoadedOnClients.Clear();
        SaveToSOClientRpc();
    }


    /// <summary>
    /// Inform all clients to store the networklists in the gameLobbySO
    /// Will send a message back to the server once it finished
    /// </summary>
    [ClientRpc]
    private void SaveToSOClientRpc()
    {
        gameLobby.LoadData(players,teams);
        GameReadyServerRpc();
    }


    /// <summary>
    /// As a client, let the server know that the SO is loaded
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void GameReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        gameLobbyLoadedOnClients.Add(rpcParams.Receive.SenderClientId);
        //If every client loaded the SO, load the game scene.
        if (gameLobbyLoadedOnClients.Count == LobbyManager.Singleton.Clients.Count) 
            NetworkManager.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
    }

    /// <summary>
    /// Called by button.
    /// </summary>
    public void BackToMain()
    {
        
        SceneChangeManager.Singleton.ReturnToMain();
    }

}
