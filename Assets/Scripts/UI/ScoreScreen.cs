using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace AmishSimulator
{
    public class ScoreScreen : MonoBehaviour
    {
        [SerializeField] private GameObject scorePanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI flavorText;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button shareButton;

        private void Awake()
        {
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
            if (shareButton    != null) shareButton.onClick.AddListener(OnShare);
            Hide();

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameWin += OnNaturalDeath;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameWin -= OnNaturalDeath;
        }

        private void OnNaturalDeath()
        {
            Show(GameManager.Instance?.GameStats ?? new GameStats());
        }

        public void Show(GameStats stats)
        {
            if (scorePanel != null) scorePanel.SetActive(true);

            if (titleText != null)
                titleText.text = "A Life Well Lived";

            if (statsText != null)
                statsText.text = BuildStatsText(stats);

            int score = stats.CalculateScore();
            if (scoreText != null)
                scoreText.text = $"Score: {score:N0}";

            if (flavorText != null)
                flavorText.text = $"You have churned {stats.ButterChurned:F0} pounds of butter.\nWeird Al would be proud.";
        }

        public void Hide()
        {
            if (scorePanel != null) scorePanel.SetActive(false);
        }

        private string BuildStatsText(GameStats stats)
        {
            return $"Final Age: {stats.Age}\n" +
                   $"Farm Size: {stats.AcresPlowed} acres\n" +
                   $"Family Size: {stats.ChildrenCount} children\n" +
                   $"Community Reputation: {stats.AverageAffinity:F0}/100\n" +
                   $"Butter Churned: {stats.ButterChurned:F0} lbs\n" +
                   $"Beard Length: {stats.BeardLengthInches:F1} inches\n" +
                   $"Years of Service: {stats.YearsServed}";
        }

        public int CalculateScore(GameStats stats) => stats.CalculateScore();

        private void OnMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void OnShare()
        {
            // WebGL: copy score text to clipboard
            if (GameManager.Instance?.GameStats != null)
            {
                var stats = GameManager.Instance.GameStats;
                string shareText = $"I lived to age {stats.Age} in Amish Simulator! " +
                                   $"Churned {stats.ButterChurned:F0}lbs of butter. " +
                                   $"Score: {stats.CalculateScore():N0}";
                GUIUtility.systemCopyBuffer = shareText;
            }
        }
    }
}
