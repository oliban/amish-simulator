using UnityEngine;
using System;

namespace AmishSimulator
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [SerializeField] private DifficultySettings currentSettings;

        public DifficultySettings CurrentSettings => currentSettings;
        public DifficultyLevel CurrentLevel => currentSettings != null ? currentSettings.level : DifficultyLevel.Ordnung;

        public event Action<DifficultySettings> OnDifficultyChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (currentSettings == null)
                currentSettings = DifficultySettings.CreateOrdnung();
        }

        public void SetDifficulty(DifficultyLevel level)
        {
            currentSettings = level switch
            {
                DifficultyLevel.Youngie => DifficultySettings.CreateYoungie(),
                DifficultyLevel.Ordnung => DifficultySettings.CreateOrdnung(),
                DifficultyLevel.Gmay   => DifficultySettings.CreateGmay(),
                _ => DifficultySettings.CreateOrdnung()
            };
            RecalibrateAllSystems();
            OnDifficultyChanged?.Invoke(currentSettings);
        }

        private void RecalibrateAllSystems()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.SetDayLength(currentSettings.dayLengthMinutes);

            if (EnergySystem.Instance != null)
                EnergySystem.Instance.SetMaxEnergy(currentSettings.energyPool);

            if (OrdnungSystem.Instance != null)
                OrdnungSystem.Instance.SetShunningThreshold(currentSettings.shunningThreshold);
        }

        public int GetEnergyPool() => currentSettings?.energyPool ?? 100;
        public float GetDayLengthMinutes() => currentSettings?.dayLengthMinutes ?? 12f;
        public bool IsOrdnungStrict() => currentSettings?.ordnungStrict ?? true;
        public bool IsStrictPunishments() => currentSettings?.strictPunishments ?? false;
        public int GetShunningThreshold() => currentSettings?.shunningThreshold ?? 5;
        public float GetNpcExpectations() => currentSettings?.npcExpectations ?? 0.5f;
    }
}
