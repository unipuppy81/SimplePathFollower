using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator _creator;

    private int _selectedIndex = -1; // current Selected idx (-1 : null)
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
        DrawSelectedPointInspector();

        base.OnInspectorGUI();
        DrawUtilityButtons();
    }
    #endregion

    #region Inspector UI
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
            if (EditorUtility.DisplayDialog(
                "Confirm Save",
                "Are you sure you want to overwrite the original asset with your current changes?",
                "Yes",
                "No"))
                _creator.SaveToAsset();
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Discard Changes"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Discard",
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

    private void DrawSelectedPointInspector()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _creator.Points.Count)
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField($"Selected Point Details (Index: {_selectedIndex})", EditorStyles.boldLabel);

            PathPoint p = _creator.Points[_selectedIndex];

            EditorGUI.BeginChangeCheck();

            // Basic Properties
            p.speed = EditorGUILayout.FloatField("Movement Speed", p.speed);
            p.handleMode = (PathPoint.HandleMode)EditorGUILayout.EnumPopup("Handle Mode", p.handleMode);
            p.position = EditorGUILayout.Vector3Field("Position", p.position);

            // Path Event Property
            p.eventName = EditorGUILayout.TextField("Event Name", p.eventName);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_creator, "Update Point Property");

                // If p is a class, the above changes are already applied. 
                // If p is a struct, you might need: _creator.Points[_selectedIndex] = p;

                EditorUtility.SetDirty(_creator);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Deselect"))
            {
                _selectedIndex = -1;
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    private void DrawSelectedPointHandles(PathPoint p, int index)
    {
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

        Handles.Label(p.position + Vector3.down * 0.4f, $"Index : {index}",
            new GUIStyle
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            });

        Handles.Label(p.tangentIn + Vector3.up * 0.2f, "In",
            new GUIStyle { fontSize = 12, normal = { textColor = Color.yellow } });

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
            new GUIStyle { fontSize = 12, normal = { textColor = Color.magenta } });

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
        if (index < _creator.Points.Count - 1)
        {
            PathPoint nextP = _creator.Points[index + 1];
            Handles.DrawBezier(p.position, nextP.position, p.tangentOut, nextP.tangentIn, Color.cyan, null, 7f);
        }
    }
    #endregion

    private void DrawBezierPath()
    {
        if (_creator.pathDataAsset == null) return;

        var points = _creator.Points;
        if (points == null || points.Count == 0) return;

        float flowTime = (float)EditorApplication.timeSinceStartup * 0.3f;

        for (int i = 0; i < _creator.Points.Count; i++)
        {
            PathPoint p = _creator.Points[i];

            float normalSize = HandleUtility.GetHandleSize(p.position) * 0.15f;
            float selectedSize = normalSize * 1.1f;
            Handles.color = (_selectedIndex == i) ? Color.cyan : Color.white;

            float currentSize = (_selectedIndex == i) ? selectedSize : normalSize;
            if (Handles.Button(p.position, Quaternion.identity, currentSize, currentSize, Handles.SphereHandleCap))
            {
                _selectedIndex = i;
                Repaint();
            }

            if (i < points.Count - 1)
            {
                PathPoint nextP = points[i + 1];
                Handles.DrawBezier(
                    p.position, nextP.position,
                    p.tangentOut, nextP.tangentIn,
                    Color.green, null, 4f
                );

                int arrowCount = 3;
                for (int j = 0; j < arrowCount; j++)
                {
                    float t = (j / (float)arrowCount + flowTime) % 1f;
                    Vector3 arrowPos = Bezier.GetPoint(p.position, p.tangentOut, nextP.tangentIn, nextP.position, t);
                    Vector3 dir = GetBezierTangent(p, nextP, t);

                    Handles.color = Color.yellow; 
                    float arrowSize = HandleUtility.GetHandleSize(arrowPos) * 0.15f;
                    Handles.ConeHandleCap(0, arrowPos, Quaternion.LookRotation(dir), arrowSize, EventType.Repaint);
                }
            }

            if (_selectedIndex == i)
            {
                DrawSelectedPointHandles(p, i);
            }          


        }

        if (!Application.isPlaying)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
    }

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

    private Vector3 GetBezierTangent(PathPoint p1, PathPoint p2, float t)
    {
        float omitT = 1f - t;
        return (3f * omitT * omitT * (p1.tangentOut - p1.position) +
                6f * omitT * t * (p2.tangentIn - p1.tangentOut) +
                3f * t * t * (p2.position - p2.tangentIn)).normalized;
    }
}
