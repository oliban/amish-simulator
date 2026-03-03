using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace AmishSimulator
{
    public class FailScreen : MonoBehaviour
    {
        [SerializeField] private GameObject failPanel;
        [SerializeField] private TextMeshProUGUI failMessageText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgain);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
            Hide();

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameOver += Show;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameOver -= Show;
        }

        public void Show(FailReason reason)
        {
            if (failPanel != null) failPanel.SetActive(true);

            if (failMessageText != null)
                failMessageText.text = GetFailMessage(reason);

            if (statsText != null && GameManager.Instance?.GameStats != null)
                statsText.text = BuildStatsText(GameManager.Instance.GameStats);
        }

        public void Hide()
        {
            if (failPanel != null) failPanel.SetActive(false);
        }

        private string GetFailMessage(FailReason reason) => reason switch
        {
            FailReason.Starvation   => "You have perished of hunger.\nA full belly honors the Lord.",
            FailReason.SpouseDeath  => "Your beloved has passed.\nThe farm stands silent.",
            FailReason.ChildDeath   => "A child has been taken.\nMay they rest in God's grace.",
            _ => "Your time on this earth has ended."
        };

        private string BuildStatsText(GameStats stats)
        {
            return $"Days Survived: {stats.DaysLived}\n" +
                   $"Butter Churned: {stats.ButterChurned:F0} lbs\n" +
                   $"Beard Length: {stats.BeardLengthInches:F1} inches\n" +
                   $"Acres Plowed: {stats.AcresPlowed}";
        }

        private void OnTryAgain()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
