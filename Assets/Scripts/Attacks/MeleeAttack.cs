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
        float attackRange = GetRange();
        
        return distanceSq <= attackRange * attackRange;
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
        if (target == null)
        {
            return;
        }

        TimeEntity targetTimeEntity = AttackUtility.FindTimeEntity(target);

        if (targetTimeEntity == null)
        {
            Debug.LogWarning(
                "MeleeAttack target has no TimeEntity and cannot take damage.",
                target
            );
            return;
        }

        if (!TryConsumeStaminaCost())
        {
            return;
        }

        float safeDamage = GetDamage();

        if (safeDamage > 0f)
        {
            targetTimeEntity.DealDamage(safeDamage);
        }

        ApplyTimeCost();
    }
}
