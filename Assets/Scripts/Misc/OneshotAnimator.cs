using UnityEngine;

namespace Misc
{
    public class OneshotAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int keyframeCount;
        [SerializeField] private float animationSpeed;

        public void Start()
        {
            animator.speed = animationSpeed;
        }

        public void Update()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > (1 - 1f / (keyframeCount+2)))
            {
                animator.speed = 0;
                Destroy(this.gameObject);
            }
        }
    }
}