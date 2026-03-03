using UnityEngine;
using UnityEngine.Events;

namespace AmishSimulator
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent gameEvent;
        public UnityEvent Response;

        private void OnEnable() => gameEvent?.RegisterListener(this);
        private void OnDisable() => gameEvent?.UnregisterListener(this);

        public void OnEventRaised() => Response?.Invoke();
    }
}
