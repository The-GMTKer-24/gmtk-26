using System;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float startingTime;
    [SerializeField] private float currentTime;
    [SerializeField] private float damageRadius;
    [SerializeField] private float radius;
    [SerializeField] private float stoppingForce;
    [SerializeField] private GameObject explosionParticles;
    [SerializeField] private GameObject clinkSound;
    [SerializeField] private GameObject boomSound;
    
    public Vector2 velocity; // This should be velocity, not speed!
    public float speed;
    public float damage;

    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;
        speed = velocity.magnitude;
        currentTime = startingTime;
    }

    private void FixedUpdate()
    {
        Vector2 v = rb.linearVelocity;

        currentTime -= Time.fixedDeltaTime;
        if (currentTime <= 0)
        {
            GameObject obj = Instantiate(explosionParticles, transform.position, transform.rotation);
            obj.GetComponent<BombExplosion>().poisionDamage = damage;
            SoundManager.Instance.CreateSoundAtPosition(boomSound, transform.position);
            Destroy(gameObject);
        }
        
        if (v.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.AddForce(-v.normalized * stoppingForce);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log(
            $"Bomb hit {other.collider.name}, " +
            $"layer: {LayerMask.LayerToName(other.gameObject.layer)}"
        );
        SoundManager.Instance.CreateSoundAtPosition(clinkSound, other.transform.position);
    }
}
