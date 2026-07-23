using System;
using TMPro;
using UnityEngine;

namespace Text_Particles
{
    public sealed class TextParticle2D : MonoBehaviour
    {
        [SerializeField] private TextMeshPro label;

        private Action<TextParticle2D> returnToPool;
        private AnimationCurve alphaOverLifetime;
        private Vector2 velocity;
        private Color startColor;
        private float gravity;
        private float lifetime;
        private float elapsed;
        private float startScale;
        private float endScale;
        private bool isPlaying;

        public bool IsPlaying => isPlaying;

        private void Reset()
        {
            EnsureReferences();

            if (label != null)
            {
                label.alignment = TextAlignmentOptions.Center;
            }
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;

            velocity.y += gravity * deltaTime;

            Vector3 position = transform.position;
            position.x += velocity.x * deltaTime;
            position.y += velocity.y * deltaTime;
            transform.position = position;

            float normalizedTime = Mathf.Clamp01(elapsed / lifetime);
            float scale = Mathf.LerpUnclamped(startScale, endScale, normalizedTime);
            transform.localScale = Vector3.one * scale;

            float alphaMultiplier = alphaOverLifetime != null
                ? Mathf.Clamp01(alphaOverLifetime.Evaluate(normalizedTime))
                : 1f - normalizedTime;

            Color currentColor = startColor;
            currentColor.a *= alphaMultiplier;
            label.color = currentColor;

            if (elapsed >= lifetime)
            {
                Finish();
            }
        }

        internal void ConfigureSorting(string sortingLayerName, int sortingOrder)
        {
            EnsureReferences();

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;
        }

        internal void Play(
            string content,
            Vector3 worldPosition,
            Color color,
            float fontSize,
            float duration,
            Vector2 initialVelocity,
            float downwardGravity,
            float initialScale,
            float finalScale,
            AnimationCurve fadeCurve,
            Action<TextParticle2D> onFinished)
        {
            if (!EnsureReferences())
            {
                Debug.LogError("TextParticle2D requires a TextMeshPro component.", this);
                return;
            }

            returnToPool = onFinished;
            alphaOverLifetime = fadeCurve;
            velocity = initialVelocity;
            startColor = color;
            gravity = downwardGravity;
            lifetime = Mathf.Max(0.01f, duration);
            elapsed = 0f;
            startScale = initialScale;
            endScale = finalScale;
            isPlaying = true;

            transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
            transform.localScale = Vector3.one * startScale;

            label.text = content;
            label.fontSize = fontSize;
            label.color = startColor;
            label.alignment = TextAlignmentOptions.Center;

            gameObject.SetActive(true);
        }
    
        public void Stop()
        {
            if (isPlaying)
            {
                Finish();
            }
        }

        private void Finish()
        {
            if (!isPlaying)
            {
                return;
            }

            isPlaying = false;

            Action<TextParticle2D> callback = returnToPool;
            returnToPool = null;
            callback?.Invoke(this);
        }

        private bool EnsureReferences()
        {
            if (label == null)
            {
                label = GetComponent<TextMeshPro>();
            }

            return label != null;
        }
    }
}
