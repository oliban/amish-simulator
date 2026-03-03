using UnityEngine;
using System;

namespace AmishSimulator
{
    public enum DayPhase { Dawn, Morning, Afternoon, Evening, Night }

    public class TimeSystem : MonoBehaviour
    {
        public static TimeSystem Instance { get; private set; }

        // Configuration (set by DifficultyManager)
        [SerializeField] private float dayLengthMinutes = 12f; // real-time minutes per in-game day

        // Current time state
        public int CurrentHour { get; private set; } = 6; // Start at 6 AM
        public int CurrentDay { get; private set; } = 1;
        public Season CurrentSeason { get; private set; } = Season.Spring;
        public int CurrentYear { get; private set; } = 1;

        public const int DaysPerSeason = 28;
        public const int SeasonsPerYear = 4;
        public const int HoursPerDay = 24;

        // Events
        public event Action<int> OnHourChanged;
        public event Action<int> OnDayChanged;
        public event Action<Season> OnSeasonChanged;
        public event Action<int> OnYearChanged;

        private float _secondsPerHour;
        private float _elapsedSeconds;
        private bool _isRunning;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            RecalculateTimeScale();
        }

        public void SetDayLength(float minutes)
        {
            dayLengthMinutes = minutes;
            RecalculateTimeScale();
        }

        private void RecalculateTimeScale()
        {
            float realSecondsPerDay = dayLengthMinutes * 60f;
            _secondsPerHour = realSecondsPerDay / HoursPerDay;
        }

        public void BeginDayCycle()
        {
            _isRunning = true;
        }

        public void StopDayCycle()
        {
            _isRunning = false;
        }

        private void Update()
        {
            if (!_isRunning) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            _elapsedSeconds += Time.deltaTime;
            if (_elapsedSeconds >= _secondsPerHour)
            {
                _elapsedSeconds -= _secondsPerHour;
                AdvanceHour();
            }
        }

        private void AdvanceHour()
        {
            CurrentHour = (CurrentHour + 1) % HoursPerDay;
            OnHourChanged?.Invoke(CurrentHour);

            if (CurrentHour == 0) // Midnight — new day
            {
                AdvanceDay();
            }
        }

        public void AdvanceTime()
        {
            AdvanceHour();
        }

        public void AdvanceToNextDay()
        {
            int hoursLeft = HoursPerDay - CurrentHour;
            for (int i = 0; i < hoursLeft; i++)
                AdvanceHour();
        }

        private void AdvanceDay()
        {
            CurrentDay++;
            if (CurrentDay > DaysPerSeason)
            {
                CurrentDay = 1;
                AdvanceSeason();
            }
            OnDayChanged?.Invoke(CurrentDay);
        }

        private void AdvanceSeason()
        {
            int next = ((int)CurrentSeason + 1) % SeasonsPerYear;
            CurrentSeason = (Season)next;
            OnSeasonChanged?.Invoke(CurrentSeason);

            if (CurrentSeason == Season.Spring)
            {
                CurrentYear++;
                OnYearChanged?.Invoke(CurrentYear);
            }
        }

        public DayPhase GetCurrentDayPhase()
        {
            if (CurrentHour >= 5 && CurrentHour < 8) return DayPhase.Dawn;
            if (CurrentHour >= 8 && CurrentHour < 12) return DayPhase.Morning;
            if (CurrentHour >= 12 && CurrentHour < 17) return DayPhase.Afternoon;
            if (CurrentHour >= 17 && CurrentHour < 21) return DayPhase.Evening;
            return DayPhase.Night;
        }

        public float GetDayProgress() => (CurrentHour + (_elapsedSeconds / _secondsPerHour)) / HoursPerDay;
    }
}
