using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Destroy all the child GameObjects
    /// </summary>
    /// <param name="transform"></param>
    public static void DestroyAllChildObjects(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}