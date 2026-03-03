using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    public class FoodSystem : MonoBehaviour
    {
        public static FoodSystem Instance { get; private set; }

        private readonly Dictionary<FoodType, int> _inventory = new();

        public event Action<FoodType, int> OnFoodAdded;
        public event Action<FoodType> OnFoodDepleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Initialize all food types at zero
            foreach (FoodType ft in Enum.GetValues(typeof(FoodType)))
                _inventory[ft] = 0;
        }

        public void AddFood(FoodType type, int quantity)
        {
            if (quantity <= 0) return;
            _inventory[type] = _inventory.GetValueOrDefault(type) + quantity;
            OnFoodAdded?.Invoke(type, _inventory[type]);

            // Track butter for score
            if (type == FoodType.Butter && GameManager.Instance?.GameStats != null)
                GameManager.Instance.GameStats.AddButter(quantity * 1f); // 1 unit = 1 pound
        }

        public FoodItem ConsumeFood(FoodType type)
        {
            if (!_inventory.TryGetValue(type, out int qty) || qty <= 0)
                return null;

            _inventory[type]--;
            if (_inventory[type] == 0)
                OnFoodDepleted?.Invoke(type);

            return FoodItem.Create(type);
        }

        public int GetQuantity(FoodType type)
        {
            return _inventory.TryGetValue(type, out int qty) ? qty : 0;
        }

        public Dictionary<FoodType, int> GetAllFood() => new(_inventory);

        public bool CanFeedFamily()
        {
            int familySize = 1; // player
            if (PlayerController.Instance != null)
                familySize += PlayerController.Instance.ChildrenCount;

            int totalFood = 0;
            foreach (var kvp in _inventory)
                totalFood += kvp.Value;

            return totalFood >= familySize;
        }

        public bool HasPreservedFood()
        {
            return _inventory.GetValueOrDefault(FoodType.CannedGoods) > 0;
        }
    }
}
