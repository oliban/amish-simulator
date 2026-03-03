using UnityEngine;
using System;

namespace AmishSimulator
{
    public class ButterChurning : ChoreBase
    {
        // Configuration — set based on difficulty
        private float _beatInterval = 1.5f;      // seconds between required presses
        private float _hitWindow = 0.25f;         // seconds either side of beat
        private int _maxMissedBeats = 2;

        // State
        private float _churnProgress = 0f;       // 0-1: cream → butter
        private int _missedBeats = 0;
        private float _timeSinceBeat = 0f;
        private bool _inputReceivedThisBeat = false;
        private bool _beatWindowOpen = false;

        private const float ProgressPerHit = 0.1f; // 10 hits to complete

        public event Action<string> OnBeatResult; // "hit" or "miss"

        private void Start()
        {
            ApplyDifficultySettings();
        }

        private void ApplyDifficultySettings()
        {
            if (DifficultyManager.Instance == null) return;
            switch (DifficultyManager.Instance.CurrentLevel)
            {
                case DifficultyLevel.Youngie:
                    _beatInterval = 1.8f; _hitWindow = 0.4f; _maxMissedBeats = 3;
                    break;
                case DifficultyLevel.Ordnung:
                    _beatInterval = 1.5f; _hitWindow = 0.25f; _maxMissedBeats = 2;
                    break;
                case DifficultyLevel.Gmay:
                    _beatInterval = 1.1f; _hitWindow = 0.15f; _maxMissedBeats = 1;
                    break;
            }
        }

        public override void StartChore()
        {
            base.StartChore();
            if (!isActive) return;

            _churnProgress = 0f;
            _missedBeats = 0;
            _timeSinceBeat = 0f;
            _inputReceivedThisBeat = false;
            _beatWindowOpen = false;

            ApplyDifficultySettings();
        }

        private void Update()
        {
            if (!isActive) return;

            _timeSinceBeat += Time.deltaTime;

            // Open hit window near beat
            float timeToNextBeat = _beatInterval - _timeSinceBeat;
            _beatWindowOpen = timeToNextBeat <= _hitWindow || _timeSinceBeat <= _hitWindow;

            // Beat arrived
            if (_timeSinceBeat >= _beatInterval)
            {
                TickBeat();
            }
        }

        /// <summary>Call when the player presses the churn button.</summary>
        public void OnPlayerInput()
        {
            if (!isActive) return;

            if (_beatWindowOpen && !_inputReceivedThisBeat)
            {
                _inputReceivedThisBeat = true;
                _churnProgress += ProgressPerHit;
                ReportProgress(_churnProgress);
                OnBeatResult?.Invoke("hit");

                if (_churnProgress >= 1f)
                {
                    CompleteChore(true);
                }
            }
            else if (!_beatWindowOpen)
            {
                // Early/late press counts as miss
                RegisterMiss();
            }
        }

        private void TickBeat()
        {
            if (!_inputReceivedThisBeat)
                RegisterMiss();

            _timeSinceBeat = 0f;
            _inputReceivedThisBeat = false;
        }

        private void RegisterMiss()
        {
            _missedBeats++;
            OnBeatResult?.Invoke("miss");

            if (_missedBeats >= _maxMissedBeats)
                CompleteChore(false);
        }

        public float GetProgress() => _churnProgress;
        public int GetMissedBeats() => _missedBeats;
        public bool IsBeatWindowOpen() => _beatWindowOpen;

        // For testing: expose internal tick
        public void SimulateBeatTick() => TickBeat();
        public void SimulateInput() => OnPlayerInput();
        public void SetBeatWindowOpen(bool open) => _beatWindowOpen = open;
        public void SetProgress(float p) => _churnProgress = p;
        public void SetMaxMissedBeats(int m) => _maxMissedBeats = m;

        protected override void OnChoreSuccess()
        {
            // Add butter to food system
            FoodSystem.Instance?.AddFood(FoodType.Butter, 2);
            Debug.Log("The butter is well-churned. God smiles on this farmstead.");
        }

        protected override void OnChoreFail()
        {
            Debug.Log("The buttermilk spills! Better luck next time.");
        }
    }
}
