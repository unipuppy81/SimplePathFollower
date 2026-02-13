using Codice.Client.Common.WebApi.Requests;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator _creator;

    private void OnEnable() => _creator = (PathCreator)target;

    private void OnSceneGUI()
    {
        HandleInput();
        DrawBezierPath();
    }

    private void DrawBezierPath()
    {
        for(int i =0;i < _creator.points.Count; i++)
        {
            PathPoint p = _creator.points[i];

            // 1. 메인 앵커 포인트 (이동 핸들)
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(p.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator, "Move Point");
                Vector3 delta = newPos - p.position;
                p.position = newPos;
                p.tangentIn += delta;  // 포인트 이동시 핸들도 같이 이동
                p.tangentOut += delta;
            }

            // --- 조절점(Tangent) 핸들 영역 ---


            // 2. Tangent In (이전 점에서 들어오는 선) - 노란색
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.yellow;
            Vector3 newIn = Handles.FreeMoveHandle(p.tangentIn, 0.1f, Vector3.zero, Handles.SphereHandleCap);

            Handles.Label(p.position + Vector3.down * 0.4f, $"Speed : {p.speed}",
                new GUIStyle
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter
                });

            Handles.Label(p.tangentIn + Vector3.up * 0.2f, "In", 
                new GUIStyle 
                { 
                    normal = { textColor = Color.yellow } 
                });

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator, "Move Tangent In");
                p.tangentIn = newIn;
                if (p.handleMode == PathPoint.HandleMode.Mirrored)
                {
                    // 중심점 기준으로 반대 방향 계산: Out = Pos + (Pos - In)
                    Vector3 dir = p.position - p.tangentIn;
                    p.tangentOut = p.position + dir;
                }
            }

            // 3. Tangent Out (다음 점으로 나가는 선) - 분홍색(마젠타)
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.magenta;
            Vector3 newOut = Handles.FreeMoveHandle(p.tangentOut, 0.1f, Vector3.zero, Handles.SphereHandleCap);
            Handles.Label(p.tangentOut + Vector3.up * 0.2f, "Out", new GUIStyle { normal = { textColor = Color.magenta } });
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator, "Move Tangent Out");
                p.tangentOut = newOut;
                if (p.handleMode == PathPoint.HandleMode.Mirrored)
                {
                    // 중심점 기준으로 반대 방향 계산: In = Pos + (Pos - Out)
                    Vector3 dir = p.position - p.tangentOut;
                    p.tangentIn = p.position + dir;
                }
            }

            Handles.color = Color.white;
            Handles.DrawLine(p.position, p.tangentIn);
            Handles.DrawLine(p.position, p.tangentOut);

            // 4. 다음 점과의 곡선 그리기
            if (i < _creator.points.Count - 1)
            {
                PathPoint nextP = _creator.points[i + 1];
                Handles.DrawBezier(p.position, nextP.position, p.tangentOut, nextP.tangentIn, Color.green, null, 2f);
            }
        }
    }

    private void HandleInput()
    {
        Event guiEvent = Event.current;

        // Shift + 마우스 왼쪽 클릭 감지
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            // 마우스 클릭 위치를 월드 좌표로 변환
            Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // 바닥 기준 (y=0)

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 newPos = ray.GetPoint(distance);

                Undo.RecordObject(_creator, "Add Path Point"); // 되돌리기 기록
                _creator.points.Add(new PathPoint(newPos));    // 리스트에 새 포인트 추가

                guiEvent.Use(); // 이벤트 소모 (다른 클릭 방지)
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Reset Path"))
        {
            Undo.RecordObject(_creator, "Reset Path");
            _creator.CreateDefaultPath();
        }
    }
}
