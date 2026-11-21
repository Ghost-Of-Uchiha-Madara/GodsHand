using UnityEngine;
using System.Collections;
using TheGodAbove.UI;
using DynamicWeatherSystem;

namespace TheGodAbove.Weather
{
    public class DivinePowerVFXSpawner : MonoBehaviour
    {
        public static DivinePowerVFXSpawner Instance { get; private set; }

        [Header("--- Dynamic Weather System Presets ---")]
        [SerializeField] private WeatherStateData presetClear;
        [SerializeField] private WeatherStateData presetRain;

        [Header("--- Disaster VFX Prefabs ---")]
        [SerializeField] private GameObject zapVFXPrefab;
        [SerializeField] private GameObject fireFloorPrefab;
        [SerializeField] private GameObject fireExplosionPrefab;
        [SerializeField] private GameObject fireTrailPrefab;
        [SerializeField] private GameObject rockMeshPrefab;
        [SerializeField] private GameObject windFlowsPrefab;
        [SerializeField] private GameObject windTrailsPrefab;
        [SerializeField] private GameObject windDebrisPrefab;
        [SerializeField] private GameObject hailPrefab;
        [SerializeField] private GameObject snowPrefab;
        [SerializeField] private GameObject frostFogPrefab;

        [Header("--- Audio Clips ---")]
        [SerializeField] private AudioClip zapSFX;
        [SerializeField] private AudioClip fireSFX;
        [SerializeField] private AudioClip windSFX;

        private DynamicWeatherSystem.WeatherManager dynamicWeatherMgr;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                SafeDestroy(gameObject);
                return;
            }

