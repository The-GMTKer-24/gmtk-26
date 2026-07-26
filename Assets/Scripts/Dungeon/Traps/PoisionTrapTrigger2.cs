using UnityEngine;

namespace Dungeon.Traps
{
    public class PoisionTrapTrigger2 : MonoBehaviour
    {
    
        private bool hasTriggered;
        private bool isEmitting;

        [SerializeField] public Sprite triggeredSprite;
        [SerializeField] public Sprite untriggeredSprite;
        [SerializeField] private GameObject particleObject;
        [SerializeField] private GameObject triggerParticle;
        [SerializeField] public GameObject poisonTrapSoundEffect;
    
        private SoundManager soundManager;
    
        private void Start()
        {
            soundManager = SoundManager.Instance;
            gameObject.GetComponent<SpriteRenderer>().sprite = untriggeredSprite;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (!hasTriggered)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = triggeredSprite;
                particleObject.SetActive(true);
                triggerParticle.SetActive(true);
                // Play sound
                soundManager.CreateSoundAtPosition(poisonTrapSoundEffect, transform.position);
                hasTriggered = true;
                Invoke(nameof(ShutdownParticles),8f);
            }
        }

        void ShutdownParticles()
        {
            ParticleSystem ps =  particleObject.GetComponent<ParticleSystem>();
            ps.Stop();
            triggerParticle.SetActive(false);
        }
    }
}
