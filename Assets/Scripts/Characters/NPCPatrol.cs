using UnityEngine;

namespace AmishSimulator
{
    public class NPCPatrol : MonoBehaviour
    {
        [SerializeField] private Vector3[] waypoints;
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float waitTime = 2f;

        private int _currentWaypointIndex;
        private float _waitTimer;
        private bool _isWaiting;
        private Vector3 _startPosition;

        private void Awake()
        {
            _startPosition = transform.position;
            if (waypoints == null || waypoints.Length == 0)
                SetDefaultWaypoints();
        }

        private void Reset()
        {
            SetDefaultWaypoints();
        }

        private void SetDefaultWaypoints()
        {
            Vector3 origin = Application.isPlaying ? _startPosition : transform.position;
            waypoints = new Vector3[]
            {
                origin + new Vector3(3f, 0f, 3f),
                origin + new Vector3(-3f, 0f, 3f),
                origin + new Vector3(-3f, 0f, -3f),
                origin + new Vector3(3f, 0f, -3f)
            };
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                    _isWaiting = false;
                return;
            }

            Vector3 target = waypoints[_currentWaypointIndex];
            Vector3 direction = target - transform.position;
            direction.y = 0f;

            if (direction.magnitude <= 0.3f)
            {
                _isWaiting = true;
                _waitTimer = waitTime;
                _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
                return;
            }

            Vector3 moveDir = direction.normalized;
            transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

            if (moveDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDir);
        }
    }
}
