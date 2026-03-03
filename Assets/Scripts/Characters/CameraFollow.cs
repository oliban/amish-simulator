using UnityEngine;

namespace AmishSimulator
{
    /// <summary>Isometric-style camera that follows the player.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0, 8, -12);
        [SerializeField] private float smoothSpeed = 8f;

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null)
                    target = PlayerController.Instance.transform;
                return;
            }

            var desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }
}
