using UnityEngine;

namespace AmishSimulator
{
    public enum DifficultyLevel { Youngie, Ordnung, Gmay }

    [CreateAssetMenu(fileName = "DifficultySettings", menuName = "AmishSimulator/DifficultySettings")]
    public class DifficultySettings : ScriptableObject
    {
        [Header("Identity")]
        public DifficultyLevel level;
        public string levelName;
        public string description;

        [Header("Time")]
        public float dayLengthMinutes = 12f;

        [Header("Energy")]
        public int energyPool = 100;

        [Header("Ordnung")]
        public bool ordnungStrict = true;
        public bool strictPunishments = false;
        public int shunningThreshold = 5;

        [Header("NPC")]
        [Range(0f, 1f)]
        public float npcExpectations = 0.5f;

        // Factory methods for default presets
        public static DifficultySettings CreateYoungie()
        {
            var s = CreateInstance<DifficultySettings>();
            s.level = DifficultyLevel.Youngie;
            s.levelName = "Youngie";
            s.description = "Take your time, young one. The Lord is patient.";
            s.dayLengthMinutes = 18f;
            s.energyPool = 150;
            s.ordnungStrict = false;
            s.strictPunishments = false;
            s.shunningThreshold = 10;
            s.npcExpectations = 0.2f;
            return s;
        }

        public static DifficultySettings CreateOrdnung()
        {
            var s = CreateInstance<DifficultySettings>();
            s.level = DifficultyLevel.Ordnung;
            s.levelName = "Ordnung";
            s.description = "Follow the order. Live well. Work hard.";
            s.dayLengthMinutes = 12f;
            s.energyPool = 100;
            s.ordnungStrict = true;
            s.strictPunishments = false;
            s.shunningThreshold = 5;
            s.npcExpectations = 0.5f;
            return s;
        }

        public static DifficultySettings CreateGmay()
        {
            var s = CreateInstance<DifficultySettings>();
            s.level = DifficultyLevel.Gmay;
            s.levelName = "Gmay";
            s.description = "The Bishop watches. Every step. Every furrow. Every turnip.";
            s.dayLengthMinutes = 8f;
            s.energyPool = 75;
            s.ordnungStrict = true;
            s.strictPunishments = true;
            s.shunningThreshold = 3;
            s.npcExpectations = 0.9f;
            return s;
        }
    }
}
