using System;
using UnityEngine;

namespace Entity
{
    public class TimeEntity : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private float currentTime;
        [SerializeField] private GameObject spawnOnDeath;
        [SerializeField] private GameObject dropOnDeath;
        public void Awake()
        {
            currentTime = maxTime;
        }

        public void Update()
        {
            currentTime -= Time.deltaTime;
            CheckDeath(true);
        }

        public float GetTime()
        {
            return currentTime;
        }

        public void DealDamage(float damage)
        {
            currentTime -= damage;
            CheckDeath(false);
        }

        public void Heal(float time)
        {
            currentTime += time;
            currentTime = Mathf.Min(currentTime, maxTime);
        }

        public float GetMaxTime()
        {
            return maxTime;
        }
        private void CheckDeath(bool natural)
        {
            if (currentTime <= 0)
            {
                if (!natural)
                    Instantiate(dropOnDeath, transform.position, Quaternion.identity);
                if (spawnOnDeath)
                    Instantiate(spawnOnDeath, transform.position, Quaternion.identity);
                Destroy(gameObject); // exploded
            }
        }
    }
}