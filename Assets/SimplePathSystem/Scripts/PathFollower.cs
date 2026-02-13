using System.Collections.Generic;
using UnityEngine;
using static PathCreator;

public class PathFollower : MonoBehaviour
{
    public Transform lookAtTarget;
    public bool isLoop = false;
    [SerializeField]
    [Range(2, 50)] private int segmentResolution = 20; // Number of vertices per curve segment

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f; 
    [SerializeField] private bool lockYRotation = true; // y-axis fix option

    public System.Action<string> OnPathEvent;

    #region Runtime State
    private List<PathData> _sampledPath = new List<PathData>();
    private int _currentPointIndex = 0;
    private bool _isAutoMoving = false;
    #endregion

    #region Public Controls

    /// <summary>
    /// Starts movement along the given path data.
    /// </summary>
    public void StartMoving(PathDataAsset pathData)
    {
        if (pathData == null || pathData.points == null || pathData.points.Count == 0)
        {
            Debug.LogWarning($"<color=red>[PathFollower]</color> Assigned path data is empty.");
            return;
        }

        GenerateSampledPath(pathData.points);

        _currentPointIndex = 0;
        _isAutoMoving = true;

        if (_sampledPath.Count > 0)
        {
            transform.position = _sampledPath[0].position;
        }
    }

    /// <summary>
    /// Stops movement.
    /// </summary>
    public void StopMoving()
    {
        _isAutoMoving = false;
    }
    #endregion

    private void Update()
    {
        if (!_isAutoMoving || _sampledPath == null || _sampledPath.Count == 0) return;

        MoveStep();
        HandleRotation();
    }

    private void GenerateSampledPath(List<PathPoint> points)
    {
        _sampledPath.Clear();

        for (int i = 0; i < points.Count - 1; i++)
        {
            PathPoint p1 = points[i];
            PathPoint p2 = points[i + 1];

            for (int j = 0; j <= segmentResolution; j++)
            {
                if (i > 0 && j == 0) continue;

                float t = j / (float)segmentResolution;

                PathData data = new PathData();
                data.position = Bezier.GetPoint(p1.position, p1.tangentOut, p2.tangentIn, p2.position, t);
                data.speed = Mathf.Lerp(p1.speed, p2.speed, t);

                // Insert Event
                if (j == segmentResolution)
                    data.triggeredEvent = p2.eventName;
                else if (i == 0 && j == 0)
                    data.triggeredEvent = p1.eventName;

                _sampledPath.Add(data);
            }
        }
    }

    /// <summary>
    /// Move Logic
    /// </summary>
    private void MoveStep()
    {
        PathData targetData = _sampledPath[_currentPointIndex];
        float speed = targetData.speed;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetData.position,
            speed * Time.deltaTime
        );

        // Check arrival at current target
        if (Vector3.Distance(transform.position, targetData.position) < 0.001f)
        {
            if(!string.IsNullOrEmpty(targetData.triggeredEvent))
            {
                ExecuteEvent(targetData.triggeredEvent);
            }


            _currentPointIndex++;

            if (_currentPointIndex >= _sampledPath.Count)
            {
                if (isLoop)
                {
                    _currentPointIndex = 0;
                }
                else
                {
                    _isAutoMoving = false;
                    transform.rotation = Quaternion.identity;
                    ResetRotationGradually();
                }
            }
        }
    }

    /// <summary>
    /// Rotation Logic
    /// </summary>
    private void HandleRotation()
    {
        Vector3 direction = Vector3.zero;

        if (lookAtTarget != null)
        {
            direction = lookAtTarget.position - transform.position;
        }
        else if (_sampledPath != null && _currentPointIndex < _sampledPath.Count)
        {
            direction = _sampledPath[_currentPointIndex].position - transform.position;
        }


        if (direction != Vector3.zero)
        {
            if (lockYRotation) direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void ResetRotationGradually()
    {
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentEuler.y, 0);
    }

    /// <summary>
    /// Handles the execution of path-linked events.
    /// This serves as the primary bridge to external game logic and systems.
    /// </summary>
    /// <param name="eventName">The unique identifier for the event to trigger</param>
    private void ExecuteEvent(string eventName)
    {
        Debug.Log($"<color=yellow>[PathEvent]</color> Trigger Event: {eventName}");

        // 1. Notify external subscribers via Action delegate (Decoupled Architecture)
        OnPathEvent?.Invoke(eventName);

        // 2. Integration with Animator component
        // Example: GetComponent<Animator>()?.SetTrigger(eventName);

        // 3. [OR] Integration with external Managers or custom logic:
        /*
            // A. Global Game State Management
            // GameManager.Instance.NotifyPointReached(eventName);

            // B. Audio & Visual Feedback Systems
            // SoundManager.Instance.PlayEffect(eventName);
            // EffectManager.Instance.SpawnAt(transform.position, eventName);

            // C. Quest & Progression logic (Relevant to RPG frameworks)
            // QuestManager.Instance.CheckMissionObjective(eventName);

            // D. Environment Interaction
            // TriggerObjectInteraction(eventName);

            // E. Broadcasting to other scripts on this GameObject
            // SendMessage(eventName, SendMessageOptions.DontRequireReceiver);
        */
    }
}
