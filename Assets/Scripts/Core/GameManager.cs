using UnityEngine;
using TheGodAbove.UI;
using TheGodAbove.Weather;
using TheGodAbove.Population;
using TheGodAbove.Economy;
using TheGodAbove.Progression;

namespace TheGodAbove.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("--- Systems References (Auto-found if null) ---")]
        [SerializeField] private WorldSimulationManager simulationManager;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private PopulationManager populationManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private GodProgressionManager progressionManager;
        [SerializeField] private HUDController hudController;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            FindSystemsIfNull();
        }

        private void Start()
        {
            BindAllSystemsToUI();
        }

        private void FindSystemsIfNull()
        {
            if (simulationManager == null) simulationManager = FindFirstObjectByType<WorldSimulationManager>();
            if (weatherManager == null) weatherManager = FindFirstObjectByType<WeatherManager>();
            if (populationManager == null) populationManager = FindFirstObjectByType<PopulationManager>();
            if (economyManager == null) economyManager = FindFirstObjectByType<EconomyManager>();
            if (progressionManager == null) progressionManager = FindFirstObjectByType<GodProgressionManager>();
            if (hudController == null) hudController = FindFirstObjectByType<HUDController>();
        }

        private void BindAllSystemsToUI()
        {
            if (hudController == null) hudController = HUDController.Instance;
            if (hudController == null) return;

            // Currencies
            if (economyManager != null)
            {
                economyManager.OnCurrenciesChanged += (coins, gems) =>
                {
                    hudController.UpdateCurrencies(coins, gems);
                };
                hudController.UpdateCurrencies(economyManager.Coins, economyManager.Gems);
            }

            // World Day
            if (simulationManager != null)
            {
                simulationManager.OnDayChanged += (day) =>
                {
                    UpdateWorldHUD();
                };
            }

            // Weather & Environment
            if (weatherManager != null)
            {
                weatherManager.OnTemperatureChanged += (temp) => UpdateWorldHUD();
                weatherManager.OnMoistureChanged += (moist) => UpdateWorldHUD();
            }

            // Population & Happiness
            if (populationManager != null)
            {
                populationManager.OnPopulationChanged += (pop, trend) => UpdateWorldHUD();
                populationManager.OnHappinessChanged += (happy) =>
                {
                    hudController.UpdateHappiness(happy);
                };
                hudController.UpdateHappiness(populationManager.CurrentHappiness);
            }

            // God Progression
            if (progressionManager != null)
            {
                progressionManager.OnGodProgressionChanged += (level, cdBonus, prodBonus, cost, canAfford) =>
                {
                    hudController.UpdateGodProgression(level, cdBonus, prodBonus, cost, canAfford);
                };

                hudController.OnUpgradeClicked += () =>
                {
                    progressionManager.TryUpgrade();
                };
            }

            UpdateWorldHUD();
        }

        private void UpdateWorldHUD()
        {
            if (hudController == null) return;

            int day = simulationManager != null ? simulationManager.CurrentDay : 1;
            int pop = populationManager != null ? populationManager.CurrentPopulation : 128;
            int trend = populationManager != null ? populationManager.DailyTrend : 2;
            float temp = weatherManager != null ? weatherManager.CurrentTemperature : 24f;
            float moist = weatherManager != null ? weatherManager.CurrentMoisture : 65f;

            hudController.UpdateWorldInfo(day, pop, trend, temp, moist);
        }
    }
}
