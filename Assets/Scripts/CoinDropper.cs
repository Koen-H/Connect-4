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


    [SerializeField]
    private GameBoard gameBoard;

    private NetworkVariable<int> selectedRow = new(0,default,NetworkVariableWritePermission.Server);

    public UnityEvent OnCoinDropped = new();

    [SerializeField, Tooltip("The row collider layer")]
    private LayerMask rowColliderLayer;

    private Vector3[] coinDropPositions;


    // Start is called before the first frame update
    void Start()
    {
        //Whenever the gameobard is generated, retrieve the new drop positions for each row.
        gameBoard.OnGameBoardGenerated.AddListener(UpdateDropPositions);
        selectedRow.OnValueChanged += UpdateCoinPosition;
    }

    /// <summary>
    /// Retrieves the updated coinDropPositions from the gameboard
    /// </summary>
    private void UpdateDropPositions()
    {
        coinDropPositions = gameBoard.CoinDropPositions;
    }


    void Update()
    {
        //Ownership is granted to the client who's turn it is
        if (!IsOwner) return;
        FindRow();
        if (Input.GetMouseButtonDown(0)) TryDropCoin();
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


    public void CreateCoin(Team currentTeam)
    {
        currentCoin = Instantiate(coinPrefab);
        currentCoin.SetTeam(currentTeam);
        Vector3 targetPos = coinDropPositions[selectedRow.Value];
        currentCoin.transform.position = targetPos;
        currentCoin.MoveTo(targetPos);
    }


    private void UpdateCoinPosition(int oldRow, int newRow)
    {
        currentCoin.MoveTo(coinDropPositions[newRow]);
    }


    /// <summary>
    /// Try to drop the coin if possible
    /// </summary>
    void TryDropCoin()
    {
        //Check locally if the coin can be dropped
        if (gameBoard.TryInsertCoin(currentCoin, selectedRow.Value))
        {
            OnCoinDropped.Invoke();
        }
    }


}
