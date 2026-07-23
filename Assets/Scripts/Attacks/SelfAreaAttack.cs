using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

public class SelfAreaAttack : MonoBehaviour, IAttackArea
{
    [SerializeField] public float damage = 10f;
    [SerializeField] public float range = 10f;
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

    public void Attack()
    {
        foreach (GameObject target in GetTargets())
        {
            TimeEntity timeEntity = target.GetComponent<TimeEntity>();
            if (timeEntity == null)
            {
                continue;
            }
            timeEntity.DealDamage(damage);
        }
    }
}
