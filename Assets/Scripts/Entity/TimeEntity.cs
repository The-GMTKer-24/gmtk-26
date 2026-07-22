using System;
using UnityEngine;

namespace Entity
{
    public class TimeEntity : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private float currentTime;

        public void Awake()
        {
            currentTime = maxTime;
        }

        public void Update()
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                Destroy(gameObject); // exploded
            }
        }

        public float GetTime()
        {
            return currentTime;
        }

        public void DealDamage(float damage)
        {
            currentTime -= damage;
            if (currentTime <= 0)
            {
                Destroy(gameObject); // exploded
            }
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
    }
}