using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Coin dropper handles player input by 
/// 
/// The ownership of this object is moved between clients, allowing only the owner to change the position of the coin.
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

    [SerializeField]
    private GameManager gameManager;


    // Start is called before the first frame update
    void Awake()
    {
        //Whenever the gameobard is generated, retrieve the new drop positions for each row.
        gameBoard.OnCoinDropPositionsGenerated += UpdateDropPositions;
        selectedRow.OnValueChanged += UpdateCoinPosition;
        gameManager.OnGameStateChange += OnGameStateChange;
    }

    private void OnGameStateChange(GameManager.GameState oldState, GameManager.GameState newState)
    {
        playerInputEnabled = newState == GameManager.GameState.Playing;
    }


    /// <summary>
    /// Retrieves the updated coinDropPositions from the gameboard
    /// </summary>
    private void UpdateDropPositions(Vector3[] newCoinDropPositions)
    {
        coinDropPositions = newCoinDropPositions;
    }


    void Update()
    {
        //Ownership is granted to the client who's turn it is, if there is no ownership the client is not allowed to change values off the current coin.
        if (!IsOwner) return;
        if (!playerInputEnabled) return;
        if (currentCoin == null) return;
        FindRow();
        if (Input.GetMouseButtonDown(0)) TryDropCoinServerRpc(selectedRow.Value);//We know this value is correct on the client that sends it
    }


    /// <summary>
    /// Find the row the player is currently hovering over
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

    private void UpdateCoinPosition(int oldRow, int newRow)
    {
        currentCoin.MoveTo(coinDropPositions[newRow]);
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


}
