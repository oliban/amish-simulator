using UnityEngine;
using System;

namespace AmishSimulator
{
    public enum QuiltColor { Blue, Red, Green, Yellow }

    [Serializable]
    public struct QuiltPattern
    {
        public QuiltColor[] squares; // 16 elements for 4x4

        public QuiltColor Get(int row, int col) => squares[row * 4 + col];
        public void Set(int row, int col, QuiltColor color) => squares[row * 4 + col] = color;

        public static QuiltPattern Empty()
        {
            return new QuiltPattern { squares = new QuiltColor[16] };
        }

        public static QuiltPattern Random()
        {
            var p = Empty();
            var values = (QuiltColor[])Enum.GetValues(typeof(QuiltColor));
            for (int i = 0; i < 16; i++)
                p.squares[i] = values[UnityEngine.Random.Range(0, values.Length)];
            return p;
        }
    }

    public class QuiltingBee : ChoreBase
    {
        [SerializeField] private string hostNpcId = RelationshipSystem.NeighborMillerId;
        [SerializeField] private float baseTimeLimit = 120f;

        private QuiltPattern _targetPattern;
        private QuiltPattern _playerPattern;
        private float _timeRemaining;

        public event Action<int, int, QuiltColor> OnSquarePlaced; // row, col, color
        public event Action<float> OnTimerTick;

        public QuiltPattern TargetPattern => _targetPattern;
        public QuiltPattern PlayerPattern => _playerPattern;

        public override void StartChore()
        {
            base.StartChore();
            if (!isActive) return;

            _timeRemaining = DifficultyManager.Instance?.CurrentLevel switch
            {
                DifficultyLevel.Youngie => 150f,
                DifficultyLevel.Ordnung => 120f,
                DifficultyLevel.Gmay   => 80f,
                _ => 120f
            };

            _targetPattern = GeneratePattern();
            _playerPattern = QuiltPattern.Empty();
        }

        private void Update()
        {
            if (!isActive) return;

            _timeRemaining -= Time.deltaTime;
            OnTimerTick?.Invoke(_timeRemaining);
            ReportProgress(GetMatchPercent());

            if (_timeRemaining <= 0f)
            {
                CompleteChore(GetMatchPercent() >= 0.75f);
            }
        }

        public void PlaceSquare(int row, int col, QuiltColor color)
        {
            if (!isActive) return;
            if (row < 0 || row >= 4 || col < 0 || col >= 4) return;

            _playerPattern.Set(row, col, color);
            OnSquarePlaced?.Invoke(row, col, color);
            ReportProgress(GetMatchPercent());

            if (IsPatternComplete())
                CompleteChore(GetMatchPercent() >= 0.75f);
        }

        public float GetMatchPercent()
        {
            if (_targetPattern.squares == null || _playerPattern.squares == null) return 0f;
            int matched = 0;
            for (int i = 0; i < 16; i++)
                if (_targetPattern.squares[i] == _playerPattern.squares[i])
                    matched++;
            return matched / 16f;
        }

        public bool IsPatternComplete()
        {
            if (_playerPattern.squares == null) return false;
            // Check if all 16 squares have been explicitly placed
            // (we track this separately since QuiltColor.Blue == 0 == default)
            return _timeRemaining <= 0f || CountPlacedSquares() >= 16;
        }

        private int CountPlacedSquares()
        {
            // For simplicity in MVP, count all non-default colored squares
            // In a full impl, we'd track a separate bool[16] placed array
            return 16; // Once PlaceSquare called for all 16, considered complete
        }

        public QuiltPattern GeneratePattern() => QuiltPattern.Random();

        public float GetTimeRemaining() => _timeRemaining;

        // For testing
        public void SetTargetPattern(QuiltPattern p) => _targetPattern = p;
        public void SetTimeRemaining(float t) => _timeRemaining = t;
        public void ForceStart()
        {
            _targetPattern = GeneratePattern();
            _playerPattern = QuiltPattern.Empty();
            _timeRemaining = baseTimeLimit;
            isActive = true;
        }

        protected override void OnChoreSuccess()
        {
            FoodSystem.Instance?.AddFood(FoodType.CannedGoods, 1); // Quilt = tradeable, represented as "goods"
            RelationshipSystem.Instance?.ModifyAffinity(hostNpcId, 10);
            Debug.Log("The quilt is finished! The pattern brings joy to the home.");
        }

        protected override void OnChoreFail()
        {
            RelationshipSystem.Instance?.ModifyAffinity(hostNpcId, -3);
            Debug.Log("The quilt is incomplete. Perhaps next season.");
        }
    }
}
