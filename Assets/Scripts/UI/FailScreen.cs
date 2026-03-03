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

        private static readonly Color OffWhite = new Color(0.92f, 0.90f, 0.85f);

        private void Awake()
        {
            BuildUI();
            if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgain);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
            Hide();

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameOver += Show;
        }

        private void BuildUI()
        {
            if (failPanel != null) return;

            // Full-screen dark panel
            var panelGo = new GameObject("FailPanel");
            panelGo.transform.SetParent(transform, false);
            var panelRT = panelGo.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.85f);
            failPanel = panelGo;

            // Fail Message — centered upper area
            var msgGo = new GameObject("FailMessageText");
            msgGo.transform.SetParent(panelGo.transform, false);
            failMessageText = msgGo.AddComponent<TextMeshProUGUI>();
            failMessageText.fontSize = 28;
            failMessageText.color = OffWhite;
            failMessageText.alignment = TextAlignmentOptions.Center;
            failMessageText.text = "";
            var msgRT = msgGo.GetComponent<RectTransform>();
            msgRT.anchorMin = new Vector2(0.1f, 0.55f);
            msgRT.anchorMax = new Vector2(0.9f, 0.75f);
            msgRT.offsetMin = Vector2.zero;
            msgRT.offsetMax = Vector2.zero;

            // Stats Text — below message
            var stGo = new GameObject("StatsText");
            stGo.transform.SetParent(panelGo.transform, false);
            statsText = stGo.AddComponent<TextMeshProUGUI>();
            statsText.fontSize = 18;
            statsText.color = OffWhite;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.text = "";
            var stRT = stGo.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.15f, 0.35f);
            stRT.anchorMax = new Vector2(0.85f, 0.55f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;

            // Try Again Button
            tryAgainButton = CreateButton(panelGo.transform, "TryAgainButton", "Try Again",
                new Vector2(0.3f, 0.15f), new Vector2(0.48f, 0.25f));

            // Main Menu Button
            mainMenuButton = CreateButton(panelGo.transform, "MainMenuButton", "Main Menu",
                new Vector2(0.52f, 0.15f), new Vector2(0.7f, 0.25f));
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
