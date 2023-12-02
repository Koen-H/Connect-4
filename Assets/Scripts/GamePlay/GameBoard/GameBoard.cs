using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI.Table;

public class GameBoard : NetworkBehaviour
{
    [Header("Board settings")]
    [SerializeField, Tooltip("The width of the gameboard")]
    private int boardWidth = 7;

    [SerializeField, Tooltip("The height of the gameboard")]
    private int boardHeight = 6;

    [SerializeField, Tooltip("How many connects are required to get a win?")]
    private int connectWinCondition = 4;

    [Header("Board prefabs")]
    [SerializeField, Tooltip("Prefab of a singular boardslot")]
    private CoinSlot coinSlotPrefab;

    [SerializeField, Tooltip("Prefab of a singular rowColider used to get the row via raycast")]
    private RowCollider rowColiderPrefab;

    /// <summary>
    /// 2D map of the gameBoard
    /// </summary>
    private CoinSlot[,] coinSlotsGrid;

    private NetworkList<Vector2Int> winningCoinSlotsPositions;

    //Keep track how many coins there are in each row
    //NOTE: Networked list instead of array because networkedArray doesnt exist.
    private NetworkList<int> rowHeight;

    //Positions above the board where the coin will be visually displayed before dropping
    private Vector3[] coinDropPositions;

    private NetworkVariable<bool> isTied = new NetworkVariable<bool>();


    /// <summary>
    /// Called when the gameboard finished generating.
    /// </summary>
    public event Action OnGameBoardGenerated;
    public event Action<Vector3[]> OnCoinDropPositionsGenerated;
    public event Action<int> OnGameWin;
    public event Action OnGameTied;

    private void Awake()
    {
        rowHeight = new NetworkList<int>();
        winningCoinSlotsPositions = new NetworkList<Vector2Int>();
        winningCoinSlotsPositions.OnListChanged += OnWinningSlotsChanged;
        isTied.OnValueChanged += OnIsTiedValueChanged;
    }

    private void OnDisable()
    {
        winningCoinSlotsPositions.OnListChanged -= OnWinningSlotsChanged;
        isTied.OnValueChanged -= OnIsTiedValueChanged;
    }

    #region Gameboard generation

    /// <summary>
    /// Tell the clients to reset the board.
    /// </summary>
    [ClientRpc]
    public void ResetBoardClientRpc()
    {
        ResetBoard();
    }

