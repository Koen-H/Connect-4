using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoinDropper : MonoBehaviour
{
    [SerializeField]
    private Coin coinPrefab;
    [Tooltip("The coin currently selected, the next one to drop")]
    private Coin currentCoin;

    private int selectedRow = 0;

    public UnityEvent OnCoinDropped = new();

    [SerializeField]
    GameBoard gameBoard;

    [SerializeField, Tooltip("The row collider layer")]
    private LayerMask rowColliderLayer;

    private Vector3[] coinDropPositions;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Singleton.OnGameInitialized.AddListener(Initalize);
    }

    private void Initalize()
    {
        coinDropPositions = gameBoard.CoinDropPositions;

    }


    void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, rowColliderLayer))
        {
            if (hitInfo.collider.gameObject.TryGetComponent<RowCollider>(out RowCollider rowCol))
            {
                SelectRow(rowCol.Row);
            }
        }
        if (Input.GetMouseButtonDown(0)) DropCoin();
    }

    public void CreateCoin(Team currentTeam)
    {
        currentCoin = Instantiate(coinPrefab);
        currentCoin.SetTeam(currentTeam);
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
        OnCoinDropped.Invoke();
    }
}
