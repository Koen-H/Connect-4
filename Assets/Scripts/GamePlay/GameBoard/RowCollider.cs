using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The gameboard creates row colliders for each row, allowing a raycast to hit it and get a reference to the row.
/// </summary>
public class RowCollider : MonoBehaviour
{
    [Tooltip("")]
    public int Row { get; set; }
    [SerializeField]
    private BoxCollider boxCol;

    public void SetColliderHeight(float newHeight)
    {
        boxCol.size = new Vector3(boxCol.size.x, newHeight, boxCol.size.z);
    }

}
