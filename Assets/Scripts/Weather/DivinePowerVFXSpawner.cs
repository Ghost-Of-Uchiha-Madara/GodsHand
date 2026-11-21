using UnityEngine;
using System.Collections;
using TheGodAbove.UI;

namespace TheGodAbove.Weather
{
    public class DivinePowerVFXSpawner : MonoBehaviour
    {
        public static DivinePowerVFXSpawner Instance { get; private set; }

        private Material particleAdditiveMat;
        private Material particleAlphaMat;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                SafeDestroy(gameObject);
                return;
            }

            InitMaterials();
        }

        private void InitMaterials()
        {
            var unlitShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");

            particleAdditiveMat = new Material(unlitShader);
            particleAdditiveMat.SetFloat("_Surface", 1); // Transparent
            particleAdditiveMat.SetFloat("_Blend", 1); // Additive

            particleAlphaMat = new Material(unlitShader);
            particleAlphaMat.SetFloat("_Surface", 1); // Transparent
            particleAlphaMat.SetFloat("_Blend", 0); // Alpha
        }

        public void SpawnVFX(DivinePowerType type, Vector3 position, float radius)
        {
            switch (type)
            {
                case DivinePowerType.Sunny:
                    SpawnSunnyVFX(position, radius);
                    break;
                case DivinePowerType.Rainy:
                    SpawnRainyVFX(position, radius);
                    break;
                case DivinePowerType.Hot:
                    SpawnHeatVFX(position, radius);
                    break;
                case DivinePowerType.Lightning:
                    SpawnLightningVFX(position, radius);
                    break;
                case DivinePowerType.Wind:
                    SpawnWindVFX(position, radius);
                    break;
                case DivinePowerType.Cold:
                    SpawnColdVFX(position, radius);
                    break;
            }
        }

        // ==========================================
        // 1. SUNNY: Sunbeam shafts & golden sparkles
        // ==========================================
        private void SpawnSunnyVFX(Vector3 pos, float radius)
        {
            var go = new GameObject("VFX_Sunny");
            go.transform.position = pos;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.5f);
            light.range = radius * 1.5f;
            light.intensity = 4f;

            var ps = go.AddComponent<ParticleSystem>();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleAdditiveMat;

            var main = ps.main;
            main.duration = 4f;
            main.loop = false;
            main.startLifetime = 2f;
            main.startSpeed = 2.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startColor = new Color(1f, 0.85f, 0.3f, 0.9f);
            main.gravityModifier = -0.15f;

            var emission = ps.emission;
            emission.rateOverTime = 35f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius * 0.7f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.7f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = grad;

            ps.Play();
            StartCoroutine(FadeLightAndDestroy(go, light, 4f));
        }

        // ==========================================
        // 2. RAINY: Raincloud & falling rain streaks
        // ==========================================
        private void SpawnRainyVFX(Vector3 pos, float radius)
        {
            var go = new GameObject("VFX_Rain");
            go.transform.position = pos + Vector3.up * 14f;

            var cloudPS = go.AddComponent<ParticleSystem>();
            var cloudRenderer = go.GetComponent<ParticleSystemRenderer>();
            cloudRenderer.material = particleAlphaMat;

            var cloudMain = cloudPS.main;
            cloudMain.duration = 5f;
            cloudMain.loop = false;
            cloudMain.startLifetime = 3f;
            cloudMain.startSpeed = 0.5f;
            cloudMain.startSize = new ParticleSystem.MinMaxCurve(3f, 5f);
            cloudMain.startColor = new Color(0.35f, 0.45f, 0.55f, 0.7f);

            var cloudEmission = cloudPS.emission;
            cloudEmission.rateOverTime = 15f;

            var cloudShape = cloudPS.shape;
            cloudShape.shapeType = ParticleSystemShapeType.Circle;
            cloudShape.radius = radius * 0.8f;
            cloudShape.rotation = new Vector3(90f, 0f, 0f);

            var rainGO = new GameObject("RainDrops");
            rainGO.transform.SetParent(go.transform, false);
            var rainPS = rainGO.AddComponent<ParticleSystem>();
            var rainRenderer = rainGO.GetComponent<ParticleSystemRenderer>();
            rainRenderer.material = particleAdditiveMat;
            rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            rainRenderer.lengthScale = 3f;
            rainRenderer.velocityScale = 0.04f;

            var rainMain = rainPS.main;
            rainMain.duration = 5f;
            rainMain.loop = false;
            rainMain.startLifetime = 0.9f;
            rainMain.startSpeed = 18f;
            rainMain.startSize = 0.15f;
            rainMain.startColor = new Color(0.6f, 0.85f, 1f, 0.8f);

            var rainEmission = rainPS.emission;
            rainEmission.rateOverTime = 120f;

            var rainShape = rainPS.shape;
            rainShape.shapeType = ParticleSystemShapeType.Circle;
            rainShape.radius = radius * 0.85f;
            rainShape.rotation = new Vector3(90f, 0f, 0f);

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.4f, 0.65f, 0.9f);
            light.range = radius * 1.5f;
            light.intensity = 1.8f;

            cloudPS.Play();
            rainPS.Play();
            StartCoroutine(FadeLightAndDestroy(go, light, 5.5f));
        }

        // ==========================================
        // 3. HEAT: Heat distortion & rising embers
        // ==========================================
        private void SpawnHeatVFX(Vector3 pos, float radius)
        {
            var go = new GameObject("VFX_Heat");
            go.transform.position = pos;

            var ps = go.AddComponent<ParticleSystem>();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleAdditiveMat;

            var main = ps.main;
            main.duration = 4.5f;
            main.loop = false;
            main.startLifetime = 2f;
            main.startSpeed = 3f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startColor = new Color(1f, 0.45f, 0.1f, 0.9f);
            main.gravityModifier = -0.2f;

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius * 0.65f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.8f, 0.2f), 0f), new GradientColorKey(new Color(0.9f, 0.2f, 0.05f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = grad;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.45f, 0.1f);
            light.range = radius * 1.4f;
            light.intensity = 5f;

            ps.Play();
            StartCoroutine(FadeLightAndDestroy(go, light, 4.5f));
        }

        // ==========================================
        // 4. LIGHTNING: Heavenly bolt strike & shockwave
        // ==========================================
        private void SpawnLightningVFX(Vector3 pos, float radius)
        {
            var go = new GameObject("VFX_Lightning");
            go.transform.position = pos;

            var boltGO = new GameObject("Bolt");
            boltGO.transform.SetParent(go.transform, false);
            var lr = boltGO.AddComponent<LineRenderer>();
            lr.material = particleAdditiveMat;
            lr.startColor = new Color(1f, 1f, 0.9f, 1f);
            lr.endColor = new Color(1f, 0.95f, 0.4f, 1f);
            lr.startWidth = 0.45f;
            lr.endWidth = 0.25f;

            int segments = 14;
            lr.positionCount = segments;
            Vector3 startPos = pos + Vector3.up * 45f;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                Vector3 p = Vector3.Lerp(startPos, pos, t);
                if (i > 0 && i < segments - 1)
                {
                    p += new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                }
                lr.SetPosition(i, p);
            }

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 1f, 0.85f);
            light.range = 35f;
            light.intensity = 16f;

            var sparkPS = go.AddComponent<ParticleSystem>();
            var sparkR = go.GetComponent<ParticleSystemRenderer>();
            sparkR.material = particleAdditiveMat;

            var sparkMain = sparkPS.main;
            sparkMain.duration = 1.2f;
            sparkMain.loop = false;
            sparkMain.startLifetime = 0.6f;
            sparkMain.startSpeed = 12f;
            sparkMain.startSize = 0.25f;
            sparkMain.startColor = new Color(1f, 0.95f, 0.4f, 1f);
            sparkMain.gravityModifier = 1f;

            var sparkEmission = sparkPS.emission;
            sparkEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

            var sparkShape = sparkPS.shape;
            sparkShape.shapeType = ParticleSystemShapeType.Sphere;
            sparkShape.radius = 0.5f;

            sparkPS.Play();
            StartCoroutine(FlashLightning(lr, light, go));
        }

        private IEnumerator FlashLightning(LineRenderer lr, Light light, GameObject root)
        {
            light.intensity = 18f;
            yield return new WaitForSeconds(0.06f);
            light.intensity = 4f;
            lr.enabled = false;
            yield return new WaitForSeconds(0.04f);

            lr.enabled = true;
            light.intensity = 14f;
            yield return new WaitForSeconds(0.08f);
            lr.enabled = false;

            float timer = 0.5f;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                light.intensity = Mathf.Lerp(0f, 4f, timer / 0.5f);
                yield return null;
            }

            SafeDestroy(root, 1f);
        }

        // ==========================================
        // 5. WIND: Directional gusts & swirling leaves
        // ==========================================
        private void SpawnWindVFX(Vector3 pos, float radius)
        {
            var go = new GameObject("VFX_Wind");
            go.transform.position = pos + Vector3.up * 1f;

            var ps = go.AddComponent<ParticleSystem>();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleAdditiveMat;

            var main = ps.main;
            main.duration = 3.5f;
            main.loop = false;
            main.startLifetime = 2f;
            main.startSpeed = 8f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = new Color(0.4f, 0.95f, 0.8f, 0.8f);

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = radius * 0.5f;
            shape.rotation = new Vector3(-10f, 65f, 0f);

            var velOverLife = ps.velocityOverLifetime;
            velOverLife.enabled = true;
            velOverLife.orbitalZ = 2f;

            ps.Play();
            SafeDestroy(go, 4f);
        }

        // ==========================================
        // 6. COLD: Snowfall & frost mist
        // ==========================================
        private void SpawnColdVFX(Vector3 pos, float radius)
        {
            var go = new GameObject("VFX_Cold");
            go.transform.position = pos + Vector3.up * 12f;

            var ps = go.AddComponent<ParticleSystem>();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleAdditiveMat;

            var main = ps.main;
            main.duration = 5f;
            main.loop = false;
            main.startLifetime = 3f;
            main.startSpeed = 4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
            main.startColor = new Color(0.7f, 0.9f, 1f, 0.9f);

            var emission = ps.emission;
            emission.rateOverTime = 65f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius * 0.75f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var velOverLife = ps.velocityOverLifetime;
            velOverLife.enabled = true;
            velOverLife.orbitalZ = 0.5f;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.85f, 1f);
            light.range = radius * 1.5f;
            light.intensity = 3.5f;

            ps.Play();
            StartCoroutine(FadeLightAndDestroy(go, light, 5.5f));
        }

        private IEnumerator FadeLightAndDestroy(GameObject root, Light light, float lifetime)
        {
            float elapsed = 0f;
            float initialIntensity = light != null ? light.intensity : 0f;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                if (light != null && elapsed > lifetime * 0.6f)
                {
                    float t = (lifetime - elapsed) / (lifetime * 0.4f);
                    light.intensity = Mathf.Lerp(0f, initialIntensity, t);
                }
                yield return null;
            }

            SafeDestroy(root);
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
