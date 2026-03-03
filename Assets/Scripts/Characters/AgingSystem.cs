using UnityEngine;
using System;

namespace AmishSimulator
{
    public class AgingSystem : MonoBehaviour
    {
        public static AgingSystem Instance { get; private set; }

        public int CurrentAge { get; private set; } = 18;
        public LifeStage CurrentLifeStage { get; private set; } = LifeStage.Youth;
        public float BeardFloat { get; private set; } = 0f; // 0-1, male only
        public Gender PlayerGender { get; private set; }

        private bool _isMarried = false;
        private int _naturalDeathAge;
        private System.Random _rng = new();

        public event Action<int> OnAgeChanged;
        public event Action<LifeStage> OnLifeStageChanged;
        public event Action<int> OnNaturalDeath;
        public event Action<float> OnBeardFloatChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.OnYearChanged += HandleYearChanged;

            var pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.OnMarried += HandleMarried;
            }
        }

        private void OnDestroy()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.OnYearChanged -= HandleYearChanged;
        }

        public void Initialize(Gender gender)
        {
            PlayerGender = gender;
            CurrentAge = 18;
            BeardFloat = 0f;
            _isMarried = false;
            // Natural death age: 75-85, weighted toward 78-82
            _naturalDeathAge = 75 + _rng.Next(0, 11);
            UpdateLifeStage();
        }

        private void HandleYearChanged(int year)
        {
            CurrentAge++;
            OnAgeChanged?.Invoke(CurrentAge);

            // Advance beard growth (males)
            if (PlayerGender == Gender.Male && _isMarried)
            {
                BeardFloat = Mathf.Min(BeardFloat + 0.08f, 1f);
                OnBeardFloatChanged?.Invoke(BeardFloat);
            }

            UpdateLifeStage();

            if (CurrentAge >= _naturalDeathAge)
            {
                OnNaturalDeath?.Invoke(CurrentAge);
                GameManager.Instance?.TriggerWinState();
            }
        }

        private void HandleMarried()
        {
            _isMarried = true;
            // Initial stubble on marriage
            if (PlayerGender == Gender.Male)
            {
                BeardFloat = 0.15f;
                OnBeardFloatChanged?.Invoke(BeardFloat);
            }
        }

        private void UpdateLifeStage()
        {
            var newStage = CurrentAge switch
            {
                < 25 => LifeStage.Youth,
                < 40 => LifeStage.YoungAdult,
                < 65 => LifeStage.MiddleAge,
                _    => LifeStage.Elder
            };

            if (newStage != CurrentLifeStage)
            {
                CurrentLifeStage = newStage;
                OnLifeStageChanged?.Invoke(CurrentLifeStage);
            }
        }

        public bool IsElder() => CurrentLifeStage == LifeStage.Elder;
    }
}
