using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TheGodAbove.UI;
using TheGodAbove.Population;

namespace TheGodAbove.Weather
{
    public class DivinePowerCaster : MonoBehaviour
    {
        public static DivinePowerCaster Instance { get; private set; }

        [Header("--- Systems References ---")]
        [SerializeField] private DivinePowerTargetIndicator targetIndicator;
        [SerializeField] private DivinePowerVFXSpawner vfxSpawner;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private PopulationManager populationManager;
        [SerializeField] private HUDController hudController;

        [Header("--- Current State ---")]
        [SerializeField] private DivinePowerType activePower = DivinePowerType.Sunny;

        public event System.Action<DivinePowerType, Vector3, float> OnPowerCast;

        private readonly Dictionary<DivinePowerType, float> cooldownTimers = new Dictionary<DivinePowerType, float>();
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            mainCamera = Camera.main;
        }

        private void Start()
        {
            FindReferencesIfNull();
            BindUIEvents();
        }

        private void FindReferencesIfNull()
        {
            if (targetIndicator == null) targetIndicator = FindFirstObjectByType<DivinePowerTargetIndicator>();
            if (vfxSpawner == null) vfxSpawner = FindFirstObjectByType<DivinePowerVFXSpawner>();
            if (weatherManager == null) weatherManager = WeatherManager.Instance;
            if (populationManager == null) populationManager = PopulationManager.Instance;
            if (hudController == null) hudController = HUDController.Instance;
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void BindUIEvents()
        {
            if (hudController != null)
            {
                hudController.OnPowerSelected += HandlePowerSelected;
            }
        }

        private void OnDestroy()
        {
            if (hudController != null)
            {
                hudController.OnPowerSelected -= HandlePowerSelected;
            }
        }

        private void Update()
        {
            UpdateCooldowns(Time.deltaTime);
            HandleCastInput();
        }

        private void UpdateCooldowns(float delta)
        {
            var keys = new List<DivinePowerType>(cooldownTimers.Keys);
            foreach (var key in keys)
            {
                if (cooldownTimers[key] > 0f)
                {
                    cooldownTimers[key] -= delta;
                    if (cooldownTimers[key] <= 0f)
                    {
                        cooldownTimers[key] = 0f;
                    }
                }
            }
        }

        public void HandlePowerSelected(DivinePowerType power)
        {
            activePower = power;
            var def = DivinePowerDefinition.GetDefault(power);

            // Blessings (Sunny & Rain): cast immediately on the entire island without target reticle!
            if (def.category == PowerCategory.Blessing)
            {
                if (targetIndicator != null)
                {
                    targetIndicator.SetPower(power);
                }

                if (!IsOnCooldown(power))
                {
                    CastBlessing(def);
                }
                else
                {
                    float rem = cooldownTimers.TryGetValue(power, out float c) ? c : 0f;
                    if (hudController != null)
                    {
                        hudController.SetActionPrompt($"⏳ {def.displayName} is recharging ({rem:F0}s remaining)...");
                    }
                }
            }
            // Disasters (Heat, Lightning, Wind, Cold): arm target reticle to aim on the island!
            else
            {
                if (targetIndicator != null)
                {
                    targetIndicator.SetPower(power);
                }

                string prompt = power switch
                {
                    DivinePowerType.Hot => "☄️ Heat: Click the island to call down an Asteroid strike from the sky!",
                    DivinePowerType.Lightning => "⚡ Lightning: Click the island to strike with heavenly Zap lightning!",
                    DivinePowerType.Wind => "🌪️ Wind: Click the island to unleash a destructive Wind storm!",
                    DivinePowerType.Cold => "❄️ Cold: Click the island to unleash a freezing Hail and snow blizzard!",
                    _ => "Click the island to cast divine disaster"
                };

                if (hudController != null)
                {
                    hudController.SetActionPrompt(prompt);
                }
            }
        }

        private void HandleCastInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Block clicks on UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var def = DivinePowerDefinition.GetDefault(activePower);

            // Blessings are handled directly upon selection; only disasters cast via mouse click on ground
            if (def.category == PowerCategory.Blessing) return;

            if (targetIndicator == null || !targetIndicator.IsTargetingValid)
                return;

            Vector3 hitPos = targetIndicator.TargetPosition;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryCastDisaster(def, hitPos);
            }
        }

        public bool IsOnCooldown(DivinePowerType power)
        {
            return cooldownTimers.TryGetValue(power, out float rem) && rem > 0f;
        }

        private void CastBlessing(DivinePowerDefinition def)
        {
            cooldownTimers[def.powerType] = def.cooldownDuration;
            if (hudController != null)
            {
                hudController.TriggerPowerCooldown(def.powerType, def.cooldownDuration);
            }

            // 1. Trigger Visuals
            if (vfxSpawner != null)
            {
                vfxSpawner.SpawnVFX(def.powerType, Vector3.zero, def.aoeRadius);
            }

            // 2. Apply Simulation Impacts
            if (weatherManager != null)
            {
                if (def.powerType == DivinePowerType.Sunny)
                {
                    // Sunny: Bring temperature and moisture towards ideal harvesting levels
                    float targetHarvestTemp = 25f;
                    float targetHarvestMoist = 62f;
                    float tempDiff = targetHarvestTemp - weatherManager.CurrentTemperature;
                    float moistDiff = targetHarvestMoist - weatherManager.CurrentMoisture;
                    weatherManager.ModifyTemperature(Mathf.Clamp(tempDiff * 0.4f, -4f, 4f));
                    weatherManager.ModifyMoisture(Mathf.Clamp(moistDiff * 0.4f, -6f, 6f));
                }
                else if (def.powerType == DivinePowerType.Rainy)
                {
                    // Rain: Hydrates soil (+18%) and cools down (-2°C)
                    weatherManager.ModifyMoisture(18f);
                    weatherManager.ModifyTemperature(-2f);
                }
            }

            if (populationManager != null)
            {
                populationManager.ModifyHappiness(def.happinessDelta);
            }

            OnPowerCast?.Invoke(def.powerType, Vector3.zero, 0f);

            string resultPrompt = def.powerType == DivinePowerType.Sunny
                ? "☀️ Sunny Blessing applied: Warm harvest light and optimal growth conditions!"
                : "🌧️ Rain Blessing applied: Rainfall hydrating the entire island for plant growth!";

            if (hudController != null)
            {
                hudController.SetActionPrompt(resultPrompt);
            }

            Debug.Log($"[DivinePowerCaster] Successfully applied world blessing: {def.displayName}!");
        }

        private void TryCastDisaster(DivinePowerDefinition def, Vector3 targetPos)
        {
            if (IsOnCooldown(def.powerType))
            {
                float rem = cooldownTimers[def.powerType];
                if (hudController != null)
                {
                    hudController.SetActionPrompt($"⏳ {def.displayName} is recharging ({rem:F0}s remaining)...");
                }
                return;
            }

            cooldownTimers[def.powerType] = def.cooldownDuration;
            if (hudController != null)
            {
                hudController.TriggerPowerCooldown(def.powerType, def.cooldownDuration);
            }

            // 1. Spawn Disaster VFX (Asteroid / Zap / Wind / Hail)
            if (vfxSpawner != null)
            {
                vfxSpawner.SpawnVFX(def.powerType, targetPos, def.aoeRadius);
            }

            // 2. Apply Environmental Simulation
            if (weatherManager != null)
            {
                weatherManager.ModifyTemperature(def.temperatureDelta);
                weatherManager.ModifyMoisture(def.moistureDelta);
            }

            // 3. Apply Population Reaction
            if (populationManager != null)
            {
                populationManager.ModifyHappiness(def.happinessDelta);
            }

            OnPowerCast?.Invoke(def.powerType, targetPos, def.aoeRadius);

            // 4. Special Effects (Lightning / Asteroid impact camera shake)
            if (def.powerType == DivinePowerType.Lightning || def.powerType == DivinePowerType.Hot)
            {
                StartCoroutine(CameraShake(0.22f, 0.4f));
            }

            Debug.Log($"[DivinePowerCaster] Cast disaster '{def.displayName}' at {targetPos}!");
        }

        private System.Collections.IEnumerator CameraShake(float duration, float magnitude)
        {
            if (mainCamera == null) yield break;
            Vector3 originalLocalPos = mainCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                mainCamera.transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.localPosition = originalLocalPos;
        }
    }
}
