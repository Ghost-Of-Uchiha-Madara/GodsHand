using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TheGodAbove.UI;

namespace TheGodAbove.Weather
{
    public class DivinePowerTargetIndicator : MonoBehaviour
    {
        public static DivinePowerTargetIndicator Instance { get; private set; }

        [Header("--- Settings ---")]
        [SerializeField] private int circleSegments = 48;
        [SerializeField] private float terrainSampleHeight = 10f;
        [SerializeField] private LayerMask groundLayerMask = ~0; // all layers or terrain

        private LineRenderer outerRing;
        private LineRenderer innerRing;
        private LineRenderer crosshairH;
        private LineRenderer crosshairV;

        private DivinePowerDefinition currentDef;
        private bool isVisible = false;
        private Vector3 currentCenter;
        private UnityEngine.Camera mainCam;

        public bool IsTargetingValid => isVisible;
        public Vector3 TargetPosition => currentCenter;
        public DivinePowerDefinition CurrentDefinition => currentDef;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            mainCam = UnityEngine.Camera.main;
            CreateIndicatorRenderers();
        }

        private void Start()
        {
            SetPower(DivinePowerType.Sunny);
        }

        private void LateUpdate()
        {
            UpdateIndicatorPosition();
        }

        private void CreateIndicatorRenderers()
        {
            // Material for glowing reticle lines (Universal Render Pipeline Particles Unlit or Sprites/Default)
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 1); // Additive

            // Outer Ring
            var outerGO = new GameObject("OuterRing");
            outerGO.transform.SetParent(transform, false);
            outerRing = outerGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(outerRing, mat, 0.16f, true);

            // Inner Ring
            var innerGO = new GameObject("InnerRing");
            innerGO.transform.SetParent(transform, false);
            innerRing = innerGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(innerRing, mat, 0.08f, true);

            // Crosshair Lines
            var crossHGO = new GameObject("CrosshairH");
            crossHGO.transform.SetParent(transform, false);
            crosshairH = crossHGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(crosshairH, mat, 0.1f, false);

            var crossVGO = new GameObject("CrosshairV");
            crossVGO.transform.SetParent(transform, false);
            crosshairV = crossVGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(crosshairV, mat, 0.1f, false);
        }

        private void ConfigureLineRenderer(LineRenderer lr, Material mat, float width, bool loop)
        {
            lr.material = mat;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.loop = loop;
            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
        }

        public void SetPower(DivinePowerType powerType)
        {
            currentDef = DivinePowerDefinition.GetDefault(powerType);
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (currentDef == null) return;
            Color col = currentDef.indicatorColor;
            SetLineColor(outerRing, col);
            SetLineColor(innerRing, new Color(col.r, col.g, col.b, col.a * 0.6f));
            SetLineColor(crosshairH, new Color(col.r, col.g, col.b, col.a * 0.9f));
            SetLineColor(crosshairV, new Color(col.r, col.g, col.b, col.a * 0.9f));
        }

        private void SetLineColor(LineRenderer lr, Color col)
        {
            if (lr == null) return;
            lr.startColor = col;
            lr.endColor = col;
        }

        private void UpdateIndicatorPosition()
        {
            if (mainCam == null) mainCam = UnityEngine.Camera.main;
            if (mainCam == null || currentDef == null || currentDef.category == PowerCategory.Blessing)
            {
                SetVisible(false);
                return;
            }

            // Check if pointer is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                SetVisible(false);
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                SetVisible(false);
                return;
            }

            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayerMask))
            {
                currentCenter = hit.point;
                SetVisible(true);
                DrawProjectedCircles(currentCenter, currentDef.aoeRadius);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void DrawProjectedCircles(Vector3 center, float radius)
        {
            // Outer Ring
            float pulse = 1f + 0.04f * Mathf.Sin(Time.time * 5f);
            float currentRadius = radius * pulse;

            Vector3[] outerPoints = new Vector3[circleSegments];
            Vector3[] innerPoints = new Vector3[circleSegments];
            float innerRadius = currentRadius * 0.35f;

            float angleStep = 360f / circleSegments;
            for (int i = 0; i < circleSegments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                outerPoints[i] = SampleGroundPoint(center + dir * currentRadius);
                innerPoints[i] = SampleGroundPoint(center + dir * innerRadius);
            }

            outerRing.positionCount = circleSegments;
            outerRing.SetPositions(outerPoints);

            innerRing.positionCount = circleSegments;
            innerRing.SetPositions(innerPoints);

            // Precision Crosshairs for Lightning or detailed target
            bool showCrosshairs = currentDef.powerType == DivinePowerType.Lightning || currentDef.powerType == DivinePowerType.Hot;
            crosshairH.enabled = showCrosshairs;
            crosshairV.enabled = showCrosshairs;

            if (showCrosshairs)
            {
                float armLen = currentRadius * 0.7f;
                crosshairH.positionCount = 2;
                crosshairH.SetPosition(0, SampleGroundPoint(center + Vector3.left * armLen));
                crosshairH.SetPosition(1, SampleGroundPoint(center + Vector3.right * armLen));

                crosshairV.positionCount = 2;
                crosshairV.SetPosition(0, SampleGroundPoint(center + Vector3.back * armLen));
                crosshairV.SetPosition(1, SampleGroundPoint(center + Vector3.forward * armLen));
            }
        }

        private Vector3 SampleGroundPoint(Vector3 origin)
        {
            Vector3 rayStart = origin + Vector3.up * terrainSampleHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, terrainSampleHeight * 2f, groundLayerMask))
            {
                return hit.point + hit.normal * 0.06f; // Lift slightly above surface to prevent z-fighting
            }
            return origin + Vector3.up * 0.06f;
        }

        private void SetVisible(bool visible)
        {
            if (isVisible == visible) return;
            isVisible = visible;

            outerRing.enabled = visible;
            innerRing.enabled = visible;
            crosshairH.enabled = visible;
            crosshairV.enabled = visible;
        }
    }
}
