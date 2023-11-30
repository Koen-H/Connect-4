using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private GameObject gamePlayUI;
    [SerializeField]
    private TextMeshProUGUI currentTurnTextUI;

    [Header("After game")]
    [SerializeField]
    private GameObject afterGameUI;
    [SerializeField]
    private TextMeshProUGUI winnerTextUI;
}
