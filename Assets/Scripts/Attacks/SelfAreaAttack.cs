using System;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using UnityEngine;

public class SelfAreaAttack : GenericAttack, IAttack
{
    [SerializeField] private GameObject frontAnimation;
    [SerializeField] private GameObject backAnimation;
    [SerializeField] private GameObject soundEffect;
    
    public override bool IsAoe()
    {
        return true;
    }
    
    public override bool CanHit(Vector2 targetPosition)
    {
        float distanceSq = Vector2.SqrMagnitude(targetPosition - (Vector2)transform.position);
        
        return distanceSq <= range * range;
    }
    
    public override float OutOfRangeDistance(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(targetPosition, (Vector2)transform.position);
        
        return distance - GetRange();
    }

    public override float CountFriendlyFires(Vector2 targetPosition)
    {
        float hits = 0f;
        
        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (CanHit(gnomeAI.transform.position))
            {
                hits += 1f;
            }
        }
        
        return hits;
    }

    public override void Attack(GameObject target)
    {
        if (!(StaminaEntity.GetStamina() >= staminaCost)) return;
        
        foreach (GameObject hit in GetAllInRange())
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
        if (frontAnimation)
            Instantiate(frontAnimation, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>()
                .sortingOrder = layer + SortingOrderHandler.RecommendedOffset(-0.3f);
        if (backAnimation)
            Instantiate(backAnimation, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>()
                .sortingOrder = layer - 1;
        if (soundEffect) SoundManager.Instance.CreateSoundAtPosition(soundEffect, transform.position);
        
        TimeEntity.DealDamage(timeCost);
        StaminaEntity.ConsumeStaminaIf(staminaCost);
    }
}
