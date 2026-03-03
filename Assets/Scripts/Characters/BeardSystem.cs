using UnityEngine;
using System;

namespace AmishSimulator
{
    public enum BeardStage { Shaven, Stubble, FullBeard, WiseBeard }

    public class BeardSystem : MonoBehaviour
    {
        // Supports both BlendShape (legacy) and ProceduralCharacter beard root
        [SerializeField] private SkinnedMeshRenderer beardMeshRenderer; // optional
        [SerializeField] private int beardBlendShapeIndex = 0;
        [SerializeField] private int mustacheBlendShapeIndex = 1;

        private Transform _beardRoot; // set from ProceduralCharacter if present
        private float _beardFloat = 0f;

        public event Action<BeardStage, string> OnBeardMilestone;
        private BeardStage _currentStage = BeardStage.Shaven;

        private static readonly string[] MilestoneMessages =
        {
            "Your beard has reached 2 inches. The Bishop nods with approval.",
            "Your beard is now 6 inches. The community whispers in admiration.",
            "Your beard has grown to a full foot. A wise man indeed.",
            "Your beard reaches your chest. Truly, a patriarch of the faith."
        };

        private void Start()
        {
            var aging = GetComponent<AgingSystem>();
            if (aging != null) aging.OnBeardFloatChanged += SetBeardFloat;

            // Auto-detect ProceduralCharacter beard root
            var pc = GetComponent<ProceduralCharacter>();
            if (pc != null) _beardRoot = pc.BeardRoot;
        }

        public void OnMarried() => SetBeardFloat(0.15f);

        public void SetBeardFloat(float value)
        {
            _beardFloat = Mathf.Clamp01(value);

            // Drive procedural beard root scale
            if (_beardRoot != null)
            {
                var s = _beardRoot.localScale;
                _beardRoot.localScale = new Vector3(s.x, _beardFloat, s.z);
            }

            // Drive BlendShape if present
            if (beardMeshRenderer != null &&
                beardBlendShapeIndex < beardMeshRenderer.sharedMesh.blendShapeCount)
            {
                beardMeshRenderer.SetBlendShapeWeight(beardBlendShapeIndex, _beardFloat * 100f);
            }

            // Check milestone
            var newStage = GetBeardStage();
            if (newStage != _currentStage)
            {
                _currentStage = newStage;
                string msg = newStage switch
                {
                    BeardStage.Stubble   => MilestoneMessages[0],
                    BeardStage.FullBeard => MilestoneMessages[2],
                    BeardStage.WiseBeard => MilestoneMessages[3],
                    _ => ""
                };
                if (!string.IsNullOrEmpty(msg))
                    OnBeardMilestone?.Invoke(newStage, msg);
            }

            // Mustache violation check
            if (beardMeshRenderer != null &&
                mustacheBlendShapeIndex < beardMeshRenderer.sharedMesh.blendShapeCount)
            {
                if (beardMeshRenderer.GetBlendShapeWeight(mustacheBlendShapeIndex) > 0.1f)
                    OrdnungSystem.Instance?.RecordViolation(OrdnungRule.MustacheDetected);
            }
        }

        public BeardStage GetBeardStage() => _beardFloat switch
        {
            < 0.1f => BeardStage.Shaven,
            < 0.3f => BeardStage.Stubble,
            < 0.7f => BeardStage.FullBeard,
            _      => BeardStage.WiseBeard
        };

        public float GetBeardLengthInches() => _beardFloat * 24f;
        public float GetBeardFloat() => _beardFloat;
    }
}
