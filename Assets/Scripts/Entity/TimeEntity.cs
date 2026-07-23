using System;
using UnityEngine;

namespace Entity
{
    public class TimeEntity : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private float currentTime;
        [SerializeField] private GameObject spawnOnDeath;
        public void Awake()
        {
            currentTime = maxTime;
        }

        public void Update()
        {
            currentTime -= Time.deltaTime;
            CheckDeath();
        }

        public float GetTime()
        {
            return currentTime;
        }

        public void DealDamage(float damage)
        {
            currentTime -= damage;
            CheckDeath();
        }

        public void Heal(float time)
        {
            currentTime += time;
            currentTime = Mathf.Max(currentTime, maxTime);
        }

        public float GetMaxTime()
        {
            return maxTime;
        }
        private void CheckDeath()
        {
            if (currentTime <= 0)
            {
                Instantiate(spawnOnDeath, transform.position, Quaternion.identity);
                Destroy(gameObject); // exploded
            }
        }
    }
}