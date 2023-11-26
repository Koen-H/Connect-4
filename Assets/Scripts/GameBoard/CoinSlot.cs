using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSlot : MonoBehaviour
{
    private CoinSlotState currentState = CoinSlotState.EMPTY;
    private int team = 0;

}


public enum CoinSlotState { EMPTY, FILLED }