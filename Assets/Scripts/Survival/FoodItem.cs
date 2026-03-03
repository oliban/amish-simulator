using System;

namespace AmishSimulator
{
    public enum FoodType { RawVegetable, CookedMeal, BreadLoaf, CannedGoods, Butter, FreshMilk }

    [Serializable]
    public class FoodItem
    {
        public FoodType Type;
        public string DisplayName;
        public float HungerRestoration;
        public bool IsPreserved;

        private FoodItem(FoodType type, string name, float restoration, bool preserved = false)
        {
            Type = type;
            DisplayName = name;
            HungerRestoration = restoration;
            IsPreserved = preserved;
        }

        public static FoodItem Create(FoodType type) => type switch
        {
            FoodType.RawVegetable => new FoodItem(type, "Raw Vegetable", 5f),
            FoodType.CookedMeal   => new FoodItem(type, "Cooked Meal",   25f),
            FoodType.BreadLoaf    => new FoodItem(type, "Bread Loaf",    20f),
            FoodType.CannedGoods  => new FoodItem(type, "Canned Goods",  18f, preserved: true),
            FoodType.Butter       => new FoodItem(type, "Butter",         8f),
            FoodType.FreshMilk    => new FoodItem(type, "Fresh Milk",    12f),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
