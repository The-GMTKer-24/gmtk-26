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
            Instantiate(explosionParticles, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        
        if (v.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.AddForce(-v.normalized * stoppingForce);

    }
}
