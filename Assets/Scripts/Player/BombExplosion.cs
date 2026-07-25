using System;
using Entity;
using UnityEngine;

public class BombExplosion : MonoBehaviour{
    private void Start()
    {
        Invoke(nameof(Teardown), .4f);
        Invoke(nameof(DisableParticles), .2f);
    }
    
    public float poisionDamage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<TimeEntity>().DealDamage(poisionDamage);
        }
    }

    void Teardown()
    {
        Destroy(gameObject);
    }

    void DisableParticles()
    {
        gameObject.GetComponent<ParticleSystem>().Stop();
    }
}
