using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace TheGodAbove.UI.Editor
{
    [InitializeOnLoad]
    public static class HUDLayoutBuilder
    {
        private const string ProcPath = "Assets/UI/Sprites/Procedural/";

        static HUDLayoutBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!System.IO.File.Exists("Assets/Prefabs/UI/GameHUD.prefab"))
                {
                    GenerateHUD();
                }
            };
        }

        [MenuItem("The God Above/Generate HUD Layout")]
        public static void GenerateHUD()
        {
            // 1. Ensure EventSystem exists with InputSystemUIInputModule
            var existingES = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existingES == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            // 2. Remove existing GameHUD in scene if present
            var existingHUD = GameObject.Find("GameHUD_Canvas");
            if (existingHUD != null)
            {
                Undo.DestroyObjectImmediate(existingHUD);
            }

            // 3. Create Root Canvas
            var canvasGO = new GameObject("GameHUD_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create GameHUD Canvas");

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

            // 4. Load Procedural Vector Sprites
            var sPanel = LoadSprite(ProcPath + "panel_slate_gold.png");
            var sPill = LoadSprite(ProcPath + "pill_badge.png");
            var sCircleBtn = LoadSprite(ProcPath + "circle_btn.png");
            var sStatueFrame = LoadSprite(ProcPath + "statue_circle_frame.png");
            var sUpgradeBtn = LoadSprite(ProcPath + "btn_upgrade_gold.png");
            var sBarTrack = LoadSprite(ProcPath + "bar_track.png");
            var sBarFillGreen = LoadSprite(ProcPath + "bar_fill_green.png");
            var sBarFillBlue = LoadSprite(ProcPath + "bar_fill_blue.png");
            var sPowerNormal = LoadSprite(ProcPath + "btn_power_normal.png");
            var sPowerSelected = LoadSprite(ProcPath + "btn_power_selected.png");

            var sIconSunny = LoadSprite(ProcPath + "icon_sunny.png");
            var sIconRain = LoadSprite(ProcPath + "icon_rain.png");
            var sIconHeat = LoadSprite(ProcPath + "icon_heat.png");
            var sIconLightning = LoadSprite(ProcPath + "icon_lightning.png");
            var sIconWind = LoadSprite(ProcPath + "icon_wind.png");
            var sIconCold = LoadSprite(ProcPath + "icon_cold.png");

            var sIconCoin = LoadSprite(ProcPath + "icon_coin.png");
            var sIconGem = LoadSprite(ProcPath + "icon_gem.png");
            var sIconPause = LoadSprite(ProcPath + "icon_pause.png");
            var sIconSettings = LoadSprite(ProcPath + "icon_settings.png");
            var sDivider = LoadSprite(ProcPath + "divider_line.png");
            var sDividerV = LoadSprite(ProcPath + "divider_line_v.png");

            // ==========================================
            // TOP-LEFT: CURRENCIES
            // ==========================================
            var currenciesRoot = CreateUIObject("TopLeft_Currencies", canvasGO.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(32f, -32f), new Vector2(320f, 52f));

            // Coins Badge
            var coinsBadge = CreateImage("CoinsBadge", currenciesRoot.transform, sPill, Image.Type.Sliced,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(160f, 46f));
            CreateImage("Icon", coinsBadge.transform, sIconCoin, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(10f, 0f), new Vector2(28f, 28f));
            var coinsText = CreateText("CoinsValue", coinsBadge.transform, "12,450", 18, TextAlignmentOptions.Right,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-14f, 0f), new Vector2(-28f, 0f), new Color(1f, 0.95f, 0.82f), FontStyles.Bold);

            // Gems Badge
            var gemsBadge = CreateImage("GemsBadge", currenciesRoot.transform, sPill, Image.Type.Sliced,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(174f, 0f), new Vector2(130f, 46f));
            CreateImage("Icon", gemsBadge.transform, sIconGem, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(10f, 0f), new Vector2(26f, 26f));
            var gemsText = CreateText("GemsValue", gemsBadge.transform, "84", 18, TextAlignmentOptions.Right,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-14f, 0f), new Vector2(-28f, 0f), new Color(0.75f, 0.95f, 1f), FontStyles.Bold);

            // ==========================================
            // TOP-RIGHT: QUICK MENU & WORLD INFO
            // ==========================================
            var topRightRoot = CreateUIObject("TopRight_MenuAndWorld", canvasGO.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-32f, -32f), new Vector2(240f, 260f));

            // Buttons Row
            var btnRow = CreateUIObject("ButtonsRow", topRightRoot.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 0f), new Vector2(100f, 46f));

            var pauseBtnGO = CreateImage("Btn_Pause", btnRow.transform, sCircleBtn, Image.Type.Simple,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(44f, 44f));
            CreateImage("Icon", pauseBtnGO.transform, sIconPause, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(18f, 18f));
            var pauseBtn = pauseBtnGO.AddComponent<Button>();

            var settingsBtnGO = CreateImage("Btn_Settings", btnRow.transform, sCircleBtn, Image.Type.Simple,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0f, 0f), new Vector2(44f, 44f));
            CreateImage("Icon", settingsBtnGO.transform, sIconSettings, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(22f, 22f));
            var settingsBtn = settingsBtnGO.AddComponent<Button>();

            // World Info Card
            var worldCard = CreateImage("WorldInfoPanel", topRightRoot.transform, sPanel, Image.Type.Sliced,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -56f), new Vector2(240f, 196f));

            var dayText = CreateText("DayHeader", worldCard.transform, "DAY 1", 17, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(200f, 24f), new Color(1f, 0.9f, 0.55f), FontStyles.Bold);

            CreateImage("Divider1", worldCard.transform, sDivider, Image.Type.Simple,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -38f), new Vector2(210f, 2f));

            // People Row
            CreateText("PeopleLabel", worldCard.transform, "PEOPLE", 12, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -62f), new Vector2(70f, 22f), new Color(0.72f, 0.77f, 0.84f), FontStyles.Normal);
            var peopleText = CreateText("PeopleValue", worldCard.transform, "128", 16, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-68f, -62f), new Vector2(45f, 22f), Color.white, FontStyles.Bold);
            var peopleTrendText = CreateText("PeopleTrend", worldCard.transform, "+2/d", 12, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -62f), new Vector2(44f, 22f), new Color(0.35f, 0.95f, 0.45f), FontStyles.Bold);

            // Temperature Row
            CreateText("TempLabel", worldCard.transform, "TEMPERATURE", 12, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -98f), new Vector2(100f, 22f), new Color(0.72f, 0.77f, 0.84f), FontStyles.Normal);
            var tempText = CreateText("TempValue", worldCard.transform, "24\u00B0C", 16, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -98f), new Vector2(60f, 22f), Color.white, FontStyles.Bold);

            // Moisture Row & Bar
            CreateText("MoistureLabel", worldCard.transform, "MOISTURE", 12, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -134f), new Vector2(85f, 22f), new Color(0.72f, 0.77f, 0.84f), FontStyles.Normal);
            var moistureText = CreateText("MoistureValue", worldCard.transform, "65%", 14, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -134f), new Vector2(50f, 22f), new Color(0.4f, 0.8f, 1f), FontStyles.Bold);

            var moistureSliderGO = CreateUIObject("MoistureSlider", worldCard.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -164f), new Vector2(208f, 12f));
            var moistureBg = moistureSliderGO.AddComponent<Image>();
            moistureBg.sprite = sBarTrack;
            moistureBg.type = Image.Type.Sliced;

            var moistureFillGO = CreateUIObject("Fill", moistureSliderGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var moistureFillImg = moistureFillGO.AddComponent<Image>();
            moistureFillImg.sprite = sBarFillBlue;
            moistureFillImg.type = Image.Type.Sliced;

            var moistureSlider = moistureSliderGO.AddComponent<Slider>();
            moistureSlider.targetGraphic = moistureFillImg;
            moistureSlider.fillRect = moistureFillGO.GetComponent<RectTransform>();
            moistureSlider.value = 0.65f;

            // ==========================================
            // LEFT PANEL: GOD PROGRESSION
            // ==========================================
            var leftPanelRoot = CreateImage("LeftPanel_GodProgression", canvasGO.transform, sPanel, Image.Type.Sliced,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(28f, 10f), new Vector2(230f, 570f));

            // Statue Frame
            var statueFrameGO = CreateImage("StatuePortrait", leftPanelRoot.transform, sStatueFrame, Image.Type.Simple,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -18f), new Vector2(165f, 165f));

            // God Level
            CreateText("GodLevelSubtitle", leftPanelRoot.transform, "GOD LEVEL", 12, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -196f), new Vector2(160f, 18f), new Color(0.95f, 0.82f, 0.45f), FontStyles.Bold);
            var godLevelText = CreateText("GodLevelNumber", leftPanelRoot.transform, "10", 34, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -226f), new Vector2(100f, 38f), new Color(1f, 0.96f, 0.85f), FontStyles.Bold);

            // Upgrade Button
            var upgradeBtnGO = CreateImage("UpgradeButton", leftPanelRoot.transform, sUpgradeBtn, Image.Type.Sliced,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -272f), new Vector2(184f, 42f));
            var upgradeBtn = upgradeBtnGO.AddComponent<Button>();

            CreateText("UpgradeLabel", upgradeBtnGO.transform, "UPGRADE", 13, TextAlignmentOptions.Right,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-22f, 0f), new Vector2(75f, 22f), new Color(0.24f, 0.15f, 0.03f), FontStyles.Bold);
            CreateImage("CoinIcon", upgradeBtnGO.transform, sIconCoin, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(24f, 0f), new Vector2(18f, 18f));
            var upgradeCostText = CreateText("UpgradeCost", upgradeBtnGO.transform, "500", 13, TextAlignmentOptions.Left,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(58f, 0f), new Vector2(45f, 22f), new Color(0.24f, 0.15f, 0.03f), FontStyles.Bold);

            CreateImage("DividerLeft1", leftPanelRoot.transform, sDivider, Image.Type.Simple,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -326f), new Vector2(190f, 2f));

            // Stats
            CreateText("StatCooldownLabel", leftPanelRoot.transform, "Power Cooldown", 12, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -348f), new Vector2(120f, 22f), new Color(0.72f, 0.77f, 0.84f), FontStyles.Normal);
            var cooldownStatText = CreateText("StatCooldownValue", leftPanelRoot.transform, "-20%", 14, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -348f), new Vector2(50f, 22f), new Color(0.35f, 0.95f, 0.75f), FontStyles.Bold);

            CreateText("StatProdLabel", leftPanelRoot.transform, "Production", 12, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -380f), new Vector2(120f, 22f), new Color(0.72f, 0.77f, 0.84f), FontStyles.Normal);
            var prodStatText = CreateText("StatProdValue", leftPanelRoot.transform, "+20%", 14, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -380f), new Vector2(50f, 22f), new Color(0.35f, 0.95f, 0.75f), FontStyles.Bold);

            CreateImage("DividerLeft2", leftPanelRoot.transform, sDivider, Image.Type.Simple,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -416f), new Vector2(190f, 2f));

            // Happiness Row
            var happyStateText = CreateText("HappyLabel", leftPanelRoot.transform, "HAPPY", 13, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -440f), new Vector2(90f, 22f), new Color(0.35f, 0.95f, 0.45f), FontStyles.Bold);
            var happyPercentText = CreateText("HappyPercent", leftPanelRoot.transform, "82%", 15, TextAlignmentOptions.Right,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -440f), new Vector2(50f, 22f), Color.white, FontStyles.Bold);

            // Happiness Bar
            var happyBarRoot = CreateUIObject("HappinessBar", leftPanelRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -472f), new Vector2(190f, 14f));
            var happyBg = happyBarRoot.AddComponent<Image>();
            happyBg.sprite = sBarTrack;
            happyBg.type = Image.Type.Sliced;

            var happyFillGO = CreateUIObject("Fill", happyBarRoot.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var happyFillImg = happyFillGO.AddComponent<Image>();
            happyFillImg.sprite = sBarFillGreen;
            happyFillImg.type = Image.Type.Sliced;

            var happySlider = happyBarRoot.AddComponent<Slider>();
            happySlider.targetGraphic = happyFillImg;
            happySlider.fillRect = happyFillGO.GetComponent<RectTransform>();
            happySlider.value = 0.82f;

            // ==========================================
            // BOTTOM CENTER: ACTION BAR & PROMPTS
            // ==========================================
            var bottomCenterRoot = CreateUIObject("Bottom_DivinePowers", canvasGO.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(880f, 185f));

            // Prompt Banner Pill
            var bannerGO = CreateImage("ActionBanner", bottomCenterRoot.transform, sPill, Image.Type.Sliced,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 0f), new Vector2(520f, 38f));
            var promptText = CreateText("PromptText", bannerGO.transform,
                "Sunny: Click or drag to bring sunlight and warm the land", 13, TextAlignmentOptions.Center,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.96f, 0.94f, 0.9f), FontStyles.Bold);

            // Powers Dock Background
            var dockGO = CreateImage("PowersDock", bottomCenterRoot.transform, sPanel, Image.Type.Sliced,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f),
                new Vector2(0f, 0f), new Vector2(860f, 134f));

            // Headers inside dock
            CreateText("HeaderBlessings", dockGO.transform, "BLESSINGS", 12, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-255f, -12f), new Vector2(160f, 18f), new Color(1f, 0.9f, 0.55f), FontStyles.Bold);

            CreateText("HeaderDisasters", dockGO.transform, "DISASTERS", 12, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(145f, -12f), new Vector2(160f, 18f), new Color(1f, 0.9f, 0.55f), FontStyles.Bold);

            // Divider between blessings and disasters
            CreateImage("DockDivider", dockGO.transform, sDividerV, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-110f, -8f), new Vector2(2f, 75f));

            // 6 Power Buttons coordinates relative to dock
            float[] xOffsets = new float[] { -320f, -190f, -30f, 95f, 220f, 345f };
            Sprite[] powerIcons = new Sprite[] { sIconSunny, sIconRain, sIconHeat, sIconLightning, sIconWind, sIconCold };
            string[] powerNames = new string[] { "SUNNY", "RAIN", "HEAT", "LIGHTNING", "WIND", "COLD" };
            string[] hotkeys = new string[] { "[1]", "[2]", "[3]", "[4]", "[5]", "[6]" };
            DivinePowerType[] types = new DivinePowerType[] {
                DivinePowerType.Sunny, DivinePowerType.Rainy, DivinePowerType.Hot,
                DivinePowerType.Lightning, DivinePowerType.Wind, DivinePowerType.Cold
            };

            var powerSlots = new List<DivinePowerSlotUI>();
            for (int i = 0; i < 6; i++)
            {
                var slot = CreatePowerSlot(dockGO.transform, types[i], powerNames[i], hotkeys[i],
                    powerIcons[i], sPowerNormal, sPowerSelected, xOffsets[i]);
                powerSlots.Add(slot);
            }

            // ==========================================
            // PAUSE MENU MODAL
            // ==========================================
            var modalRoot = CreateUIObject("Modal_PauseMenu", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var modalBg = modalRoot.AddComponent<Image>();
            modalBg.color = new Color(0f, 0f, 0f, 0.78f);
            modalRoot.SetActive(false);

            var dialogBox = CreateImage("DialogBox", modalRoot.transform, sPanel, Image.Type.Sliced,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(350f, 390f));

            CreateText("PauseTitle", dialogBox.transform, "GAME PAUSED", 20, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -28f), new Vector2(280f, 32f), new Color(1f, 0.9f, 0.55f), FontStyles.Bold);

            CreateImage("DialogDivider", dialogBox.transform, sDivider, Image.Type.Simple,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -65f), new Vector2(270f, 2f));

            var resumeBtn = CreateSimpleButton("Btn_Resume", dialogBox.transform, "RESUME", sPowerNormal, new Vector2(0f, -95f));
            var settingsModalBtn = CreateSimpleButton("Btn_Settings", dialogBox.transform, "SETTINGS", sPowerNormal, new Vector2(0f, -160f));
            var restartBtn = CreateSimpleButton("Btn_Restart", dialogBox.transform, "RESTART WORLD", sPowerNormal, new Vector2(0f, -225f));
            var quitBtn = CreateSimpleButton("Btn_Quit", dialogBox.transform, "MAIN MENU", sPowerNormal, new Vector2(0f, -290f));

            // ==========================================
            // WIRE UP CONTROLLER FIELDS
            // ==========================================
            SetPrivateField(hudController, "coinsText", coinsText);
            SetPrivateField(hudController, "gemsText", gemsText);
            SetPrivateField(hudController, "pauseButton", pauseBtn);
            SetPrivateField(hudController, "settingsButton", settingsBtn);
            SetPrivateField(hudController, "dayText", dayText);
            SetPrivateField(hudController, "peopleText", peopleText);
            SetPrivateField(hudController, "peopleTrendText", peopleTrendText);
            SetPrivateField(hudController, "temperatureText", tempText);
            SetPrivateField(hudController, "soilMoistureSlider", moistureSlider);
            SetPrivateField(hudController, "soilMoistureText", moistureText);

            SetPrivateField(hudController, "statuePortraitImage", statueFrameGO.GetComponent<Image>());
            SetPrivateField(hudController, "godLevelText", godLevelText);
            SetPrivateField(hudController, "upgradeButton", upgradeBtn);
            SetPrivateField(hudController, "upgradeCostText", upgradeCostText);
            SetPrivateField(hudController, "cooldownStatText", cooldownStatText);
            SetPrivateField(hudController, "productionStatText", prodStatText);
            SetPrivateField(hudController, "happinessSlider", happySlider);
            SetPrivateField(hudController, "happinessFillImage", happyFillImg);
            SetPrivateField(hudController, "happinessPercentText", happyPercentText);
            SetPrivateField(hudController, "happinessStateText", happyStateText);

            SetPrivateField(hudController, "actionPromptText", promptText);
            SetPrivateField(hudController, "powerSlots", powerSlots);

            SetPrivateField(hudController, "pauseMenuModal", modalRoot);
            SetPrivateField(hudController, "resumeButton", resumeBtn);
            SetPrivateField(hudController, "restartButton", restartBtn);
            SetPrivateField(hudController, "settingsModalButton", settingsModalBtn);
            SetPrivateField(hudController, "quitButton", quitBtn);

            // Save as Prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, "Assets/Prefabs/UI/GameHUD.prefab", InteractionMode.AutomatedAction);
            Debug.Log("[The God Above] Clean procedural GameHUD successfully generated and saved to Assets/Prefabs/UI/GameHUD.prefab!");
        }

        private static DivinePowerSlotUI CreatePowerSlot(Transform parent, DivinePowerType type, string name, string hotkey,
            Sprite iconSprite, Sprite slotBgSprite, Sprite slotSelectedSprite, float xPos)
        {
            var btnRoot = CreateUIObject($"Power_{name}", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(xPos, -12f), new Vector2(86f, 86f));

            var btnImg = btnRoot.AddComponent<Image>();
            btnImg.sprite = slotBgSprite;
            btnImg.type = Image.Type.Sliced;
            var btn = btnRoot.AddComponent<Button>();

            // Selection Glow Outline
            var glowGO = CreateUIObject("SelectedGlow", btnRoot.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(94f, 94f));
            var glowImg = glowGO.AddComponent<Image>();
            glowImg.sprite = slotSelectedSprite;
            glowImg.type = Image.Type.Sliced;
            glowGO.transform.SetAsFirstSibling();
            glowGO.SetActive(type == DivinePowerType.Sunny); // Sunny selected by default

            // Hotkey Badge in Top-Left
            var hotkeyBadge = CreateText("HotkeyBadge", btnRoot.transform, hotkey, 11, TextAlignmentOptions.Center,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(4f, -4f), new Vector2(24f, 16f), new Color(1f, 0.9f, 0.55f), FontStyles.Bold);

            // Power Icon in Upper Center
            var iconGO = CreateImage("PowerIcon", btnRoot.transform, iconSprite, Image.Type.Simple,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 6f), new Vector2(44f, 44f));

            // Power Name underneath
            var nameLabel = CreateText("NameLabel", btnRoot.transform, name, 11, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 10f), new Vector2(80f, 16f), new Color(1f, 0.95f, 0.85f), FontStyles.Bold);

            // Cooldown Radial Fill
            var cdGO = CreateUIObject("CooldownRadial", btnRoot.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(86f, 86f));
            var cdImg = cdGO.AddComponent<Image>();
            cdImg.color = new Color(0f, 0f, 0f, 0.7f);
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = (int)Image.Origin360.Top;
            cdImg.fillClockwise = false;
            cdImg.fillAmount = 0f;

            // Cooldown Countdown Text
            var cdText = CreateText("CooldownTimer", btnRoot.transform, "", 16, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(70f, 30f), Color.white, FontStyles.Bold);
            cdText.gameObject.SetActive(false);

            return new DivinePowerSlotUI
            {
                powerType = type,
                button = btn,
                iconImage = iconGO.GetComponent<Image>(),
                selectedGlow = glowGO,
                cooldownRadial = cdImg,
                cooldownTimerText = cdText,
                hotkeyBadgeText = hotkeyBadge,
                nameText = nameLabel
            };
        }

        private static Button CreateSimpleButton(string name, Transform parent, string text, Sprite bgSprite, Vector2 anchoredPos)
        {
            var go = CreateUIObject(name, parent,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                anchoredPos, new Vector2(250f, 46f));
            var img = go.AddComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();

            var label = CreateText("Text", go.transform, text, 15, TextAlignmentOptions.Center,
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
                Debug.LogWarning($"[HUDLayoutBuilder] Missing sprite at {path}");
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
