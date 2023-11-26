using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinDropper : MonoBehaviour
{
    [SerializeField]
    private Coin coinPrefab;
    [Tooltip("The coin currently selected, the next one to drop")]
    private Coin currentCoin;

    private int selectedRow = 0;

    [SerializeField]
    GameBoard gameBoard;


    private Vector3[] coinDropPositions;

    // Start is called before the first frame update
    void Start()
    {
        coinDropPositions = gameBoard.GetCoinDropPositions();
        CreateCoin();
    }

    //Temporary
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space)) DropCoin();
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectRow(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectRow(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectRow(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectRow(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectRow(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectRow(5);
        
    }

    void CreateCoin()
    {
        currentCoin = Instantiate(coinPrefab);
        currentCoin.transform.position = coinDropPositions[selectedRow];
    }

    void SelectRow(int newSelectedRow = 0)
    {
        currentCoin.transform.position = coinDropPositions[newSelectedRow];
        selectedRow = newSelectedRow;
    }

    void DropCoin()
    {
        gameBoard.InsertCoin(currentCoin, selectedRow);
        CreateCoin();
    }
}
