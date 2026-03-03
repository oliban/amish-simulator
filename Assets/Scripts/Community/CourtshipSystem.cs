using UnityEngine;
using System;

namespace AmishSimulator
{
    public enum CourtshipStage { None, MeetingPhase, CourtingPhase, Engaged, Married }

    public class CourtshipSystem : MonoBehaviour
    {
        public static CourtshipSystem Instance { get; private set; }

        public CourtshipStage CurrentStage { get; private set; } = CourtshipStage.None;
        private string _targetNpcId;

        public event Action<CourtshipStage> OnStageChanged;
        public event Action OnMarriageComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartCourtship(string npcId)
        {
            if (CurrentStage != CourtshipStage.None) return;
            _targetNpcId = npcId;
            SetStage(CourtshipStage.MeetingPhase);
        }

        public bool AdvanceCourtship()
        {
            if (RelationshipSystem.Instance == null) return false;
            int affinity = RelationshipSystem.Instance.GetAffinity(_targetNpcId);

            var nextStage = CurrentStage switch
            {
                CourtshipStage.MeetingPhase  when affinity >= 40 => CourtshipStage.CourtingPhase,
                CourtshipStage.CourtingPhase when affinity >= 60 => CourtshipStage.Engaged,
                CourtshipStage.Engaged       when affinity >= 80 => CourtshipStage.Married,
                _ => CurrentStage
            };

            if (nextStage == CurrentStage) return false;
            SetStage(nextStage);

            if (nextStage == CourtshipStage.Married)
            {
                RelationshipSystem.Instance.ConfirmMarriage(_targetNpcId);
                OnMarriageComplete?.Invoke();
            }

            return true;
        }

        public bool ProposeMarriage()
        {
            if (CurrentStage != CourtshipStage.Engaged) return false;
            if (RelationshipSystem.Instance == null) return false;
            return RelationshipSystem.Instance.InitiateMarriage(_targetNpcId);
        }

        public CourtshipStage GetCourtshipStage() => CurrentStage;

        private void SetStage(CourtshipStage stage)
        {
            CurrentStage = stage;
            OnStageChanged?.Invoke(stage);
        }
    }
}
