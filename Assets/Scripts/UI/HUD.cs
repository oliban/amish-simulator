using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmishSimulator
{
    public class HUD : MonoBehaviour
    {
        [Header("Energy")]
        [SerializeField] private Slider energyBar;
        [SerializeField] private TextMeshProUGUI energyText;

        [Header("Hunger")]
        [SerializeField] private Slider hungerBar;
        [SerializeField] private TextMeshProUGUI hungerText;

        [Header("Time")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI seasonDayText;
        [SerializeField] private TextMeshProUGUI yearAgeText;

        [Header("Notification")]
        [SerializeField] private TextMeshProUGUI notificationText;
        private float _notificationTimer;

        private void Start()
        {
            SubscribeToSystems();
        }

        private void SubscribeToSystems()
        {
            if (EnergySystem.Instance != null)
                EnergySystem.Instance.OnEnergyChanged += UpdateEnergyDisplay;

            if (HungerSystem.Instance != null)
                HungerSystem.Instance.OnHungerChanged += UpdateHungerDisplay;

            if (TimeSystem.Instance != null)
            {
                TimeSystem.Instance.OnHourChanged += _ => UpdateTimeDisplay();
                TimeSystem.Instance.OnDayChanged  += _ => UpdateTimeDisplay();
                TimeSystem.Instance.OnYearChanged += _ => UpdateTimeDisplay();
            }

            if (AgingSystem.Instance != null)
                AgingSystem.Instance.OnAgeChanged += UpdateAgeDisplay;
        }

        private void OnDestroy()
        {
            if (EnergySystem.Instance != null)
                EnergySystem.Instance.OnEnergyChanged -= UpdateEnergyDisplay;

            if (HungerSystem.Instance != null)
                HungerSystem.Instance.OnHungerChanged -= UpdateHungerDisplay;
        }

        private void Update()
        {
            if (_notificationTimer > 0f)
            {
                _notificationTimer -= Time.deltaTime;
                if (_notificationTimer <= 0f && notificationText != null)
                    notificationText.text = "";
            }
        }

        public void UpdateEnergyDisplay(int current, int max)
        {
            if (energyBar != null) { energyBar.maxValue = max; energyBar.value = current; }
            if (energyText != null) energyText.text = $"{current}/{max}";
        }

        public void UpdateHungerDisplay(float level)
        {
            if (hungerBar != null) { hungerBar.maxValue = 100f; hungerBar.value = level; }
            if (hungerText != null) hungerText.text = $"{level:F0}%";
        }

        private void UpdateTimeDisplay()
        {
            if (TimeSystem.Instance == null) return;
            int hour = TimeSystem.Instance.CurrentHour;
            int day  = TimeSystem.Instance.CurrentDay;
            Season season = TimeSystem.Instance.CurrentSeason;
            int year = TimeSystem.Instance.CurrentYear;

            string ampm = hour >= 12 ? "PM" : "AM";
            int h12 = hour % 12; if (h12 == 0) h12 = 12;

            if (timeText != null)     timeText.text = $"{h12}:00 {ampm}";
            if (seasonDayText != null) seasonDayText.text = $"{season} Day {day}";
            if (yearAgeText != null)  yearAgeText.text = $"Year {year}";
        }

        public void UpdateAgeDisplay(int age)
        {
            if (yearAgeText != null && TimeSystem.Instance != null)
                yearAgeText.text = $"Year {TimeSystem.Instance.CurrentYear} | Age {age}";
        }

        public void ShowNotification(string message, float duration = 4f)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
                _notificationTimer = duration;
            }
        }
    }
}
