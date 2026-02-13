using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathManager : MonoBehaviour
{
    [Header("Path Assets")]
    public List<PathDataAsset> pathList = new List<PathDataAsset>();

    [Header("Follower Target")]
    public PathFollower follower;

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            PlayPath(0);
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            PlayPath(1);
        }
    }

    /// <summary>
    /// Play Path Sample
    /// </summary>
    /// <param name="index"></param>
    public void PlayPath(int index)
    {
        if (index < 0 || index >= pathList.Count)
        {
            Debug.LogError($"<color=red>[PathManager]</color> Index : {index} path is Empty!");
            return;
        }

        if (follower == null)
        {
            Debug.LogError("<color=red>[PathManager]</color> Follower is Null!");
            return;
        }

        follower.StartMoving(pathList[index]);
        Debug.Log($"<color=lime>[PathManager]</color> {pathList[index].name} Path Start!");
    }
}

/// <summary>
/// Bezier Utility
/// </summary>
public static class Bezier
{
    public static Vector3 GetPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return Mathf.Pow(1 - t, 3) * a +
               3 * Mathf.Pow(1 - t, 2) * t * b +
               3 * (1 - t) * Mathf.Pow(t, 2) * c +
               Mathf.Pow(t, 3) * d;
    }
}
