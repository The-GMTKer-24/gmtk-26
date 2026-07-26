using System;
using System.Collections.Generic;
using Text_Particles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entity
{
    public class TimeEntity : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private float currentTime;
        [SerializeField] private GameObject spawnOnDeath;
        [SerializeField] private GameObject dropOnDeath;
        [SerializeField] private GameObject deathSound;
        [SerializeField] private List<GameObject> damageSounds;
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
            if (damageSounds.Count != 0) SoundManager.Instance.CreateSoundAtPosition(damageSounds[Random.Range(0,damageSounds.Count)], transform.position);
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
        public void SetMaxTime(float time)
        {
            maxTime = time;
        }
        private void CheckDeath(bool natural)
        {
            if (dead)
            {
                return;
            }
            if (currentTime <= 0)
            {
                if (deathSound) SoundManager.Instance.CreateSoundAtPosition(deathSound, transform.position);
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