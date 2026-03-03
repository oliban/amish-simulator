using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    public enum OrdnungRule
    {
        UsingElectricity,
        SkippingGmay,
        MustacheDetected,
        ImpureThoughts,
        DiagonalWalking,      // Gmay only
        UnapprovedTurnipShape // Gmay only
    }

    public class OrdnungSystem : MonoBehaviour
    {
        public static OrdnungSystem Instance { get; private set; }

        private readonly Dictionary<OrdnungRule, int> _violations = new();
        private int _shunningThreshold = 5;
        private bool _isShunned = false;

        public event Action<OrdnungRule> OnViolationRecorded;
        public event Action OnShunningTriggered;
        public event Action OnAbsolution;

        private static readonly HashSet<OrdnungRule> GmayOnlyRules = new()
        {
            OrdnungRule.DiagonalWalking,
            OrdnungRule.UnapprovedTurnipShape
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (OrdnungRule rule in Enum.GetValues(typeof(OrdnungRule)))
                _violations[rule] = 0;
        }

        private void Start()
        {
            if (DifficultyManager.Instance != null)
            {
                _shunningThreshold = DifficultyManager.Instance.GetShunningThreshold();
                DifficultyManager.Instance.OnDifficultyChanged += OnDifficultyChanged;
            }
        }

        private void OnDestroy()
        {
            if (DifficultyManager.Instance != null)
                DifficultyManager.Instance.OnDifficultyChanged -= OnDifficultyChanged;
        }

        private void OnDifficultyChanged(DifficultySettings settings)
        {
            SetShunningThreshold(settings.shunningThreshold);
        }

        public void SetShunningThreshold(int threshold) => _shunningThreshold = threshold;

        public void RecordViolation(OrdnungRule rule)
        {
            // Gmay-only rules only enforced on Gmay difficulty
            if (GmayOnlyRules.Contains(rule))
            {
                if (DifficultyManager.Instance == null ||
                    DifficultyManager.Instance.CurrentLevel != DifficultyLevel.Gmay)
                    return;
            }

            _violations[rule]++;
            OnViolationRecorded?.Invoke(rule);

            // Reputation loss with Bishop
            RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.BishopYoderId, -5);

            // Check for shunning
            if (!_isShunned && GetTotalViolations() >= _shunningThreshold)
            {
                _isShunned = true;
                OnShunningTriggered?.Invoke();
                // Community-wide affinity penalty
                RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.NeighborMillerId, -10);
                RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.NeighborBeilerId, -10);
            }
        }

        public int GetViolationCount(OrdnungRule rule) =>
            _violations.TryGetValue(rule, out int v) ? v : 0;

        public int GetTotalViolations()
        {
            int total = 0;
            foreach (var v in _violations.Values) total += v;
            return total;
        }

        public bool IsShunned() => _isShunned;

        public void ClearViolation(OrdnungRule rule)
        {
            _violations[rule] = 0;
            if (_isShunned && GetTotalViolations() < _shunningThreshold)
            {
                _isShunned = false;
                OnAbsolution?.Invoke();
            }
        }

        public void GrantAbsolution()
        {
            foreach (var rule in new List<OrdnungRule>(_violations.Keys))
                _violations[rule] = 0;
            _isShunned = false;
            OnAbsolution?.Invoke();
        }
    }
}
