using System;
using UnityEngine;
using TheGodAbove.Core;

namespace TheGodAbove.Weather
{
    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("--- Temperature Settings (°C) ---")]
        [SerializeField] private float baselineTemperature = 24f;
        [SerializeField] private float currentTemperature = 24f;
        [SerializeField] private float temperatureEquilibriumSpeed = 0.05f;

        [Header("--- Soil Moisture Settings (0 - 100) ---")]
        [SerializeField] private float baselineMoisture = 65f;
        [SerializeField] private float currentMoisture = 65f;
        [SerializeField] private float moistureEquilibriumSpeed = 0.05f;

        [Header("--- Sky & Atmospheric Color ---")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Color normalSunColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] private Color hotSunColor = new Color(1f, 0.65f, 0.3f);
        [SerializeField] private Color coldSunColor = new Color(0.7f, 0.85f, 1f);

        public float CurrentTemperature => currentTemperature;
        public float CurrentMoisture => currentMoisture;

        public event Action<float> OnTemperatureChanged;
        public event Action<float> OnMoistureChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (sunLight == null)
            {
                var sunObj = GameObject.Find("Directional Light");
                if (sunObj != null) sunLight = sunObj.GetComponent<Light>();
            }

            if (WorldSimulationManager.Instance != null)
            {
                WorldSimulationManager.Instance.OnSimulationTick += HandleSimulationTick;
            }

            OnTemperatureChanged?.Invoke(currentTemperature);
            OnMoistureChanged?.Invoke(currentMoisture);
        }

        private void OnDestroy()
        {
            if (WorldSimulationManager.Instance != null)
            {
                WorldSimulationManager.Instance.OnSimulationTick -= HandleSimulationTick;
            }
        }

        private void HandleSimulationTick()
        {
            // Passive drift toward baseline
            if (Mathf.Abs(currentTemperature - baselineTemperature) > 0.05f)
            {
                currentTemperature = Mathf.MoveTowards(currentTemperature, baselineTemperature, temperatureEquilibriumSpeed);
                OnTemperatureChanged?.Invoke(currentTemperature);
            }

            if (Mathf.Abs(currentMoisture - baselineMoisture) > 0.05f)
            {
                currentMoisture = Mathf.MoveTowards(currentMoisture, baselineMoisture, moistureEquilibriumSpeed);
                OnMoistureChanged?.Invoke(currentMoisture);
            }

            UpdateLightingColor();
        }

        public void ModifyTemperature(float delta)
        {
            currentTemperature = Mathf.Clamp(currentTemperature + delta, -20f, 60f);
            OnTemperatureChanged?.Invoke(currentTemperature);
            UpdateLightingColor();
        }

        public void ModifyMoisture(float delta)
        {
            currentMoisture = Mathf.Clamp(currentMoisture + delta, 0f, 100f);
            OnMoistureChanged?.Invoke(currentMoisture);
        }

        private void UpdateLightingColor()
        {
            if (sunLight == null) return;

            if (currentTemperature > 35f)
            {
                float t = Mathf.InverseLerp(35f, 50f, currentTemperature);
                sunLight.color = Color.Lerp(normalSunColor, hotSunColor, t);
            }
            else if (currentTemperature < 5f)
            {
                float t = Mathf.InverseLerp(5f, -15f, currentTemperature);
                sunLight.color = Color.Lerp(normalSunColor, coldSunColor, t);
            }
            else
            {
                sunLight.color = normalSunColor;
            }
        }
    }
}
