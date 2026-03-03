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

        private static readonly Color OffWhite = new Color(0.92f, 0.90f, 0.85f);

        private void Awake()
        {
            BuildUI();
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
            if (shareButton    != null) shareButton.onClick.AddListener(OnShare);
            Hide();

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameWin += OnNaturalDeath;
        }

        private void BuildUI()
        {
            if (scorePanel != null) return;

            // Full-screen dark panel
            var panelGo = new GameObject("ScorePanel");
            panelGo.transform.SetParent(transform, false);
            var panelRT = panelGo.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.85f);
            scorePanel = panelGo;

            // Title — "A Life Well Lived"
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(panelGo.transform, false);
            titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.fontSize = 36;
            titleText.color = OffWhite;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = "A Life Well Lived";
            var titleRT = titleGo.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.80f);
            titleRT.anchorMax = new Vector2(0.9f, 0.92f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Score — large number
            var scoreGo = new GameObject("ScoreText");
            scoreGo.transform.SetParent(panelGo.transform, false);
            scoreText = scoreGo.AddComponent<TextMeshProUGUI>();
            scoreText.fontSize = 30;
            scoreText.color = OffWhite;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.text = "";
            var scoreRT = scoreGo.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.1f, 0.70f);
            scoreRT.anchorMax = new Vector2(0.9f, 0.80f);
            scoreRT.offsetMin = Vector2.zero;
            scoreRT.offsetMax = Vector2.zero;

            // Stats Text — center
            var stGo = new GameObject("StatsText");
            stGo.transform.SetParent(panelGo.transform, false);
            statsText = stGo.AddComponent<TextMeshProUGUI>();
            statsText.fontSize = 18;
            statsText.color = OffWhite;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.text = "";
            var stRT = stGo.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.15f, 0.35f);
            stRT.anchorMax = new Vector2(0.85f, 0.68f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;

            // Flavor Text — below stats
            var flavorGo = new GameObject("FlavorText");
            flavorGo.transform.SetParent(panelGo.transform, false);
            flavorText = flavorGo.AddComponent<TextMeshProUGUI>();
            flavorText.fontSize = 16;
            flavorText.color = new Color(0.75f, 0.72f, 0.65f);
            flavorText.fontStyle = FontStyles.Italic;
            flavorText.alignment = TextAlignmentOptions.Center;
            flavorText.text = "";
            var flavorRT = flavorGo.GetComponent<RectTransform>();
            flavorRT.anchorMin = new Vector2(0.15f, 0.26f);
            flavorRT.anchorMax = new Vector2(0.85f, 0.35f);
            flavorRT.offsetMin = Vector2.zero;
            flavorRT.offsetMax = Vector2.zero;

            // Main Menu Button
            mainMenuButton = CreateButton(panelGo.transform, "MainMenuButton", "Main Menu",
                new Vector2(0.25f, 0.10f), new Vector2(0.48f, 0.20f));

            // Share Button
            shareButton = CreateButton(panelGo.transform, "ShareButton", "Copy Score",
                new Vector2(0.52f, 0.10f), new Vector2(0.75f, 0.20f));
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var btnRT = btnGo.AddComponent<RectTransform>();
            btnRT.anchorMin = anchorMin;
            btnRT.anchorMax = anchorMax;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.25f, 0.22f, 0.18f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRT = txtGo.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.color = OffWhite;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
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
