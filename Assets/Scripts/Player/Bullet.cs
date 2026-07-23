using System;
using Entity;
using UnityEngine;

namespace Player
{
    public class Bullet : MonoBehaviour
    {
        public Vector2 speed; // This should be velocity, not speed!
        public float damage;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private GameObject deathParticles;
        public void Start()
        {
            rb.linearVelocity = speed;
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.CompareTag("Enemy"))
            {
                TimeEntity hp = other.gameObject.GetComponent<TimeEntity>();
                hp.DealDamage(damage);
                Player.Instance.TimeEntity.Heal(damage * Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeSteal));
            }
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}