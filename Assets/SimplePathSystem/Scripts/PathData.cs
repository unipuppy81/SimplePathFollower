using UnityEngine;

[System.Serializable]
public class PathPoint
{
    /// <summary>
    /// Defines how the incoming and outgoing tangents behave.
    /// </summary>
    public enum HandleMode 
    {
        /// <summary>
        /// Incoming and outgoing tangents can be adjusted independently (asymmetrical).
        /// </summary>
        Free,

        /// <summary>
        /// Incoming and outgoing tangents are mirrored (symmetrical).
        /// </summary>
        Mirrored
    };

    public HandleMode handleMode = HandleMode.Mirrored;

    public Vector3 position;
    public Vector3 tangentIn;
    public Vector3 tangentOut;

    public float speed = 5f;

    // Copy constructor
    public PathPoint(PathPoint source)
    {
        this.position = source.position;
        this.tangentIn = source.tangentIn;
        this.tangentOut = source.tangentOut;
        this.handleMode = source.handleMode;
        this.speed = source.speed;
    }

    // Default constructor
    public PathPoint(Vector3 pos)
    {
        position = pos;
        tangentIn = pos + new Vector3(1, 1, 0);
        tangentOut = pos + new Vector3(3, 3, 0);
        speed = 5f;
    }
}

public struct PathData
{
    public Vector3 position;
    public float speed;
}
