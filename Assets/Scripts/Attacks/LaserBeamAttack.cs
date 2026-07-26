using Attacks;
using Entity;
using UnityEngine;
using UnityEngine.Serialization;

public class LaserBeamAttack : GenericAttack, IAttack
{
    [SerializeField] GameObject beamAnimation;

    public override bool IsAoe()
    {
        return false;
    }

    public override float CountFriendlyFires(Vector2 targetPosition)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.gameObject.transform.position,
            targetPosition - (Vector2)this.gameObject.transform.position, range);

        float friendlyFires = 0f;
        
        foreach (RaycastHit2D hit in hits)
        {
            friendlyFires += GnomeTracker.Instance.DoesGnomeExist(hit.collider.gameObject.GetEntityId()) ? 1f : 0f;
        }
        
        return friendlyFires;
    }

    public override void Attack(GameObject target)
    {
        if (!(StaminaEntity.GetStamina() >= staminaCost)) return;
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.gameObject.transform.position,
            (Vector2)target.transform.position - (Vector2)this.gameObject.transform.position, range);

        foreach (RaycastHit2D rayHit in hits)
        {
            GameObject hit = rayHit.collider.gameObject;
            
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
        
        GameObject beam = Instantiate(this.beamAnimation, transform.position, Quaternion.identity);
        beam.transform.rotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(target.transform.position - transform.position, Vector2.up));
        
        TimeEntity.DealDamage(timeCost);
        StaminaEntity.ConsumeStaminaIf(staminaCost);
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
}