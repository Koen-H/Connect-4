using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    //Team
    //Color


    [Header("Rendering")]
    [SerializeField, Tooltip("Mesh renderer of the coin")]
    private MeshRenderer meshRenderer;

    public void SetColor(Color newColor)
    {
        meshRenderer.material.SetColor("_BaseColor", newColor);
    }

    private void Awake()
    {

    }

}
