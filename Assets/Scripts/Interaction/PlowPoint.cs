using UnityEngine;

namespace AmishSimulator
{
    /// <summary>
    /// World object for plowing. Player presses E to start the
    /// plowing grid mini-game.
    /// </summary>
    public class PlowPoint : MonoBehaviour, IInteractable
    {
        public string InteractionLabel => "Press E to plow the field";
        public float InteractionRadius => 3f;

        private Plowing _chore;
        private PlowingUI _ui;

        private void Awake()
        {
            _chore = GetComponent<Plowing>();
            if (_chore == null)
                _chore = gameObject.AddComponent<Plowing>();
        }

        public void Interact()
        {
            if (_chore.IsActive) return;

            // Find or create the UI
            if (_ui == null)
            {
                _ui = FindFirstObjectByType<PlowingUI>();
                if (_ui == null)
                {
                    var uiGO = new GameObject("PlowingUI");
                    _ui = uiGO.AddComponent<PlowingUI>();
                }
            }

            _chore.StartChore();

            if (_chore.IsActive)
                _ui.Show(_chore);
        }
    }
}
