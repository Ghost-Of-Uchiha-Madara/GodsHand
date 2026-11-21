using System.Collections.Generic;
using UnityEngine;
using TheGodAbove.Population;

namespace TheGodAbove.Population
{
    public class VillagerPopulationSpawner : MonoBehaviour
    {
        public static VillagerPopulationSpawner Instance { get; private set; }

        [Header("--- Villager Prefabs ---")]
        [SerializeField] private GameObject[] peasantPrefabs;

        [Header("--- Spawn Settings ---")]
        [SerializeField] private int initialVillagerCount = 10;
        [SerializeField] private Transform villagersContainer;

        [Header("--- Village Waypoints ---")]
        [SerializeField] private List<Vector3> villageWaypoints = new List<Vector3>()
        {
            new Vector3(32f, 3.8f, 55f),  // Arched wooden bridge
            new Vector3(25f, 3.4f, 48f),  // Apple orchard / farm
            new Vector3(42f, 3.8f, 62f),  // Village gate & fences
            new Vector3(35f, 3.0f, 45f),  // Riverbank
            new Vector3(22f, 3.5f, 54f),  // West river path
            new Vector3(45f, 3.6f, 55f),  // East forest road
            new Vector3(33f, 3.0f, 38f),  // South river crossing
            new Vector3(28f, 3.5f, 66f),  // North path
            new Vector3(38f, 3.5f, 50f),  // Village square / center
            new Vector3(21f, 3.6f, 44f)   // West orchard grove
        };

        private readonly string[] villagerNames = new string[]
        {
            "Arthur", "Matilda", "Rowan", "Beatrice",
            "Jasper", "Elspeth", "Gideon", "Maren",
            "Finn", "Elowen", "Tobias", "Greta"
        };

        private readonly List<VillagerController> activeVillagers = new List<VillagerController>();
        public IReadOnlyList<VillagerController> ActiveVillagers => activeVillagers;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadPrefabsIfNull();
        }

        private void Start()
        {
            SpawnInitialPopulation();
        }

        private void LoadPrefabsIfNull()
        {
#if UNITY_EDITOR
            if (peasantPrefabs == null || peasantPrefabs.Length == 0)
            {
                var prefabs = new List<GameObject>();
                string[] paths = new string[]
                {
                    "Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_NPC/Peasants_Citizens/Sets/PT_Male_Peasant_01.prefab",
                    "Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_NPC/Peasants_Citizens/Sets/PT_Female_Peasant_01_a.prefab",
                    "Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_NPC/Peasants_Citizens/Sets/PT_Female_Peasant_01_b.prefab",
                    "Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_NPC/Peasants_Citizens/Sets/PT_Boy_Peasant_01.prefab"
                };

                foreach (var path in paths)
                {
                    var p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (p != null) prefabs.Add(p);
                }

                peasantPrefabs = prefabs.ToArray();
            }
#endif
        }

        public void SpawnInitialPopulation()
        {
            if (villagersContainer == null)
            {
                var containerGO = GameObject.Find("--- VILLAGERS ---");
                if (containerGO == null) containerGO = new GameObject("--- VILLAGERS ---");
                villagersContainer = containerGO.transform;
            }

            // Don't duplicate if already populated in scene
            var existing = villagersContainer.GetComponentsInChildren<VillagerController>();
            if (existing != null && existing.Length >= initialVillagerCount)
            {
                activeVillagers.Clear();
                activeVillagers.AddRange(existing);
                Debug.Log($"[VillagerPopulationSpawner] Found {existing.Length} existing villagers in scene.");
                return;
            }

            LoadPrefabsIfNull();

            if (peasantPrefabs == null || peasantPrefabs.Length == 0)
            {
                Debug.LogWarning("[VillagerPopulationSpawner] No peasant prefabs found to spawn!");
                return;
            }

            int count = Mathf.Min(initialVillagerCount, villagerNames.Length);
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = peasantPrefabs[i % peasantPrefabs.Length];
                Vector3 spawnPos = villageWaypoints[i % villageWaypoints.Count];

                // Add slight jitter so they don't stack on exact same spot
                Vector2 jitter = Random.insideUnitCircle * 1.5f;
                spawnPos = new Vector3(spawnPos.x + jitter.x, spawnPos.y, spawnPos.z + jitter.y);

                SpawnVillager(prefab, spawnPos, villagerNames[i]);
            }

            Debug.Log($"[VillagerPopulationSpawner] Successfully spawned {activeVillagers.Count} living villagers!");
        }

        private VillagerController SpawnVillager(GameObject prefab, Vector3 position, string name)
        {
            GameObject villagerGO = new GameObject($"Villager_{name}");
            villagerGO.transform.position = position;
            villagerGO.transform.SetParent(villagersContainer);

            // Create VisualRoot child for procedural animation bobbing
            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(villagerGO.transform, false);

            // Instantiate low-poly model under VisualRoot
            GameObject model = Instantiate(prefab, visualRoot.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            // Ensure proper colliders and raycast interaction (on Ignore Raycast layer to avoid blocking terrain raycasts)
            villagerGO.layer = 2; // Ignore Raycast
            var capsule = villagerGO.AddComponent<CapsuleCollider>();
            capsule.height = 1.8f;
            capsule.radius = 0.35f;
            capsule.center = new Vector3(0f, 0.9f, 0f);

            // Disable empty Animator so procedural animations & poses take full effect
            var anim = villagerGO.GetComponentInChildren<Animator>();
            if (anim != null && anim.runtimeAnimatorController == null)
            {
                anim.enabled = false;
            }

            // Setup Emote Bubble
            var bubbleGO = new GameObject("EmoteBubble");
            bubbleGO.transform.SetParent(villagerGO.transform, false);
            bubbleGO.transform.localPosition = new Vector3(0f, 2.3f, 0f);
            var bubble = bubbleGO.AddComponent<VillagerEmoteBubble>();

            // Attach VillagerController
            var controller = villagerGO.AddComponent<VillagerController>();
            controller.Initialize(name, villageWaypoints);
            controller.SetArmPoseDefault();

            activeVillagers.Add(controller);
            return controller;
        }
    }
}
