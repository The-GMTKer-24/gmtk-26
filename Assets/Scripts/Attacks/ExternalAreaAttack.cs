using System;
using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

public class ExternalAreaAttack : MonoBehaviour, IAttack
{
    [SerializeField] private GameObject floorAnimation;
    [SerializeField] private GameObject frontAnimation;
    [SerializeField] private GameObject backAnimation;
    
    [SerializeField] public float damage = 60f;
    [SerializeField] public float shotRange = 10f;
    [SerializeField] public float shotRadius = 2.5f;
    [SerializeField] public float staminaCost = 10f;
    [SerializeField] public float timeCost = 10f;
    
    private TimeEntity _timeEntity;
    private StaminaEntity _staminaEntity;
    
    private void Awake()
    {
        _timeEntity = this.gameObject.GetComponent<TimeEntity>();
        if (_timeEntity == null && timeCost != 0)
        {
            throw new Exception("Cannot apply a nonzero time-cost attack to an object with no TimeEntity!");
        }
        _staminaEntity = this.gameObject.GetComponent<StaminaEntity>();
        if (_staminaEntity == null && staminaCost != 0)
        {
            throw new Exception("Cannot apply a nonzero stamina-cost attack to an object with no StaminaEntity!");
        }
    }
    
    public bool IsAoe()
    {
        return true;
    }
    
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
        return shotRange + shotRadius;
    }
    
    public bool CanHit(Vector2 targetPosition)
    {
        float distanceSq = Vector2.SqrMagnitude(targetPosition - (Vector2)transform.position);
        
        return distanceSq <= shotRange * shotRange;
    }

    public float OutOfRangeDistance(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(targetPosition, (Vector2)transform.position);
        
        return distance - GetRange();
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        float hits = 0f;
        
        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (Vector2.SqrMagnitude((Vector2)gnomeAI.transform.position - targetPosition) <= shotRadius * shotRadius)
            {
                hits += 1f;
            }
        }
        
        return hits;
    }

    public void Attack(GameObject target)
    {
        if (!(_staminaEntity.GetStamina() >= staminaCost)) return;

        foreach (GameObject hit in GetAllInRange(1f, target.transform.position))
        {
            TimeEntity targetTimeEntity;
            if (Player.Player.Instance.gameObject.Equals(hit)) targetTimeEntity = Player.Player.Instance.TimeEntity;
            else
            {
                GnomeAI potentialGnome = GnomeTracker.Instance.GetGnome(hit.GetEntityId());
                if (potentialGnome)
                {
                    targetTimeEntity = potentialGnome.timeEntity;
                }
                else
                {
                    targetTimeEntity = hit.GetComponent<TimeEntity>();
                }
            }

            if (!targetTimeEntity)
            {
                continue;
            }

            targetTimeEntity.DealDamage(damage);
        }
            
        //print (GnomeTracker.Instance.GetGnome(hit.GetEntityId()));
        int layer = GnomeTracker.Instance.GetGnome(this.gameObject.GetEntityId()).GetSortingOrder();
        if (floorAnimation) Instantiate(floorAnimation, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>().sortingOrder = -32767;
        if (frontAnimation) Instantiate(frontAnimation, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>().sortingOrder = layer + SortingOrderHandler.RecommendedOffset(-0.3f);
        if (backAnimation) Instantiate(backAnimation, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>().sortingOrder = layer - 1;
        
        _timeEntity.DealDamage(timeCost);
        _staminaEntity.ConsumeStaminaIf(staminaCost);
    }
    
    private Collection<GameObject> GetAllInRange(float factor, Vector2 center)
    {
        Collection<GameObject> targets = new Collection<GameObject>();
        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (gnomeAI.gameObject.Equals(this.gameObject)) continue;
            if (Vector2.SqrMagnitude((Vector2)gnomeAI.gameObject.transform.position - center) >= factor * factor * shotRadius * shotRadius) continue;
    
            targets.Add(gnomeAI.gameObject);
        }
            
        if (Vector2.SqrMagnitude((Vector2)Player.Player.Instance.gameObject.transform.position - center) <= factor * factor * shotRadius * shotRadius) { targets.Add(Player.Player.Instance.gameObject); }
            
        return targets;
    }
}