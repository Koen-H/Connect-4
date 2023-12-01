using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerItem : MonoBehaviour
{
    [SerializeField]
    private Button removePlayerButton;

    [SerializeField]
    private TextMeshProUGUI playerNameUI;

    private TeamManager teamManager;
    private Player player;

    /// <summary>
    /// Adds a playerRemove listener for when the button is pressed
    /// </summary>
    /// <param name="teamManager"></param>
    public void SetUp(TeamManager _teamManager, Player _player)
    {
        teamManager = _teamManager;
        player = _player;
        playerNameUI.text = player.PlayerName;
        removePlayerButton.onClick.AddListener(RequestPlayerRemoveal);
    }

    private void RequestPlayerRemoveal()
    {
        teamManager.RemovePlayerServerRpc(player.PlayerID);//NOTE: We only need to send the id over, not the whole player.
    }

    private void OnDisable()
    {
        removePlayerButton.onClick.RemoveListener(RequestPlayerRemoveal);
    }

}
