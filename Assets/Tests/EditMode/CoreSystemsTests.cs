using NUnit.Framework;
using AmishSimulator;
using UnityEngine;

namespace AmishSimulator.Tests
{
    public class CoreSystemsTests
    {
        // ── DifficultySettings ──────────────────────────────────────────────

        [Test]
        public void DifficultySettings_Youngie_HasCorrectValues()
        {
            var settings = DifficultySettings.CreateYoungie();
            Assert.AreEqual(DifficultyLevel.Youngie, settings.level);
            Assert.AreEqual(18f, settings.dayLengthMinutes);
            Assert.AreEqual(150, settings.energyPool);
            Assert.IsFalse(settings.ordnungStrict);
            Assert.AreEqual(10, settings.shunningThreshold);
        }

        [Test]
        public void DifficultySettings_Ordnung_HasCorrectValues()
        {
            var settings = DifficultySettings.CreateOrdnung();
            Assert.AreEqual(DifficultyLevel.Ordnung, settings.level);
            Assert.AreEqual(12f, settings.dayLengthMinutes);
            Assert.AreEqual(100, settings.energyPool);
            Assert.IsTrue(settings.ordnungStrict);
            Assert.IsFalse(settings.strictPunishments);
            Assert.AreEqual(5, settings.shunningThreshold);
        }

        [Test]
        public void DifficultySettings_Gmay_HasCorrectValues()
        {
            var settings = DifficultySettings.CreateGmay();
            Assert.AreEqual(DifficultyLevel.Gmay, settings.level);
            Assert.AreEqual(8f, settings.dayLengthMinutes);
            Assert.AreEqual(75, settings.energyPool);
            Assert.IsTrue(settings.ordnungStrict);
            Assert.IsTrue(settings.strictPunishments);
            Assert.AreEqual(3, settings.shunningThreshold);
        }

        // ── EnergySystem (pure logic, no MonoBehaviour lifecycle) ───────────

        [Test]
        public void EnergySystem_ConsumeEnergy_ReducesEnergy()
        {
            var energy = new EnergySystemLogic(100);
            energy.Consume(30);
            Assert.AreEqual(70, energy.Current);
        }

        [Test]
        public void EnergySystem_ConsumeEnergy_ReturnsFalseWhenInsufficient()
        {
            var energy = new EnergySystemLogic(50);
            bool result = energy.Consume(60);
            Assert.IsFalse(result);
            Assert.AreEqual(50, energy.Current); // unchanged
        }

        [Test]
        public void EnergySystem_RestoreEnergy_CapsAtMax()
        {
            var energy = new EnergySystemLogic(100);
            energy.Consume(50);
            energy.Restore(9999);
            Assert.AreEqual(100, energy.Current);
        }

        [Test]
        public void EnergySystem_DepletedEvent_FiresAtZero()
        {
            bool fired = false;
            var energy = new EnergySystemLogic(10);
            energy.OnDepleted += () => fired = true;
            energy.Consume(10);
            Assert.IsTrue(fired);
        }

        [Test]
        public void EnergySystem_GetEnergyPercent_CorrectAtHalf()
        {
            var energy = new EnergySystemLogic(100);
            energy.Consume(50);
            Assert.AreEqual(0.5f, energy.GetPercent(), 0.001f);
        }

        // ── TimeSystem day phase ─────────────────────────────────────────────

        [Test]
        public void TimeSystem_DayPhase_Dawn_At6()
        {
            var phase = TimeSystemLogic.GetPhaseForHour(6);
            Assert.AreEqual(DayPhase.Dawn, phase);
        }

        [Test]
        public void TimeSystem_DayPhase_Night_AtMidnight()
        {
            var phase = TimeSystemLogic.GetPhaseForHour(0);
            Assert.AreEqual(DayPhase.Night, phase);
        }

        [Test]
        public void TimeSystem_DayPhase_Morning_At10()
        {
            var phase = TimeSystemLogic.GetPhaseForHour(10);
            Assert.AreEqual(DayPhase.Morning, phase);
        }
    }

    // ── Pure-logic test helpers (no MonoBehaviour needed) ──────────────────

    /// <summary>Pure-logic wrapper for EnergySystem, usable in EditMode tests.</summary>
    public class EnergySystemLogic
    {
        public int Current { get; private set; }
        public int Max { get; private set; }
        public event System.Action OnDepleted;

        public EnergySystemLogic(int max) { Max = max; Current = max; }

        public bool Consume(int amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            if (Current <= 0) OnDepleted?.Invoke();
            return true;
        }

        public void Restore(int amount) => Current = Mathf.Min(Current + amount, Max);
        public float GetPercent() => Max > 0 ? (float)Current / Max : 0f;
    }

    /// <summary>Pure-logic helper for TimeSystem day phase calculation.</summary>
    public static class TimeSystemLogic
    {
        public static DayPhase GetPhaseForHour(int hour)
        {
            if (hour >= 5 && hour < 8) return DayPhase.Dawn;
            if (hour >= 8 && hour < 12) return DayPhase.Morning;
            if (hour >= 12 && hour < 17) return DayPhase.Afternoon;
            if (hour >= 17 && hour < 21) return DayPhase.Evening;
            return DayPhase.Night;
        }
    }
}
