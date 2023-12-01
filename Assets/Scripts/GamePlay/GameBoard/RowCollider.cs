using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The gameboard creates row colliders for each row, allowing a raycast to hit it and get a reference to the row.
/// </summary>
public class RowCollider : MonoBehaviour
{
    public int Row { get; set; }
    private BoxCollider boxCol;

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider>();
    }

    public void SetColliderHeight(float newHeight)
    {
        boxCol.size = new Vector3(boxCol.size.x, newHeight, boxCol.size.z);
    }

}
