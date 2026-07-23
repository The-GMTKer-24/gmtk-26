using System;
using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

public class MeleeAttack : MonoBehaviour, IAttackTargeted
{
    [SerializeField] public float damage = 20f;
    [SerializeField] public float range = 1f;
    [SerializeField] public float staminaCost = 10f;
    [SerializeField] public float timeCost = 10f;
    
    // TODO: Add sprite config
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public bool InRange(Vector2 targetPosition)
    {
        Vector2 thisPosition = this.gameObject.transform.position;
        float distance = Vector2.Distance(thisPosition, targetPosition);
        return distance <= range;
    }

    // Returns all entities that can take damage within range
    public Collection<GameObject> GetTargets()
    {
        Collection<GameObject> targets = new Collection<GameObject>();
        foreach (GameObject target in FindObjectsByType<GameObject>())
        {
            if (target.Equals(this.gameObject))
            {
                continue;
            }

            if (target.GetComponent<TimeEntity>() != null)
            {
                targets.Add(target);
            }
        }
        return targets;
    }

    public void Attack(GameObject target)
    {
        TimeEntity targetTimeEntity = target.GetComponent<TimeEntity>(); 
        if (targetTimeEntity == null)
        {
            throw new Exception("Target entity cannot take damage!");
        }
        
        bool success = true;

        if (timeCost != 0)
        {
            TimeEntity timeEntity = this.gameObject.GetComponent<TimeEntity>();
            if (timeEntity == null)
            {
                throw new Exception("Cannot apply a nonzero time-cost attack to an object with no TimeEntity!");
            }
            timeEntity.DealDamage(timeCost);
        }

        if (staminaCost != 0)
        {
            StaminaEntity staminaEntity = this.gameObject.GetComponent<StaminaEntity>();
            if (staminaEntity == null)
            {
                throw new Exception("Cannot apply a nonzero stamina-cost attack to an object with no StaminaEntity!");
            }

            if (!staminaEntity.ConsumeStaminaIf(staminaCost))
            {
                success = false;
            }
        }

        if (success)
        {
            targetTimeEntity.DealDamage(damage);
        }
    }
}