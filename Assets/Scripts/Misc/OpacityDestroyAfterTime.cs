using UnityEngine;

namespace Misc
{
    public class OpacityDestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float time;
        private float elapsed;
        private Color _originalColor;

        public void Start()
        {
            _originalColor = spriteRenderer.color;
        }
        
        public void Update()
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(_originalColor, Color.clear, elapsed / time);
            if (elapsed >= time)
            {
                Destroy(gameObject);
            }
        }
    }
}