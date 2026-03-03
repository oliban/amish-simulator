using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    public enum RelationshipLevel { Stranger, Acquaintance, Friend, GoodFriend, TrustedCompanion }

    public class RelationshipSystem : MonoBehaviour
    {
        public static RelationshipSystem Instance { get; private set; }

        private readonly Dictionary<string, int> _affinities = new();

        public event Action<string, int, int> OnAffinityChanged; // (npcId, oldVal, newVal)
        public event Action<string> OnMarriageProposal;
        public event Action<string> OnMarried;

        // Pre-configured NPC IDs
        public const string BishopYoderId    = "bishop_yoder";
        public const string NeighborMillerId = "neighbor_miller";
        public const string NeighborBeilerId = "neighbor_beiler";
        public const string SpouseCandidateId = "spouse_candidate";
        public const string ElderMarthaId    = "elder_martha";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeAffinities();
        }

        private void InitializeAffinities()
        {
            _affinities[BishopYoderId]     = 50;
            _affinities[NeighborMillerId]  = 40;
            _affinities[NeighborBeilerId]  = 40;
            _affinities[SpouseCandidateId] = 30;
            _affinities[ElderMarthaId]     = 50;
        }

        public int GetAffinity(string npcId)
        {
            return _affinities.TryGetValue(npcId, out int v) ? v : 0;
        }

        public void ModifyAffinity(string npcId, int delta)
        {
            if (!_affinities.TryGetValue(npcId, out int current))
                current = 0;

            int newVal = Mathf.Clamp(current + delta, 0, 100);
            _affinities[npcId] = newVal;
            OnAffinityChanged?.Invoke(npcId, current, newVal);
        }

        public void NotifyAffinityChanged(string npcId, int oldVal, int newVal)
        {
            _affinities[npcId] = newVal;
            OnAffinityChanged?.Invoke(npcId, oldVal, newVal);
        }

        public bool CanMarry(string npcId) => GetAffinity(npcId) >= 80;

        public bool InitiateMarriage(string npcId)
        {
            if (!CanMarry(npcId)) return false;
            OnMarriageProposal?.Invoke(npcId);
            return true;
        }

        public void ConfirmMarriage(string npcId)
        {
            OnMarried?.Invoke(npcId);
            PlayerController.Instance?.Marry(npcId);
        }

        public RelationshipLevel GetRelationshipLevel(string npcId)
        {
            int affinity = GetAffinity(npcId);
            return affinity switch
            {
                < 20  => RelationshipLevel.Stranger,
                < 40  => RelationshipLevel.Acquaintance,
                < 60  => RelationshipLevel.Friend,
                < 80  => RelationshipLevel.GoodFriend,
                _     => RelationshipLevel.TrustedCompanion
            };
        }

        public float GetAverageAffinity()
        {
            if (_affinities.Count == 0) return 0f;
            float total = 0f;
            foreach (var v in _affinities.Values) total += v;
            return total / _affinities.Count;
        }
    }
}
