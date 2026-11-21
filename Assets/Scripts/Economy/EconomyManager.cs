using System;
using UnityEngine;
using TheGodAbove.Core;
using TheGodAbove.Population;

namespace TheGodAbove.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("--- Currencies ---")]
        [SerializeField] private int currentCoins = 12450;
        [SerializeField] private int currentGems = 84;

        [Header("--- Production Settings ---")]
        [SerializeField] private float baseCoinRatePerPerson = 0.05f; // coins per tick per person
        [SerializeField] private float productionSpeedMultiplier = 1.2f; // +20% default

        public int Coins => currentCoins;
        public int Gems => currentGems;
        public float ProductionMultiplier => productionSpeedMultiplier;

        public event Action<int, int> OnCurrenciesChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (WorldSimulationManager.Instance != null)
            {
                WorldSimulationManager.Instance.OnSimulationTick += GeneratePassiveIncome;
            }

            OnCurrenciesChanged?.Invoke(currentCoins, currentGems);
        }

        private void OnDestroy()
        {
            if (WorldSimulationManager.Instance != null)
            {
                WorldSimulationManager.Instance.OnSimulationTick -= GeneratePassiveIncome;
            }
        }

        private void GeneratePassiveIncome()
        {
            int pop = PopulationManager.Instance != null ? PopulationManager.Instance.CurrentPopulation : 100;
            float happyMultiplier = PopulationManager.Instance != null ? (PopulationManager.Instance.CurrentHappiness / 100f) : 1f;

            float generated = pop * baseCoinRatePerPerson * happyMultiplier * productionSpeedMultiplier;
            int wholeCoins = Mathf.FloorToInt(generated);

            if (wholeCoins > 0)
            {
                AddCoins(wholeCoins);
            }
        }

        public void AddCoins(int amount)
        {
            currentCoins = Mathf.Max(0, currentCoins + amount);
            OnCurrenciesChanged?.Invoke(currentCoins, currentGems);
        }

        public bool SpendCoins(int amount)
        {
            if (currentCoins >= amount)
            {
                currentCoins -= amount;
                OnCurrenciesChanged?.Invoke(currentCoins, currentGems);
                return true;
            }
            return false;
        }

        public void AddGems(int amount)
        {
            currentGems = Mathf.Max(0, currentGems + amount);
            OnCurrenciesChanged?.Invoke(currentCoins, currentGems);
        }

        public bool SpendGems(int amount)
        {
            if (currentGems >= amount)
            {
                currentGems -= amount;
                OnCurrenciesChanged?.Invoke(currentCoins, currentGems);
                return true;
            }
            return false;
        }

        public void SetProductionMultiplier(float multiplier)
        {
            productionSpeedMultiplier = multiplier;
        }
    }
}
