using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace TheGodAbove.UI.Editor
{
    [InitializeOnLoad]
    public static class GameUISceneBuilder
    {
        private const string UIAssetPath = "Assets/the_god_above_ui_assets/";
        private const string PrefabPath = "Assets/Prefabs/UI/GameHUD.prefab";
        private const string UIScenePath = "Assets/Scenes/GameUIScene.unity";
        private const string GameWorldScenePath = "Assets/Scenes/GameWorld.unity";

        static GameUISceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!System.IO.File.Exists(UIScenePath))
                {
                    BuildGameUIScene();
                }
            };
        }

        [MenuItem("The God Above/Convert UI Assets to Sprites")]
        public static void ConvertAllTexturesToSprites()
        {
            if (!System.IO.Directory.Exists(UIAssetPath)) return;

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UIAssetPath });
            int converted = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool modified = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        modified = true;
                    }
                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        modified = true;
                    }
                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        modified = true;
                    }
                    if (path.Contains("dialog_panel") || path.Contains("action_button"))
                    {
                        importer.spriteBorder = new Vector4(24, 24, 24, 24);
                        modified = true;
                    }

                    if (modified)
                    {
                        importer.SaveAndReimport();
                        converted++;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[GameUISceneBuilder] Converted {converted} textures to Sprites.");
        }

        [MenuItem("The God Above/Build Game UI Scene")]
        public static void BuildGameUIScene()
        {
            ConvertAllTexturesToSprites();

            // 1. Create a brand new Scene for dedicated UI
            var uiScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Add UI Camera (for beautiful preview when viewing GameUIScene alone)
            var camGO = new GameObject("UICamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.12f, 0.16f, 1f); // Deep forest god-game slate
            cam.orthographic = true;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // 3. Add EventSystem
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

            // 4. Generate GameHUD Canvas & hierarchy
            var canvasGO = CreateGameHUDHierarchy();

            // 5. Save prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, PrefabPath, InteractionMode.AutomatedAction);

            // 6. Save GameUIScene.unity
            EditorSceneManager.SaveScene(uiScene, UIScenePath);
            Debug.Log($"[The God Above] Saved pixel-perfect UI scene to {UIScenePath}");

            // 7. Ensure GameWorld.unity has UI removed and UISceneLoader attached
            CleanGameWorldScene();

            // 8. Register scenes in Build Settings
            RegisterBuildScenes();
        }

        [MenuItem("The God Above/Clean GameWorld Scene (Remove UI)")]
        public static void CleanGameWorldScene()
        {
            if (!System.IO.File.Exists(GameWorldScenePath)) return;

            var worldScene = EditorSceneManager.OpenScene(GameWorldScenePath, OpenSceneMode.Single);

            // Remove GameHUD_Canvas from GameWorld
            var existingHUD = GameObject.Find("GameHUD_Canvas");
            if (existingHUD != null)
            {
                Object.DestroyImmediate(existingHUD);
            }

            // Remove EventSystem from GameWorld
            var existingES = GameObject.Find("EventSystem");
            if (existingES != null)
            {
                Object.DestroyImmediate(existingES);
            }

            // Add UISceneLoader to --- MANAGERS ---
            var managers = GameObject.Find("--- MANAGERS ---");
            if (managers != null)
            {
                if (managers.GetComponent<UISceneLoader>() == null)
                {
                    managers.AddComponent<UISceneLoader>();
                }
            }

            EditorSceneManager.SaveScene(worldScene);
            Debug.Log($"[GameUISceneBuilder] Cleaned and saved {GameWorldScenePath}.");
        }

        private static void RegisterBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            if (System.IO.File.Exists(GameWorldScenePath))
                scenes.Add(new EditorBuildSettingsScene(GameWorldScenePath, true));
            if (System.IO.File.Exists(UIScenePath))
                scenes.Add(new EditorBuildSettingsScene(UIScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static GameObject CreateGameHUDHierarchy()
        {
            // Root Canvas
            var canvasGO = new GameObject("GameHUD_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            var hudController = canvasGO.AddComponent<HUDController>();

            // Load all high-fidelity assets
            var sCoinsCounter = LoadSprite(UIAssetPath + "currency_coins_counter.png");
            var sGemsCounter = LoadSprite(UIAssetPath + "currency_gems_counter.png");
            var sGodStatuePanel = LoadSprite(UIAssetPath + "god_statue_panel.png");
            var sLargeLeftInfoPanel = LoadSprite(UIAssetPath + "large_left_info_panel.png");
            var sGoldenHaloRing = LoadSprite(UIAssetPath + "golden_halo_ring.png");
            var sCloudLeft = LoadSprite(UIAssetPath + "cloud_cluster_left.png");
            var sCloudRight = LoadSprite(UIAssetPath + "cloud_cluster_right.png");
            var sDivineSparkle = LoadSprite(UIAssetPath + "divine_sparkle.png");
            var sOrnateDivider1 = LoadSprite(UIAssetPath + "ornate_divider_1.png");
            var sOrnateDivider2 = LoadSprite(UIAssetPath + "ornate_divider_2.png");
            var sOrnateTab = LoadSprite(UIAssetPath + "ornate_empty_tab.png");

            var sGreenBar = LoadSprite(UIAssetPath + "green_progress_bar.png");
            var sHappyPill = LoadSprite(UIAssetPath + "status_pill_green.png");
            var sHappyIcon = LoadSprite(UIAssetPath + "happiness_icon.png");

            var sHelpBtn = LoadSprite(UIAssetPath + "help_button.png");
            var sPauseBtn = LoadSprite(UIAssetPath + "pause_button.png");
            var sGearBtn = LoadSprite(UIAssetPath + "gear_button.png");

            var sWorldCard = LoadSprite(UIAssetPath + "small_dialog_panel.png");
            var sSunIcon = LoadSprite(UIAssetPath + "sun_status_icon.png");
            var sBackBtn = LoadSprite(UIAssetPath + "back_button.png");
            var sForwardBtn = LoadSprite(UIAssetPath + "forward_button.png");
            var sPeopleIcon = LoadSprite(UIAssetPath + "population_icon.png");
            var sMoistureBar = LoadSprite(UIAssetPath + "blue_moisture_bar.png");

            var sDock = LoadSprite(UIAssetPath + "blessings_disasters_power_dock.png");
            var sLeafIcon = LoadSprite(UIAssetPath + "leaf_icon.png");
            var sWarningIcon = LoadSprite(UIAssetPath + "warning_icon.png");
            var sPromptBanner = LoadSprite(UIAssetPath + "wide_cloud_dialog_panel.png");
            var sSelectedGlow = LoadSprite(UIAssetPath + "selected_glow_button.png");

            var sPowerSunny = LoadSprite(UIAssetPath + "power_sunny.png");
            var sPowerRain = LoadSprite(UIAssetPath + "power_rain.png");
            var sPowerHeat = LoadSprite(UIAssetPath + "power_heat.png");
            var sPowerLightning = LoadSprite(UIAssetPath + "power_lightning.png");
            var sPowerWind = LoadSprite(UIAssetPath + "power_wind.png");
            var sPowerCold = LoadSprite(UIAssetPath + "power_cold.png");

            var sModalPanel = LoadSprite(UIAssetPath + "medium_dialog_panel.png");
            var sActionBtn = LoadSprite(UIAssetPath + "dark_action_button.png");
            var sCloseBtn = LoadSprite(UIAssetPath + "close_button.png");

            // ==========================================
            // 1. TOP-LEFT: CURRENCIES (Separated & Aligned)
            // ==========================================
            var currenciesRoot = CreateUIObject("TopLeft_Currencies", canvasGO.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(32f, -28f), new Vector2(460f, 65f));

            // Coins Counter Pill (242x74)
            var coinsBadge = CreateImage("CoinsCounter", currenciesRoot.transform, sCoinsCounter, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(230f, 62f));
            var coinsText = CreateText("CoinsValue", coinsBadge.transform, "12,450", 19, TextAlignmentOptions.Center,
                new Vector2(0.24f, 0f), new Vector2(0.78f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.82f), FontStyles.Bold);

            // Gems Counter Pill (194x73) - Placed cleanly to the right
            var gemsBadge = CreateImage("GemsCounter", currenciesRoot.transform, sGemsCounter, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(245f, 0f), new Vector2(185f, 62f));
            var gemsText = CreateText("GemsValue", gemsBadge.transform, "84", 19, TextAlignmentOptions.Center,
                new Vector2(0.24f, 0f), new Vector2(0.76f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.75f, 0.95f, 1f), FontStyles.Bold);

            // ==========================================
            // 2. LEFT PANEL: GOD PROGRESSION (Exact Proportions)
            // ==========================================
            var leftPanelRoot = CreateImage("LeftPanel_GodProgression", canvasGO.transform, sGodStatuePanel, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(32f, -10f), new Vector2(300f, 744f));

            // God Level Number (Overlays on laurel wreath)
            var godLevelText = CreateText("GodLevelNumber", leftPanelRoot.transform, "10", 38, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -345f), new Vector2(90f, 44f), new Color(1f, 0.98f, 0.92f), FontStyles.Bold);

            // UPGRADE Button (Invisible clickable trigger perfectly over golden upgrade button)
            var upgradeBtnGO = CreateUIObject("UpgradeButton", leftPanelRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -425f), new Vector2(225f, 58f));
            var upgradeBtn = upgradeBtnGO.AddComponent<Button>();

            // Upgrade Cost text inside button
            var upgradeCostText = CreateText("UpgradeCost", upgradeBtnGO.transform, "500", 15, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(20f, -11f), new Vector2(60f, 20f), new Color(0.24f, 0.15f, 0.03f), FontStyles.Bold);

            // Dynamic Stats Text (Overlays exactly over printed stat lines)
            var cooldownStatText = CreateText("StatCooldownValue", leftPanelRoot.transform, "-20%", 14, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-36f, -495f), new Vector2(65f, 20f), new Color(0.35f, 0.95f, 0.85f), FontStyles.Bold);

            var prodStatText = CreateText("StatProdValue", leftPanelRoot.transform, "+20%", 14, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-36f, -538f), new Vector2(65f, 20f), new Color(0.35f, 0.95f, 0.85f), FontStyles.Bold);

            // Happiness Percentage
            var happyPercentText = CreateText("HappinessPercent", leftPanelRoot.transform, "82%", 15, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-36f, -638f), new Vector2(55f, 20f), Color.white, FontStyles.Bold);

            // Status Pill Text
            var happyStateText = CreateText("PillLabel", leftPanelRoot.transform, "Happy", 12, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -682f), new Vector2(74f, 22f), Color.white, FontStyles.Bold);

            // Dynamic Happiness Slider overlay
            var happyBarRoot = CreateUIObject("HappinessBar", leftPanelRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -718f), new Vector2(234f, 14f));
            var happySlider = happyBarRoot.AddComponent<Slider>();
            happySlider.value = 0.82f;

            // ==========================================
            // 3. TOP-RIGHT: MENU BUTTONS & WORLD INFO CARD
            // ==========================================
            var topRightRoot = CreateUIObject("TopRight_MenuAndWorld", canvasGO.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-32f, -28f), new Vector2(360f, 320f));

            // Buttons Row: [Help] [Pause] [Settings]
            var btnRow = CreateUIObject("ButtonsRow", topRightRoot.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 0f), new Vector2(185f, 52f));

            var helpBtnGO = CreateImage("Btn_Help", btnRow.transform, sHelpBtn, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(50f, 50f));
            var helpBtn = helpBtnGO.AddComponent<Button>();

            var pauseBtnGO = CreateImage("Btn_Pause", btnRow.transform, sPauseBtn, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(50f, 50f));
            var pauseBtn = pauseBtnGO.AddComponent<Button>();

            var settingsBtnGO = CreateImage("Btn_Settings", btnRow.transform, sGearBtn, Image.Type.Simple,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0f, 0f), new Vector2(50f, 50f));
            var settingsBtn = settingsBtnGO.AddComponent<Button>();

            // World Info Card (small_dialog_panel.png)
            var worldCard = CreateImage("WorldInfoPanel", topRightRoot.transform, sWorldCard, Image.Type.Sliced,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -65f), new Vector2(354f, 230f));

            // Header: < [Sun] DAY 1 >
            var dayHeaderRoot = CreateUIObject("DayHeader", worldCard.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(300f, 36f));

            CreateImage("BackBtn", dayHeaderRoot.transform, sBackBtn, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, 0f), new Vector2(22f, 26f));
            CreateImage("SunIcon", dayHeaderRoot.transform, sSunIcon, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-54f, 0f), new Vector2(32f, 24f));
            var dayText = CreateText("DayText", dayHeaderRoot.transform, "DAY 1", 18, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(16f, 0f), new Vector2(100f, 26f), new Color(1f, 0.95f, 0.85f), FontStyles.Bold);
            CreateImage("FwdBtn", dayHeaderRoot.transform, sForwardBtn, Image.Type.Simple,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-12f, 0f), new Vector2(22f, 26f));

            // People Row
            CreateImage("PeopleIcon", worldCard.transform, sPeopleIcon, Image.Type.Simple,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -74f), new Vector2(28f, 22f));
            CreateText("PeopleLabel", worldCard.transform, "People", 14, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(62f, -74f), new Vector2(70f, 22f), new Color(0.85f, 0.9f, 0.95f), FontStyles.Normal);
            var peopleText = CreateText("PeopleValue", worldCard.transform, "128", 17, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-125f, -74f), new Vector2(55f, 22f), Color.white, FontStyles.Bold);
            var peopleTrendText = CreateText("PeopleTrend", worldCard.transform, "+2 / day", 13, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -74f), new Vector2(90f, 22f), new Color(0.35f, 0.95f, 0.45f), FontStyles.Bold);

            // Temperature Row
            CreateText("TempLabel", worldCard.transform, "Temperature", 14, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(62f, -118f), new Vector2(110f, 22f), new Color(0.85f, 0.9f, 0.95f), FontStyles.Normal);
            var tempText = CreateText("TempValue", worldCard.transform, "24\u00B0C", 17, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -118f), new Vector2(75f, 22f), Color.white, FontStyles.Bold);

            // Soil Moisture Row & Bar
            CreateText("MoistureLabel", worldCard.transform, "Soil Moisture", 14, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(62f, -158f), new Vector2(110f, 22f), new Color(0.85f, 0.9f, 0.95f), FontStyles.Normal);
            var moistureText = CreateText("MoistureValue", worldCard.transform, "56%", 15, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -158f), new Vector2(65f, 22f), new Color(0.4f, 0.85f, 1f), FontStyles.Bold);

            var moistureSliderGO = CreateUIObject("MoistureSlider", worldCard.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -192f), new Vector2(300f, 16f));
            var moistureBg = moistureSliderGO.AddComponent<Image>();
            moistureBg.sprite = sMoistureBar;
            moistureBg.type = Image.Type.Simple;
            moistureBg.color = new Color(0.1f, 0.15f, 0.25f, 0.8f);

            var moistureFillGO = CreateUIObject("Fill", moistureSliderGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var moistureFillImg = moistureFillGO.AddComponent<Image>();
            moistureFillImg.sprite = sMoistureBar;
            moistureFillImg.type = Image.Type.Simple;

            var moistureSlider = moistureSliderGO.AddComponent<Slider>();
            moistureSlider.targetGraphic = moistureFillImg;
            moistureSlider.fillRect = moistureFillGO.GetComponent<RectTransform>();
            moistureSlider.value = 0.56f;

            // ==========================================
            // 4. BOTTOM CENTER: PROMPT BANNER
            // ==========================================
            var promptBannerGO = CreateImage("ActionPromptBanner", canvasGO.transform, sPromptBanner, Image.Type.Sliced,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 218f), new Vector2(620f, 44f));
            var promptText = CreateText("PromptText", promptBannerGO.transform,
                "Select a divine power, then click on the island to apply.", 14, TextAlignmentOptions.Center,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.96f, 0.94f, 0.9f), FontStyles.Bold);

            // ==========================================
            // 5. BOTTOM CENTER: DIVINE POWERS DOCK (995x279)
            // ==========================================
            var dockGO = CreateImage("PowersDock", canvasGO.transform, sDock, Image.Type.Simple,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(1040f, 192f));

            // Section Headers: BLESSINGS & DISASTERS
            if (sLeafIcon != null)
            {
                CreateImage("LeafIcon", dockGO.transform, sLeafIcon, Image.Type.Simple,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(100f, -12f), new Vector2(24f, 24f));
            }
            CreateText("HeaderBlessings", dockGO.transform, "BLESSINGS", 13, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(130f, -14f), new Vector2(150f, 20f), new Color(1f, 0.92f, 0.65f), FontStyles.Bold);

            if (sWarningIcon != null)
            {
                CreateImage("WarningIcon", dockGO.transform, sWarningIcon, Image.Type.Simple,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(530f, -12f), new Vector2(24f, 24f));
            }
            CreateText("HeaderDisasters", dockGO.transform, "DISASTERS", 13, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(560f, -14f), new Vector2(150f, 20f), new Color(1f, 0.92f, 0.65f), FontStyles.Bold);

            // 6 Power Slots (Clean single-button cards fitting inside the dock)
            float[] xPosArray = new float[] { -380f, -250f, -75f, 55f, 185f, 315f };
            Sprite[] powerSprites = new Sprite[] { sPowerSunny, sPowerRain, sPowerHeat, sPowerLightning, sPowerWind, sPowerCold };
            string[] powerNames = new string[] { "SUNNY", "RAIN", "HOT", "LIGHTNING", "WIND", "COLD" };
            DivinePowerType[] types = new DivinePowerType[] {
                DivinePowerType.Sunny, DivinePowerType.Rainy, DivinePowerType.Hot,
                DivinePowerType.Lightning, DivinePowerType.Wind, DivinePowerType.Cold
            };

            var powerSlots = new List<DivinePowerSlotUI>();
            for (int i = 0; i < 6; i++)
            {
                var slot = CreatePowerSlot(dockGO.transform, types[i], powerNames[i],
                    powerSprites[i], sSelectedGlow, xPosArray[i]);
                powerSlots.Add(slot);
            }

            // ==========================================
            // 6. MODALS: PAUSE, SETTINGS, & GOD DETAILS
            // ==========================================
            // Pause Modal
            var pauseModalRoot = CreateUIObject("Modal_PauseMenu", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var pauseModalOverlay = pauseModalRoot.AddComponent<Image>();
            pauseModalOverlay.color = new Color(0f, 0f, 0f, 0.72f);
            pauseModalRoot.SetActive(false);

            var pauseBox = CreateImage("DialogBox", pauseModalRoot.transform, sModalPanel, Image.Type.Sliced,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(360f, 380f));

            CreateText("Title", pauseBox.transform, "Pause Menu", 20, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(250f, 28f), new Color(1f, 0.95f, 0.85f), FontStyles.Bold);

            var pauseCloseBtnGO = CreateImage("Btn_Close", pauseBox.transform, sCloseBtn, Image.Type.Simple,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -16f), new Vector2(32f, 32f));
            var pauseCloseBtn = pauseCloseBtnGO.AddComponent<Button>();

            var resumeBtn = CreateActionButton("Btn_Resume", pauseBox.transform, "Resume", sActionBtn, new Vector2(0f, -80f));
            var settingsModalBtn = CreateActionButton("Btn_Settings", pauseBox.transform, "Settings", sActionBtn, new Vector2(0f, -145f));
            var restartBtn = CreateActionButton("Btn_Restart", pauseBox.transform, "Restart World", sActionBtn, new Vector2(0f, -210f));
            var quitBtn = CreateActionButton("Btn_MainMenu", pauseBox.transform, "Main Menu", sActionBtn, new Vector2(0f, -275f));

            // Settings Modal
            var settingsModalRoot = CreateUIObject("Modal_Settings", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var settingsOverlay = settingsModalRoot.AddComponent<Image>();
            settingsOverlay.color = new Color(0f, 0f, 0f, 0.72f);
            settingsModalRoot.SetActive(false);

            var settingsBox = CreateImage("DialogBox", settingsModalRoot.transform, sModalPanel, Image.Type.Sliced,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(420f, 400f));

            CreateText("Title", settingsBox.transform, "Settings", 20, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(250f, 28f), new Color(1f, 0.95f, 0.85f), FontStyles.Bold);

            var settingsCloseBtnGO = CreateImage("Btn_Close", settingsBox.transform, sCloseBtn, Image.Type.Simple,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -16f), new Vector2(32f, 32f));
            var settingsCloseBtn = settingsCloseBtnGO.AddComponent<Button>();

            // God Details Page
            var godPageModal = CreateUIObject("Modal_GodDetailsPage", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var godPageOverlay = godPageModal.AddComponent<Image>();
            godPageOverlay.color = new Color(0f, 0f, 0f, 0.72f);
            godPageModal.SetActive(false);

            var godPagePanel = CreateImage("DialogBox", godPageModal.transform, sLargeLeftInfoPanel, Image.Type.Sliced,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(480f, 440f));

            if (sOrnateTab != null)
            {
                var tabGO = CreateImage("OrnateTabHeader", godPagePanel.transform, sOrnateTab, Image.Type.Simple,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, 20f), new Vector2(250f, 60f));
                CreateText("TabText", tabGO.transform, "DIVINE PROGRESSION", 13, TextAlignmentOptions.Center,
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.82f), FontStyles.Bold);
            }

            var godCloseBtnGO = CreateImage("Btn_Close", godPagePanel.transform, sCloseBtn, Image.Type.Simple,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -16f), new Vector2(32f, 32f));
            godCloseBtnGO.AddComponent<Button>();

            // ==========================================
            // WIRE CONTROLLER FIELDS
            // ==========================================
            SetPrivateField(hudController, "coinsText", coinsText);
            SetPrivateField(hudController, "gemsText", gemsText);
            SetPrivateField(hudController, "pauseButton", pauseBtn);
            SetPrivateField(hudController, "settingsButton", settingsBtn);
            SetPrivateField(hudController, "helpButton", helpBtn);
            SetPrivateField(hudController, "dayText", dayText);
            SetPrivateField(hudController, "peopleText", peopleText);
            SetPrivateField(hudController, "peopleTrendText", peopleTrendText);
            SetPrivateField(hudController, "temperatureText", tempText);
            SetPrivateField(hudController, "soilMoistureSlider", moistureSlider);
            SetPrivateField(hudController, "soilMoistureText", moistureText);

            SetPrivateField(hudController, "godLevelText", godLevelText);
            SetPrivateField(hudController, "upgradeButton", upgradeBtn);
            SetPrivateField(hudController, "upgradeCostText", upgradeCostText);
            SetPrivateField(hudController, "cooldownStatText", cooldownStatText);
            SetPrivateField(hudController, "productionStatText", prodStatText);
            SetPrivateField(hudController, "happinessSlider", happySlider);
            SetPrivateField(hudController, "happinessPercentText", happyPercentText);
            SetPrivateField(hudController, "happinessStateText", happyStateText);

            SetPrivateField(hudController, "actionPromptText", promptText);
            SetPrivateField(hudController, "powerSlots", powerSlots);

            SetPrivateField(hudController, "pauseMenuModal", pauseModalRoot);
            SetPrivateField(hudController, "resumeButton", resumeBtn);
            SetPrivateField(hudController, "restartButton", restartBtn);
            SetPrivateField(hudController, "settingsModalButton", settingsModalBtn);
            SetPrivateField(hudController, "quitButton", quitBtn);
            SetPrivateField(hudController, "pauseCloseButton", pauseCloseBtn);

            SetPrivateField(hudController, "settingsModal", settingsModalRoot);
            SetPrivateField(hudController, "settingsCloseButton", settingsCloseBtn);

            return canvasGO;
        }

        private static DivinePowerSlotUI CreatePowerSlot(Transform parent, DivinePowerType type, string name,
            Sprite powerSprite, Sprite glowSprite, float xPos)
        {
            var btnRoot = CreateUIObject($"Slot_{name}", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(xPos, -15f), new Vector2(108f, 116f));

            // Selected Glow (frames the card when selected)
            var glowGO = CreateImage("SelectedGlow", btnRoot.transform, glowSprite, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(124f, 130f));
            glowGO.transform.SetAsFirstSibling();
            glowGO.SetActive(type == DivinePowerType.Sunny);

            // Power Card Button (power_sunny.png, etc. already contains the icon, name, and hotkey!)
            var cardImgGO = CreateImage("CardImage", btnRoot.transform, powerSprite, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(108f, 116f));
            var btn = btnRoot.AddComponent<Button>();
            btn.targetGraphic = cardImgGO.GetComponent<Image>();

            // Cooldown Radial Fill
            var cdGO = CreateUIObject("CooldownRadial", btnRoot.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(96f, 96f));
            var cdImg = cdGO.AddComponent<Image>();
            cdImg.color = new Color(0f, 0f, 0f, 0.72f);
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = (int)Image.Origin360.Top;
            cdImg.fillClockwise = false;
            cdImg.fillAmount = 0f;

            // Cooldown text (e.g. 8s, 4s)
            var cdText = CreateText("CooldownTimer", btnRoot.transform, "", 16, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(70f, 30f), Color.white, FontStyles.Bold);
            cdText.gameObject.SetActive(false);

            return new DivinePowerSlotUI
            {
                powerType = type,
                button = btn,
                iconImage = cardImgGO.GetComponent<Image>(),
                selectedGlow = glowGO,
                cooldownRadial = cdImg,
                cooldownTimerText = cdText
            };
        }

        private static Button CreateActionButton(string name, Transform parent, string text, Sprite bgSprite, Vector2 anchoredPos)
        {
            var go = CreateUIObject(name, parent,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                anchoredPos, new Vector2(250f, 50f));
            var img = go.AddComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Simple;
            var btn = go.AddComponent<Button>();

            CreateText("Label", go.transform, text, 15, TextAlignmentOptions.Center,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.85f), FontStyles.Bold);

            return btn;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private static GameObject CreateImage(string name, Transform parent, Sprite sprite, Image.Type type, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = type;
            if (sprite != null) img.color = Color.white;
            return go;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string content, float fontSize, TextAlignmentOptions alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color, FontStyles style = FontStyles.Normal)
        {
            var go = CreateUIObject(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                Debug.LogWarning($"[GameUISceneBuilder] Missing sprite at {path}");
            }
            return s;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