    /// <summary>
    /// Emtpy the slots if filled and resets gameplay variables on server.
    /// </summary>
    public void ResetBoard()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                coinSlotsGrid[x, y]?.EmptySlot();
            }
        }
        if (IsServer)
        {
            winningCoinSlotsPositions.Clear();
            rowHeight.Clear();//Make row list empty again
            for (int i = 0; i < boardWidth; i++)
            {
                rowHeight.Add(0);
            }
        }
    }

    /// <summary>
    /// Inform all clients to generate the board, this includes the server as the server will always be a host.
    /// </summary>
    [ClientRpc]
    public void GenerateBoardClientRpc()
    {
        GenerateBoard();
    }


    /// <summary>
    /// Generates the gameboard using tiles of Coinslots prefab
    /// </summary>
    private void GenerateBoard()
    {
        Debug.Log("Generating board", this);
        //Destroy all children to destroy potential previous board.
        transform.DestroyAllChildObjects();
        coinSlotsGrid = new CoinSlot[boardWidth, boardHeight];
        ResetBoard();
        coinDropPositions = new Vector3[boardWidth];

        //Row colliders are placed in the center of the board
        int rowColHeight = (boardHeight) / 2;

        //Generate a board, starting with 0,0 in the bottom left
        for (int x = 0; x < boardWidth; x++)
        {

            for (int y = 0; y < boardHeight; y++)
            {
                CoinSlot newInstance = Instantiate(coinSlotPrefab, this.transform);
                newInstance.SlotPosition = new Vector2Int(x, y);
                newInstance.gameObject.name = $"CoinSlot {x},{y}";
                newInstance.transform.localPosition = new Vector3(x, y, 0);
                coinSlotsGrid[x, y] = newInstance;
            }
            //Create coin Drop position
            coinDropPositions[x] = coinSlotsGrid[x, boardHeight - 1].transform.position + new Vector3(0, 0.75f, 0);

            //Instantiate a row collider on the row
            RowCollider rowCol = Instantiate(rowColiderPrefab, this.transform);
            rowCol.gameObject.name = $"RowCol {x}";
            rowCol.transform.localPosition = new Vector3(x, rowColHeight, 0);
            rowCol.Row = x;
            rowCol.SetColliderHeight(boardHeight + 1);
        }
        OnCoinDropPositionsGenerated?.Invoke(coinDropPositions);
        OnGameBoardGenerated?.Invoke();
    }

    /// <summary>
    /// Creates an empty gameobject in the center of the board
    /// </summary>
    /// <returns>The transform placed in the center of the board</returns>
    public Transform GetCenter()
    {
        Transform centerTransform = new GameObject("Center of board").transform;
        Vector3 centerPosition = (coinSlotsGrid[boardWidth - 1, boardHeight - 1].transform.position + coinSlotsGrid[0, 0].transform.position) / 2;
        centerTransform.position = centerPosition;
        return centerTransform;
    }

    #endregion

    #region Win Condition
    /// <summary>
    /// Starting from bottom left, loop through each slot and check above, diagnoally and to the right of the slot
    /// </summary>
    private void CheckForWin()
    {
        List<CoinSlot> winningSlots = new();

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                //The slot we are checking diagonal, vertical and horizontally from
                CoinSlot currentCoinSlot = coinSlotsGrid[x, y];

                //If the slot doesn't have a coin, continue
                if (!currentCoinSlot.HasCoin) continue;

                //We don't need to check if there aren't enough slots to the side, also prevents out of bounds
                //NOTE: -1 as the current position is already considered as one
                bool checkX = x + (connectWinCondition - 1) < boardWidth;
                bool checkY = y + (connectWinCondition - 1) < boardHeight;
                bool checkReverseX = x - (connectWinCondition - 1) >= 0;


                //Check slots above
                if (checkY)
                {
                    List<CoinSlot> topSlots = new List<CoinSlot> { currentCoinSlot };
                    for (int t = 1; t < connectWinCondition; t++)
                    {
                        CoinSlot slotToCheck = coinSlotsGrid[x, y + t];
                        //Stop checking this direction if it doesn't have a coin or the teamID don't match
                        if (!currentCoinSlot.IsCoinMatch(slotToCheck)) break;
                        topSlots.Add(slotToCheck);
                    }
                    if (topSlots.Count == connectWinCondition) winningSlots.AddRange(topSlots);
                }
                //Check slots right
                if (checkX)
                {
                    List<CoinSlot> rightSlots = new List<CoinSlot> { currentCoinSlot };
                    for (int r = 1; r < connectWinCondition; r++)
                    {
                        CoinSlot slotToCheck = coinSlotsGrid[x + r, y];
                        //Stop checking this direction if it doesn't have a coin or the teamID don't match
                        if (!currentCoinSlot.IsCoinMatch(slotToCheck)) break;
                        rightSlots.Add(slotToCheck);
                    }
                    if (rightSlots.Count == connectWinCondition) winningSlots.AddRange(rightSlots);
                }
                //Check slots diagonal to right
                if (checkX && checkY)
                {
                    List<CoinSlot> diagonalRightSlots = new List<CoinSlot> { currentCoinSlot };
                    for (int dr = 1; dr < connectWinCondition; dr++)
                    {
                        CoinSlot slotToCheck = coinSlotsGrid[x + dr, y + dr];
                        //Stop checking this direction if it doesn't have a coin or the teamID don't match
                        if (!currentCoinSlot.IsCoinMatch(slotToCheck)) break;
                        diagonalRightSlots.Add(slotToCheck);
                    }
                    if (diagonalRightSlots.Count == connectWinCondition) winningSlots.AddRange(diagonalRightSlots);
                }
                //Check slots diagonal to left
                if (checkReverseX && checkY)
                {
                    List<CoinSlot> diagonalLeftSlots = new List<CoinSlot> { currentCoinSlot };
                    for (int dl = 1; dl < connectWinCondition; dl++)
                    {
                        CoinSlot slotToCheck = coinSlotsGrid[x - dl, y + dl];
                        //Stop checking this direction if it doesn't have a coin or the teamID don't match
                        if (!currentCoinSlot.IsCoinMatch(slotToCheck)) break;
                        diagonalLeftSlots.Add(slotToCheck);
                    }
                    if (diagonalLeftSlots.Count == connectWinCondition) winningSlots.AddRange(diagonalLeftSlots);
                }
            }
        }
        //We got a winner if there are winning slots
        if (winningSlots.Count > 0)
        {
            winningSlots.Distinct().ToList();
            winningCoinSlotsPositions.Clear();

            //Turn the winning slots in to vector2Int positions that can be send over the network.
            foreach (CoinSlot slot in winningSlots)
            {
                winningCoinSlotsPositions.Add(slot.SlotPosition);
            }
        }
        else
        {
            //Check if it's a tie.
            isTied.Value = IsBoardFull();
        }
    }

    /// <summary>
    /// Checks the height in each row and if one isn't maxed it isn't full yet.
    /// </summary>
    /// <returns>Returns true if all the rows have reached the boardheight</returns>
    public bool IsBoardFull()
    {
        //return rowHeight.Any(height => height < boardHeight); Does not work on networklist...
        foreach (int height in rowHeight)
        {
            if (height < boardHeight)
            {
                return false;
            }
        }
        Debug.Log("BOARD FULL");
        return true;
    }


    private void OnWinningSlotsChanged(NetworkListEvent<Vector2Int> changeEvent)
    {
        //Invoke in networkListEvent to invoke the win on all clients
        if (winningCoinSlotsPositions.Count > 0)
        {
            OnGameWin?.Invoke(coinSlotsGrid[winningCoinSlotsPositions[0].x, winningCoinSlotsPositions[0].y].OwnerTeamID);
        }
        foreach (Vector2Int slotPosition in winningCoinSlotsPositions)
        {
            //Start the glow effect on the coin inside the slot
            coinSlotsGrid[slotPosition.x, slotPosition.y].MakeCoinGlow();
        }
    }

    private void OnIsTiedValueChanged(bool prevValue, bool newValue)
    {
        if(newValue) OnGameTied?.Invoke();
    }

    #endregion

    #region Coin dropping

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns a struct </returns>
    public (bool, int) GetRandomValidRow()
    {
        List<int> validRows = new();
        for (int i = 0; i < rowHeight.Count; i++) if (rowHeight[i] < boardHeight) validRows.Add(i);
        validRows.Shuffle();
        if (validRows.Count > 0) return (true, validRows[0]);
        return (false, validRows[0]);
    }

    /// <summary>
    /// Check if the coin can be dropped in this row, or if it's full already
    /// </summary>
    /// <param name="row">The row in question</param>
    /// <returns>returns true if it can be dropped</returns>
    public bool CanDrop(int row)
    {
        int currentHeight = rowHeight[row];
        return currentHeight != boardHeight;
    }

    /// <summary>
    /// Tries to insert the coin in the given row
    /// </summary>
    /// <param name="insertedCoin">Coin to insert</param>
    /// <param name="row">The row</param>
    /// <returns>Returns false if the row is already filled</returns>
    public bool InsertCoin(Coin insertedCoin, int row)
    {
        //Get the current height of that row
        int currentHeight = rowHeight[row];

        CoinSlot insertedCoinSlot = coinSlotsGrid[row, currentHeight];
        insertedCoinSlot.FillSlot(insertedCoin);
        insertedCoin.transform.position = coinDropPositions[row];//Make sure the coin is above the slot before dropping it to prevent diagonal movement through the board.
        insertedCoin.MoveTo(coinSlotsGrid[row, currentHeight].transform.position);
        if (IsServer)
        {
            rowHeight[row]++;
            CheckForWin();
        }
        return true;
    }

    #endregion
}
