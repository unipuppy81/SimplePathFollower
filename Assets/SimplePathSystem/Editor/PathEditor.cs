using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator _creator;

    #region Unity Callbacks
    private void OnEnable() => _creator = (PathCreator)target;

    private void OnSceneGUI()
    {
        // Disable scene interaction if no asset is assigned
        if (_creator.pathDataAsset == null) return;

        HandleInput();
        DrawBezierPath();
    }

    public override void OnInspectorGUI()
    {
        _creator = (PathCreator)target;

        DrawAssetField();

        if (_creator.pathDataAsset == null)
            return;

        DrawAssetControls();
        base.OnInspectorGUI();
        DrawUtilityButtons();
    }
    #endregion

    #region Scene Drawing

    private void DrawBezierPath()
    {
        if (_creator.pathDataAsset == null) return;

        var points = _creator.pathDataAsset.points;
        if (points == null || points.Count == 0) return;

        for (int i = 0; i < _creator.Points.Count; i++)
        {
            PathPoint p = _creator.Points[i];

            // Anchor handle
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(p.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator.pathDataAsset, "Move Point");
                Vector3 delta = newPos - p.position;
                p.position = newPos;
                p.tangentIn += delta;
                p.tangentOut += delta;
            }

            // Tangent In
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
                new GUIStyle { normal = { textColor = Color.yellow } });

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator.pathDataAsset, "Move Tangent In");
                p.tangentIn = newIn;

                if (p.handleMode == PathPoint.HandleMode.Mirrored)
                {
                    // Mirror tangent around anchor
                    Vector3 dir = p.position - p.tangentIn;
                    p.tangentOut = p.position + dir;
                }
            }

            // Tangent Out
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.magenta;
            Vector3 newOut = Handles.FreeMoveHandle(p.tangentOut, 0.1f, Vector3.zero, Handles.SphereHandleCap);

            Handles.Label(p.tangentOut + Vector3.up * 0.2f, "Out",
                new GUIStyle { normal = { textColor = Color.magenta } });

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator.pathDataAsset, "Move Tangent Out");
                p.tangentOut = newOut;

                if (p.handleMode == PathPoint.HandleMode.Mirrored)
                {
                    // Mirror tangent around anchor
                    Vector3 dir = p.position - p.tangentOut;
                    p.tangentIn = p.position + dir;
                }
            }

            Handles.color = Color.white;
            Handles.DrawLine(p.position, p.tangentIn);
            Handles.DrawLine(p.position, p.tangentOut);

            // Draw bezier to next point
            if (i < _creator.Points.Count - 1)
            {
                PathPoint nextP = _creator.Points[i + 1];
                Handles.DrawBezier(p.position, nextP.position, p.tangentOut, nextP.tangentIn, Color.green, null, 2f);
            }
        }
    }

    #endregion


    #region Scene Input

    private void HandleInput()
    {
        Event guiEvent = Event.current;

        // Shift + Left Click adds a new point on ground plane (Y=0)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 newPos = ray.GetPoint(distance);

                Undo.RecordObject(_creator, "Add Path Point");
                _creator.Points.Add(new PathPoint(newPos));

                EditorUtility.SetDirty(_creator);

                guiEvent.Use();
                SceneView.RepaintAll();
            }
        }
    }

    #endregion


    #region Inspector UI

    private void DrawAssetField()
    {
        EditorGUI.BeginChangeCheck();
        _creator.pathDataAsset = (PathDataAsset)EditorGUILayout.ObjectField(
            "Path Data (ScriptableObject)",
            _creator.pathDataAsset,
            typeof(PathDataAsset),
            false);

        if (EditorGUI.EndChangeCheck())
        {
            _creator.LoadFromAsset();
            SceneView.RepaintAll();
        }

        if (_creator.pathDataAsset == null)
        {
            EditorGUILayout.HelpBox(
                "Assign or create a PathData asset to begin editing.",
                MessageType.Warning);

            if (GUILayout.Button("Create New Path Data", GUILayout.Height(30)))
            {
                CreateNewAsset();
            }
        }
    }

    private void DrawAssetControls()
    {
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Save Changes To Asset", GUILayout.Height(30)))
        {
            _creator.SaveToAsset();
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Discard Changes"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Discard all current changes and reload from asset?",
                "Yes",
                "No"))
            {
                _creator.LoadFromAsset();
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.Space();
    }

    private void DrawUtilityButtons()
    {
        if (GUILayout.Button("Reset Path"))
        {
            if (EditorUtility.DisplayDialog(
                "Warning",
                "Reset the entire path?",
                "Yes",
                "No"))
            {
                Undo.RecordObject(_creator, "Reset Path");
                _creator.CreateDefaultPath();
                SceneView.RepaintAll();
            }
        }
    }

    #endregion


    #region Asset Creation

    private void CreateNewAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save New Path Data",
            "NewPathData",
            "asset",
            "Select a location to save the new path data.");

        if (string.IsNullOrEmpty(path)) return;

        PathDataAsset newAsset = ScriptableObject.CreateInstance<PathDataAsset>();

        // Add two default points for initial usability
        newAsset.points.Add(new PathPoint(Vector3.zero));
        newAsset.points.Add(new PathPoint(Vector3.forward * 5f));

        AssetDatabase.CreateAsset(newAsset, path);

        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Undo.RecordObject(_creator, "Assign New Path Asset");
        _creator.pathDataAsset = newAsset;

        Debug.Log($"<color=green>[PathSystem]</color> New asset created: {path}");
    }

    #endregion

    /*
    private void DrawBezierPath()
    {
        if (_creator.pathDataAsset == null) return;

        var points = _creator.pathDataAsset.points;
        if (points == null || points.Count == 0) return;

        for (int i =0;i < _creator.Points.Count; i++)
        {
            PathPoint p = _creator.Points[i];

            // 1. 메인 앵커 포인트 (이동 핸들)
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(p.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator.pathDataAsset, "Move Point");
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
                Undo.RecordObject(_creator.pathDataAsset, "Move Tangent In");
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
                Undo.RecordObject(_creator.pathDataAsset, "Move Tangent Out");
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
            if (i < _creator.Points.Count - 1)
            {
                PathPoint nextP = _creator.Points[i + 1];
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
                _creator.Points.Add(new PathPoint(newPos));    // 리스트에 새 포인트 추가

                // 4. [핵심] 데이터가 바뀌었음을 유니티에 알림 (이게 있어야 인스펙터에 즉시 반영됨)
                EditorUtility.SetDirty(_creator);

                // 5. 이벤트 사용 처리 (다른 의도치 않은 클릭 방지)
                guiEvent.Use();

                // 6. 씬 뷰와 인스펙터 강제 리페인트
                SceneView.RepaintAll();
            }
        }
    }


    private void CreateNewAsset()
    {
        // 1. 저장할 폴더와 이름 선택창 띄우기
        string path = EditorUtility.SaveFilePanelInProject(
            "Save New Path Data",
            "NewPathData",
            "asset",
            "새 경로 데이터를 저장할 위치를 선택하세요.");

        if (string.IsNullOrEmpty(path)) return;

        // 2. 메모리에 SO 인스턴스 생성
        PathDataAsset newAsset = ScriptableObject.CreateInstance<PathDataAsset>();

        // 3. 기본 포인트 2개 추가 (비어있으면 조작이 힘드니까요)
        newAsset.points.Add(new PathPoint(Vector3.zero));
        newAsset.points.Add(new PathPoint(Vector3.forward * 5f));

        // 4. 실제로 프로젝트에 파일로 생성
        AssetDatabase.CreateAsset(newAsset, path);

        // 5. 물리적 저장 및 DB 갱신
        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 6. 현재 Creator에 자동 할당
        Undo.RecordObject(_creator, "Assign New Path Asset");
        _creator.pathDataAsset = newAsset;

        Debug.Log($"<color=green>[PathSystem]</color> 새 에셋 생성 완료: {path}");
    }
    */
}
