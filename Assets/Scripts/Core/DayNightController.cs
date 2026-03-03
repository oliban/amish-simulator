using UnityEngine;

namespace AmishSimulator
{
    public class DayNightController : MonoBehaviour
    {
        private Camera _mainCamera;
        private Light _directionalLight;

        // Color keyframes
        private static readonly Color DawnColor    = new Color(1.0f, 0.6f, 0.3f);
        private static readonly Color MorningColor = new Color(0.53f, 0.74f, 0.90f);
        private static readonly Color EveningColor = new Color(0.9f, 0.45f, 0.2f);
        private static readonly Color NightColor   = new Color(0.05f, 0.05f, 0.15f);

        private const float DayLightIntensity   = 1.2f;
        private const float NightLightIntensity = 0.15f;

        private void Start()
        {
            _mainCamera = Camera.main;
            _directionalLight = FindDirectionalLight();
        }

        private Light FindDirectionalLight()
        {
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                    return light;
            }
            return null;
        }

        private void Update()
        {
            if (TimeSystem.Instance == null || _mainCamera == null) return;

            int hour = TimeSystem.Instance.CurrentHour;
            Color skyColor = EvaluateSkyColor(hour);
            _mainCamera.backgroundColor = skyColor;

            if (_directionalLight != null)
            {
                _directionalLight.intensity = EvaluateLightIntensity(hour);
            }
        }

        private Color EvaluateSkyColor(int hour)
        {
            // Night (21-5): NightColor
            // Dawn (5-8): NightColor -> DawnColor -> MorningColor
            // Day (8-17): MorningColor
            // Evening (17-21): MorningColor -> EveningColor -> NightColor

            if (hour >= 21 || hour < 5)
            {
                return NightColor;
            }
            else if (hour >= 5 && hour < 8)
            {
                float t = (hour - 5) / 3f;
                if (t < 0.5f)
                    return Color.Lerp(NightColor, DawnColor, t * 2f);
                else
                    return Color.Lerp(DawnColor, MorningColor, (t - 0.5f) * 2f);
            }
            else if (hour >= 8 && hour < 17)
            {
                return MorningColor;
            }
            else // 17-21
            {
                float t = (hour - 17) / 4f;
                if (t < 0.5f)
                    return Color.Lerp(MorningColor, EveningColor, t * 2f);
                else
                    return Color.Lerp(EveningColor, NightColor, (t - 0.5f) * 2f);
            }
        }

        private float EvaluateLightIntensity(int hour)
        {
            if (hour >= 21 || hour < 5)
                return NightLightIntensity;
            else if (hour >= 5 && hour < 8)
                return Mathf.Lerp(NightLightIntensity, DayLightIntensity, (hour - 5) / 3f);
            else if (hour >= 8 && hour < 17)
                return DayLightIntensity;
            else // 17-21
                return Mathf.Lerp(DayLightIntensity, NightLightIntensity, (hour - 17) / 4f);
        }
    }
}
