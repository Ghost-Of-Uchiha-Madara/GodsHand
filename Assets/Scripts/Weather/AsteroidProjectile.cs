using UnityEngine;

namespace TheGodAbove.Weather
{
    public class AsteroidProjectile : MonoBehaviour
    {
        [Header("--- Flight Settings ---")]
        [SerializeField] private float speed = 40f;
        [SerializeField] private float rotationSpeed = 240f;

        private Vector3 targetPosition;
        private Vector3 direction;
        private GameObject fireFloorPrefab;
        private GameObject explosionPrefab;
        private AudioClip impactAudio;
        private bool hasHit = false;

        public void Initialize(Vector3 target, GameObject fireFloor, GameObject explosion, AudioClip sfx)
        {
            targetPosition = target;
            fireFloorPrefab = fireFloor;
            explosionPrefab = explosion;
            impactAudio = sfx;

            direction = (targetPosition - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void Update()
        {
            if (hasHit) return;

            // Rotate rock
            transform.Rotate(Vector3.right * (rotationSpeed * Time.deltaTime));

            // Move towards target
            float step = speed * Time.deltaTime;
            float distToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distToTarget <= step)
            {
                transform.position = targetPosition;
                OnImpact();
            }
            else
            {
                transform.position += direction * step;
            }
        }

        private void OnImpact()
        {
            if (hasHit) return;
            hasHit = true;

            // 1. Spawn Ground Fire (Free Fire VFX)
            if (fireFloorPrefab != null)
            {
                var fireObj = Instantiate(fireFloorPrefab, targetPosition, Quaternion.identity);
                Destroy(fireObj, 10f); // Fire burns for 10 seconds
            }

            // 2. Spawn Impact Blast
            if (explosionPrefab != null)
            {
                var expObj = Instantiate(explosionPrefab, targetPosition, Quaternion.identity);
                Destroy(expObj, 4f);
            }

            // 3. Play Fire Impact SFX
            if (impactAudio != null)
            {
                var sfxGO = new GameObject("SFX_AsteroidImpact");
                sfxGO.transform.position = targetPosition;
                var aSource = sfxGO.AddComponent<AudioSource>();
                aSource.clip = impactAudio;
                aSource.volume = 0.9f;
                aSource.spatialBlend = 0.5f;
                aSource.Play();
                Destroy(sfxGO, impactAudio.length + 0.5f);
            }

            // 4. Trigger Camera Shake
            if (Camera.main != null)
            {
                var camCtrl = Camera.main.GetComponent<TheGodAbove.CameraControl.IsometricCameraController>();
                if (camCtrl != null)
                {
                    // Camera impulse
                }
            }

            // Destroy the projectile rock and trailing fire
            Destroy(gameObject, 0.05f);
        }
    }
}
