using UnityEngine;
using System;

namespace AmishSimulator
{
    public class EnergySystem : MonoBehaviour
    {
        public static EnergySystem Instance { get; private set; }

        private int _maxEnergy = 100;
        private int _currentEnergy;

        public int CurrentEnergy => _currentEnergy;
        public int MaxEnergy => _maxEnergy;
        public bool IsExhausted => _currentEnergy <= 0;

        public event Action<int, int> OnEnergyChanged; // (current, max)
        public event Action OnEnergyDepleted;
        public event Action OnEnergyRestored; // fired when coming back from 0

        private bool _wasExhausted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (DifficultyManager.Instance != null)
            {
                _maxEnergy = DifficultyManager.Instance.GetEnergyPool();
                DifficultyManager.Instance.OnDifficultyChanged += OnDifficultyChanged;
            }
            _currentEnergy = _maxEnergy;
        }

        private void OnDestroy()
        {
            if (DifficultyManager.Instance != null)
                DifficultyManager.Instance.OnDifficultyChanged -= OnDifficultyChanged;
        }

        private void OnDifficultyChanged(DifficultySettings settings)
        {
            SetMaxEnergy(settings.energyPool);
        }

        public void SetMaxEnergy(int max)
        {
            _maxEnergy = max;
            _currentEnergy = Mathf.Min(_currentEnergy, _maxEnergy);
            OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        }

        public bool ConsumeEnergy(int amount)
        {
            if (amount <= 0) return true;
            if (_currentEnergy < amount) return false;

            _currentEnergy -= amount;
            OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);

            if (_currentEnergy <= 0 && !_wasExhausted)
            {
                _wasExhausted = true;
                OnEnergyDepleted?.Invoke();
            }
            return true;
        }

        public void RestoreEnergy(int amount)
        {
            bool wasZero = _currentEnergy <= 0;
            _currentEnergy = Mathf.Min(_currentEnergy + amount, _maxEnergy);
            OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);

            if (wasZero && _currentEnergy > 0)
            {
                _wasExhausted = false;
                OnEnergyRestored?.Invoke();
            }
        }

        public void RestoreFullEnergy()
        {
            RestoreEnergy(_maxEnergy);
        }

        public float GetEnergyPercent() => _maxEnergy > 0 ? (float)_currentEnergy / _maxEnergy : 0f;
    }
}
