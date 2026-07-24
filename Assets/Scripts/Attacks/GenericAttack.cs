using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

namespace Attacks
{
    public class GenericAttack : MonoBehaviour, IAttack
    {
        [SerializeField] public float damage = 10f;
        [SerializeField] public float range = 10f;
        [SerializeField] public float staminaCost = 10f;
        [SerializeField] public float timeCost = 10f;
        
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
        public Collection<GameObject> GetAllInRange(float factor)
        {
            Collection<GameObject> targets = new Collection<GameObject>();
            foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
            {
                if (gnomeAI.gameObject.Equals(this.gameObject)) continue;
                if (Vector2.Distance(gnomeAI.gameObject.transform.position, this.gameObject.transform.position) >= factor * range) continue;
    
                targets.Add(gnomeAI.gameObject);
            }
            
            if (Vector2.Distance(Player.Player.Instance.gameObject.transform.position, this.gameObject.transform.position) <= factor * range) { targets.Add(Player.Player.Instance.gameObject); }
            
            return targets;
        }
    
        public Collection<GameObject> GetAllInRange()
        {
            return GetAllInRange(1f);
        }
    }
}