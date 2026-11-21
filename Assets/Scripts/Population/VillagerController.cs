using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheGodAbove.Core;
using TheGodAbove.Weather;
using TheGodAbove.Economy;
using TheGodAbove.UI;

namespace TheGodAbove.Population
{
    public enum VillagerBehaviorState
    {
        Wander,
        Idle,
        Farming,
        Fleeing,
        Shocked,
        Shivering,
        WindStruggle
    }

    [ExecuteAlways]
    public class VillagerController : MonoBehaviour
    {
        [Header("--- Villager Info ---")]
        [SerializeField] private string villagerName = "Villager";
        [SerializeField] private VillagerBehaviorState currentState = VillagerBehaviorState.Wander;

        [Header("--- Speeds & Movement ---")]
        [SerializeField] private float walkSpeed = 2.0f;
        [SerializeField] private float fleeSpeed = 4.2f;
        [SerializeField] private float rotationSpeed = 8.0f;

        [Header("--- Emote Bubble ---")]
        [SerializeField] private VillagerEmoteBubble emoteBubble;

        [Header("--- Waypoints & Navigation ---")]
        [SerializeField] private List<Vector3> waypoints = new List<Vector3>();

        private Transform visualRoot;
        private Transform leftArmTransform;
        private Transform rightArmTransform;
        private Vector3 targetDestination;
        private Vector3 fleeDirection;
        private float stateTimer = 0f;
        private float titheTimer = 0f;
        private float walkCycle = 0f;
        private float baseHeightOffset = 0f;
        private bool isGrounded = true;

        public string VillagerName => villagerName;
        public VillagerBehaviorState State => currentState;

        private void Awake()
        {
            // Visual child root for procedural animation tilts and squashes
            visualRoot = transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            // Find arm bones for natural posing and procedural swing
            foreach (var t in GetComponentsInChildren<Transform>())
            {
                if (t.name == "PT_LeftArm") leftArmTransform = t;
                if (t.name == "PT_RightArm") rightArmTransform = t;
            }

            SetArmPoseDefault();
            titheTimer = Random.Range(10f, 25f);
        }

        private void Start()
        {
            SetupEmoteBubbleIfNull();
            PickNewWanderTarget();

            // Snap to terrain on start
            SnapToTerrain();

            // Subscribe to Divine Powers
            if (DivinePowerCaster.Instance != null)
            {
                DivinePowerCaster.Instance.OnPowerCast += HandlePowerCast;
            }
        }

        private void OnDestroy()
        {
            if (DivinePowerCaster.Instance != null)
            {
                DivinePowerCaster.Instance.OnPowerCast -= HandlePowerCast;
            }
        }

        public void Initialize(string name, List<Vector3> villageWaypoints)
        {
            this.villagerName = name;
            this.waypoints = villageWaypoints != null ? new List<Vector3>(villageWaypoints) : new List<Vector3>();
            PickNewWanderTarget();
            SnapToTerrain();
        }

        private void SetupEmoteBubbleIfNull()
        {
            if (emoteBubble != null) return;

            var existing = GetComponentInChildren<VillagerEmoteBubble>();
            if (existing != null)
            {
                emoteBubble = existing;
                return;
            }

            var bubbleGO = new GameObject("EmoteBubble");
            bubbleGO.transform.SetParent(transform, false);
            bubbleGO.transform.localPosition = new Vector3(0f, 2.3f, 0f);
            emoteBubble = bubbleGO.AddComponent<VillagerEmoteBubble>();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                SetArmPoseDefault();
                return;
            }

            switch (currentState)
            {
                case VillagerBehaviorState.Wander:
                    UpdateWander();
                    break;
                case VillagerBehaviorState.Idle:
                    UpdateIdle();
                    break;
                case VillagerBehaviorState.Farming:
                    UpdateFarming();
                    break;
                case VillagerBehaviorState.Fleeing:
                    UpdateFleeing();
                    break;
                case VillagerBehaviorState.Shocked:
                    UpdateShocked();
                    break;
                case VillagerBehaviorState.Shivering:
                    UpdateShivering();
                    break;
                case VillagerBehaviorState.WindStruggle:
                    UpdateWindStruggle();
                    break;
            }

            UpdateTitheGeneration();
        }

