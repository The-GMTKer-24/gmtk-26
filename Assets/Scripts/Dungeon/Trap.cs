using System;
using Entity;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private bool hasTriggered;

    [SerializeField] public Sprite triggeredSprite;
    [SerializeField] public Sprite untriggeredSprite;

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
            TimeEntity time =  other.gameObject.GetComponent<TimeEntity>();
            time.DealDamage( (float) Math.Round( time.GetTime()/4));
            print("Player!");
            gameObject.GetComponent<SpriteRenderer>().sprite = triggeredSprite;
            hasTriggered = true;
        }
    }
}
