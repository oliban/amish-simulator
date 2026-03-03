using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmishSimulator
{
    /// <summary>
    /// Self-building UI overlay for the plowing grid mini-game.
    /// Creates an 8x6 grid of cell images plus timer and progress display.
    /// </summary>
    public class PlowingUI : MonoBehaviour
    {
        private Plowing _chore;

        // Root panel
        private GameObject _panel;

        // Grid
        private Image[,] _cellImages;
        private int _gridW = 8;
        private int _gridH = 6;
        private const float CellSize = 40f;

        // Info
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _progressText;
        private TextMeshProUGUI _resultText;

        private static readonly Color UnplowedColor = new Color(0.45f, 0.35f, 0.2f);
        private static readonly Color PlowedColor = new Color(0.3f, 0.55f, 0.2f);
        private static readonly Color ObstacleColor = new Color(0.5f, 0.4f, 0.35f);
        private static readonly Color PlayerColor = new Color(1f, 0.9f, 0.3f);
        private static readonly Color PanelBgColor = new Color(0.05f, 0.05f, 0.05f, 0.85f);
        private static readonly Color CellBorderColor = new Color(0.25f, 0.2f, 0.15f);

        private Vector2Int _lastPlayerPos = new Vector2Int(-1, -1);

        public void Show(Plowing chore)
        {
            _chore = chore;

            if (_panel == null)
                BuildUI();

            _panel.SetActive(true);
            ResetGrid();

            _chore.OnPositionChanged += HandlePositionChanged;
            _chore.OnTimerTick += HandleTimerTick;
            _chore.OnChoreCompleted += HandleChoreCompleted;

            // Show initial position
            HandlePositionChanged(_chore.GetCurrentPosition());
        }

        public void Hide()
        {
            if (_chore != null)
            {
                _chore.OnPositionChanged -= HandlePositionChanged;
                _chore.OnTimerTick -= HandleTimerTick;
                _chore.OnChoreCompleted -= HandleChoreCompleted;
                _chore = null;
            }

            if (_panel != null)
                _panel.SetActive(false);
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf || _chore == null) return;

            // Directional input
            Vector2Int dir = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                dir = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                dir = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                dir = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                dir = Vector2Int.right;

            if (dir != Vector2Int.zero)
            {
                _chore.MoveHorse(dir);
            }

            // Escape to cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _chore.EndChore();
                Hide();
                return;
            }

            // Update progress text
            if (_progressText != null)
            {
                float pct = _chore.GetFieldCompletionPercent() * 100f;
                _progressText.text = $"Plowed: {pct:F0}%  ({_chore.GetPlowedCells()}/{_chore.GetTotalCells()})";
            }
        }

        private void HandlePositionChanged(Vector2Int pos)
        {
            if (_cellImages == null) return;

            // Restore previous cell color
            if (_lastPlayerPos.x >= 0 && _lastPlayerPos.x < _gridW &&
                _lastPlayerPos.y >= 0 && _lastPlayerPos.y < _gridH)
            {
                _cellImages[_lastPlayerPos.x, _lastPlayerPos.y].color = PlowedColor;
            }

            // Color current cell as player
            if (pos.x >= 0 && pos.x < _gridW && pos.y >= 0 && pos.y < _gridH)
            {
                _cellImages[pos.x, pos.y].color = PlayerColor;
            }

            _lastPlayerPos = pos;
        }

        private void HandleTimerTick(float timeRemaining)
        {
            if (_timerText == null) return;

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            _timerText.text = $"Time: {minutes}:{seconds:D2}";

            if (timeRemaining < 30f)
                _timerText.color = new Color(0.9f, 0.3f, 0.3f);
            else
                _timerText.color = Color.white;
        }

        private void HandleChoreCompleted(ChoreType type, bool success)
        {
            if (_resultText != null)
            {
                _resultText.text = success ? "FIELD PLOWED!" : "TIME'S UP!";
                _resultText.color = success
                    ? new Color(0.2f, 0.8f, 0.2f)
                    : new Color(0.8f, 0.2f, 0.2f);
                _resultText.gameObject.SetActive(true);
            }

            Invoke(nameof(Hide), 1.5f);
        }

        private void ResetGrid()
        {
            if (_cellImages == null || _chore == null) return;

            _lastPlayerPos = new Vector2Int(-1, -1);

            for (int x = 0; x < _gridW; x++)
            {
                for (int y = 0; y < _gridH; y++)
                {
                    if (_chore.IsCellObstacle(new Vector2Int(x, y)))
                        _cellImages[x, y].color = ObstacleColor;
                    else
                        _cellImages[x, y].color = UnplowedColor;
                }
            }

            if (_resultText != null)
                _resultText.gameObject.SetActive(false);
        }

        private void BuildUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("PlowingUI: No Canvas found in scene.");
                return;
            }

            // Full-screen dark panel
            _panel = new GameObject("PlowingPanel");
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
                "PLOWING THE FIELD", 32, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 170), new Vector2(400, 50));

            // Instructions
            var instrText = CreateText(_panel.transform, "InstructionText",
                "WASD / Arrows to move the plow", 18, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 140), new Vector2(400, 30));
            instrText.color = new Color(0.8f, 0.8f, 0.7f);

            // Grid container (centered)
            float gridPixelW = _gridW * CellSize;
            float gridPixelH = _gridH * CellSize;

            var gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(_panel.transform, false);
            var gridRT = gridGO.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.5f, 0.5f);
            gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.anchoredPosition = new Vector2(0, 0);
            gridRT.sizeDelta = new Vector2(gridPixelW, gridPixelH);
            gridRT.pivot = new Vector2(0.5f, 0.5f);

            // Create cells
            _cellImages = new Image[_gridW, _gridH];
            for (int x = 0; x < _gridW; x++)
            {
                for (int y = 0; y < _gridH; y++)
                {
                    var cellGO = new GameObject($"Cell_{x}_{y}");
                    cellGO.transform.SetParent(gridGO.transform, false);
                    var cellRT = cellGO.AddComponent<RectTransform>();

                    // Position: bottom-left origin, y=0 is bottom row
                    float px = (x * CellSize) - (gridPixelW * 0.5f) + (CellSize * 0.5f);
                    float py = (y * CellSize) - (gridPixelH * 0.5f) + (CellSize * 0.5f);
                    cellRT.anchorMin = new Vector2(0.5f, 0.5f);
                    cellRT.anchorMax = new Vector2(0.5f, 0.5f);
                    cellRT.anchoredPosition = new Vector2(px, py);
                    cellRT.sizeDelta = new Vector2(CellSize - 2f, CellSize - 2f); // 1px border gap

                    var cellImg = cellGO.AddComponent<Image>();
                    cellImg.color = UnplowedColor;
                    _cellImages[x, y] = cellImg;
                }
            }

            // Timer text (above grid)
            _timerText = CreateText(_panel.transform, "TimerText",
                "Time: --:--", 22, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -gridPixelH * 0.5f - 30f), new Vector2(200, 30));

            // Progress text (below timer)
            _progressText = CreateText(_panel.transform, "ProgressText",
                "Plowed: 0%", 18, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -gridPixelH * 0.5f - 60f), new Vector2(300, 24));

            // Result text (hidden until chore ends)
            _resultText = CreateText(_panel.transform, "ResultText",
                "", 36, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -gridPixelH * 0.5f - 100f), new Vector2(400, 50));
            _resultText.gameObject.SetActive(false);

            // Escape hint
            var escText = CreateText(_panel.transform, "EscHint",
                "[ESC] Cancel", 14, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -gridPixelH * 0.5f - 130f), new Vector2(200, 24));
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
