using UnityEngine;
using System;

namespace AmishSimulator
{
    public class Plowing : ChoreBase
    {
        [SerializeField] private int fieldWidth = 8;
        [SerializeField] private int fieldHeight = 6;
        [SerializeField] private float baseTimeLimit = 180f; // seconds

        private bool[,] _rowsPlowed;
        private bool[,] _obstacles;
        private Vector2Int _currentPosition;
        private int _totalCells;
        private int _plowedCells;
        private float _timeRemaining;

        public event Action<Vector2Int> OnPositionChanged;
        public event Action<float> OnTimerTick;

        private void Start()
        {
            ApplyDifficultyTimeLimit();
        }

        private void ApplyDifficultyTimeLimit()
        {
            if (DifficultyManager.Instance == null) return;
            baseTimeLimit = DifficultyManager.Instance.CurrentLevel switch
            {
                DifficultyLevel.Youngie => 240f,
                DifficultyLevel.Ordnung => 180f,
                DifficultyLevel.Gmay   => 120f,
                _ => 180f
            };
        }

        public override void StartChore()
        {
            base.StartChore();
            if (!isActive) return;

            ApplyDifficultyTimeLimit();
            InitializeField();
        }

        private void InitializeField()
        {
            _rowsPlowed = new bool[fieldWidth, fieldHeight];
            _obstacles  = new bool[fieldWidth, fieldHeight];
            _totalCells = fieldWidth * fieldHeight;
            _plowedCells = 0;
            _currentPosition = Vector2Int.zero;
            _timeRemaining = baseTimeLimit;

            // Randomly place obstacles (~15% of cells)
            var rng = new System.Random();
            for (int x = 0; x < fieldWidth; x++)
                for (int y = 0; y < fieldHeight; y++)
                    if (x > 0 && y > 0 && rng.NextDouble() < 0.15) // never block start
                        _obstacles[x, y] = true;
        }

        private void Update()
        {
            if (!isActive) return;

            _timeRemaining -= Time.deltaTime;
            OnTimerTick?.Invoke(_timeRemaining);
            ReportProgress(GetFieldCompletionPercent());

            if (_timeRemaining <= 0f)
            {
                float completion = GetFieldCompletionPercent();
                CompleteChore(completion >= 0.8f);
            }
        }

        public bool MoveHorse(Vector2Int direction)
        {
            if (!isActive) return false;

            var newPos = _currentPosition + direction;

            // Bounds check
            if (newPos.x < 0 || newPos.x >= fieldWidth || newPos.y < 0 || newPos.y >= fieldHeight)
                return false;

            _currentPosition = newPos;
            OnPositionChanged?.Invoke(_currentPosition);

            // Mark cell as plowed
            if (!_rowsPlowed[newPos.x, newPos.y])
            {
                _rowsPlowed[newPos.x, newPos.y] = true;
                _plowedCells++;

                // Obstacle costs extra energy
                int extraCost = _obstacles[newPos.x, newPos.y] ? 2 : 0;
                if (extraCost > 0)
                    EnergySystem.Instance?.ConsumeEnergy(extraCost);

                // Energy cost per cell
                EnergySystem.Instance?.ConsumeEnergy(2);
            }

            return true;
        }

        public bool IsCellObstacle(Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= fieldWidth || pos.y < 0 || pos.y >= fieldHeight) return false;
            return _obstacles[pos.x, pos.y];
        }

        public float GetFieldCompletionPercent()
        {
            if (_totalCells == 0) return 0f;
            return (float)_plowedCells / _totalCells;
        }

        public Vector2Int GetCurrentPosition() => _currentPosition;
        public float GetTimeRemaining() => _timeRemaining;
        public int GetPlowedCells() => _plowedCells;
        public int GetTotalCells() => _totalCells;

        // For testing
        public void SetField(int width, int height) { fieldWidth = width; fieldHeight = height; }
        public void SetTimeRemaining(float t) => _timeRemaining = t;
        public void ForceInitializeField() => InitializeField();

        protected override void OnChoreSuccess()
        {
            // Field is ready for planting
            Debug.Log("The field is plowed. Ein guter Mann pflügt geraden Furchen.");
            if (GameManager.Instance?.GameStats != null)
                GameManager.Instance.GameStats.AddAcre();
        }

        protected override void OnChoreFail()
        {
            Debug.Log("The sun sets on an unfinished field. Tomorrow, try again.");
        }
    }
}
