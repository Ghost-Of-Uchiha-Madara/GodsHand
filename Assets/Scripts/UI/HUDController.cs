using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace TheGodAbove.UI
{
    public enum DivinePowerType
    {
        Sunny = 0,
        Rainy = 1,
        Hot = 2,
        Lightning = 3,
        Wind = 4,
        Cold = 5
    }

    [System.Serializable]
    public class DivinePowerSlotUI
    {
        public DivinePowerType powerType;
        public Button button;
        public Image iconImage;
        public GameObject selectedGlow;
        public Image cooldownRadial;
        public TMP_Text cooldownTimerText;
        public TMP_Text hotkeyBadgeText;
        public TMP_Text nameText;

        [HideInInspector] public float cooldownRemaining;
        [HideInInspector] public float cooldownDuration;
    }

    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("--- Top-Left: Currencies ---")]
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text gemsText;

        [Header("--- Top-Right: Quick Menu & World Info ---")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private TMP_Text peopleText;
        [SerializeField] private TMP_Text peopleTrendText;
        [SerializeField] private TMP_Text temperatureText;
        [SerializeField] private Slider soilMoistureSlider;
        [SerializeField] private TMP_Text soilMoistureText;

        [Header("--- Left Panel: God Progression ---")]
        [SerializeField] private Image statuePortraitImage;
        [SerializeField] private TMP_Text godLevelText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text upgradeCostText;
        [SerializeField] private TMP_Text cooldownStatText;
        [SerializeField] private TMP_Text productionStatText;
        [SerializeField] private Slider happinessSlider;
        [SerializeField] private Image happinessFillImage;
        [SerializeField] private TMP_Text happinessPercentText;
        [SerializeField] private TMP_Text happinessStateText;

        [Header("--- Bottom Center: Action Bar & Prompts ---")]
        [SerializeField] private TMP_Text actionPromptText;
        [SerializeField] private List<DivinePowerSlotUI> powerSlots = new List<DivinePowerSlotUI>();

        [Header("--- Modals: Pause & Settings ---")]
        [SerializeField] private GameObject pauseMenuModal;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsModalButton;
        [SerializeField] private Button quitButton;

        [Header("--- Runtime Values (Debug / Preview) ---")]
        public int currentCoins = 12450;
        public int currentGems = 84;
        public int currentDay = 1;
        public int currentPopulation = 128;
        public int currentPopTrend = 2; // +2/day
        public float currentTemperature = 24f;
        public float currentMoisture = 65f;
        public int currentGodLevel = 10;
        public float currentCooldownBonus = -20f;
        public float currentProductionBonus = 20f;
        public int currentUpgradeCost = 500;
        public float currentHappiness = 82f;
        public DivinePowerType selectedPower = DivinePowerType.Sunny;

        public event Action<DivinePowerType> OnPowerSelected;
        public event Action OnUpgradeClicked;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            SetupButtonCallbacks();
            RefreshAllUI();
            SelectPower(selectedPower);
        }

        private void Update()
        {
            HandleKeyboardInput();
            UpdateCooldownTimers(Time.deltaTime);
        }

        private void SetupButtonCallbacks()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => TogglePauseMenu(true));

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => TogglePauseMenu(true));

            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => TogglePauseMenu(false));

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(HandleUpgradeClick);

            foreach (var slot in powerSlots)
            {
                var capturedSlot = slot;
                if (capturedSlot.button != null)
                {
                    capturedSlot.button.onClick.AddListener(() => SelectPower(capturedSlot.powerType));
                }
            }
        }

        private void HandleKeyboardInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) SelectPower(DivinePowerType.Sunny);
            else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) SelectPower(DivinePowerType.Rainy);
            else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) SelectPower(DivinePowerType.Hot);
            else if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame) SelectPower(DivinePowerType.Lightning);
            else if (kb.digit5Key.wasPressedThisFrame || kb.numpad5Key.wasPressedThisFrame) SelectPower(DivinePowerType.Wind);
            else if (kb.digit6Key.wasPressedThisFrame || kb.numpad6Key.wasPressedThisFrame) SelectPower(DivinePowerType.Cold);

            if (kb.escapeKey.wasPressedThisFrame)
            {
                bool isCurrentlyOpen = pauseMenuModal != null && pauseMenuModal.activeSelf;
                TogglePauseMenu(!isCurrentlyOpen);
            }
        }

        private void UpdateCooldownTimers(float delta)
        {
            foreach (var slot in powerSlots)
            {
                if (slot.cooldownRemaining > 0f)
                {
                    slot.cooldownRemaining -= delta;
                    if (slot.cooldownRemaining <= 0f)
                    {
                        slot.cooldownRemaining = 0f;
                        if (slot.cooldownRadial != null) slot.cooldownRadial.fillAmount = 0f;
                        if (slot.cooldownTimerText != null) slot.cooldownTimerText.gameObject.SetActive(false);
                    }
                    else
                    {
                        float fill = slot.cooldownRemaining / Mathf.Max(0.01f, slot.cooldownDuration);
                        if (slot.cooldownRadial != null) slot.cooldownRadial.fillAmount = fill;
                        if (slot.cooldownTimerText != null)
                        {
                            slot.cooldownTimerText.gameObject.SetActive(true);
                            slot.cooldownTimerText.text = $"{slot.cooldownRemaining:F0}s";
                        }
                    }
                }
            }
        }

        // ==========================================
        // Public API
        // ==========================================

        public void RefreshAllUI()
        {
            UpdateCurrencies(currentCoins, currentGems);
            UpdateWorldInfo(currentDay, currentPopulation, currentPopTrend, currentTemperature, currentMoisture);
            UpdateGodProgression(currentGodLevel, currentCooldownBonus, currentProductionBonus, currentUpgradeCost, currentCoins >= currentUpgradeCost);
            UpdateHappiness(currentHappiness);
        }

        public void UpdateCurrencies(int coins, int gems)
        {
            currentCoins = coins;
            currentGems = gems;
            if (coinsText != null) coinsText.text = $"{coins:N0}";
            if (gemsText != null) gemsText.text = $"{gems:N0}";
        }

        public void UpdateWorldInfo(int day, int people, int trend, float temperature, float moisture)
        {
            currentDay = day;
            currentPopulation = people;
            currentPopTrend = trend;
            currentTemperature = temperature;
            currentMoisture = moisture;

            if (dayText != null) dayText.text = $"DAY {day}";
            if (peopleText != null) peopleText.text = $"{people:N0}";
            if (peopleTrendText != null)
            {
                if (trend > 0) peopleTrendText.text = $"<color=#64FF64>+{trend}/d</color>";
                else if (trend < 0) peopleTrendText.text = $"<color=#FF6464>{trend}/d</color>";
                else peopleTrendText.text = "<color=#AAAAAA>--</color>";
            }

            if (temperatureText != null)
            {
                temperatureText.text = $"{temperature:F0}\u00B0C";
                if (temperature < 0) temperatureText.color = new Color(0.4f, 0.8f, 1f);
                else if (temperature > 35) temperatureText.color = new Color(1f, 0.45f, 0.2f);
                else temperatureText.color = Color.white;
            }

            if (soilMoistureSlider != null) soilMoistureSlider.value = Mathf.Clamp01(moisture / 100f);
            if (soilMoistureText != null) soilMoistureText.text = $"{moisture:F0}%";
        }

        public void UpdateGodProgression(int level, float cdBonus, float prodBonus, int upgradeCost, bool canAfford)
        {
            currentGodLevel = level;
            currentCooldownBonus = cdBonus;
            currentProductionBonus = prodBonus;
            currentUpgradeCost = upgradeCost;

            if (godLevelText != null) godLevelText.text = $"{level}";
            if (cooldownStatText != null) cooldownStatText.text = $"{cdBonus:F0}%";
            if (productionStatText != null) productionStatText.text = $"+{prodBonus:F0}%";

            if (upgradeCostText != null)
                upgradeCostText.text = $"{upgradeCost:N0}";

            if (upgradeButton != null)
                upgradeButton.interactable = canAfford;
        }

        public void UpdateHappiness(float happinessPercent)
        {
            currentHappiness = Mathf.Clamp(happinessPercent, 0f, 100f);

            if (happinessSlider != null)
                happinessSlider.value = currentHappiness / 100f;

            if (happinessPercentText != null)
                happinessPercentText.text = $"{currentHappiness:F0}%";

            // State label & fill color based on GDD Section 7.1
            string state;
            Color barColor;

            if (currentHappiness >= 81f)
            {
                state = "VERY HAPPY";
                barColor = new Color(0.2f, 0.85f, 0.3f);
            }
            else if (currentHappiness >= 61f)
            {
                state = "HAPPY";
                barColor = new Color(0.35f, 0.85f, 0.35f);
            }
            else if (currentHappiness >= 41f)
            {
                state = "NORMAL";
                barColor = new Color(0.9f, 0.8f, 0.2f);
            }
            else if (currentHappiness >= 21f)
            {
                state = "UNHAPPY";
                barColor = new Color(1f, 0.5f, 0.2f);
            }
            else
            {
                state = "MISERABLE";
                barColor = new Color(0.9f, 0.2f, 0.2f);
            }

            if (happinessStateText != null)
            {
                happinessStateText.text = state;
                happinessStateText.color = barColor;
            }

            if (happinessFillImage != null)
            {
                happinessFillImage.color = barColor;
            }
        }

        public void SelectPower(DivinePowerType power)
        {
            selectedPower = power;

            foreach (var slot in powerSlots)
            {
                bool isSelected = slot.powerType == power;
                if (slot.selectedGlow != null) slot.selectedGlow.SetActive(isSelected);
            }

            string prompt = power switch
            {
                DivinePowerType.Sunny => "Sunny: Click or drag to bring sunlight and warm the land",
                DivinePowerType.Rainy => "Rainy: Click or drag to create rain clouds and hydrate soil",
                DivinePowerType.Hot => "Hot: Click to scorch the area with intense heat",
                DivinePowerType.Lightning => "Lightning: Click to strike lightning at the island",
                DivinePowerType.Wind => "Wind: Click and drag to unleash a gust of directional wind",
                DivinePowerType.Cold => "Cold: Click to chill the area with frost and snow",
                _ => "Select a divine power, then click the island"
            };

            SetActionPrompt(prompt);
            OnPowerSelected?.Invoke(power);
        }

        public void TriggerPowerCooldown(DivinePowerType power, float duration)
        {
            foreach (var slot in powerSlots)
            {
                if (slot.powerType == power)
                {
                    slot.cooldownDuration = duration;
                    slot.cooldownRemaining = duration;
                    if (slot.cooldownRadial != null) slot.cooldownRadial.fillAmount = 1f;
                    if (slot.cooldownTimerText != null)
                    {
                        slot.cooldownTimerText.gameObject.SetActive(true);
                        slot.cooldownTimerText.text = $"{duration:F0}s";
                    }
                    break;
                }
            }
        }

        public void SetActionPrompt(string message)
        {
            if (actionPromptText != null) actionPromptText.text = message;
        }

        public void TogglePauseMenu(bool show)
        {
            if (pauseMenuModal != null)
            {
                pauseMenuModal.SetActive(show);
                Time.timeScale = show ? 0f : 1f;
            }
        }

        private void HandleUpgradeClick()
        {
            if (currentCoins >= currentUpgradeCost)
            {
                currentCoins -= currentUpgradeCost;
                currentGodLevel++;
                currentCooldownBonus -= 2f;
                currentProductionBonus += 2f;
                currentUpgradeCost = Mathf.RoundToInt(currentUpgradeCost * 1.5f);
                RefreshAllUI();
                OnUpgradeClicked?.Invoke();
            }
        }
    }
}
