using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Coin dropper handles player input
/// 
/// The ownership of this object is moved between clients, allowing only the owner to write the selectedRow variable.
/// </summary>
public class CoinDropper : NetworkBehaviour
{
    private Coin currentCoin;

    [SerializeField]
    private Coin coinPrefab;

    [SerializeField, Tooltip("Reference to the gameLobbyData")]
    private GameLobbySO gameLobbyData;

    public event Action OnCoinDropped;

    [SerializeField]
    private GameBoard gameBoard;

    private NetworkVariable<int> selectedRow = new(0, default, NetworkVariableWritePermission.Owner);


    [SerializeField, Tooltip("The row collider layer")]
    private LayerMask rowColliderLayer;

    private Vector3[] coinDropPositions;

    private bool playerInputEnabled = false;
    private Coroutine randomDropCoroutine;

    [SerializeField]
    private GameManager gameManager;


    void Awake()
    {
        //Whenever the gameboard is generated, retrieve the new drop positions for each row.
        gameBoard.OnCoinDropPositionsGenerated += UpdateDropPositions;
        selectedRow.OnValueChanged += UpdateCoinPosition;
        gameManager.OnGameStateChange += OnGameStateChange;
    }

    void Update()
    {
        //Ownership is granted to the client who's turn it is, if there is no ownership the client is not allowed to change values off the current coin.
        if (!IsOwner) return;
        if (!playerInputEnabled) return;
        //if (currentCoin == null) return;//Because of latency, it can happen that a coin is temporarily null. (Should be fixed, keep commented as reminder)
        FindRow();
        if (Input.GetMouseButtonDown(0)) TryDropCoinServerRpc(selectedRow.Value);//We know this value is correct on the client that sends it
    }

    private void OnDisable()
    {
        gameBoard.OnCoinDropPositionsGenerated -= UpdateDropPositions;
        selectedRow.OnValueChanged -= UpdateCoinPosition;
        gameManager.OnGameStateChange -= OnGameStateChange;
    }

    /// <summary>
    /// When the gamestate changes, disable the input all together based on if the state is playing or not.
    /// </summary>
    private void OnGameStateChange(GameManager.GameState oldState, GameManager.GameState newState)
    {
        playerInputEnabled = newState == GameManager.GameState.Playing;
        if (!playerInputEnabled)
        {
            Debug.Log("Input stopped");
            if(randomDropCoroutine != null) StopCoroutine(randomDropCoroutine);
        }
    }


    /// <summary>
    /// Retrieves the updated coinDropPositions from the gameboard
    /// </summary>
    private void UpdateDropPositions(Vector3[] newCoinDropPositions)
    {
        coinDropPositions = newCoinDropPositions;
    }


    /// <summary>
    /// Tell the client to create a coin for the provided team
    /// </summary>
    /// <param name="teamID">The teamID of the team that's currently playing</param>
    [ClientRpc]
    public void CreateCoinClientRpc(int teamID)
    {
        Team team = gameLobbyData.GetTeamByID(teamID);
        currentCoin = Instantiate(coinPrefab);
        currentCoin.SetTeam(team);

        Vector3 targetPos = coinDropPositions[selectedRow.Value];
        currentCoin.transform.position = targetPos;
        currentCoin.MoveTo(targetPos);
    }
    #region Row selecting
    /// <summary>
    /// Find the row the player is currently hovering over and selects it.
    /// </summary>
    private void FindRow()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, rowColliderLayer))
        {
            if (hitInfo.collider.gameObject.TryGetComponent<RowCollider>(out RowCollider rowCol))
            {
                selectedRow.Value = rowCol.Row;
            }
        }
    }



    /// <summary>
    /// Whenever the networkvarialbe selected row changes, move the coin locally on each client.
    /// </summary>
    /// <param name="oldRow"></param>
    /// <param name="newRow"></param>
    private void UpdateCoinPosition(int oldRow, int newRow)
    {
        currentCoin.MoveTo(coinDropPositions[newRow]);
    }
    #endregion

    #region CoinDropping

    public void StartRandomDrop()
    {
        randomDropCoroutine = StartCoroutine(DropRandomRow());
    }

    /// <summary>
    /// Drops a random row.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DropRandomRow()
    {
        if (!playerInputEnabled) yield break;
        int totalSecondsWait = 3;
        float changeInterval = 0.5f;
        float timeWaited = 0;

        (bool, int) randomRow = gameBoard.GetRandomValidRow();

        // Check if there are valid rows
        if (!randomRow.Item1) yield break;
        playerInputEnabled = false;

        // Make it look like the enemy player is debating
        while (timeWaited < totalSecondsWait)
        {
            timeWaited += changeInterval;
            randomRow = gameBoard.GetRandomValidRow();
            selectedRow.Value = randomRow.Item2;
            yield return new WaitForSeconds(changeInterval);
        }

        playerInputEnabled = true;
        TryDropCoinServerRpc(randomRow.Item2);
    }



    /// <summary>
    /// Tell the server to drop coin, will check if possible
    /// </summary>
    /// <param name="dropInRow">The row selected</param>
    [ServerRpc]
    private void TryDropCoinServerRpc(int dropInRow)
    {
        //NOTE: Send row as rpc param incase of latency in the networkvariable SelectedRow.
        if (gameBoard.CanDrop(dropInRow))
        {
            //Inform all clients to drop in this row, this includes the server as the server is a host (server and client)
            InsertCoinClientRpc(dropInRow);
            OnCoinDropped?.Invoke();
        }
    }

    /// <summary>
    /// Inform clients to drop the coin in the selected row.
    /// </summary>
    [ClientRpc]
    private void InsertCoinClientRpc(int dropInRow)
    {
        gameBoard.InsertCoin(currentCoin, dropInRow);
    }
    #endregion



}
