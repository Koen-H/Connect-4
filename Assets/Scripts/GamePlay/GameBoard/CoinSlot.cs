using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coinslot is one tile of the gameboard.
/// </summary>
public class CoinSlot : MonoBehaviour
{

    private Coin insertedCoin = null;
    private int ownerTeamID = 0;
    public int OwnerTeamID {  get { return ownerTeamID; } }
    private bool hasCoin = false;
    public bool HasCoin { get { return hasCoin; } }

    public Vector2Int SlotPosition = Vector2Int.zero;

    /// <summary>
    /// Fill this slot with a coin
    /// </summary>
    /// <param name="newInsertedCoin"></param>
    public void FillSlot(Coin newInsertedCoin)
    {
        insertedCoin = newInsertedCoin;
        ownerTeamID = insertedCoin.Team.TeamID;
        hasCoin = true;
    }

    /// <summary>
    /// Empty the slot and enable the dropPhysics on the inserted coin, if there is one
    /// </summary>
    public void EmptySlot()
    {
        insertedCoin?.EnableDropPhysics();
        insertedCoin = null;
        hasCoin = false;
    }

    /// <summary>
    /// Make the inserted coin glow, if there is one
    /// </summary>
    public void MakeCoinGlow()
    {
        insertedCoin?.StartGlow();
    }

    /// <summary>
    /// Check if the slot matches coins with another slot
    /// </summary>
    /// <param name="slotToCheck"></param>
    /// <returns></returns>
    public bool IsCoinMatch(CoinSlot slotToCheck)
    {
        if(!slotToCheck.HasCoin) return false;
        if (slotToCheck.OwnerTeamID != ownerTeamID) return false;
        return true;
    }

}
