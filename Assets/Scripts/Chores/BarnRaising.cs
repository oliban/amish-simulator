using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    [Serializable]
    public struct BeamPlacement
    {
        public int beamId;
        public Vector2Int position;
        public bool isPlaced;
        public bool requiresPlayer;
    }

    public class BarnRaising : ChoreBase
    {
        [SerializeField] private int totalBeams = 12;
        [SerializeField] private float sunsetDuration = 300f; // 5 min real time

        private List<BeamPlacement> _beamQueue;
        private int _placedBeams;
        private int _npcCount;
        private float _sunsetTimer;
        private float _npcWorkRate; // beams per second

        public event Action<int> OnBeamPlaced;
        public event Action<float> OnSunsetTimerTick;

        public override void StartChore()
        {
            base.StartChore();
            if (!isActive) return;

            // NPC count from relationship affinities
            _npcCount = CalculateNpcHelpers();
            _npcWorkRate = _npcCount * 0.008f; // ~1 beam per 15s per NPC helper
            _sunsetTimer = sunsetDuration;
            _placedBeams = 0;

            // Apply difficulty time limit
            if (DifficultyManager.Instance != null)
            {
                _sunsetTimer = DifficultyManager.Instance.CurrentLevel switch
                {
                    DifficultyLevel.Youngie => 360f,
                    DifficultyLevel.Ordnung => 300f,
                    DifficultyLevel.Gmay   => 180f,
                    _ => 300f
                };
            }

            InitializeBeams();
        }

        private void InitializeBeams()
        {
            _beamQueue = new List<BeamPlacement>();
            var rng = new System.Random();
            for (int i = 0; i < totalBeams; i++)
            {
                _beamQueue.Add(new BeamPlacement
                {
                    beamId = i,
                    position = new Vector2Int(i % 4, i / 4),
                    isPlaced = false,
                    requiresPlayer = rng.NextDouble() < 0.4f // 40% need player
                });
            }
        }

        private int CalculateNpcHelpers()
        {
            if (RelationshipSystem.Instance == null) return 1;
            int helpers = 0;
            if (RelationshipSystem.Instance.GetAffinity(RelationshipSystem.NeighborMillerId) > 60) helpers++;
            if (RelationshipSystem.Instance.GetAffinity(RelationshipSystem.NeighborBeilerId) > 60) helpers++;
            return Mathf.Max(1, helpers);
        }

        private void Update()
        {
            if (!isActive) return;

            // NPC auto-place non-player beams
            float beamsThisFrame = _npcWorkRate * Time.deltaTime;
            AutoPlaceBeams(beamsThisFrame);

            _sunsetTimer -= Time.deltaTime;
            OnSunsetTimerTick?.Invoke(_sunsetTimer);
            ReportProgress(GetCompletionPercent());

            if (_sunsetTimer <= 0f)
            {
                CompleteChore(_placedBeams >= totalBeams);
            }
            else if (_placedBeams >= totalBeams)
            {
                CompleteChore(true);
            }
        }

        private void AutoPlaceBeams(float beamCount)
        {
            float accumulated = beamCount;
            foreach (var beam in _beamQueue)
            {
                if (accumulated < 1f) break;
                // Use a local copy to modify
                for (int i = 0; i < _beamQueue.Count; i++)
                {
                    var b = _beamQueue[i];
                    if (!b.isPlaced && !b.requiresPlayer)
                    {
                        b.isPlaced = true;
                        _beamQueue[i] = b;
                        _placedBeams++;
                        accumulated -= 1f;
                        OnBeamPlaced?.Invoke(b.beamId);
                        if (accumulated < 1f) break;
                    }
                }
                break;
            }
        }

        public bool PlaceBeam(int beamId)
        {
            if (!isActive) return false;
            for (int i = 0; i < _beamQueue.Count; i++)
            {
                var b = _beamQueue[i];
                if (b.beamId == beamId && !b.isPlaced)
                {
                    b.isPlaced = true;
                    _beamQueue[i] = b;
                    _placedBeams++;
                    OnBeamPlaced?.Invoke(beamId);
                    ReportProgress(GetCompletionPercent());
                    return true;
                }
            }
            return false;
        }

        public BeamPlacement? GetNextPriorityBeam()
        {
            foreach (var b in _beamQueue)
                if (!b.isPlaced && b.requiresPlayer)
                    return b;
            return null;
        }

        public float GetCompletionPercent() =>
            totalBeams > 0 ? (float)_placedBeams / totalBeams : 0f;

        public int GetPlacedBeams() => _placedBeams;
        public int GetTotalBeams() => totalBeams;
        public float GetSunsetTimer() => _sunsetTimer;

        // For testing
        public void SetTotalBeams(int n) => totalBeams = n;
        public void SetSunsetTimer(float t) => _sunsetTimer = t;
        public void SetNpcWorkRate(float r) => _npcWorkRate = r;
        public void ForceInitializeBeams() => InitializeBeams();

        protected override void OnChoreSuccess()
        {
            // Affinity bonus for all attendees
            RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.NeighborMillerId, 15);
            RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.NeighborBeilerId, 15);
            RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.BishopYoderId, 10);
            Debug.Log("The barn is raised! The community rejoices.");
        }

        protected override void OnChoreFail()
        {
            RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.NeighborMillerId, -5);
            RelationshipSystem.Instance?.ModifyAffinity(RelationshipSystem.NeighborBeilerId, -5);
            Debug.Log("The sun has set on an unfinished barn. The community is disappointed.");
        }
    }
}
