using System.Collections.Generic;
using UnityEngine;

namespace Text_Particles
{
    public sealed class TextParticleSystem2D : MonoBehaviour
    {
        public static TextParticleSystem2D Instance;
    
        [Header("Prefab and Pool")]
        [SerializeField] private TextParticle2D particlePrefab;
        [SerializeField, Min(0)] private int prewarmCount = 16;

        [Header("Default Appearance")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField, Min(0.01f)] private float fontSize = 4f;
        [SerializeField, Min(0.01f)] private float lifetime = 0.9f;
        [SerializeField, Min(0f)] private float startScale = 0.85f;
        [SerializeField, Min(0f)] private float endScale = 1.1f;
        [SerializeField] private AnimationCurve alphaOverLifetime = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.65f, 1f),
            new Keyframe(1f, 0f));

        [Header("Default Motion")]
        [SerializeField] private Vector2 initialVelocity = new Vector2(0f, 1.5f);
        [SerializeField] private Vector2 velocityRandomness = new Vector2(0.35f, 0.2f);
        [SerializeField] private float gravity = 0f;

        [Header("2D Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 100;

        private readonly Queue<TextParticle2D> availableParticles = new Queue<TextParticle2D>();

        private void Awake()
        {
            Instance = this;
            if (particlePrefab == null)
            {
                Debug.LogError("Assign a TextParticle2D prefab before using this system.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < prewarmCount; i++)
            {
                availableParticles.Enqueue(CreateParticle());
            }
        }


        public TextParticle2D Spawn(string content, Vector2 worldPosition)
        {
            return Spawn(content, worldPosition, defaultColor);
        }
    
        public TextParticle2D Spawn(string content, Vector2 worldPosition, Color color)
        {
            Vector2 randomVelocity = new Vector2(
                Random.Range(-Mathf.Abs(velocityRandomness.x), Mathf.Abs(velocityRandomness.x)),
                Random.Range(-Mathf.Abs(velocityRandomness.y), Mathf.Abs(velocityRandomness.y)));

            return Spawn(
                content,
                worldPosition,
                color,
                fontSize,
                lifetime,
                initialVelocity + randomVelocity);
        }
    
        public TextParticle2D Spawn(
            string content,
            Vector2 worldPosition,
            Color color,
            float customFontSize,
            float customLifetime,
            Vector2 customVelocity)
        {
            if (particlePrefab == null || string.IsNullOrEmpty(content))
            {
                return null;
            }

            TextParticle2D particle = GetParticle();
            Vector3 spawnPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);

            particle.Play(
                content,
                spawnPosition,
                color,
                Mathf.Max(0.01f, customFontSize),
                Mathf.Max(0.01f, customLifetime),
                customVelocity,
                gravity,
                startScale,
                endScale,
                alphaOverLifetime,
                ReturnToPool);

            return particle;
        }

        private TextParticle2D GetParticle()
        {
            while (availableParticles.Count > 0)
            {
                TextParticle2D particle = availableParticles.Dequeue();
                if (particle != null)
                {
                    return particle;
                }
            }

            return CreateParticle();
        }

        private TextParticle2D CreateParticle()
        {
            TextParticle2D particle = Instantiate(particlePrefab, transform);
            particle.name = particlePrefab.name + " (Pooled)";
            particle.ConfigureSorting(sortingLayerName, sortingOrder);
            particle.gameObject.SetActive(false);
            return particle;
        }

        private void ReturnToPool(TextParticle2D particle)
        {
            if (particle == null)
            {
                return;
            }

            particle.gameObject.SetActive(false);
            particle.transform.SetParent(transform, true);
            availableParticles.Enqueue(particle);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            prewarmCount = Mathf.Max(0, prewarmCount);
            fontSize = Mathf.Max(0.01f, fontSize);
            lifetime = Mathf.Max(0.01f, lifetime);
            startScale = Mathf.Max(0f, startScale);
            endScale = Mathf.Max(0f, endScale);

            if (alphaOverLifetime == null || alphaOverLifetime.length == 0)
            {
                alphaOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            }
        }
#endif
    }
}
