using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmishSimulator
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu Panel")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button creditsButton;

        [Header("Gender Selection Panel")]
        [SerializeField] private GameObject genderPanel;
        [SerializeField] private Button maleButton;
        [SerializeField] private Button femaleButton;
        [SerializeField] private Button genderBackButton;

        [Header("Loading Tip")]
        [SerializeField] private TextMeshProUGUI loadingTipText;

        private static readonly string[] LoadingTips =
        {
            "Ein guter Mann pflügt geraden Furchen.\n(A good man plows straight furrows.)",
            "Fleißige Hände sind Gottes Werkzeug.\n(Diligent hands are God's instrument.)",
            "Der frühe Vogel ist auch von Gott aufgeweckt worden.\n(The early bird was also woken by God.)",
            "Butter churns itself for no man.",
            "A straight furrow honors the community.",
            "The beard does not grow on a lazy chin.",
            "Bishop Yoder sees all. Even the diagonal walkers.",
        };

        private void Awake()
        {
            if (newGameButton  != null) newGameButton.onClick.AddListener(ShowGenderSelection);
            if (creditsButton  != null) creditsButton.onClick.AddListener(ShowCredits);
            if (maleButton     != null) maleButton.onClick.AddListener(() => StartGame(Gender.Male));
            if (femaleButton   != null) femaleButton.onClick.AddListener(() => StartGame(Gender.Female));
            if (genderBackButton != null) genderBackButton.onClick.AddListener(ShowMainMenu);
        }

        private void Start()
        {
            ShowMainMenu();
            ShowRandomTip();
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (genderPanel   != null) genderPanel.SetActive(false);
        }

        private void ShowGenderSelection()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (genderPanel   != null) genderPanel.SetActive(true);
        }

        private void StartGame(Gender gender)
        {
            GameManager.Instance?.StartGame(gender);
        }

        private void ShowCredits()
        {
            Debug.Log("Inspired by Stardew Valley, Zelda, and Weird Al's 'Amish Paradise'.");
        }

        private void ShowRandomTip()
        {
            if (loadingTipText != null)
                loadingTipText.text = LoadingTips[Random.Range(0, LoadingTips.Length)];
        }
    }
}
