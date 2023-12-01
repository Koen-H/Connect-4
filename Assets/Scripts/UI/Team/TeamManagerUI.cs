using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI interface to add/remove players to a team and change the teamname.
/// </summary>
public class UITeamManager : MonoBehaviour
{

    [Header("Team")]
    [SerializeField]
    private TeamManager teamManager;
    [SerializeField]
    private TMP_InputField teamName;
    [Tooltip("The team that this UI is managing/displaying")]
    private int teamID;

    [SerializeField, Tooltip("UI Background")]
    private Image background;

    [Header("Player")]
    [SerializeField]
    private TMP_InputField playerName;
    [SerializeField]
    private Transform playerList;

    [SerializeField]
    private UIPlayerItem UIPlayerItemPrefab;



    public void SetTeamRef(Team teamRef)
    {
        background.color = teamRef.TeamColor;
        teamID = teamRef.TeamID;
    }


    public void SetTeamName()
    {
        string newTeamName = teamName.text;
        teamManager.SetTeamNameServerRpc(teamID, newTeamName);
    }

    public void DisplayNewTeamName(string newTeamName)
    {
        teamName.text = newTeamName;
    }

    public void AddPlayer()
    {
        string newPlayerName = playerName.text;
        ulong clientID = NetworkManager.Singleton.LocalClientId;

        teamManager.AddPlayerServerRpc(newPlayerName,clientID, teamID);
    }

    public void DestroyPlayerList()
    {
        playerList.DestroyAllChildObjects();
    }

    public void AddPlayerUICard(Player player)
    {
        UIPlayerItem instance = Instantiate(UIPlayerItemPrefab, playerList);
        instance.SetUp(teamManager, player);
    }


    /// <summary>
    /// Try to auto set the TeamMaanger when prefab is placed in scene.
    /// </summary>
    private void OnValidate()
    {
        TeamManager potentialTeamManager = FindObjectOfType<TeamManager>();
        if(potentialTeamManager != null) { teamManager = potentialTeamManager; }
    }

}
