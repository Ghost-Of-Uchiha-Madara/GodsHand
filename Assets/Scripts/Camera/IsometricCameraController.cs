using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TheGodAbove.CameraControl
{
    public class IsometricCameraController : MonoBehaviour
    {
        public static IsometricCameraController Instance { get; private set; }

        [Header("--- Target & Pivot ---")]
        [SerializeField] private Transform targetPivot;
        [SerializeField] private Vector3 islandCenter = Vector3.zero;

        [Header("--- Pan Settings ---")]
        [SerializeField] private float panSpeed = 25f;
        [SerializeField] private float panSmoothing = 8f;
        [SerializeField] private Vector2 panLimitX = new Vector2(-60f, 60f);
        [SerializeField] private Vector2 panLimitZ = new Vector2(-60f, 60f);

        [Header("--- Zoom Settings ---")]
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float zoomSmoothing = 8f;
        [SerializeField] private float minDistance = 15f;
        [SerializeField] private float maxDistance = 75f;
        [SerializeField] private float defaultDistance = 42f;

        [Header("--- Angle Settings ---")]
        [SerializeField] private float pitchAngle = 48f;
        [SerializeField] private float yawAngle = 45f;

        private Vector3 currentPivotPosition;
        private Vector3 targetPivotPosition;
        private float currentDistance;
        private float targetDistance;
        private Vector3 dragPlaneOrigin;
        private bool isDragging;

        private UnityEngine.Camera cam;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null) cam = UnityEngine.Camera.main;
        }

        private void Start()
        {
            if (targetPivot == null)
            {
                var pivotGO = new GameObject("CameraPivot");
                targetPivot = pivotGO.transform;
            }

            targetPivotPosition = islandCenter;
            currentPivotPosition = targetPivotPosition;
            targetDistance = defaultDistance;
            currentDistance = targetDistance;

            UpdateCameraTransform();
        }

        private void LateUpdate()
        {
            HandlePanInput();
            HandleZoomInput();
            UpdateCameraTransform();
        }

        private void HandlePanInput()
        {
            var mouse = Mouse.current;
            var kb = Keyboard.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();

            // Right mouse drag or Middle mouse drag to pan
            bool rmbDown = mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame;
            bool rmbUp = mouse.rightButton.wasReleasedThisFrame || mouse.middleButton.wasReleasedThisFrame;
            bool rmbHeld = mouse.rightButton.isPressed || mouse.middleButton.isPressed;

            if (rmbDown)
            {
                if (!IsPointerOverUI())
                {
                    isDragging = true;
                    dragPlaneOrigin = GetGroundPoint(mousePos);
                }
            }

            if (rmbUp)
            {
                isDragging = false;
            }

            if (isDragging && rmbHeld)
            {
                Vector3 currentGroundPoint = GetGroundPoint(mousePos);
                Vector3 diff = dragPlaneOrigin - currentGroundPoint;
                diff.y = 0f;
                targetPivotPosition += diff;

                // Re-evaluate ground point after moving target
                dragPlaneOrigin = GetGroundPoint(mousePos);
            }

            // Keyboard WASD or Arrow keys to pan
            if (kb != null)
            {
                float h = 0f;
                float v = 0f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;

                if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                {
                    Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
                    Vector3 moveDir = yawRotation * new Vector3(h, 0f, v).normalized;
                    targetPivotPosition += moveDir * (panSpeed * Time.unscaledDeltaTime);
                }

                // Reset view on 'F' key
                if (kb.fKey.wasPressedThisFrame)
                {
                    targetPivotPosition = islandCenter;
                    targetDistance = defaultDistance;
                }
            }

            // Clamp inside island boundaries
            targetPivotPosition.x = Mathf.Clamp(targetPivotPosition.x, panLimitX.x, panLimitX.y);
            targetPivotPosition.z = Mathf.Clamp(targetPivotPosition.z, panLimitZ.x, panLimitZ.y);

            currentPivotPosition = Vector3.Lerp(currentPivotPosition, targetPivotPosition, Time.unscaledDeltaTime * panSmoothing);
            targetPivot.position = currentPivotPosition;
        }

        private void HandleZoomInput()
        {
            if (IsPointerOverUI()) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) > 0.001f)
            {
                float scrollStep = Mathf.Sign(scrollY) * zoomSpeed * 2f;
                targetDistance -= scrollStep;
                targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }

            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.unscaledDeltaTime * zoomSmoothing);
        }

        private void UpdateCameraTransform()
        {
            Quaternion rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
            Vector3 offset = rotation * (Vector3.back * currentDistance);

            transform.rotation = rotation;
            transform.position = currentPivotPosition + offset;
        }

        private Vector3 GetGroundPoint(Vector3 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, currentPivotPosition.y, 0f));

            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return currentPivotPosition;
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        public void FocusOn(Vector3 worldPos)
        {
            targetPivotPosition = new Vector3(worldPos.x, islandCenter.y, worldPos.z);
        }
    }
}
