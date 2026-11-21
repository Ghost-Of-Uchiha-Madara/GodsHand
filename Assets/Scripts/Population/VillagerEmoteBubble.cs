using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheGodAbove.Population
{
    public enum EmoteType
    {
        Happy,
        Rain,
        Heat,
        Shock,
        Wind,
        Cold,
        Coin
    }

    public class VillagerEmoteBubble : MonoBehaviour
    {
        [Header("--- Sprites ---")]
        [SerializeField] private Sprite bubbleBgSprite;
        [SerializeField] private Sprite emoteHappySprite;
        [SerializeField] private Sprite emoteRainSprite;
        [SerializeField] private Sprite emoteHeatSprite;
        [SerializeField] private Sprite emoteShockSprite;
        [SerializeField] private Sprite emoteWindSprite;
        [SerializeField] private Sprite emoteColdSprite;
        [SerializeField] private Sprite emoteCoinSprite;

        [Header("--- Transforms & Components ---")]
        [SerializeField] private Canvas bubbleCanvas;
        [SerializeField] private Image bubbleBgImage;
        [SerializeField] private Image emoteIconImage;

        private Camera mainCamera;
        private Coroutine activePopRoutine;
        private Vector3 initialLocalPos;
        private bool isVisible = false;

        private void Awake()
        {
            mainCamera = Camera.main;
            initialLocalPos = transform.localPosition;
            LoadSpritesIfNull();
            BuildHierarchyIfNull();
            transform.localScale = Vector3.zero;
        }

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (!isVisible) return;

            // 1. Maintain Billboard facing camera
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }

            // 2. Gentle floating bobbing
            float bob = Mathf.Sin(Time.time * 3.5f) * 0.06f;
            transform.localPosition = initialLocalPos + Vector3.up * bob;
        }

        public void ShowEmote(EmoteType type, float duration = 3.2f)
        {
            LoadSpritesIfNull();
            BuildHierarchyIfNull();

            Sprite icon = GetSpriteForEmote(type);
            if (emoteIconImage != null && icon != null)
            {
                emoteIconImage.sprite = icon;
                emoteIconImage.enabled = true;
            }

            if (activePopRoutine != null) StopCoroutine(activePopRoutine);
            activePopRoutine = StartCoroutine(PopBubbleRoutine(duration));
        }

        private Sprite GetSpriteForEmote(EmoteType type)
        {
            switch (type)
            {
                case EmoteType.Happy: return emoteHappySprite;
                case EmoteType.Rain:  return emoteRainSprite;
                case EmoteType.Heat:  return emoteHeatSprite;
                case EmoteType.Shock: return emoteShockSprite;
                case EmoteType.Wind:  return emoteWindSprite;
                case EmoteType.Cold:  return emoteColdSprite;
                case EmoteType.Coin:  return emoteCoinSprite;
                default:              return emoteHappySprite;
            }
        }

        private IEnumerator PopBubbleRoutine(float duration)
        {
            isVisible = true;

            // Pop in with spring overshoot
            float t = 0f;
            float popTime = 0.22f;
            while (t < popTime)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / popTime);
                // Overshoot curve
                float scale = Mathf.Sin(progress * Mathf.PI * 0.5f) * 1.15f;
                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Settle to 1.0
            t = 0f;
            float settleTime = 0.1f;
            while (t < settleTime)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / settleTime);
                transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, progress);
                yield return null;
            }

            transform.localScale = Vector3.one;

            // Wait visible
            yield return new WaitForSeconds(duration);

            // Pop out
            t = 0f;
            float outTime = 0.15f;
            while (t < outTime)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / outTime);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, progress);
                yield return null;
            }

            transform.localScale = Vector3.zero;
            isVisible = false;
            activePopRoutine = null;
        }

        private void LoadSpritesIfNull()
        {
#if UNITY_EDITOR
            if (bubbleBgSprite == null)
                bubbleBgSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/bubble_bg.png");
            if (emoteHappySprite == null)
                emoteHappySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/emote_happy.png");
            if (emoteRainSprite == null)
                emoteRainSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/emote_rain.png");
            if (emoteHeatSprite == null)
                emoteHeatSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/emote_heat.png");
            if (emoteShockSprite == null)
                emoteShockSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/emote_shock.png");
            if (emoteWindSprite == null)
                emoteWindSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/emote_wind.png");
            if (emoteColdSprite == null)
                emoteColdSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/emote_cold.png");
            if (emoteCoinSprite == null)
                emoteCoinSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Procedural/icon_coin.png");
#endif
        }

        private void BuildHierarchyIfNull()
        {
            if (bubbleCanvas != null && bubbleBgImage != null && emoteIconImage != null)
                return;

            // Setup World Space Canvas
            bubbleCanvas = GetComponent<Canvas>();
            if (bubbleCanvas == null) bubbleCanvas = gameObject.AddComponent<Canvas>();
            bubbleCanvas.renderMode = RenderMode.WorldSpace;

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1.2f, 1.2f);

            // Background Image
            var bgGO = transform.Find("BubbleBg");
            if (bgGO == null)
            {
                bgGO = new GameObject("BubbleBg").transform;
                bgGO.SetParent(transform, false);
            }
            bubbleBgImage = bgGO.GetComponent<Image>();
            if (bubbleBgImage == null) bubbleBgImage = bgGO.gameObject.AddComponent<Image>();
            bubbleBgImage.sprite = bubbleBgSprite;
            bubbleBgImage.rectTransform.sizeDelta = new Vector2(1.1f, 1.1f);
            bubbleBgImage.rectTransform.localPosition = Vector3.zero;

            // Icon Image
            var iconGO = transform.Find("EmoteIcon");
            if (iconGO == null)
            {
                iconGO = new GameObject("EmoteIcon").transform;
                iconGO.SetParent(transform, false);
            }
            emoteIconImage = iconGO.GetComponent<Image>();
            if (emoteIconImage == null) emoteIconImage = iconGO.gameObject.AddComponent<Image>();
            emoteIconImage.sprite = emoteHappySprite;
            emoteIconImage.rectTransform.sizeDelta = new Vector2(0.68f, 0.68f);
            emoteIconImage.rectTransform.localPosition = new Vector3(0f, 0.08f, 0f);
        }
    }
}
