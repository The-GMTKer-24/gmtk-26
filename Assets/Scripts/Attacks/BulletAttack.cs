using System;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using Player;
using Unity.VisualScripting;
using UnityEngine;

public class BulletAttack : GenericAttack, IAttackTargeted
{
    [SerializeField] public float speed = 10f;

    [SerializeField] public GameObject bulletPrefab;
    
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

    public void Attack(GameObject target)
    {
        /*TimeEntity timeEntity = target.GetComponent<TimeEntity>(); 
        if (timeEntity == null)
        {
            throw new Exception("Target entity cannot take damage!");
        }
        timeEntity.DealDamage(damage);*/

        bool success = true;

        if (timeCost != 0)
        {
            _timeEntity.DealDamage(timeCost);
        }

        if (staminaCost != 0)
        {
            if (!_staminaEntity.ConsumeStaminaIf(staminaCost))
            {
                success = false;
            }
        }

        if (success)
        {
            GameObject bullet = Instantiate(this.bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().velocity =
                this.speed * Vector2.Normalize(target.transform.position - transform.position);
            bullet.GetComponent<EnemyBullet>().remainingTime = range / speed;
            bullet.GetComponent<Rigidbody2D>().rotation = -Vector2.SignedAngle(target.transform.position - transform.position, Vector2.up);
        }
    }
}
