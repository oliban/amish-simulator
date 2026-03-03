using UnityEngine;

namespace AmishSimulator
{
    /// <summary>
    /// World object for the butter churn. Player presses E to start the
    /// butter churning rhythm mini-game.
    /// </summary>
    public class ChurnStation : MonoBehaviour, IInteractable
    {
        public string InteractionLabel => "Press E to churn butter";
        public float InteractionRadius => 2.5f;

        private ButterChurning _chore;
        private ButterChurnUI _ui;

        private void Awake()
        {
            _chore = GetComponent<ButterChurning>();
            if (_chore == null)
                _chore = gameObject.AddComponent<ButterChurning>();
        }

        public void Interact()
        {
            if (_chore.IsActive) return;

            // Find or create the UI
            if (_ui == null)
            {
                _ui = FindFirstObjectByType<ButterChurnUI>();
                if (_ui == null)
                {
                    var uiGO = new GameObject("ButterChurnUI");
                    _ui = uiGO.AddComponent<ButterChurnUI>();
                }
            }

            _chore.StartChore();

            if (_chore.IsActive)
                _ui.Show(_chore);
        }
    }
}
