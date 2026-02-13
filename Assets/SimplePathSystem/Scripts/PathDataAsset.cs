using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPathData", menuName = "PathSystem/PathData")]
public class PathDataAsset : ScriptableObject
{
    [SerializeField] public List<PathPoint> points = new List<PathPoint>();
}