        private void UpdateWander()
        {
            MoveTowards(targetDestination, walkSpeed);

            // Reached destination?
            float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                          new Vector2(targetDestination.x, targetDestination.z));
            if (dist < 1.0f)
            {
                // Either switch to Idle or Farming near orchards
                if (Random.value < 0.45f)
                {
                    currentState = VillagerBehaviorState.Farming;
                    stateTimer = Random.Range(4f, 8f);
                }
                else
                {
                    currentState = VillagerBehaviorState.Idle;
                    stateTimer = Random.Range(3f, 6f);
                }
            }
        }

        private void UpdateIdle()
        {
            // Idle breathing / looking around procedural animation
            ApplyIdleAnimation();

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewWanderTarget();
                currentState = VillagerBehaviorState.Wander;
            }
        }

        private void UpdateFarming()
        {
            // Farming dip / tending crops
            ApplyFarmingAnimation();

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewWanderTarget();
                currentState = VillagerBehaviorState.Wander;
            }
        }

        private void UpdateFleeing()
        {
            Vector3 fleeTarget = transform.position + fleeDirection * 10f;
            MoveTowards(fleeTarget, fleeSpeed);

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewWanderTarget();
                currentState = VillagerBehaviorState.Wander;
            }
        }

        private void UpdateShocked()
        {
            // Shaking in terror, frozen in place
            if (visualRoot != null)
            {
                visualRoot.localPosition = Random.insideUnitSphere * 0.04f;
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewWanderTarget();
                currentState = VillagerBehaviorState.Wander;
            }
        }

        private void UpdateShivering()
        {
            // Jitter / teeth chattering
            if (visualRoot != null)
            {
                visualRoot.localPosition = new Vector3(Random.Range(-0.03f, 0.03f), 0f, Random.Range(-0.03f, 0.03f));
                visualRoot.localRotation = Quaternion.Euler(8f, 0f, Mathf.Sin(Time.time * 25f) * 3f);
            }

            // Move slowly towards nearest village waypoint for shelter
            MoveTowards(targetDestination, walkSpeed * 0.6f);

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewWanderTarget();
                currentState = VillagerBehaviorState.Wander;
            }
        }

        private void UpdateWindStruggle()
        {
            // Lean backward heavily against the wind
            if (visualRoot != null)
            {
                visualRoot.localRotation = Quaternion.Euler(-14f, 0f, Mathf.Sin(Time.time * 8f) * 6f);
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewWanderTarget();
                currentState = VillagerBehaviorState.Wander;
            }
        }

        private void MoveTowards(Vector3 destination, float speed)
        {
            Vector3 diff = destination - transform.position;
            diff.y = 0f;

            if (diff.sqrMagnitude > 0.04f)
            {
                Vector3 moveDir = diff.normalized;
                transform.position += moveDir * speed * Time.deltaTime;

                // Smooth rotation
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

                // Procedural waddle & bounce animation
                walkCycle += Time.deltaTime * (currentState == VillagerBehaviorState.Fleeing ? 14f : 8f);
                float roll = Mathf.Sin(walkCycle) * (currentState == VillagerBehaviorState.Fleeing ? 8f : 5f);
                float bounce = Mathf.Abs(Mathf.Sin(walkCycle)) * 0.12f;

                if (visualRoot != null && visualRoot != transform)
                {
                    visualRoot.localRotation = Quaternion.Euler(currentState == VillagerBehaviorState.Fleeing ? 6f : 3f, 0f, roll);
                    visualRoot.localPosition = new Vector3(0f, bounce, 0f);
                }

                // Dynamic arm animations
                if (leftArmTransform != null && rightArmTransform != null)
                {
                    if (currentState == VillagerBehaviorState.Fleeing)
                    {
                        // Flail arms in air when fleeing
                        float armFlail = Mathf.Sin(walkCycle * 2f) * 20f;
                        leftArmTransform.localRotation = Quaternion.Euler(armFlail, 0f, 150f);
                        rightArmTransform.localRotation = Quaternion.Euler(-armFlail, 0f, -150f);
                    }
                    else
                    {
                        // Natural walking arm swing
                        float armSwing = Mathf.Sin(walkCycle) * 25f;
                        leftArmTransform.localRotation = Quaternion.Euler(armSwing, 0f, 65f);
                        rightArmTransform.localRotation = Quaternion.Euler(-armSwing, 0f, -65f);
                    }
                }
            }

            SnapToTerrain();
        }

        private void SnapToTerrain()
        {
            // Cast ray downwards from high above to firmly anchor onto terrain and bridges without hitting self
            Vector3 origin = new Vector3(transform.position.x, 50f, transform.position.z);
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 100f);
            
            float highestGroundY = float.MinValue;
            bool foundGround = false;

            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i].collider;
                // Ignore self, child colliders, and other villagers
                if (col.transform == transform || col.transform.IsChildOf(transform) || col.GetComponentInParent<VillagerController>() != null)
                    continue;

                // Ignore triggers (reticles, boundary colliders)
                if (col.isTrigger)
                    continue;

                if (hits[i].point.y > highestGroundY)
                {
                    highestGroundY = hits[i].point.y;
                    foundGround = true;
                }
            }

            if (foundGround)
            {
                transform.position = new Vector3(transform.position.x, highestGroundY, transform.position.z);
            }
        }

        private void ApplyIdleAnimation()
        {
            if (visualRoot != null && visualRoot != transform)
            {
                float breathe = Mathf.Sin(Time.time * 2.5f) * 0.02f;
                float sway = Mathf.Sin(Time.time * 1.2f) * 1.5f;
                visualRoot.localPosition = new Vector3(0f, breathe, 0f);
                visualRoot.localRotation = Quaternion.Euler(0f, sway, 0f);
            }
            SetArmPoseDefault();
        }

        public void SetArmPoseDefault()
        {
            if (leftArmTransform == null || rightArmTransform == null)
            {
                foreach (var t in GetComponentsInChildren<Transform>())
                {
                    if (t.name == "PT_LeftArm") leftArmTransform = t;
                    if (t.name == "PT_RightArm") rightArmTransform = t;
                }
            }

            if (leftArmTransform != null) leftArmTransform.localRotation = Quaternion.Euler(0f, 0f, 65f);
            if (rightArmTransform != null) rightArmTransform.localRotation = Quaternion.Euler(0f, 0f, -65f);
        }

        private void ApplyFarmingAnimation()
        {
            if (visualRoot != null && visualRoot != transform)
            {
                float dip = Mathf.PingPong(Time.time * 1.5f, 0.14f);
                float pitch = Mathf.Sin(Time.time * 1.5f) * 12f;
                visualRoot.localPosition = new Vector3(0f, -dip, 0f);
                visualRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void PickNewWanderTarget()
        {
            if (waypoints != null && waypoints.Count > 0)
            {
                Vector3 baseWp = waypoints[Random.Range(0, waypoints.Count)];
                // Add small natural random offset around waypoint
                Vector2 circle = Random.insideUnitCircle * 4f;
                targetDestination = new Vector3(baseWp.x + circle.x, baseWp.y, baseWp.z + circle.y);
            }
            else
            {
                // Fallback default village area around river and bridges
                targetDestination = new Vector3(Random.Range(20f, 45f), 3f, Random.Range(45f, 65f));
            }
        }

        private void UpdateTitheGeneration()
        {
            // Happy villagers produce tithes periodically
            titheTimer -= Time.deltaTime;
            if (titheTimer <= 0f)
            {
                titheTimer = Random.Range(18f, 32f);

                float happiness = PopulationManager.Instance != null ? PopulationManager.Instance.CurrentHappiness : 80f;
                if (happiness >= 65f)
                {
                    int coinsEarned = Random.Range(5, 12);
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.AddCoins(coinsEarned);
                    }

                    if (emoteBubble != null && Random.value < 0.35f)
                    {
                        emoteBubble.ShowEmote(EmoteType.Coin, 2.5f);
                    }
                }
            }
        }

        public void HandlePowerCast(DivinePowerType powerType, Vector3 castPosition, float aoeRadius)
        {
            float distToCast = Vector3.Distance(transform.position, castPosition);

            switch (powerType)
            {
                case DivinePowerType.Sunny:
                    // Island-wide celebration
                    if (emoteBubble != null) emoteBubble.ShowEmote(EmoteType.Happy, 3.5f);
                    currentState = VillagerBehaviorState.Idle;
                    stateTimer = 4f;
                    break;

                case DivinePowerType.Rainy:
                    // Island-wide hydration celebration
                    if (emoteBubble != null) emoteBubble.ShowEmote(EmoteType.Rain, 3.5f);
                    currentState = VillagerBehaviorState.Idle;
                    stateTimer = 4f;
                    break;

                case DivinePowerType.Hot:
                    // Asteroid strike! Flee if within impact danger zone
                    if (distToCast <= aoeRadius * 2.2f)
                    {
                        fleeDirection = (transform.position - castPosition).normalized;
                        if (fleeDirection == Vector3.zero) fleeDirection = Random.insideUnitSphere.normalized;
                        fleeDirection.y = 0f;

                        currentState = VillagerBehaviorState.Fleeing;
                        stateTimer = Random.Range(5f, 8f);
                        if (emoteBubble != null) emoteBubble.ShowEmote(EmoteType.Heat, 4.0f);
                    }
                    break;

                case DivinePowerType.Lightning:
                    // Lightning strike! Shocked panic if nearby
                    if (distToCast <= aoeRadius * 1.8f)
                    {
                        currentState = VillagerBehaviorState.Shocked;
                        stateTimer = Random.Range(3.5f, 5.5f);
                        if (emoteBubble != null) emoteBubble.ShowEmote(EmoteType.Shock, 3.8f);
                    }
                    break;

                case DivinePowerType.Wind:
                    // Whirlwind storm
                    if (distToCast <= aoeRadius * 2.0f)
                    {
                        currentState = VillagerBehaviorState.WindStruggle;
                        stateTimer = Random.Range(4f, 6f);
                        if (emoteBubble != null) emoteBubble.ShowEmote(EmoteType.Wind, 4.0f);
                    }
                    break;

                case DivinePowerType.Cold:
                    // Blizzard / hail freeze
                    if (distToCast <= aoeRadius * 2.2f || distToCast < 25f)
                    {
                        currentState = VillagerBehaviorState.Shivering;
                        stateTimer = Random.Range(5f, 7.5f);
                        if (emoteBubble != null) emoteBubble.ShowEmote(EmoteType.Cold, 4.0f);
                    }
                    break;
            }
        }
    }
}
