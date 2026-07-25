using System;
using Entity;
using UnityEngine;
using UnityEngine.Serialization;

namespace Boss
{
    public class BulletShip : MonoBehaviour
    {
        public Vector2 velocity;
        public float damage;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float fireRate;
        [FormerlySerializedAs("bullet")] [SerializeField] private EnemyBullet toFire;
        [SerializeField] private float bulletSpeed;
        
        private float fireTime;

        public void Awake()
        {
            print("I LIVE");
        }

        public void Update()
        {
            if (fireTime > fireRate)
            {
                fireTime = 0;
                EnemyBullet bullet = Instantiate(toFire,transform.position,Quaternion.identity);
                bullet.velocity = Vector2.down * bulletSpeed;
                bullet.damage = damage;
                bullet.remainingTime = 10000;
                bullet.GetComponent<Rigidbody2D>().rotation =
                    -Vector2.SignedAngle(Vector2.down, Vector2.up);
            }

            fireTime += Time.deltaTime;
            rb.linearVelocity = velocity;
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            Destroy(gameObject);
        }
    }
}