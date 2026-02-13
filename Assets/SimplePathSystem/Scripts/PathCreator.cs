using UnityEngine;
using System.Collections.Generic;

public class PathCreator : MonoBehaviour
{
    public List<PathPoint> points = new List<PathPoint>();

    [Range(5, 50)] public int segmentResolution = 20; // 곡선 구간당 정점 수

    public void CreateDefaultPath()
    {
        points.Clear();
        points.Add(new PathPoint(Vector3.zero));
        points.Add(new PathPoint(Vector3.forward * 5f));
    }

    public List<PathData> GetPathData()
    {
        List<PathData> result = new List<PathData>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            PathPoint p1 = points[i];
            PathPoint p2 = points[i + 1];

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

    // Bezier 수식 헬퍼 클래스
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
