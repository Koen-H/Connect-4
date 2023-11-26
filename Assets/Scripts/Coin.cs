using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    //Team
    [Tooltip("To which team does this coin belong to?")]
    private Team team;
    public Team Team { get { return team; } }


    [Header("Rendering")]
    [SerializeField, Tooltip("Mesh renderer of the coin")]
    private MeshRenderer meshRenderer;

    public void SetTeam(Team newTeam)
    {
        team = newTeam;
        SetColor(team.TeamColor);
    }

    private void SetColor(Color newColor)
    {
        meshRenderer.material.SetColor("_BaseColor", newColor);
    }

}
