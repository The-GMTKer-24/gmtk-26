using Player;
using UnityEngine;

namespace Entity
{
    public class EnemyBullet : MonoBehaviour
    {
        public Vector2 velocity;
        public float speed;
        public float damage;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private GameObject deathParticles;
        
        [SerializeField] public float remainingTime;
        public void Start()
        {
            rb.linearVelocity = velocity;
            speed = velocity.magnitude;
        }

        public void FixedUpdate()
        {
            remainingTime -= Time.fixedDeltaTime;
            if (remainingTime <= 0)
            {
                Destroy(gameObject);
            }
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