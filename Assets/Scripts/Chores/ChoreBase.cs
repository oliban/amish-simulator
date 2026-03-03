using UnityEngine;
using System;

namespace AmishSimulator
{
    public abstract class ChoreBase : MonoBehaviour
    {
        [SerializeField] protected ChoreType choreType;
        [SerializeField] protected int energyCost = 10;

        protected bool isActive = false;

        public event Action<ChoreType, bool> OnChoreCompleted; // (type, success)
        public event Action<float> OnChoreProgress;           // 0-1

        public bool IsActive => isActive;
        public ChoreType ChoreType => choreType;

        public virtual void StartChore()
        {
            if (isActive) return;

            if (EnergySystem.Instance != null && !EnergySystem.Instance.ConsumeEnergy(energyCost))
            {
                Debug.Log($"Not enough energy for {choreType}.");
                return;
            }

            isActive = true;
        }

        public virtual void EndChore()
        {
            isActive = false;
        }

        protected void CompleteChore(bool success)
        {
            isActive = false;
            OnChoreCompleted?.Invoke(choreType, success);
            if (success) OnChoreSuccess();
            else OnChoreFail();
        }

        protected void ReportProgress(float progress)
        {
            OnChoreProgress?.Invoke(Mathf.Clamp01(progress));
        }

        protected abstract void OnChoreSuccess();
        protected abstract void OnChoreFail();
    }
}
