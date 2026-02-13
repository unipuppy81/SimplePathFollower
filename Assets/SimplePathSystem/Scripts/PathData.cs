using UnityEngine;

[System.Serializable]
public class PathPoint
{
    public enum HandleMode { Free, Mirrored };
    public HandleMode handleMode = HandleMode.Mirrored;

    public Vector3 position;
    public Vector3 tangentIn;  // ÀÌÀü Á¡°úÀÇ °î·üÀ» °áÁ¤
    public Vector3 tangentOut; // ´ÙÀ½ Á¡°úÀÇ °î·üÀ» °áÁ¤

    public float speed = 5f;

    public PathPoint(Vector3 pos)
    {
        position = pos;
        tangentIn = pos + new Vector3(-1, 0, 0);
        tangentOut = pos + new Vector3(1, 0, 0);
        speed = 5f;
    }
}

public struct PathData
{
    public Vector3 position;
    public float speed;
}
