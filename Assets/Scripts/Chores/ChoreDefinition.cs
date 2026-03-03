using UnityEngine;

namespace AmishSimulator
{
    [CreateAssetMenu(fileName = "ChoreDefinition", menuName = "AmishSimulator/Chore Definition")]
    public class ChoreDefinition : ScriptableObject
    {
        public ChoreType choreType;
        public string displayName;
        [TextArea] public string description;
        public int energyCost;
        public Season[] seasonalAvailability;
    }
}
