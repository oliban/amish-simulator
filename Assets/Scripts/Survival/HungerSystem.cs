using UnityEngine;
using System;

namespace AmishSimulator
{
    public enum HungerState { Full, Satisfied, Hungry, Starving, Critical }

    public class HungerSystem : MonoBehaviour
    {
        public static HungerSystem Instance { get; private set; }

        [SerializeField] private float hungerLevel = 80f;
        [SerializeField] private float baseDepletionPerHour = 2f;

        private const float MaxHunger = 100f;
        private bool _starvationEventFired = false;

        public float HungerLevel => hungerLevel;

        public event Action<float> OnHungerChanged;
        public event Action OnStarving;
        public event Action OnStarvationDeath;

        private HungerState _previousState = HungerState.Satisfied;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.OnHourChanged += OnHourPassed;
        }

        private void OnDestroy()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.OnHourChanged -= OnHourPassed;
        }

        private void OnHourPassed(int hour)
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            TickHunger(1f); // 1 hour tick
        }

        /// <summary>Directly tick hunger depletion. Exposed for testing.</summary>
        public void TickHunger(float hours)
        {
            float depletion = baseDepletionPerHour * hours;
            hungerLevel = Mathf.Max(0f, hungerLevel - depletion);
            OnHungerChanged?.Invoke(hungerLevel);
            CheckHungerState();
        }

        /// <summary>Apply energy expenditure bonus depletion.</summary>
        public void OnEnergyExpended(int energyAmount)
        {
            float bonus = energyAmount * 0.5f;
            hungerLevel = Mathf.Max(0f, hungerLevel - bonus);
            OnHungerChanged?.Invoke(hungerLevel);
            CheckHungerState();
        }

        private void CheckHungerState()
        {
            var currentState = GetHungerState();

            if (currentState == HungerState.Starving && _previousState != HungerState.Starving
                && _previousState != HungerState.Critical)
            {
                OnStarving?.Invoke();
            }

            if (hungerLevel <= 0f && !_starvationEventFired)
            {
                _starvationEventFired = true;
                OnStarvationDeath?.Invoke();
                GameManager.Instance?.TriggerFailState(FailReason.Starvation);
            }

            _previousState = currentState;
        }

        public void ConsumeFood(FoodItem food)
        {
            if (food == null) return;
            hungerLevel = Mathf.Min(hungerLevel + food.HungerRestoration, MaxHunger);
            _starvationEventFired = false;
            OnHungerChanged?.Invoke(hungerLevel);
            CheckHungerState();
        }

        public float GetHungerLevel() => hungerLevel;

        public HungerState GetHungerState() => hungerLevel switch
        {
            >= 75f => HungerState.Full,
            >= 50f => HungerState.Satisfied,
            >= 25f => HungerState.Hungry,
            >= 10f => HungerState.Starving,
            _      => HungerState.Critical
        };

        /// <summary>Force set hunger level (for testing/debugging).</summary>
        public void SetHungerLevel(float level)
        {
            hungerLevel = Mathf.Clamp(level, 0f, MaxHunger);
            _starvationEventFired = false;
            OnHungerChanged?.Invoke(hungerLevel);
        }
    }
}
