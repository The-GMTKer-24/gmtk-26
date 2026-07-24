using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entity
{
    public class StaminaEntity : MonoBehaviour
    {
        [SerializeField] private float maxStamina;
        [SerializeField] private float minStartingStamina;
        [SerializeField] private float maxStartingStamina;
        [SerializeField] private float currentStamina = 0f;
        [SerializeField] private float staminaRegenerationRate;
        
        public void Awake()
        {
            currentStamina = Random.Range(minStartingStamina, maxStartingStamina);
        }

        public void FixedUpdate()
        {
            currentStamina += Time.fixedDeltaTime * staminaRegenerationRate;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
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
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }

        public float GetMaxStamina()
        {
            return maxStamina;
        }

        public void ResetStamina()
        {
            currentStamina = 0f;
        }
    }
}