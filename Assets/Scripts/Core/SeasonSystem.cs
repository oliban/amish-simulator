using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    public enum Season { Spring, Summer, Fall, Winter }
    public enum ChoreType { ButterChurning, Plowing, BarnRaising, QuiltingBee, WoodChopping, Canning, ApplePressing }

    public class SeasonSystem : MonoBehaviour
    {
        public static SeasonSystem Instance { get; private set; }

        private Season _currentSeason = Season.Spring;
        private int _dayInSeason = 1;

        public event Action<Season> OnSeasonChanged;

        // Which chores are available in each season
        private static readonly Dictionary<Season, ChoreType[]> SeasonalChores = new()
        {
            { Season.Spring, new[] { ChoreType.Plowing, ChoreType.BarnRaising, ChoreType.ButterChurning } },
            { Season.Summer, new[] { ChoreType.ButterChurning, ChoreType.Plowing } },
            { Season.Fall,   new[] { ChoreType.QuiltingBee, ChoreType.ApplePressing, ChoreType.Canning, ChoreType.ButterChurning } },
            { Season.Winter, new[] { ChoreType.WoodChopping, ChoreType.ButterChurning } },
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (TimeSystem.Instance != null)
            {
                TimeSystem.Instance.OnSeasonChanged += HandleSeasonChanged;
                TimeSystem.Instance.OnDayChanged += HandleDayChanged;
            }
        }

        private void OnDestroy()
        {
            if (TimeSystem.Instance != null)
            {
                TimeSystem.Instance.OnSeasonChanged -= HandleSeasonChanged;
                TimeSystem.Instance.OnDayChanged -= HandleDayChanged;
            }
        }

        private void HandleSeasonChanged(Season season)
        {
            _currentSeason = season;
            _dayInSeason = 1;
            OnSeasonChanged?.Invoke(season);
        }

        private void HandleDayChanged(int day) => _dayInSeason = day;

        public Season GetCurrentSeason() => _currentSeason;
        public int GetDayInSeason() => _dayInSeason;

        public bool IsSeasonalChoreAvailable(ChoreType choreType)
        {
            if (!SeasonalChores.TryGetValue(_currentSeason, out var chores)) return false;
            foreach (var c in chores)
                if (c == choreType) return true;
            return false;
        }
    }
}
