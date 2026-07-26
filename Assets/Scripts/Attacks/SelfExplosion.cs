using System;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using UnityEngine;

public class SelfExplosion : GenericAttack, IAttack
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject explosionSound;
    [SerializeField] private GameObject boomGnomeAttack;
    [SerializeField] private float boomDelay=.5f;
    
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

    public override void Attack(GameObject desiredTarget)
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
        
        //print (GnomeTracker.Instance.GetGnome(target.GetEntityId()));
        int layer = GnomeTracker.Instance.GetGnome(this.gameObject.GetEntityId()).GetSortingOrder();
        // targetTimeEntity.DealDamage(damage);
        SoundManager.Instance.CreateSoundAtPosition(boomGnomeAttack, transform.position);
        Invoke(nameof(Explode),boomDelay);
        
        TimeEntity.DealDamage(timeCost);
        StaminaEntity.ConsumeStaminaIf(staminaCost);
    }

    private void Explode()
    {
        Instantiate(explosion,transform.position,Quaternion.identity);
        SoundManager.Instance.CreateSoundAtPosition(explosionSound, transform.position);
        Destroy(gameObject);
    }
}
