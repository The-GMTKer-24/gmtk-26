using System;
using Text_Particles;
using UnityEngine;

namespace Entity
{
    public class TimeEntity : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private float currentTime;
        [SerializeField] private GameObject spawnOnDeath;
        [SerializeField] private GameObject dropOnDeath;
        private bool dead;
        public void Awake()
        {
            currentTime = maxTime;
        }

        public void FixedUpdate()
        {
            currentTime -= Time.fixedDeltaTime;
            CheckDeath(true);
        }

        public float GetTime()
        {
            return currentTime;
        }

        public void DealDamage(float damage)
        {
            TextParticleSystem2D.Instance.Spawn($"-{damage}s", transform.position, Color.softRed);
            currentTime -= damage;
            CheckDeath(false);
        }

        public void Heal(float time)
        {
            TextParticleSystem2D.Instance.Spawn($"+{time}s", transform.position, Color.lightGreen);
            currentTime += time;
            currentTime = Mathf.Min(currentTime, maxTime);
        }

        public float GetMaxTime()
        {
            return maxTime;
        }
        private void CheckDeath(bool natural)
        {
            if (dead)
            {
                return;
            }
            if (currentTime <= 0)
            {
                if (!natural)
                    Instantiate(dropOnDeath, transform.position, Quaternion.identity);
                if (spawnOnDeath)
                    Instantiate(spawnOnDeath, transform.position, Quaternion.identity);
                dead = true;
                Destroy(gameObject); // exploded
            }
        }
    }
}