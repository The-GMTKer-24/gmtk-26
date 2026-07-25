using System;
using UnityEngine;

namespace Entity
{
    public class SortingOrderHandler : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private const float Factor = 100f;
        private const int Offset = 0;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingLayerName = "Game Objects";
        }
        
        private void Update()
        {
            print(gameObject.transform.position.y + ", " + Mathf.RoundToInt(gameObject.transform.position.y * -Factor) + ", " + (Mathf.RoundToInt(gameObject.transform.position.y * -Factor) + Offset));
            spriteRenderer.sortingOrder = Mathf.RoundToInt(gameObject.transform.position.y * -Factor) + Offset;
        }

        public static int RecommendedOffset(float yOffset)
        {
            return Math.Max(1, Mathf.RoundToInt(yOffset * -Factor));
        }
    }
}