using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmishSimulator
{
    /// <summary>
    /// Self-building UI overlay for the butter churning rhythm mini-game.
    /// Creates its own Canvas children programmatically.
    /// </summary>
    public class ButterChurnUI : MonoBehaviour
    {
        private ButterChurning _chore;

        // Root panel
        private GameObject _panel;

        // UI elements
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _instructionText;
        private TextMeshProUGUI _beatResultText;
        private TextMeshProUGUI _progressLabel;
        private RectTransform _progressBarBg;
        private RectTransform _progressBarFill;
        private Image _beatIndicator;
        private TextMeshProUGUI _missCountText;

        private float _beatResultTimer;

        private static readonly Color HitColor = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color MissColor = new Color(0.8f, 0.2f, 0.2f);
        private static readonly Color BeatActiveColor = new Color(1f, 0.85f, 0.3f);
        private static readonly Color BeatIdleColor = new Color(0.4f, 0.4f, 0.4f);
        private static readonly Color BarBgColor = new Color(0.2f, 0.15f, 0.1f);
        private static readonly Color BarFillColor = new Color(0.95f, 0.85f, 0.4f);
        private static readonly Color PanelBgColor = new Color(0.05f, 0.05f, 0.05f, 0.85f);

        public void Show(ButterChurning chore)
        {
            _chore = chore;

            if (_panel == null)
                BuildUI();

            _panel.SetActive(true);
            UpdateProgressBar(0f);

            _chore.OnBeatResult += HandleBeatResult;
            _chore.OnChoreProgress += UpdateProgressBar;
            _chore.OnChoreCompleted += HandleChoreCompleted;
        }

        public void Hide()
        {
            if (_chore != null)
            {
                _chore.OnBeatResult -= HandleBeatResult;
                _chore.OnChoreProgress -= UpdateProgressBar;
                _chore.OnChoreCompleted -= HandleChoreCompleted;
                _chore = null;
            }

            if (_panel != null)
                _panel.SetActive(false);
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf || _chore == null) return;

            // Handle player input (Space or left mouse)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                _chore.OnPlayerInput();
            }

            // Escape to cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _chore.EndChore();
                Hide();
                return;
            }

            // Beat indicator color
            if (_beatIndicator != null)
            {
                _beatIndicator.color = _chore.IsBeatWindowOpen() ? BeatActiveColor : BeatIdleColor;
            }

            // Miss counter
            if (_missCountText != null)
            {
                _missCountText.text = $"Missed: {_chore.GetMissedBeats()}";
            }

            // Beat result fade
            if (_beatResultTimer > 0f)
            {
                _beatResultTimer -= Time.deltaTime;
                if (_beatResultTimer <= 0f && _beatResultText != null)
                    _beatResultText.text = "";
            }
        }

        private void HandleBeatResult(string result)
        {
            if (_beatResultText == null) return;

            if (result == "hit")
            {
                _beatResultText.text = "HIT!";
                _beatResultText.color = HitColor;
            }
            else
            {
                _beatResultText.text = "MISS";
                _beatResultText.color = MissColor;
            }
            _beatResultTimer = 0.6f;
        }

        private void UpdateProgressBar(float progress)
        {
            if (_progressBarFill == null || _progressBarBg == null) return;

            float maxWidth = _progressBarBg.sizeDelta.x;
            var sd = _progressBarFill.sizeDelta;
            sd.x = maxWidth * Mathf.Clamp01(progress);
            _progressBarFill.sizeDelta = sd;

            if (_progressLabel != null)
                _progressLabel.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        private void HandleChoreCompleted(ChoreType type, bool success)
        {
            if (_beatResultText != null)
            {
                _beatResultText.text = success ? "BUTTER MADE!" : "SPILLED!";
                _beatResultText.color = success ? HitColor : MissColor;
                _beatResultText.fontSize = 36;
            }

            // Auto-hide after a short delay
            Invoke(nameof(Hide), 1.5f);
        }

        private void BuildUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("ButterChurnUI: No Canvas found in scene.");
                return;
            }

            // Full-screen dark panel
            _panel = new GameObject("ButterChurnPanel");
            _panel.transform.SetParent(canvas.transform, false);
            var panelRT = _panel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = PanelBgColor;

            // Title
            _titleText = CreateText(_panel.transform, "TitleText",
                "BUTTER CHURNING", 32, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 140), new Vector2(400, 50));

            // Instructions
            _instructionText = CreateText(_panel.transform, "InstructionText",
                "Press SPACE on the beat!", 20, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 100), new Vector2(400, 30));
            _instructionText.color = new Color(0.8f, 0.8f, 0.7f);

            // Beat indicator (large circle)
            var beatGO = new GameObject("BeatIndicator");
            beatGO.transform.SetParent(_panel.transform, false);
            var beatRT = beatGO.AddComponent<RectTransform>();
            beatRT.anchorMin = new Vector2(0.5f, 0.5f);
            beatRT.anchorMax = new Vector2(0.5f, 0.5f);
            beatRT.anchoredPosition = new Vector2(0, 30);
            beatRT.sizeDelta = new Vector2(80, 80);
            _beatIndicator = beatGO.AddComponent<Image>();
            _beatIndicator.color = BeatIdleColor;

            // Beat result text (HIT/MISS)
            _beatResultText = CreateText(_panel.transform, "BeatResultText",
                "", 28, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(200, 40));

            // Progress bar background
            var barBgGO = new GameObject("ProgressBarBg");
            barBgGO.transform.SetParent(_panel.transform, false);
            _progressBarBg = barBgGO.AddComponent<RectTransform>();
            _progressBarBg.anchorMin = new Vector2(0.5f, 0.5f);
            _progressBarBg.anchorMax = new Vector2(0.5f, 0.5f);
            _progressBarBg.anchoredPosition = new Vector2(0, -80);
            _progressBarBg.sizeDelta = new Vector2(300, 24);
            var barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = BarBgColor;

            // Progress bar fill (child, left-anchored)
            var barFillGO = new GameObject("ProgressBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            _progressBarFill = barFillGO.AddComponent<RectTransform>();
            _progressBarFill.anchorMin = new Vector2(0f, 0f);
            _progressBarFill.anchorMax = new Vector2(0f, 1f);
            _progressBarFill.pivot = new Vector2(0f, 0.5f);
            _progressBarFill.anchoredPosition = Vector2.zero;
            _progressBarFill.sizeDelta = new Vector2(0, 0);
            var barFillImg = barFillGO.AddComponent<Image>();
            barFillImg.color = BarFillColor;

            // Progress label
            _progressLabel = CreateText(_panel.transform, "ProgressLabel",
                "0%", 18, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -110), new Vector2(100, 24));

            // Miss counter
            _missCountText = CreateText(_panel.transform, "MissCount",
                "Missed: 0", 18, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -140), new Vector2(200, 24));
            _missCountText.color = new Color(0.8f, 0.5f, 0.5f);

            // Escape hint
            var escText = CreateText(_panel.transform, "EscHint",
                "[ESC] Cancel", 14, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -180), new Vector2(200, 24));
            escText.color = new Color(0.5f, 0.5f, 0.5f);

            _panel.SetActive(false);
        }

        private TextMeshProUGUI CreateText(Transform parent, string name,
            string text, float fontSize, TextAlignmentOptions alignment,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return tmp;
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}
