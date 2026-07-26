using System;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using Player;
using Unity.VisualScripting;
using UnityEngine;

public class BulletAttack : GenericAttack, IAttack
{
    [SerializeField] public float speed = 10f;

    [SerializeField] public GameObject bulletPrefab;

    public override bool IsAoe()
    {
        return false;
    }
    
    public override bool CanHit(Vector2 targetPosition)
    {
        if (Vector2.SqrMagnitude(targetPosition - (Vector2)this.transform.position) > range * range) return false;
        
        RaycastHit2D rayHit = Physics2D.Raycast(this.gameObject.transform.position,
            targetPosition - (Vector2)this.gameObject.transform.position, range);
        //print(rayHit);
        if (rayHit) return false;
        GameObject hit = rayHit.rigidbody.gameObject;
        if (!hit) return false;
        
        return Player.Player.Instance.gameObject.GetEntityId().Equals(hit.gameObject.GetEntityId());
    }
    
    public override float OutOfRangeDistance(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(targetPosition, (Vector2)transform.position);
        
        return distance - GetRange();
    }

    public override float CountFriendlyFires(Vector2 targetPosition)
    {
        RaycastHit2D rayHit = Physics2D.Raycast(this.gameObject.transform.position,
            targetPosition - (Vector2)this.gameObject.transform.position, range);
        //print(rayHit);
        if (rayHit) return 0;
        GameObject hit = rayHit.rigidbody.gameObject;
        
        if (!hit) return 0;
        
        return GnomeTracker.Instance.DoesGnomeExist(hit.gameObject.GetEntityId()) ? 1 : 0;
    }

    public override void Attack(GameObject target)
    {
        /*TimeEntity timeEntity = target.GetComponent<TimeEntity>(); 
        if (timeEntity == null)
        {
            throw new Exception("Target entity cannot take damage!");
        }
        timeEntity.DealDamage(damage);*/

        if (StaminaEntity.GetStamina() >= staminaCost)
        {
            GameObject bullet = Instantiate(this.bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().velocity =
                this.speed * Vector2.Normalize(target.transform.position - transform.position);
            bullet.GetComponent<EnemyBullet>().remainingTime = range / speed;
            bullet.GetComponent<Rigidbody2D>().rotation = -Vector2.SignedAngle(target.transform.position - transform.position, Vector2.up);
            
            TimeEntity.DealDamage(timeCost);
            StaminaEntity.ConsumeStaminaIf(staminaCost);
        }
    }
}
