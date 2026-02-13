using UnityEngine;
using System.Collections.Generic;

public class PathCreator : MonoBehaviour
{
    #region Fields & Properties
    [HideInInspector] 
    public PathDataAsset pathDataAsset;
    
    [SerializeField] 
    private List<PathPoint> _workingPoints = new List<PathPoint>();

    public List<PathPoint> Points => _workingPoints;

    private int segmentResolution = 20; // Number of vertices per curve segment

    #endregion

    #region Asset Synchronization (ScriptableObject <-> Workspace)
    /// <summary>
    /// SO -> Work Space
    /// </summary>
    public void LoadFromAsset()
    {
        if (pathDataAsset == null) return;

        _workingPoints.Clear();
        foreach (var p in pathDataAsset.points)
        {
            _workingPoints.Add(new PathPoint(p)); 
        }
    }

    /// <summary>
    /// Work Space -> SO
    /// </summary>
    public void SaveToAsset()
    {
        if (pathDataAsset == null) return;

        UnityEditor.Undo.RecordObject(pathDataAsset, "Save Path to SO");

        pathDataAsset.points.Clear();
        foreach (var p in _workingPoints)
        {
            pathDataAsset.points.Add(new PathPoint(p));
        }

        UnityEditor.EditorUtility.SetDirty(pathDataAsset);
        UnityEditor.AssetDatabase.SaveAssets();
    }

    #endregion

    /// <summary>
    /// Path Initialization
    /// </summary>
    public void CreateDefaultPath()
    {
        if (pathDataAsset == null) return;

        _workingPoints.Clear();
        _workingPoints.Add(new PathPoint(Vector3.zero));
        _workingPoints.Add(new PathPoint(Vector3.forward * 5f));

        UnityEditor.EditorUtility.SetDirty(this);
    }

    public List<PathData> GetPathData()
    {
        List<PathData> result = new List<PathData>();
        for (int i = 0; i < _workingPoints.Count - 1; i++)
        {
            PathPoint p1 = _workingPoints[i];
            PathPoint p2 = _workingPoints[i + 1];

            for (int j = 0; j <= segmentResolution; j++)
            {
                float t = j / (float)segmentResolution;
                PathData data;
                data.position = Bezier.GetPoint(p1.position, p1.tangentOut, p2.tangentIn, p2.position, t);

                // 두 점 사이의 속도를 선형 보간(Lerp)으로 계산
                data.speed = Mathf.Lerp(p1.speed, p2.speed, t);
                result.Add(data);
            }
        }
        return result;
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
}
