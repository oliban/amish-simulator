using NUnit.Framework;
using AmishSimulator;
using UnityEngine;

namespace AmishSimulator.Tests
{
    public class SurvivalSystemTests
    {
        // ── HungerSystem pure logic ──────────────────────────────────────────

        [Test]
        public void HungerLogic_DepletesOnTick()
        {
            var logic = new HungerLogic(80f, depletionPerHour: 2f);
            logic.Tick(1f);
            Assert.AreEqual(78f, logic.Level, 0.001f);
        }

        [Test]
        public void HungerLogic_NeverGoesBelowZero()
        {
            var logic = new HungerLogic(5f, depletionPerHour: 2f);
            logic.Tick(10f);
            Assert.AreEqual(0f, logic.Level, 0.001f);
        }

        [Test]
        public void HungerLogic_ConsumeFood_RestoresHunger()
        {
            var logic = new HungerLogic(30f, depletionPerHour: 2f);
            logic.ConsumeFood(25f); // CookedMeal restoration
            Assert.AreEqual(55f, logic.Level, 0.001f);
        }

        [Test]
        public void HungerLogic_ConsumeFood_CapsAt100()
        {
            var logic = new HungerLogic(90f, depletionPerHour: 2f);
            logic.ConsumeFood(25f);
            Assert.AreEqual(100f, logic.Level, 0.001f);
        }

        [Test]
        public void HungerLogic_StarvationEvent_FiresAtZero()
        {
            bool fired = false;
            var logic = new HungerLogic(3f, depletionPerHour: 2f);
            logic.OnStarvationDeath += () => fired = true;
            logic.Tick(2f); // drains to 0 (or below, capped at 0)
            Assert.IsTrue(fired);
        }

        [Test]
        public void HungerLogic_State_Full_At80()
        {
            var logic = new HungerLogic(80f, depletionPerHour: 2f);
            Assert.AreEqual(HungerState.Full, logic.GetState());
        }

        [Test]
        public void HungerLogic_State_Hungry_At30()
        {
            var logic = new HungerLogic(30f, depletionPerHour: 2f);
            Assert.AreEqual(HungerState.Hungry, logic.GetState());
        }

        [Test]
        public void HungerLogic_State_Critical_At5()
        {
            var logic = new HungerLogic(5f, depletionPerHour: 2f);
            Assert.AreEqual(HungerState.Critical, logic.GetState());
        }

        [Test]
        public void HungerLogic_State_Starving_At15()
        {
            var logic = new HungerLogic(15f, depletionPerHour: 2f);
            Assert.AreEqual(HungerState.Starving, logic.GetState());
        }

        // ── FoodSystem pure logic ────────────────────────────────────────────

        [Test]
        public void FoodInventory_AddFood_IncreasesQuantity()
        {
            var inv = new FoodInventory();
            inv.Add(FoodType.BreadLoaf, 3);
            Assert.AreEqual(3, inv.Get(FoodType.BreadLoaf));
        }

        [Test]
        public void FoodInventory_ConsumeFood_DecreasesQuantity()
        {
            var inv = new FoodInventory();
            inv.Add(FoodType.BreadLoaf, 3);
            inv.Consume(FoodType.BreadLoaf);
            Assert.AreEqual(2, inv.Get(FoodType.BreadLoaf));
        }

        [Test]
        public void FoodInventory_ConsumeFood_ReturnsNullWhenEmpty()
        {
            var inv = new FoodInventory();
            var result = inv.Consume(FoodType.CookedMeal);
            Assert.IsNull(result);
        }

        [Test]
        public void FoodInventory_GetQuantity_ZeroForUnknown()
        {
            var inv = new FoodInventory();
            Assert.AreEqual(0, inv.Get(FoodType.FreshMilk));
        }

        [Test]
        public void FoodItem_Create_HasCorrectRestoration()
        {
            var item = FoodItem.Create(FoodType.CookedMeal);
            Assert.AreEqual(25f, item.HungerRestoration, 0.001f);
        }

        [Test]
        public void FoodItem_CannedGoods_IsPreserved()
        {
            var item = FoodItem.Create(FoodType.CannedGoods);
            Assert.IsTrue(item.IsPreserved);
        }

        [Test]
        public void FoodItem_RawVegetable_IsNotPreserved()
        {
            var item = FoodItem.Create(FoodType.RawVegetable);
            Assert.IsFalse(item.IsPreserved);
        }
    }

    // ── Pure-logic test helpers ──────────────────────────────────────────────

    public class HungerLogic
    {
        public float Level { get; private set; }
        private readonly float _depletionPerHour;
        private bool _deathFired;

        public event System.Action OnStarvationDeath;

        public HungerLogic(float startLevel, float depletionPerHour)
        {
            Level = startLevel;
            _depletionPerHour = depletionPerHour;
        }

        public void Tick(float hours)
        {
            Level = Mathf.Max(0f, Level - _depletionPerHour * hours);
            if (Level <= 0f && !_deathFired)
            {
                _deathFired = true;
                OnStarvationDeath?.Invoke();
            }
        }

        public void ConsumeFood(float restoration) => Level = Mathf.Min(Level + restoration, 100f);

        public HungerState GetState() => Level switch
        {
            >= 75f => HungerState.Full,
            >= 50f => HungerState.Satisfied,
            >= 25f => HungerState.Hungry,
            >= 10f => HungerState.Starving,
            _      => HungerState.Critical
        };
    }

    public class FoodInventory
    {
        private readonly System.Collections.Generic.Dictionary<FoodType, int> _data = new();

        public void Add(FoodType type, int qty)
        {
            _data[type] = (_data.TryGetValue(type, out int cur) ? cur : 0) + qty;
        }

        public FoodItem Consume(FoodType type)
        {
            if (!_data.TryGetValue(type, out int qty) || qty <= 0) return null;
            _data[type]--;
            return FoodItem.Create(type);
        }

        public int Get(FoodType type) => _data.TryGetValue(type, out int qty) ? qty : 0;
    }
}
