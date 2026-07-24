using UnityEngine;

namespace Entity
{
    public class WalkCycleCardinal : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        /*[SerializeField] Sprite[] northSprites;
        [SerializeField] Sprite[] southSprites;
        [SerializeField] Sprite[] eastSprites;
        [SerializeField] Sprite[] westSprites;*/
        [SerializeField] Sprite defaultSprite;
        [SerializeField] Animator animator;
        [SerializeField] float cyclesPerSecond;
        
        public Vector2 velocity; // Current velocity of the character. Should affect walk cycle speed as well as sprite direction choice
        
        void Start()
        {
            spriteRenderer.sprite = defaultSprite;
        }

        void FixedUpdate()
        {
            
        }
    }
}