using System;
using UnityEngine;
using TheGodAbove.Core;
using TheGodAbove.Weather;

namespace TheGodAbove.Population
{
    public class PopulationManager : MonoBehaviour
    {
        public static PopulationManager Instance { get; private set; }

        [Header("--- Population Counts ---")]
        [SerializeField] private int currentPopulation = 128;
        [SerializeField] private int minPopulation = 10;
        [SerializeField] private int maxPopulation = 500;
        [SerializeField] private int populationDailyTrend = 2;

        [Header("--- Happiness (0 - 100%) ---")]
        [Range(0f, 100f)]
        [SerializeField] private float currentHappiness = 82f;

        public int CurrentPopulation => currentPopulation;
        public int DailyTrend => populationDailyTrend;
        public float CurrentHappiness => currentHappiness;

        public event Action<int, int> OnPopulationChanged;
        public event Action<float> OnHappinessChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (WorldSimulationManager.Instance != null)
            {
                WorldSimulationManager.Instance.OnDayChanged += HandleDayChanged;
                WorldSimulationManager.Instance.OnSimulationTick += CalculateHappiness;
            }

            OnPopulationChanged?.Invoke(currentPopulation, populationDailyTrend);
            OnHappinessChanged?.Invoke(currentHappiness);
        }

        private void OnDestroy()
        {
            if (WorldSimulationManager.Instance != null)
            {
                WorldSimulationManager.Instance.OnDayChanged -= HandleDayChanged;
                WorldSimulationManager.Instance.OnSimulationTick -= CalculateHappiness;
            }
        }

        private void CalculateHappiness()
        {
            float temp = WeatherManager.Instance != null ? WeatherManager.Instance.CurrentTemperature : 24f;
            float moist = WeatherManager.Instance != null ? WeatherManager.Instance.CurrentMoisture : 65f;

            // Ideal temp is between 20°C and 28°C
            float tempPenalty = 0f;
            if (temp < 15f) tempPenalty = Mathf.Abs(15f - temp) * 1.5f;
            else if (temp > 30f) tempPenalty = (temp - 30f) * 2.0f;

            // Ideal moisture is between 40% and 80%
            float moistPenalty = 0f;
            if (moist < 35f) moistPenalty = (35f - moist) * 0.8f;
            else if (moist > 85f) moistPenalty = (moist - 85f) * 0.8f;

            float targetHappiness = 100f - tempPenalty - moistPenalty;
            targetHappiness = Mathf.Clamp(targetHappiness, 5f, 100f);

            // Smooth interpolation
            currentHappiness = Mathf.Lerp(currentHappiness, targetHappiness, Time.deltaTime * 0.5f);
            OnHappinessChanged?.Invoke(currentHappiness);

            // Recalculate daily trend based on happiness
            if (currentHappiness >= 75f) populationDailyTrend = UnityEngine.Random.Range(2, 4);
            else if (currentHappiness >= 55f) populationDailyTrend = UnityEngine.Random.Range(1, 2);
            else if (currentHappiness >= 40f) populationDailyTrend = 0;
            else if (currentHappiness >= 25f) populationDailyTrend = -1;
            else populationDailyTrend = -UnityEngine.Random.Range(2, 4);
        }

        private void HandleDayChanged(int day)
        {
            // Apply daily population change
            currentPopulation = Mathf.Clamp(currentPopulation + populationDailyTrend, minPopulation, maxPopulation);
            OnPopulationChanged?.Invoke(currentPopulation, populationDailyTrend);
        }

        public void ModifyHappiness(float delta)
        {
            currentHappiness = Mathf.Clamp(currentHappiness + delta, 0f, 100f);
            OnHappinessChanged?.Invoke(currentHappiness);
        }
    }
}
