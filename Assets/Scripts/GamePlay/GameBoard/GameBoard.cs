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

    private List<Coin> insertedCoins = new();

    /// <summary>
    /// 2D map of the gameBoard
    /// </summary>
    private CoinSlot[,] coinSlotsGrid;
    public CoinSlot[,] CoinSlotsGrid { get { return coinSlotsGrid; } }

    private NetworkList<Vector2Int> winningCoinSlotsPositions;

    //Keep track how many coins there are in each row
    //NOTE: Networked list instead of array because networkedArray doesnt exist.
    private NetworkList<int> rowHeight;

    //Positions above the board where the coin will be visually displayed before dropping
    private Vector3[] coinDropPositions;

    /// <summary>
    /// Called when the gameboard finished generating.
    /// </summary>
    public event Action OnGameBoardGenerated;
    public event Action<Vector3[]> OnCoinDropPositionsGenerated;
    public event Action<int> OnGameWin;

    private void Awake()
    {
        rowHeight = new NetworkList<int>();
        winningCoinSlotsPositions = new NetworkList<Vector2Int>();
        winningCoinSlotsPositions.OnListChanged += OnWinningSlotsChanged;
    }

    /// <summary>
    /// Tell the clients to reset the board.
    /// </summary>
    [ClientRpc]
    public void ResetBoardClientRpc()
    {
        ResetBoard();
    }


    /// <summary>
    /// Enable physics drop on coins, resets gameplay variables.
    /// </summary>
    public void ResetBoard()
    {
        foreach (Coin coin in insertedCoins) coin.EnableDropPhysics();
        insertedCoins.Clear();
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

    private void GenerateBoard()
    {
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
    /// Inform all clients to generate the board, this includes the server as the server will always be a host.
    /// </summary>
    [ClientRpc]
    public void GenerateBoardClientRpc()
    {
        GenerateBoard();
    }


    /// <summary>
    /// Starting from bottom left, loop through each slot and check above, diagnoally and to the right of the slot
    /// </summary>
    public void CheckForWin()
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
                bool checkReverseX = x - (connectWinCondition - 1) > 0;


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
            winningSlots.Distinct();
            winningCoinSlotsPositions.Clear();
            
            //Turn the winning slots in to vector2Int positions that can be send over the network.
            foreach (CoinSlot slot in winningSlots)
            {
                winningCoinSlotsPositions.Add(slot.SlotPosition);
            }
            Debug.Log("WINNER!");
        }
    }

    private void OnWinningSlotsChanged(NetworkListEvent<Vector2Int> changeEvent)
    {
        //Invoke in networkListEvent to invoke the win on all clients
        if(winningCoinSlotsPositions.Count > 0)
        {
            OnGameWin?.Invoke(coinSlotsGrid[ winningCoinSlotsPositions[0].x, winningCoinSlotsPositions[0].y].OwnerTeamID);
        }
        foreach (Vector2Int slotPosition in winningCoinSlotsPositions)
        {
            //Get the coin in the slot and start the winning glow effect on it.
            coinSlotsGrid[slotPosition.x, slotPosition.y].GetCoin().StartGlow();
        }
    }

    /// <summary>
    /// Check if the coin can be dropped in this row, or if it's full already
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
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
        insertedCoins.Add(insertedCoin);
        if (IsServer)
        {
            rowHeight[row]++;
            if (IsServer) CheckForWin();
        }
        return true;
    }


}
