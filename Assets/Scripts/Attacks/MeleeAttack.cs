using System;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using UnityEngine;

public class MeleeAttack : GenericAttack, IAttack
{
    // TODO: Add sprite config
    
    public override bool IsAoe()
    {
        return false;
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
        return 0f;
    }

    public override void Attack(GameObject target)
    {
        if (!(StaminaEntity.GetStamina() >= staminaCost)) return;
            
        TimeEntity targetTimeEntity;
        if (Player.Player.Instance.gameObject.Equals(target)) targetTimeEntity = Player.Player.Instance.TimeEntity;
        else
        {
            GnomeAI potentialGnome = GnomeTracker.Instance.GetGnome(target.GetEntityId());
            if (potentialGnome != null)
            {
                targetTimeEntity = potentialGnome.timeEntity;
            }
            else
            {
                targetTimeEntity = target.GetComponent<TimeEntity>();
            }
        }

        if (targetTimeEntity == null)
        {
            throw new Exception("Target entity cannot take damage!");
        }
            
        targetTimeEntity.DealDamage(damage);
        
        TimeEntity.DealDamage(timeCost);
        StaminaEntity.ConsumeStaminaIf(staminaCost);
    }
}