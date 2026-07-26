using System;
using Entity;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private bool hasTriggered;

    [SerializeField] public Sprite triggeredSprite;
    [SerializeField] public Sprite untriggeredSprite;
    [SerializeField] public GameObject timeTrapSound;
    
    private SoundManager _soundManager;

    private void Start()
    {
        _soundManager = SoundManager.Instance;
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
            TimeEntity time =  other.gameObject.GetComponent<TimeEntity>();
            time.DealDamage( (float) Math.Round( time.GetTime()/4));
            gameObject.GetComponent<SpriteRenderer>().sprite = triggeredSprite;
            _soundManager.CreateSoundAtPosition(timeTrapSound, transform.position);
            hasTriggered = true;
        }
    }
}
