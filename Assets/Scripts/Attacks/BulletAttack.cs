using System;
using System.Collections.ObjectModel;
using Entity;
using Player;
using UnityEngine;

public class BulletAttack : MonoBehaviour, IAttackTargeted
{
    [SerializeField] public float damage = 10f;
    [SerializeField] public float range = 10f;
    [SerializeField] public float staminaCost = 10f;
    [SerializeField] public float timeCost = 10f;
    [SerializeField] public float speed = 10f;
    
    [SerializeField] public GameObject bulletPrefab;
    
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
        /*TimeEntity timeEntity = target.GetComponent<TimeEntity>(); 
        if (timeEntity == null)
        {
            throw new Exception("Target entity cannot take damage!");
        }
        timeEntity.DealDamage(damage);*/

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
            Instantiate(this.bulletPrefab, transform.position, Quaternion.identity).GetComponent<EnemyBullet>().velocity =
                this.speed * Vector2.Normalize(target.transform.position - transform.position);
        }
    }
}
