using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class GameBoard : MonoBehaviour
{
    [Header("Board settings")]
    [SerializeField, Tooltip("The width of the gameboard")]
    private int boardWidth = 7;

    [SerializeField, Tooltip("The height of the gameboard")]
    private int boardHeight = 6;

    [SerializeField, Tooltip("Prefab of a singular boardslot")]
    private CoinSlot coinSlotPrefab;


    private CoinSlot[,] coinSlots;
    public CoinSlot[,] CoinSlots {  get { return coinSlots; } }
    
    //Keep track how many coins there are in each row
    private int[] rowHeight;

    private void Awake()
    {
        GenerateBoard();
    }


    private void GenerateBoard()
    {
        coinSlots = new CoinSlot[boardWidth, boardHeight];
        rowHeight = new int[boardWidth];
        //Generate a board, starting with 0,0 in the bottom left
        for (int x = 0; x < boardWidth; x++)
        {
            for(int y = 0; y < boardHeight; y++)
            {
                CoinSlot newInstance = Instantiate(coinSlotPrefab, this.transform);
                newInstance.gameObject.name = $"CoinSlot {x},{y}";
                newInstance.transform.localPosition = new Vector3(x, y, 0);
                coinSlots[x, y] = newInstance;

            }
        }
    }

    public Vector3[] GetCoinDropPositions()
    {
        Vector3[] dropPositions = new Vector3[boardWidth];
        for (int x = 0; x < boardWidth; x++)
        {
            //TODO: see vector3
            dropPositions[x] = coinSlots[x, boardHeight - 1].transform.position + new Vector3(0, 0.75f, 0);
        }
        return dropPositions;
    }


    public void InsertCoin(Coin insertedCoin, int row)
    {
        int currentHeight = rowHeight[row];
        insertedCoin.transform.position = coinSlots[row, currentHeight].transform.position;
        rowHeight[row]++;
    }


    private void Update()
    {

    }

}
