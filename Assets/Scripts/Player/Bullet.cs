using System;
using Entity;
using Text_Particles;
using UnityEngine;

namespace Player
{
    public class Bullet : MonoBehaviour
    {
        public Vector2 velocity; // This should be velocity, not speed!
        public float speed;
        public float damage;
        public int bounces;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private GameObject deathParticles;
        public void Start()
        {
            rb.linearVelocity = velocity;
            speed = velocity.magnitude;
            bounces = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.BulletBounces);
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.CompareTag("Enemy"))
            {
                TimeEntity hp = other.gameObject.GetComponent<TimeEntity>();
                hp.DealDamage(damage);
                float toHeal = damage * Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeSteal);
                if (toHeal > 0)
                {
                    Player.Instance.TimeEntity.Heal(toHeal);
                }
            }
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            if (bounces > 0)
            {
                bounces--;
                rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, other.contacts[0].normal);
                rb.linearVelocity = rb.linearVelocity.normalized * speed;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}