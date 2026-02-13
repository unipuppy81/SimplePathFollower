using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public PathCreator pathCreator;
    public Transform lookAtTarget; // 바라볼 특정 오브젝트
    public bool isLoop = false;

    private List<PathData> _path;
    private int _currentPointIndex = 0;
    private bool _isAutoMoving = true;

    private void Awake()
    {
        pathCreator = GetComponent<PathCreator>();
    }

    private void Start()
    {
        if (pathCreator != null)
        {
            _path = pathCreator.GetPathData();
            if (_path.Count > 0)
            {
                transform.position = _path[0].position; // 시작 위치로 강제 이동
            }
        }
    }

    private void Update()
    {
        if (_path == null || _path.Count == 0 || !_isAutoMoving) return;

        // 1. 이동 로직
        PathData currentTarget = _path[_currentPointIndex];
        float currentSpeed = currentTarget.speed;
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, currentSpeed * Time.deltaTime);

        // 2. 시선 처리
        if (lookAtTarget != null)
        {
            transform.LookAt(lookAtTarget);
        }

        // 3. 다음 점으로 갱신
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.01f)
        {
            _currentPointIndex++;

            // 경로 끝에 도달하면 처음으로 (Loop)
            if (_currentPointIndex >= _path.Count)
            {
                if (isLoop)
                {
                    _currentPointIndex = 0; // 처음으로 되돌림
                }
                else
                {
                    _currentPointIndex = _path.Count - 1;
                    _isAutoMoving = false; // 이동 중지
                }
            }
        }
    }

    public void ResetFollower()
    {
        _currentPointIndex = 0;
        _isAutoMoving = true;
        if (_path != null && _path.Count > 0) transform.position = _path[0].position;
    }
}
