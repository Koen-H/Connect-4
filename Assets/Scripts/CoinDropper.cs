using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Coin dropper controls the position of the coin using player input.
/// </summary>
public class CoinDropper : MonoBehaviour
{
    private Coin currentCoin;

    [SerializeField]
    private Coin coinPrefab;


    [SerializeField]
    private GameBoard gameBoard;

    //NetworkVariable
    private int selectedRow = 0;

    public UnityEvent OnCoinDropped = new();

    [SerializeField, Tooltip("The row collider layer")]
    private LayerMask rowColliderLayer;

    private Vector3[] coinDropPositions;


    // Start is called before the first frame update
    void Start()
    {
        GameManager.Singleton.OnGameAfterInitialize.AddListener(Initalize);
    }

    private void Initalize()
    {
        coinDropPositions = gameBoard.CoinDropPositions;
    }


    void Update()
    {
        FindRow();
        if (Input.GetMouseButtonDown(0)) TryDropCoin();
    }


    /// <summary>
    /// Find the row the player is currently hovering
    /// </summary>
    private void FindRow()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, rowColliderLayer))
        {
            if (hitInfo.collider.gameObject.TryGetComponent<RowCollider>(out RowCollider rowCol))
            {
                SelectRow(rowCol.Row);
            }
        }
    }

    public void CreateCoin(Team currentTeam)
    {
        currentCoin = Instantiate(coinPrefab);
        currentCoin.SetTeam(currentTeam);
        Vector3 targetPos = coinDropPositions[selectedRow];
        currentCoin.transform.position = targetPos;
        currentCoin.MoveTo(targetPos);
    }

    void SelectRow(int newSelectedRow = 0)
    {
        //Move coin to row
        currentCoin.MoveTo(coinDropPositions[newSelectedRow]);

        selectedRow = newSelectedRow;
    }

    /// <summary>
    /// Try to drop the coin if possible
    /// </summary>
    void TryDropCoin()
    {
        if (gameBoard.TryInsertCoin(currentCoin, selectedRow))
        {
            OnCoinDropped.Invoke();

        }
    }
}
