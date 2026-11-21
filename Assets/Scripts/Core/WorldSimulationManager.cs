using System;
using UnityEngine;

namespace TheGodAbove.Core
{
    public class WorldSimulationManager : MonoBehaviour
    {
        public static WorldSimulationManager Instance { get; private set; }

        [Header("--- Day & Time Config ---")]
        [Tooltip("Real-time seconds per in-game day (default 60s)")]
        [SerializeField] private float dayDurationSeconds = 60f;
        [SerializeField] private int currentDay = 1;
        [SerializeField] private bool isSimulationRunning = true;

        [Header("--- Sun & Lighting Direction ---")]
        [SerializeField] private Light directionalSun;
        [SerializeField] private Vector3 sunAxis = new Vector3(50f, -30f, 0f);

        [Header("--- Simulation Tick ---")]
        [Tooltip("Interval in seconds for continuous environmental and economic calculation ticks")]
        [SerializeField] private float tickIntervalSeconds = 1f;

        private float dayTimer = 0f;
        private float tickTimer = 0f;

        public int CurrentDay => currentDay;
        public float DayProgress => Mathf.Clamp01(dayTimer / Mathf.Max(0.1f, dayDurationSeconds));
        public bool IsRunning => isSimulationRunning;

        public event Action<int> OnDayChanged;
        public event Action<float> OnDayProgress;
        public event Action OnSimulationTick;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (directionalSun == null)
            {
                var sunObj = GameObject.Find("Directional Light");
                if (sunObj != null) directionalSun = sunObj.GetComponent<Light>();
            }

            OnDayChanged?.Invoke(currentDay);
        }

        private void Update()
        {
            if (!isSimulationRunning) return;

            float dt = Time.deltaTime;
            dayTimer += dt;
            tickTimer += dt;

            // Day Progress event
            float progress = DayProgress;
            OnDayProgress?.Invoke(progress);
            UpdateSunRotation(progress);

            // Simulation Tick event
            if (tickTimer >= tickIntervalSeconds)
            {
                tickTimer = 0f;
                OnSimulationTick?.Invoke();
            }

            // End of Day
            if (dayTimer >= dayDurationSeconds)
            {
                dayTimer = 0f;
                currentDay++;
                OnDayChanged?.Invoke(currentDay);
            }
        }

        private void UpdateSunRotation(float progress)
        {
            if (directionalSun == null) return;

            // Gentle solar arc through the day
            float sunAngle = Mathf.Lerp(15f, 75f, Mathf.PingPong(progress * 2f, 1f));
            directionalSun.transform.rotation = Quaternion.Euler(sunAngle, 50f + progress * 60f, 0f);
        }

        public void SetSimulationSpeed(float multiplier)
        {
            Time.timeScale = Mathf.Clamp(multiplier, 0f, 5f);
        }

        public void TogglePause(bool pause)
        {
            isSimulationRunning = !pause;
            Time.timeScale = pause ? 0f : 1f;
        }

        public void RestartSimulation()
        {
            currentDay = 1;
            dayTimer = 0f;
            tickTimer = 0f;
            isSimulationRunning = true;
            Time.timeScale = 1f;
            OnDayChanged?.Invoke(currentDay);
        }
    }
}
