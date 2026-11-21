using System;
using UnityEngine;
using TheGodAbove.Economy;

namespace TheGodAbove.Progression
{
    public class GodProgressionManager : MonoBehaviour
    {
        public static GodProgressionManager Instance { get; private set; }

        [Header("--- God Level & Stats ---")]
        [SerializeField] private int godLevel = 10;
        [SerializeField] private float cooldownReductionPercent = 20f; // -20%
        [SerializeField] private float productionBonusPercent = 20f;   // +20%
        [SerializeField] private int nextUpgradeCost = 500;
        [SerializeField] private float costScalingFactor = 1.35f;

        public int GodLevel => godLevel;
        public float CooldownDiscountMultiplier => Mathf.Clamp01(1f - (cooldownReductionPercent / 100f));
        public float ProductionMultiplier => 1f + (productionBonusPercent / 100f);
        public int UpgradeCost => nextUpgradeCost;

        public event Action<int, float, float, int, bool> OnGodProgressionChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrenciesChanged += (c, g) => NotifyState();
            }

            NotifyState();
        }

        public bool TryUpgrade()
        {
            if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(nextUpgradeCost))
            {
                godLevel++;
                cooldownReductionPercent = Mathf.Min(60f, cooldownReductionPercent + 2f);
                productionBonusPercent += 2.5f;
                nextUpgradeCost = Mathf.RoundToInt(nextUpgradeCost * costScalingFactor);

                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SetProductionMultiplier(ProductionMultiplier);
                }

                NotifyState();
                return true;
            }

            return false;
        }

        private void NotifyState()
        {
            bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.Coins >= nextUpgradeCost;
            OnGodProgressionChanged?.Invoke(godLevel, -cooldownReductionPercent, productionBonusPercent, nextUpgradeCost, canAfford);
        }
    }
}
