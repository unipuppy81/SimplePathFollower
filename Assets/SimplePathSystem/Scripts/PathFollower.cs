using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Transform lookAtTarget;
    public bool isLoop = false;

    #region Runtime State

    private List<PathPoint> _activePoints;
    private int _currentPointIndex = 0;
    private bool _isAutoMoving = true;
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

        _activePoints = new List<PathPoint>(pathData.points);
        _currentPointIndex = 0;
        _isAutoMoving = true;

        transform.position = _activePoints[0].position;
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
        if (!_isAutoMoving || _activePoints == null || _activePoints.Count == 0) return;

        MoveStep();
        HandleRotation();
    }

    /// <summary>
    /// Move Logic
    /// </summary>
    private void MoveStep()
    {
        PathPoint targetPoint = _activePoints[_currentPointIndex];
        float speed = targetPoint.speed;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // Check arrival at current target
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.001f)
        {
            _currentPointIndex++;

            if (_currentPointIndex >= _activePoints.Count)
            {
                if (isLoop)
                {
                    _currentPointIndex = 0;
                }
                else
                {
                    _isAutoMoving = false;
                }
            }
        }
    }

    /// <summary>
    /// Rotation Logic
    /// </summary>
    private void HandleRotation()
    {
        if (lookAtTarget != null)
        {
            transform.LookAt(lookAtTarget);
        }
        else if (_activePoints != null && _currentPointIndex < _activePoints.Count)
        {
            Vector3 direction = _activePoints[_currentPointIndex].position - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
