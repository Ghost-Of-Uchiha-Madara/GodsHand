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

        private readonly Dictionary<DivinePowerType, float> cooldownTimers = new Dictionary<DivinePowerType, float>();
        private Camera mainCamera;
        private Vector3 lastCastPosition;
        private float continuousCastTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            mainCamera = Camera.main;
            HUDController.OnHUDReady += HandleHUDReady;
        }

        private void HandleHUDReady()
        {
            FindReferencesIfNull();
            BindUIEvents();
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
            HUDController.OnHUDReady -= HandleHUDReady;
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

        private void HandlePowerSelected(DivinePowerType power)
        {
            activePower = power;
            if (targetIndicator != null)
            {
                targetIndicator.SetPower(power);
            }
        }

        private void HandleCastInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Block clicks on UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (targetIndicator == null || !targetIndicator.IsTargetingValid)
                return;

            Vector3 hitPos = targetIndicator.TargetPosition;
            var def = targetIndicator.CurrentDefinition;
            if (def == null) return;

            // Check single click
            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryCastPower(def, hitPos);
            }
            // Drag support for Sunny, Rain, Wind
            else if (mouse.leftButton.isPressed && IsContinuousPower(activePower))
            {
                continuousCastTimer += Time.deltaTime;
                if (continuousCastTimer >= 0.35f && Vector3.Distance(hitPos, lastCastPosition) > 4f)
                {
                    continuousCastTimer = 0f;
                    TryCastPower(def, hitPos);
                }
            }
        }

        private bool IsContinuousPower(DivinePowerType type)
        {
            return type == DivinePowerType.Sunny || type == DivinePowerType.Rainy || type == DivinePowerType.Wind;
        }

        public bool IsOnCooldown(DivinePowerType power)
        {
            return cooldownTimers.TryGetValue(power, out float rem) && rem > 0f;
        }

        private void TryCastPower(DivinePowerDefinition def, Vector3 targetPos)
        {
            if (IsOnCooldown(def.powerType))
            {
                return;
            }

            // Set cooldown
            cooldownTimers[def.powerType] = def.cooldownDuration;
            if (hudController != null)
            {
                hudController.TriggerPowerCooldown(def.powerType, def.cooldownDuration);
            }

            lastCastPosition = targetPos;

            // 1. Spawn VFX
            if (vfxSpawner != null)
            {
                vfxSpawner.SpawnVFX(def.powerType, targetPos, def.aoeRadius);
            }

            // 2. Apply Weather Impact
            if (weatherManager != null)
            {
                weatherManager.ModifyTemperature(def.temperatureDelta);
                weatherManager.ModifyMoisture(def.moistureDelta);
            }

            // 3. Apply Population Impact
            if (populationManager != null)
            {
                populationManager.ModifyHappiness(def.happinessDelta);
            }

            // 4. Special Effects (Lightning Camera Shake)
            if (def.powerType == DivinePowerType.Lightning && mainCamera != null)
            {
                StartCoroutine(CameraShake(0.2f, 0.35f));
            }

            Debug.Log($"[DivinePowerCaster] Cast divine power '{def.displayName}' at {targetPos}!");
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
