using UnityEngine;

public class PoisionTrapTrigger2 : MonoBehaviour
{
    
    private bool hasTriggered;
    private bool isEmitting;

    [SerializeField] public Sprite triggeredSprite;
    [SerializeField] public Sprite untriggeredSprite;
    [SerializeField] private GameObject particleObject;
    [SerializeField] private GameObject triggerParticle;
    
    
    private void Start()
    {
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
            gameObject.GetComponent<AudioSource>().Play();
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