            LoadAssetsIfNull();
        }

        private void Start()
        {
            dynamicWeatherMgr = Object.FindFirstObjectByType<DynamicWeatherSystem.WeatherManager>();
        }

        private void LoadAssetsIfNull()
        {
#if UNITY_EDITOR
            if (presetClear == null)
                presetClear = UnityEditor.AssetDatabase.LoadAssetAtPath<WeatherStateData>("Assets/DynamicWeatherSystem/Presets/WS_Clear.asset");
            if (presetRain == null)
                presetRain = UnityEditor.AssetDatabase.LoadAssetAtPath<WeatherStateData>("Assets/DynamicWeatherSystem/Presets/WS_Rain.asset");

            if (zapVFXPrefab == null)
                zapVFXPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Zap VFX URP/VFX/Zap/Particles/VFX_Zap_03_Yellow.prefab");
            if (fireFloorPrefab == null)
                fireFloorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_Floor_01.prefab");
            if (fireExplosionPrefab == null)
                fireExplosionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_01_Big.prefab");
            if (fireTrailPrefab == null)
                fireTrailPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_01_Small_Smoke.prefab");
            if (rockMeshPrefab == null)
                rockMeshPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Generic_Rock_01.prefab");

            if (windFlowsPrefab == null)
                windFlowsPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Wind VFX URP/VFX/Particles/VFX_Wind_Flows_01.prefab");
            if (windTrailsPrefab == null)
                windTrailsPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Wind VFX URP/VFX/Particles/VFX_Wind_Flows_Trails_02_Fast.prefab");
            if (windDebrisPrefab == null)
                windDebrisPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vefects/Wind VFX URP/VFX/Particles/VFX_Wind_Debris_Tree_Leaves_01.prefab");

            if (hailPrefab == null)
                hailPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Realistic Hail Set/Prefabs/Hail - Heavy.prefab");
            if (snowPrefab == null)
                snowPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fog Particles/Bonus/Snow Particles.prefab");
            if (frostFogPrefab == null)
                frostFogPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fog Particles/Prefabs/Bluish Fog.prefab");

            if (zapSFX == null)
                zapSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Vefects/Zap VFX URP/Audio/WAV/SFX_Vefects_Zap_Big_01.wav");
            if (fireSFX == null)
                fireSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Vefects/Free Fire VFX URP/Audio/SFX_FireBig_L.wav");
            if (windSFX == null)
                windSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/DynamicWeatherSystem/Audio/storm-wind-loop.wav");
#endif
        }

        public void SpawnVFX(DivinePowerType type, Vector3 position, float radius)
        {
            switch (type)
            {
                case DivinePowerType.Sunny:
                    SpawnSunnyWeather();
                    break;
                case DivinePowerType.Rainy:
                    SpawnRainyWeather();
                    break;
                case DivinePowerType.Hot:
                    SpawnAsteroidStrike(position);
                    break;
                case DivinePowerType.Lightning:
                    SpawnZapLightning(position);
                    break;
                case DivinePowerType.Wind:
                    SpawnWindStorm(position, radius);
                    break;
                case DivinePowerType.Cold:
                    SpawnColdHailStorm(position, radius);
                    break;
            }
        }

        // ==========================================
        // 1. SUNNY: Global warm sunny weather
        // ==========================================
        public void SpawnSunnyWeather()
        {
            if (dynamicWeatherMgr == null) dynamicWeatherMgr = Object.FindFirstObjectByType<DynamicWeatherSystem.WeatherManager>();
            if (dynamicWeatherMgr != null && presetClear != null)
            {
                dynamicWeatherMgr.SetWeather(presetClear, 2.5f);
            }

            Debug.Log("[DivinePowerVFXSpawner] Applied SUNNY blessing: Warm sunny atmosphere across the island!");
        }

        // ==========================================
        // 2. RAIN: Global rain over the island
        // ==========================================
        public void SpawnRainyWeather()
        {
            if (dynamicWeatherMgr == null) dynamicWeatherMgr = Object.FindFirstObjectByType<DynamicWeatherSystem.WeatherManager>();
            if (dynamicWeatherMgr != null && presetRain != null)
            {
                dynamicWeatherMgr.SetWeather(presetRain, 2.5f);
            }

            Debug.Log("[DivinePowerVFXSpawner] Applied RAIN blessing: Whole island raining to hydrate soil!");
        }

        // ==========================================
        // 3. HEAT: Asteroid strike from sky + Free Fire VFX
        // ==========================================
        public void SpawnAsteroidStrike(Vector3 targetPos)
        {
            Vector3 spawnPos = targetPos + new Vector3(-24f, 48f, 16f);
            GameObject meteorGO;

            if (rockMeshPrefab != null)
            {
                meteorGO = Instantiate(rockMeshPrefab, spawnPos, Quaternion.identity);
                meteorGO.transform.localScale = Vector3.one * 1.8f;
            }
            else
            {
                meteorGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meteorGO.transform.position = spawnPos;
                meteorGO.transform.localScale = Vector3.one * 2.2f;
            }

            meteorGO.name = "Asteroid_Meteor";

            // Attach trailing fire
            if (fireTrailPrefab != null)
            {
                var trail = Instantiate(fireTrailPrefab, meteorGO.transform);
                trail.transform.localPosition = Vector3.zero;
            }

            // Fiery orange point light
            var light = meteorGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.45f, 0.1f);
            light.range = 25f;
            light.intensity = 8f;

            // Attach AsteroidProjectile logic
            var proj = meteorGO.AddComponent<AsteroidProjectile>();
            proj.Initialize(targetPos, fireFloorPrefab, fireExplosionPrefab, fireSFX);

            Debug.Log($"[DivinePowerVFXSpawner] Launched Asteroid from sky towards {targetPos}!");
        }

        // ==========================================
        // 4. LIGHTNING: Zap VFX Strike
        // ==========================================
        public void SpawnZapLightning(Vector3 targetPos)
        {
            if (zapVFXPrefab != null)
            {
                var zapObj = Instantiate(zapVFXPrefab, targetPos, Quaternion.identity);
                SafeDestroy(zapObj, 2f);
            }

            // High voltage flash light
            var flashGO = new GameObject("ZapFlashLight");
            flashGO.transform.position = targetPos + Vector3.up * 1.5f;
            var light = flashGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 1f, 0.9f);
            light.range = 40f;
            light.intensity = 20f;
            StartCoroutine(FlashZapLight(light, flashGO));

            // Play Zap SFX
            if (zapSFX != null)
            {
                PlayAudioAtPoint(zapSFX, targetPos, 1f);
            }

            Debug.Log($"[DivinePowerVFXSpawner] Struck target with Zap VFX at {targetPos}!");
        }

        private IEnumerator FlashZapLight(Light light, GameObject go)
        {
            yield return new WaitForSeconds(0.08f);
            light.intensity = 6f;
            yield return new WaitForSeconds(0.05f);
            light.intensity = 15f;
            yield return new WaitForSeconds(0.08f);
            SafeDestroy(go);
        }

        // ==========================================
        // 5. WIND: Wind VFX URP (Flows, Trails, Leaves)
        // ==========================================
        public void SpawnWindStorm(Vector3 targetPos, float radius)
        {
            var windRoot = new GameObject("VFX_WindStorm");
            windRoot.transform.position = targetPos;

            if (windFlowsPrefab != null)
            {
                var flows = Instantiate(windFlowsPrefab, windRoot.transform);
                flows.transform.localPosition = Vector3.zero;
            }

            if (windTrailsPrefab != null)
            {
                var trails = Instantiate(windTrailsPrefab, windRoot.transform);
                trails.transform.localPosition = Vector3.zero;
            }

            if (windDebrisPrefab != null)
            {
                var debris = Instantiate(windDebrisPrefab, windRoot.transform);
                debris.transform.localPosition = Vector3.zero;
            }

            if (windSFX != null)
            {
                PlayAudioAtPoint(windSFX, targetPos, 0.85f);
            }

            SafeDestroy(windRoot, 6f);
            Debug.Log($"[DivinePowerVFXSpawner] Spawned Wind VFX storm at {targetPos}!");
        }

        // ==========================================
        // 6. COLD: Hail & Snow Particles Pack
        // ==========================================
        public void SpawnColdHailStorm(Vector3 targetPos, float radius)
        {
            var coldRoot = new GameObject("VFX_ColdHailStorm");
            coldRoot.transform.position = targetPos;

            // Falling heavy hail particles
            if (hailPrefab != null)
            {
                var hail = Instantiate(hailPrefab, coldRoot.transform);
                hail.transform.localPosition = Vector3.up * 10f;
            }

            // Snow particles
            if (snowPrefab != null)
            {
                var snow = Instantiate(snowPrefab, coldRoot.transform);
                snow.transform.localPosition = Vector3.up * 8f;
            }

            // Rolling ground frost fog
            if (frostFogPrefab != null)
            {
                var fog = Instantiate(frostFogPrefab, coldRoot.transform);
                fog.transform.localPosition = Vector3.up * 0.5f;
            }

            // Chill ice-blue ambient light
            var light = coldRoot.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.55f, 0.85f, 1f);
            light.range = radius * 1.6f;
            light.intensity = 4f;

            SafeDestroy(coldRoot, 7f);
            Debug.Log($"[DivinePowerVFXSpawner] Spawned Hail & Snow cold storm at {targetPos}!");
        }

        private void PlayAudioAtPoint(AudioClip clip, Vector3 pos, float volume)
        {
            var audioGO = new GameObject($"Audio_{clip.name}");
            audioGO.transform.position = pos;
            var src = audioGO.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = volume;
            src.spatialBlend = 0.4f;
            src.Play();
            SafeDestroy(audioGO, clip.length + 0.5f);
        }

        private void SafeDestroy(GameObject go, float delay = 0f)
        {
            if (go == null) return;
            if (Application.isPlaying)
            {
                if (delay > 0f) Destroy(go, delay);
                else Destroy(go);
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (go != null) DestroyImmediate(go);
                };
#else
                Destroy(go);
#endif
            }
        }
    }
}
