using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmishSimulator
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private DifficultyUI difficultyUI;

        private void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (quitButton   != null) quitButton.onClick.AddListener(OnQuitClicked);
            Hide();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                Toggle();
#else
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
                Toggle();
#endif
        }

        private void Toggle()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState == GameState.Playing) Show();
            else if (GameManager.Instance.CurrentState == GameState.Paused) Hide();
        }

        public void Show()
        {
            GameManager.Instance?.PauseGame();
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Hide()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
            Hide();
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
