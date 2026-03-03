using UnityEngine;

namespace AmishSimulator
{
    public class SleepPoint : MonoBehaviour, IInteractable
    {
        public string InteractionLabel => "Press E to sleep (end day)";
        public float InteractionRadius => 2.5f;

        public void Interact()
        {
            // Restore energy to full
            EnergySystem.Instance?.RestoreFullEnergy();

            // Sleeping burns a little hunger
            if (HungerSystem.Instance != null)
            {
                float current = HungerSystem.Instance.HungerLevel;
                HungerSystem.Instance.SetHungerLevel(current - 5f);
            }

            // Advance to next day
            TimeSystem.Instance?.AdvanceToNextDay();

            // Show notification
            var hud = FindFirstObjectByType<HUD>();
            if (hud != null) hud.ShowNotification("You slept soundly. A new day dawns.");
        }
    }
}
