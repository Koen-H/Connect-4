using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [Header("Board settings")]
    [SerializeField, Tooltip("The width of the gameboard")]
    private int boardWidth = 7;

    [SerializeField, Tooltip("The height of the gameboard")]
    private int boardHeight = 6;

    [SerializeField]
    private int connectWinCondition = 4;

    [SerializeField, Tooltip("Prefab of a singular boardslot")]
    private CoinSlot coinSlotPrefab;


    [SerializeField, Tooltip("Prefab of a singular rowColider used to get the row via raycast")]
    private RowCollider rowColiderPrefab;


    private CoinSlot[,] coinSlots;
    public CoinSlot[,] CoinSlots { get { return coinSlots; } }

    //Keep track how many coins there are in each row
    private int[] rowHeight;
    private Vector3[] coinDropPositions;
    public Vector3[] CoinDropPositions { get { return coinDropPositions; } }


    public void GenerateBoard()
    {
        coinSlots = new CoinSlot[boardWidth, boardHeight];
        rowHeight = new int[boardWidth];
        coinDropPositions = new Vector3[boardWidth];


        //Row colliders are placed in the center of the board
        int rowColHeight = (boardHeight) / 2;

        //Generate a board, starting with 0,0 in the bottom left
        for (int x = 0; x < boardWidth; x++)
        {

            for (int y = 0; y < boardHeight; y++)
            {
                CoinSlot newInstance = Instantiate(coinSlotPrefab, this.transform);
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
    }

    /// <summary>
    /// Starting from bottom left, check above, diagnoally and to the right
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
            //Enable the glow on the winning coins!
            foreach (CoinSlot slot in winningSlots)
            {
                slot.GetCoin().StartGlow();
            }
            Debug.Log("WINNER!");
        }

    }


    /// <summary>
    /// Insert the coin in a specific row
    /// </summary>
    /// <param name="insertedCoin">Coin to insert</param>
    /// <param name="row">The row</param>
    public void InsertCoin(Coin insertedCoin, int row)
    {
        int currentHeight = rowHeight[row];
        CoinSlot insertedCoinSlot = coinSlots[row, currentHeight];
        insertedCoinSlot.FillSlot(insertedCoin);
        insertedCoin.transform.position = coinSlots[row, currentHeight].transform.position;
        rowHeight[row]++;
        CheckForWin();
    }


}
