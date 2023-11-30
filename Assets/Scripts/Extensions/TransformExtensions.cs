using UnityEngine;

public static class TransformExtensions
{
    public static void DestroyAllChildObjects(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}