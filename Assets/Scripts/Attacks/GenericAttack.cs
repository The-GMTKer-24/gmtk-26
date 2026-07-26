using System;
using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

namespace Attacks
{
    public abstract class GenericAttack : MonoBehaviour, IAttack
    {
        [SerializeField] public float damage = 10f;
        [SerializeField] public float range = 10f;
        [SerializeField] public float staminaCost = 10f;
        [SerializeField] public float timeCost = 10f;
        
        protected TimeEntity TimeEntity;
        protected StaminaEntity StaminaEntity;

        protected void Awake()
        {
            TimeEntity = this.gameObject.GetComponent<TimeEntity>();
            if (TimeEntity == null && timeCost != 0)
            {
                throw new Exception("Cannot apply a nonzero time-cost attack to an object with no TimeEntity!");
            }
            StaminaEntity = this.gameObject.GetComponent<StaminaEntity>();
            if (StaminaEntity == null && staminaCost != 0)
            {
                throw new Exception("Cannot apply a nonzero stamina-cost attack to an object with no StaminaEntity!");
            }
        }

        public abstract bool IsAoe();
        public abstract void Attack(GameObject target);
        public abstract bool CanHit(Vector2 targetPosition);
        public abstract float OutOfRangeDistance(Vector2 targetPosition);
        public abstract float CountFriendlyFires(Vector2 targetPosition);
        
        public float GetDelay() {
                return 0f;
        }
        
        public float GetDamage()
        {
            return damage;
        }
    
        public float GetStaminaCost()
        {
            return staminaCost;
        }
    
        public float GetTimeCost()
        {
            return timeCost;
        }
    
        public float GetRange()
        {
            return range;
        }
    
        public bool InRange(Vector2 targetPosition)
        {
            Vector2 thisPosition = this.gameObject.transform.position;
            float distance = Vector2.Distance(thisPosition, targetPosition);
            return distance <= range;
        }
    
        // Returns all entities that can take damage within range
        protected Collection<GameObject> GetAllInRange(float factor)
        {
            Collection<GameObject> targets = new Collection<GameObject>();
            foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
            {
                if (gnomeAI.gameObject.Equals(this.gameObject)) continue;
                if (Vector2.SqrMagnitude(gnomeAI.gameObject.transform.position - this.gameObject.transform.position) >= factor * factor * range * range) continue;
    
                targets.Add(gnomeAI.gameObject);
            }
            
            if (Vector2.SqrMagnitude(Player.Player.Instance.gameObject.transform.position - this.gameObject.transform.position) <= factor * factor * range * range) { targets.Add(Player.Player.Instance.gameObject); }
            
            return targets;
        }
    
        protected Collection<GameObject> GetAllInRange()
        {
            return GetAllInRange(1f);
        }
    }
}