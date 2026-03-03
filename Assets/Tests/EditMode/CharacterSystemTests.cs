using NUnit.Framework;
using AmishSimulator;
using UnityEngine;

namespace AmishSimulator.Tests
{
    public class CharacterSystemTests
    {
        // ── AgingSystem pure logic ───────────────────────────────────────────

        [Test]
        public void AgingLogic_StartsAt18()
        {
            var logic = new AgingLogic(Gender.Male);
            Assert.AreEqual(18, logic.Age);
        }

        [Test]
        public void AgingLogic_AgeIncrements_OnYear()
        {
            var logic = new AgingLogic(Gender.Male);
            logic.AdvanceYear();
            Assert.AreEqual(19, logic.Age);
        }

        [Test]
        public void AgingLogic_BeardFloat_ZeroBeforeMarriage()
        {
            var logic = new AgingLogic(Gender.Male);
            logic.AdvanceYear();
            Assert.AreEqual(0f, logic.BeardFloat);
        }

        [Test]
        public void AgingLogic_BeardFloat_IncreasesAfterMarriage()
        {
            var logic = new AgingLogic(Gender.Male);
            logic.Marry();
            logic.AdvanceYear();
            Assert.Greater(logic.BeardFloat, 0f);
        }

        [Test]
        public void AgingLogic_BeardFloat_MaxIsOne()
        {
            var logic = new AgingLogic(Gender.Male);
            logic.Marry();
            for (int i = 0; i < 100; i++) logic.AdvanceYear();
            Assert.LessOrEqual(logic.BeardFloat, 1f);
        }

        [Test]
        public void AgingLogic_Female_BeardFloat_AlwaysZero()
        {
            var logic = new AgingLogic(Gender.Female);
            logic.Marry();
            for (int i = 0; i < 20; i++) logic.AdvanceYear();
            Assert.AreEqual(0f, logic.BeardFloat);
        }

        [Test]
        public void AgingLogic_LifeStage_Youth_Under25()
        {
            var logic = new AgingLogic(Gender.Male);
            Assert.AreEqual(LifeStage.Youth, logic.GetLifeStage());
        }

        [Test]
        public void AgingLogic_LifeStage_Elder_At65()
        {
            var logic = new AgingLogic(Gender.Male);
            for (int i = 0; i < 47; i++) logic.AdvanceYear(); // 18 + 47 = 65
            Assert.AreEqual(LifeStage.Elder, logic.GetLifeStage());
        }

        // ── BeardSystem pure logic ───────────────────────────────────────────

        [Test]
        public void BeardLogic_Shaven_AtZero()
        {
            var logic = new BeardLogic();
            Assert.AreEqual(BeardStage.Shaven, logic.GetStage(0f));
        }

        [Test]
        public void BeardLogic_Stubble_At02()
        {
            var logic = new BeardLogic();
            Assert.AreEqual(BeardStage.Stubble, logic.GetStage(0.2f));
        }

        [Test]
        public void BeardLogic_FullBeard_At05()
        {
            var logic = new BeardLogic();
            Assert.AreEqual(BeardStage.FullBeard, logic.GetStage(0.5f));
        }

        [Test]
        public void BeardLogic_WiseBeard_At09()
        {
            var logic = new BeardLogic();
            Assert.AreEqual(BeardStage.WiseBeard, logic.GetStage(0.9f));
        }

        [Test]
        public void BeardLogic_LengthInches_ScalesTo24()
        {
            var logic = new BeardLogic();
            Assert.AreEqual(24f, logic.GetLengthInches(1f), 0.001f);
        }
    }

    // ── Pure-logic test helpers ──────────────────────────────────────────────

    public class AgingLogic
    {
        public int Age { get; private set; } = 18;
        public float BeardFloat { get; private set; } = 0f;

        private readonly Gender _gender;
        private bool _isMarried = false;

        public AgingLogic(Gender gender) { _gender = gender; }

        public void Marry()
        {
            _isMarried = true;
            if (_gender == Gender.Male) BeardFloat = 0.15f;
        }

        public void AdvanceYear()
        {
            Age++;
            if (_gender == Gender.Male && _isMarried)
                BeardFloat = Mathf.Min(BeardFloat + 0.08f, 1f);
        }

        public LifeStage GetLifeStage() => Age switch
        {
            < 25 => LifeStage.Youth,
            < 40 => LifeStage.YoungAdult,
            < 65 => LifeStage.MiddleAge,
            _    => LifeStage.Elder
        };
    }

    public class BeardLogic
    {
        public BeardStage GetStage(float f) => f switch
        {
            < 0.1f => BeardStage.Shaven,
            < 0.3f => BeardStage.Stubble,
            < 0.7f => BeardStage.FullBeard,
            _      => BeardStage.WiseBeard
        };

        public float GetLengthInches(float f) => f * 24f;
    }
}
