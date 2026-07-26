using System.Collections.Generic;
using Attacks;
using Entity;
using UnityEngine;

public class LaserBeamAttack : GenericAttack, IAttack
{
    [SerializeField] GameObject beamAnimation;

    public override bool IsAoe()
    {
        return false;
    }

    public override float CountFriendlyFires(Vector2 targetPosition)
    {
        if (GnomeTracker.Instance == null)
        {
            return 0f;
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;
        float attackRange = GetRange();

        if (direction.sqrMagnitude <= Mathf.Epsilon ||
            attackRange <= 0f)
        {
            return 0f;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction.normalized,
            attackRange
        );

        float friendlyFires = 0f;
        HashSet<EntityId> countedGnomes = new HashSet<EntityId>();
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            GnomeAI gnome = hit.collider.GetComponentInParent<GnomeAI>();

            if (gnome != null &&
                gnome.gameObject != gameObject &&
                countedGnomes.Add(gnome.gameObject.GetEntityId()))
            {
                friendlyFires += 1f;
            }
        }
        
        return friendlyFires;
    }

    public override void Attack(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Vector2 direction =
            (Vector2)target.transform.position - (Vector2)transform.position;
        float attackRange = GetRange();

        if (direction.sqrMagnitude <= Mathf.Epsilon ||
            attackRange <= 0f ||
            !TryConsumeStaminaCost())
        {
            return;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction.normalized,
            attackRange
        );
        HashSet<EntityId> damagedEntities = new HashSet<EntityId>();

        foreach (RaycastHit2D rayHit in hits)
        {
            if (rayHit.collider == null)
            {
                continue;
            }

            TimeEntity targetTimeEntity =
                AttackUtility.FindTimeEntity(rayHit.collider.gameObject);

            if (targetTimeEntity == null ||
                targetTimeEntity == TimeEntity ||
                !damagedEntities.Add(
                    targetTimeEntity.gameObject.GetEntityId()
                ))
            {
                continue;
            }

            float safeDamage = GetDamage();

            if (safeDamage > 0f)
            {
                targetTimeEntity.DealDamage(safeDamage);
            }
        }

        if (beamAnimation != null)
        {
            GameObject beam = Instantiate(
                beamAnimation,
                transform.position,
                Quaternion.identity
            );

            beam.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                -Vector2.SignedAngle(direction, Vector2.up)
            );
        }

        ApplyTimeCost();
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
}
