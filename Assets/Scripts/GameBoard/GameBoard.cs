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
    private CoinSlot[,] coinSlots;
    public CoinSlot[,] CoinSlots { get { return coinSlots; } }

    public NetworkList<Vector2Int> winningCoinSlotsPositions;

    //Keep track how many coins there are in each row
    //NOTE: Networked list instead of array because networkedArray doesnt exist.
    private NetworkList<int> rowHeight;

    //Positions above the board where the coin will be visually displayed before dropping
    private Vector3[] coinDropPositions;

    /// <summary>
    /// Called when the gameboard finished generating.
    /// </summary>
    public event Action OnGameBoardGenerated = delegate { };
    public event Action<Vector3[]> OnCoinDropPositionsGenerated = delegate { };

    private void Awake()
    {
        rowHeight = new NetworkList<int>();
        winningCoinSlotsPositions = new NetworkList<Vector2Int>();
        winningCoinSlotsPositions.OnListChanged += OnWinningSlotsChanged;
    }
    

    /// <summary>
    /// Enable physics drop on coins, resets gameplay variables.
    /// </summary>
    public void ResetBoard()
    {
        foreach (Coin coin in insertedCoins) coin.EnableDropPhysics();
        insertedCoins.Clear();
        //TODO:: MArk slots as empty

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
                coinSlots[x, y] = newInstance;
            }
            //Create coin Drop position
            coinDropPositions[x] = coinSlots[x, boardHeight - 1].transform.position + new Vector3(0, 0.75f, 0);

            //Instantiate a row collider on the row
            RowCollider rowCol = Instantiate(rowColiderPrefab, this.transform);
            rowCol.gameObject.name = $"RowCol {x}";
            rowCol.transform.localPosition = new Vector3(x, rowColHeight, 0);
            rowCol.Row = x;
            rowCol.SetColliderHeight(boardHeight + 1);
        }
        OnCoinDropPositionsGenerated.Invoke(coinDropPositions);
        OnGameBoardGenerated.Invoke();
    }

    /// <summary>
    /// Tell clients to generate the board
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
                CoinSlot currentCoinSlot = coinSlots[x, y];

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
                        CoinSlot slotToCheck = coinSlots[x, y + t];
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
                        CoinSlot slotToCheck = coinSlots[x + r, y];
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
                        CoinSlot slotToCheck = coinSlots[x + dr, y + dr];
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
                        CoinSlot slotToCheck = coinSlots[x - dl, y + dl];
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
        foreach (Vector2Int slotPosition in winningCoinSlotsPositions)
        {
            //Get the coin in the slot and start the winning glow effect on it.
            coinSlots[slotPosition.x, slotPosition.y].GetCoin().StartGlow();
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

        CoinSlot insertedCoinSlot = coinSlots[row, currentHeight];
        insertedCoinSlot.FillSlot(insertedCoin);
        insertedCoin.transform.position = coinDropPositions[row];//Make sure the coin is above the slot before dropping it to prevent diagonal movement through the board.
        insertedCoin.MoveTo(coinSlots[row, currentHeight].transform.position);
        insertedCoins.Add(insertedCoin);
        if (IsServer)
        {
            rowHeight[row]++;
            if (IsServer) CheckForWin();
        }
        return true;
    }


}
