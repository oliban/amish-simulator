using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace AmishSimulator
{
    public class DifficultyUI : MonoBehaviour
    {
        [SerializeField] private Button youngieButton;
        [SerializeField] private Button ordnungButton;
        [SerializeField] private Button gmayButton;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private Button confirmYes;
        [SerializeField] private Button confirmNo;

        private DifficultyLevel _pendingLevel;
        private bool _isMidGame = false;

        public event Action<DifficultyLevel> OnDifficultySelected;

        private void Awake()
        {
            if (youngieButton != null) youngieButton.onClick.AddListener(() => RequestDifficulty(DifficultyLevel.Youngie));
            if (ordnungButton != null) ordnungButton.onClick.AddListener(() => RequestDifficulty(DifficultyLevel.Ordnung));
            if (gmayButton    != null) gmayButton.onClick.AddListener(()    => RequestDifficulty(DifficultyLevel.Gmay));
            if (confirmYes    != null) confirmYes.onClick.AddListener(ApplyDifficulty);
            if (confirmNo     != null) confirmNo.onClick.AddListener(CancelDifficulty);

            if (confirmDialog != null) confirmDialog.SetActive(false);
        }

        private void Start()
        {
            _isMidGame = GameManager.Instance?.CurrentState == GameState.Playing;
            UpdateButtonHighlights();
        }

        private void RequestDifficulty(DifficultyLevel level)
        {
            _pendingLevel = level;
            ShowTooltip(level);

            if (_isMidGame)
            {
                // Show confirmation dialog mid-game
                if (confirmDialog != null) confirmDialog.SetActive(true);
            }
            else
            {
                ApplyDifficulty();
            }
        }

        private void ApplyDifficulty()
        {
            DifficultyManager.Instance?.SetDifficulty(_pendingLevel);
            OnDifficultySelected?.Invoke(_pendingLevel);
            if (confirmDialog != null) confirmDialog.SetActive(false);
            UpdateButtonHighlights();
        }

        private void CancelDifficulty()
        {
            if (confirmDialog != null) confirmDialog.SetActive(false);
        }

        private void ShowTooltip(DifficultyLevel level)
        {
            if (tooltipText == null) return;
            tooltipText.text = level switch
            {
                DifficultyLevel.Youngie => "Youngie — 18 min days, 150 energy, lenient Ordnung",
                DifficultyLevel.Ordnung => "Ordnung — 12 min days, 100 energy, standard rules",
                DifficultyLevel.Gmay    => "Gmay — 8 min days, 75 energy, strict punishments",
                _ => ""
            };
        }

        private void UpdateButtonHighlights()
        {
            if (DifficultyManager.Instance == null) return;
            var current = DifficultyManager.Instance.CurrentLevel;

            SetButtonHighlight(youngieButton, current == DifficultyLevel.Youngie);
            SetButtonHighlight(ordnungButton, current == DifficultyLevel.Ordnung);
            SetButtonHighlight(gmayButton,    current == DifficultyLevel.Gmay);
        }

        private void SetButtonHighlight(Button btn, bool active)
        {
            if (btn == null) return;
            var colors = btn.colors;
            colors.normalColor = active ? new Color(0.8f, 0.7f, 0.4f) : Color.white;
            btn.colors = colors;
        }
    }
}
