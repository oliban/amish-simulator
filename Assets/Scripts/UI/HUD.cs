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

        [Header("Interaction")]
        [SerializeField] private TextMeshProUGUI interactionPrompt;

        private static readonly Color OffWhite = new Color(0.92f, 0.90f, 0.85f);
        private static readonly Color BarBackground = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        private static readonly Color EnergyColor = new Color(0.85f, 0.75f, 0.35f);
        private static readonly Color HungerColor = new Color(0.6f, 0.45f, 0.25f);

        private void Awake()
        {
            BuildUI();
            SubscribeToSystems();
        }

        private void BuildUI()
        {
            if (energyBar != null) return; // Already wired

            // Energy Panel — top-left
            var energyPanel = CreatePanel("EnergyPanel");
            var energyRT = energyPanel.GetComponent<RectTransform>();
            SetAnchor(energyRT, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            energyRT.anchoredPosition = new Vector2(20, -20);
            energyRT.sizeDelta = new Vector2(200, 50);

            energyText = CreateLabel(energyPanel.transform, "EnergyLabel", "Energy", 14);
            var energyLabelRT = energyText.GetComponent<RectTransform>();
            SetAnchor(energyLabelRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            energyLabelRT.anchoredPosition = new Vector2(0, 0);
            energyLabelRT.sizeDelta = new Vector2(0, 20);

            energyBar = CreateSlider(energyPanel.transform, "EnergyBar", EnergyColor);
            var energyBarRT = energyBar.GetComponent<RectTransform>();
            SetAnchor(energyBarRT, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            energyBarRT.anchoredPosition = new Vector2(0, 10);
            energyBarRT.sizeDelta = new Vector2(-10, 16);

            // Hunger Panel — below energy
            var hungerPanel = CreatePanel("HungerPanel");
            var hungerRT = hungerPanel.GetComponent<RectTransform>();
            SetAnchor(hungerRT, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            hungerRT.anchoredPosition = new Vector2(20, -80);
            hungerRT.sizeDelta = new Vector2(200, 50);

            hungerText = CreateLabel(hungerPanel.transform, "HungerLabel", "Hunger", 14);
            var hungerLabelRT = hungerText.GetComponent<RectTransform>();
            SetAnchor(hungerLabelRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            hungerLabelRT.anchoredPosition = new Vector2(0, 0);
            hungerLabelRT.sizeDelta = new Vector2(0, 20);

            hungerBar = CreateSlider(hungerPanel.transform, "HungerBar", HungerColor);
            var hungerBarRT = hungerBar.GetComponent<RectTransform>();
            SetAnchor(hungerBarRT, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            hungerBarRT.anchoredPosition = new Vector2(0, 10);
            hungerBarRT.sizeDelta = new Vector2(-10, 16);

            // Time Panel — top-center
            var timePanel = CreatePanel("TimePanel");
            var timePanelRT = timePanel.GetComponent<RectTransform>();
            SetAnchor(timePanelRT, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            timePanelRT.anchoredPosition = new Vector2(0, -20);
            timePanelRT.sizeDelta = new Vector2(250, 80);

            timeText = CreateLabel(timePanel.transform, "TimeText", "12:00 PM", 20);
            var timeTextRT = timeText.GetComponent<RectTransform>();
            SetAnchor(timeTextRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            timeTextRT.anchoredPosition = new Vector2(0, -5);
            timeTextRT.sizeDelta = new Vector2(0, 26);
            timeText.alignment = TextAlignmentOptions.Center;

            seasonDayText = CreateLabel(timePanel.transform, "SeasonDayText", "Spring Day 1", 14);
            var seasonRT = seasonDayText.GetComponent<RectTransform>();
            SetAnchor(seasonRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            seasonRT.anchoredPosition = new Vector2(0, -32);
            seasonRT.sizeDelta = new Vector2(0, 20);
            seasonDayText.alignment = TextAlignmentOptions.Center;

            yearAgeText = CreateLabel(timePanel.transform, "YearAgeText", "Year 1", 14);
            var yearRT = yearAgeText.GetComponent<RectTransform>();
            SetAnchor(yearRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            yearRT.anchoredPosition = new Vector2(0, -54);
            yearRT.sizeDelta = new Vector2(0, 20);
            yearAgeText.alignment = TextAlignmentOptions.Center;

            // Notification Text — bottom-center
            var notifGo = new GameObject("NotificationText");
            notifGo.transform.SetParent(transform, false);
            notificationText = notifGo.AddComponent<TextMeshProUGUI>();
            notificationText.fontSize = 18;
            notificationText.color = OffWhite;
            notificationText.alignment = TextAlignmentOptions.Center;
            notificationText.text = "";
            var notifRT = notifGo.GetComponent<RectTransform>();
            SetAnchor(notifRT, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            notifRT.anchoredPosition = new Vector2(0, 60);
            notifRT.sizeDelta = new Vector2(500, 30);

            // Interaction Prompt — bottom-center above notification
            var promptGo = new GameObject("InteractionPrompt");
            promptGo.transform.SetParent(transform, false);
            interactionPrompt = promptGo.AddComponent<TextMeshProUGUI>();
            interactionPrompt.fontSize = 16;
            interactionPrompt.color = OffWhite;
            interactionPrompt.alignment = TextAlignmentOptions.Center;
            interactionPrompt.text = "";
            var promptRT = promptGo.GetComponent<RectTransform>();
            SetAnchor(promptRT, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            promptRT.anchoredPosition = new Vector2(0, 100);
            promptRT.sizeDelta = new Vector2(400, 30);
        }

        private GameObject CreatePanel(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = BarBackground;
            return go;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string name, string defaultText, float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.color = OffWhite;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }

        private Slider CreateSlider(Transform parent, string name, Color fillColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRT = bgGo.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Fill Area
            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(go.transform, false);
            var fillAreaRT = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRT = fillGo.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.targetGraphic = bgImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            slider.interactable = false;

            return slider;
        }

        private void SetAnchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
        }

        public void SetInteractionPrompt(string text)
        {
            if (interactionPrompt != null) interactionPrompt.text = text;
        }

        public void ClearInteractionPrompt()
        {
            if (interactionPrompt != null) interactionPrompt.text = "";
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
