using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TheGodAbove.Core;
using TheGodAbove.CameraControl;
using TheGodAbove.Weather;
using TheGodAbove.Population;
using TheGodAbove.Economy;
using TheGodAbove.Progression;
using TheGodAbove.UI;

namespace TheGodAbove.Environment.Editor
{
    [InitializeOnLoad]
    public static class WorldSceneBuilder
    {
        static WorldSceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                // Auto-build if GameWorld scene doesn't exist
                if (!System.IO.File.Exists("Assets/Scenes/GameWorld.unity"))
                {
                    BuildScene();
                }
            };
        }

        [MenuItem("The God Above/Build GameWorld Scene")]
        public static void BuildScene()
        {
            string sourceScenePath = "Assets/Polytope Studio/Lowpoly_Demos/Environment_Free/Environment_Free.unity";

            if (!System.IO.File.Exists(sourceScenePath))
            {
                Debug.LogError($"[The God Above] Source environment scene not found at {sourceScenePath}!");
                return;
            }

            // 1. Open Polytope Studio's beautifully sculpted terrain scene as template
            var scene = EditorSceneManager.OpenScene(sourceScenePath, OpenSceneMode.Single);

            // 2. Remove the old first-person Player
            var playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                Object.DestroyImmediate(playerGO);
            }

            // 3. Remove old camera or repurpose it
            var oldCam = GameObject.Find("Main Camera");
            if (oldCam != null)
            {
                Object.DestroyImmediate(oldCam);
            }

            // 4. Create proper Isometric Camera
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 45f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            camGO.AddComponent<AudioListener>();

            var camCtrl = camGO.AddComponent<IsometricCameraController>();

            // Configure Isometric Camera to center on the valley / river crossing
            var pivotField = typeof(IsometricCameraController).GetField("islandCenter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pivotField != null) pivotField.SetValue(camCtrl, new Vector3(32f, 4f, 55f));

            var limitXField = typeof(IsometricCameraController).GetField("panLimitX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (limitXField != null) limitXField.SetValue(camCtrl, new Vector2(-40f, 100f));

            var limitZField = typeof(IsometricCameraController).GetField("panLimitZ", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (limitZField != null) limitZField.SetValue(camCtrl, new Vector2(-10f, 120f));

            var distField = typeof(IsometricCameraController).GetField("defaultDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (distField != null) distField.SetValue(camCtrl, 45f);

            var pitchField = typeof(IsometricCameraController).GetField("pitchAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pitchField != null) pitchField.SetValue(camCtrl, 42f);

            var yawField = typeof(IsometricCameraController).GetField("yawAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (yawField != null) yawField.SetValue(camCtrl, 40f);

            // Set initial camera transform so in Edit Mode it looks at the valley
            Vector3 centerPos = new Vector3(32f, 4f, 55f);
            Quaternion camRot = Quaternion.Euler(42f, 40f, 0f);
            camGO.transform.position = centerPos + camRot * new Vector3(0f, 0f, -45f);
            camGO.transform.rotation = camRot;

            // 5. Ensure Directional Light has soft shadows and warm god-game tone
            var dirLightGO = GameObject.Find("Directional Light");
            if (dirLightGO != null)
            {
                var light = dirLightGO.GetComponent<Light>();
                if (light != null)
                {
                    light.shadows = LightShadows.Soft;
                    light.color = new Color(1f, 0.96f, 0.88f);
                    light.intensity = 1.2f;
                }
            }

            // 6. Village & River Valley Dressing
            var villageRoot = GameObject.Find("--- VILLAGE & SCENERY ---");
            if (villageRoot != null) Object.DestroyImmediate(villageRoot);
            villageRoot = new GameObject("--- VILLAGE & SCENERY ---");

            // Bridge over river near path
            var bridgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Bridge/PT_Wooden_Bridge_02.prefab");
            if (bridgePrefab != null)
            {
                var bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab, villageRoot.transform);
                bridge.transform.position = new Vector3(32f, 2.7f, 54f);
                bridge.transform.rotation = Quaternion.Euler(0f, 65f, 0f);
                bridge.transform.localScale = Vector3.one * 1.25f;
            }

            // Village Fences & Gate
            var fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_01.prefab");
            var gatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Gate_Wood_01.prefab");
            if (fencePrefab != null && gatePrefab != null)
            {
                var gate = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab, villageRoot.transform);
                gate.transform.position = new Vector3(42f, 3.8f, 62f);
                gate.transform.rotation = Quaternion.Euler(0f, -30f, 0f);

                var f1 = (GameObject)PrefabUtility.InstantiatePrefab(fencePrefab, villageRoot.transform);
                f1.transform.position = new Vector3(39f, 3.8f, 64f);
                f1.transform.rotation = Quaternion.Euler(0f, -30f, 0f);

                var f2 = (GameObject)PrefabUtility.InstantiatePrefab(fencePrefab, villageRoot.transform);
                f2.transform.position = new Vector3(45f, 3.8f, 60f);
                f2.transform.rotation = Quaternion.Euler(0f, -30f, 0f);
            }

            // Fruit Trees with Apples
            var appleTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_apples.prefab");
            var greenTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_green.prefab");
            if (appleTreePrefab != null && greenTreePrefab != null)
            {
                var t1 = (GameObject)PrefabUtility.InstantiatePrefab(appleTreePrefab, villageRoot.transform);
                t1.transform.position = new Vector3(25f, 3.4f, 48f);
                t1.transform.rotation = Quaternion.Euler(0f, 35f, 0f);

                var t2 = (GameObject)PrefabUtility.InstantiatePrefab(greenTreePrefab, villageRoot.transform);
                t2.transform.position = new Vector3(46f, 4.2f, 52f);
                t2.transform.rotation = Quaternion.Euler(0f, 120f, 0f);

                var t3 = (GameObject)PrefabUtility.InstantiatePrefab(appleTreePrefab, villageRoot.transform);
                t3.transform.position = new Vector3(38f, 4.0f, 42f);
                t3.transform.rotation = Quaternion.Euler(0f, 215f, 0f);
            }

            // Riverbank Rock Piles
            var riverRockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_River_Rock_Pile_02.prefab");
            if (riverRockPrefab != null)
            {
                var r1 = (GameObject)PrefabUtility.InstantiatePrefab(riverRockPrefab, villageRoot.transform);
                r1.transform.position = new Vector3(30f, 2.5f, 52f);
                r1.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

                var r2 = (GameObject)PrefabUtility.InstantiatePrefab(riverRockPrefab, villageRoot.transform);
                r2.transform.position = new Vector3(35f, 2.5f, 57f);
                r2.transform.rotation = Quaternion.Euler(0f, 130f, 0f);
            }

            // 7. Core Simulation Managers
            var existingManagers = GameObject.Find("--- MANAGERS ---");
            if (existingManagers != null) Object.DestroyImmediate(existingManagers);

            var managersGO = new GameObject("--- MANAGERS ---");
            managersGO.AddComponent<WorldSimulationManager>();
            managersGO.AddComponent<WeatherManager>();
            managersGO.AddComponent<PopulationManager>();
            managersGO.AddComponent<EconomyManager>();
            managersGO.AddComponent<GodProgressionManager>();
            managersGO.AddComponent<GameManager>();
            managersGO.AddComponent<DivinePowerTargetIndicator>();
            managersGO.AddComponent<DivinePowerVFXSpawner>();
            managersGO.AddComponent<DivinePowerCaster>();
            managersGO.AddComponent<UISceneLoader>();
            managersGO.AddComponent<VillagerPopulationSpawner>();

            // 7.5 Ensure Dynamic Weather System exists
            if (Object.FindFirstObjectByType<DynamicWeatherSystem.WeatherManager>() == null)
            {
                EditorApplication.ExecuteMenuItem("GameObject/Dynamic Weather System/Weather System");
            }

            // 8. Ensure UI Canvas is removed from GameWorld (UI lives in dedicated GameUIScene)
            var existingHUD = GameObject.Find("GameHUD_Canvas");
            if (existingHUD != null) Object.DestroyImmediate(existingHUD);

            var existingES = GameObject.Find("EventSystem");
            if (existingES != null) Object.DestroyImmediate(existingES);

            // 9. Save as Assets/Scenes/GameWorld.unity
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameWorld.unity");
            Debug.Log("[The God Above] Successfully generated rich diorama GameWorld scene at Assets/Scenes/GameWorld.unity!");
        }
    }
}
