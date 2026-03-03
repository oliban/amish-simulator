using UnityEngine;

namespace AmishSimulator
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float refreshInterval = 2f;

        private MonoBehaviour[] _allBehaviours;
        private float _refreshTimer;
        private HUD _hud;
        private IInteractable _currentNearest;

        private void Start()
        {
            _hud = FindFirstObjectByType<HUD>();
            RefreshInteractables();
        }

        private void Update()
        {
            _refreshTimer += Time.deltaTime;
            if (_refreshTimer >= refreshInterval)
            {
                _refreshTimer = 0f;
                RefreshInteractables();
            }

            IInteractable nearest = FindNearest();

            if (nearest != null)
            {
                if (nearest != _currentNearest)
                {
                    _currentNearest = nearest;
                    if (_hud != null) _hud.SetInteractionPrompt(_currentNearest.InteractionLabel);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    nearest.Interact();
                }
            }
            else if (_currentNearest != null)
            {
                _currentNearest = null;
                if (_hud != null) _hud.ClearInteractionPrompt();
            }
        }

        private void RefreshInteractables()
        {
            _allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        }

        private IInteractable FindNearest()
        {
            if (_allBehaviours == null) return null;

            IInteractable nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 playerPos = transform.position;

            for (int i = 0; i < _allBehaviours.Length; i++)
            {
                MonoBehaviour mb = _allBehaviours[i];
                if (mb == null) continue;

                IInteractable interactable = mb as IInteractable;
                if (interactable == null) continue;

                float dist = Vector3.Distance(playerPos, mb.transform.position);
                if (dist <= interactable.InteractionRadius && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = interactable;
                }
            }

            return nearest;
        }
    }
}
