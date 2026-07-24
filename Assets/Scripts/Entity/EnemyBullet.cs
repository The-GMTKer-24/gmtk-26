using Player;
using UnityEngine;

namespace Entity
{
    public class EnemyBullet : MonoBehaviour
    {
        public Vector2 velocity; // This should be velocity, not speed!
        public float speed;
        public float damage;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private GameObject deathParticles;
        public void Start()
        {
            rb.linearVelocity = velocity;
            speed = velocity.magnitude;
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.CompareTag("Player"))
            {
                TimeEntity hp = other.gameObject.GetComponent<TimeEntity>();
                hp.DealDamage(damage);

            }
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}