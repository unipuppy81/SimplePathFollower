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
}
