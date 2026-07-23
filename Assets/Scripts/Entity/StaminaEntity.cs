using System;
using UnityEngine;

namespace Entity
{
    public class StaminaEntity : MonoBehaviour
    {
        [SerializeField] private float maxStamina;
        [SerializeField] private float currentStamina = 0f;
        [SerializeField] private float staminaRegenerationRate;
        
        public void Awake()
        {
            currentStamina = maxStamina;
        }

        public void Update()
        {
            currentStamina += Time.deltaTime * staminaRegenerationRate;
        }

        public float GetStamina()
        {
            return currentStamina;
        }

        /**
         * Consumes stamina if enough is available. Returns whether enough was available.
         */
        public bool ConsumeStaminaIf(float cost)
        {
            if (currentStamina >= cost)
            {
                currentStamina -= cost;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RegenerateStamina(float amount)
        {
            currentStamina += amount;
            currentStamina = Mathf.Max(currentStamina, maxStamina);
        }

        public float GetMaxStamina()
        {
            return maxStamina;
        }
    }
}