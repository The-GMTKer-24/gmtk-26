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
        float hits = 0f;

        if (GnomeTracker.Instance == null)
        {
            return hits;
        }

        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (gnomeAI != null &&
                gnomeAI.gameObject != gameObject &&
                CanHit(gnomeAI.transform.position))
            {
                hits += 1f;
            }
        }
        
        return hits;
    }

    public override void Attack(GameObject target)
    {
        if (!TryConsumeStaminaCost())
        {
            return;
        }
        
        foreach (GameObject hit in GetAllInRange())
        {
            TimeEntity targetTimeEntity = AttackUtility.FindTimeEntity(hit);

            if (targetTimeEntity == null)
            {
                continue;
            }

            float safeDamage = GetDamage();

            if (safeDamage > 0f)
            {
                targetTimeEntity.DealDamage(safeDamage);
            }
        }

        int layer = AttackUtility.GetSortingOrder(gameObject);

        if (frontAnimation != null)
        {
            GameObject animationObject = Instantiate(
                frontAnimation,
                transform.position,
                Quaternion.identity
            );

            AttackUtility.SetSortingOrder(
                animationObject,
                layer + SortingOrderHandler.RecommendedOffset(-0.3f)
            );
        }

        if (backAnimation != null)
        {
            GameObject animationObject = Instantiate(
                backAnimation,
                transform.position,
                Quaternion.identity
            );

            AttackUtility.SetSortingOrder(animationObject, layer - 1);
        }

        if (soundEffect != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.CreateSoundAtPosition(
                soundEffect,
                transform.position
            );
        }
        
        ApplyTimeCost();
    }
}
