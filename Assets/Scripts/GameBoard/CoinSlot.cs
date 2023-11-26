using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSlot : MonoBehaviour
{

    private Coin insertedCoin = null;
    private int ownerTeamID = 0;
    public int OwnerTeamID {  get { return ownerTeamID; } }
    private bool hasCoin = false;
    public bool HasCoin { get { return hasCoin; } }

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

    public Coin GetCoin()
    {
        return insertedCoin;
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
